using System;
using System.Collections.Generic;
using Fizz.Common;
using Fizz.Common.Json;

namespace Fizz.Chat.Impl
{
    public class FizzChatClient : IFizzChatClient
    {
        private static readonly FizzException ERROR_INVALID_EVENTBUS = new FizzException(FizzError.ERROR_BAD_ARGUMENT, "invalid_eventbus");
        private static readonly FizzException ERROR_INVALID_DISPATCHER = new FizzException(FizzError.ERROR_BAD_ARGUMENT, "invalid_dispatcher");
        private static readonly FizzException ERROR_INVALID_REST_CLIENT = new FizzException(FizzError.ERROR_BAD_ARGUMENT, "invalid_rest_client");
        private static readonly FizzException ERROR_INVALID_CHANNEL = new FizzException(FizzError.ERROR_BAD_ARGUMENT, "invalid_channel_id");
        private static readonly FizzException ERROR_INVALID_MESSAGE_TYPE = new FizzException(FizzError.ERROR_BAD_ARGUMENT, "invalid_message_type");
        private static readonly FizzException ERROR_INVALID_MESSAGE_DATA = new FizzException(FizzError.ERROR_BAD_ARGUMENT, "invalid_message_data");
        private static readonly FizzException ERROR_INVALID_RESPONSE_FORMAT = new FizzException(FizzError.ERROR_REQUEST_FAILED, "invalid_response_format");
        private static readonly FizzException ERROR_INVALID_MESSAGE_QUERY_COUNT = new FizzException(FizzError.ERROR_BAD_ARGUMENT, "invalid_query_count");

        private readonly IFizzActionDispatcher _dispatcher;
        private readonly FizzMQTTChannelMessageListener _messageListener;
        private IFizzAuthRestClient _restClient;
        private string _userId;
        protected string _sessionId;

        public IFizzChannelMessageListener Listener 
        { 
            get
            {
                return _messageListener;
            }
        }

        public FizzChatClient(string appId, IFizzActionDispatcher dispatcher)
        {
            if (dispatcher == null)
            {
                throw ERROR_INVALID_DISPATCHER;
            }

            _dispatcher = dispatcher;

            _messageListener = CreateListener(appId, _dispatcher);
        }

        public void Open(string userId, IFizzAuthRestClient client)
        {
			IfClosed (() => {
				if (string.IsNullOrEmpty (userId)) {
					throw FizzException.ERROR_INVALID_USER_ID;
				}
				if (client == null) {
					throw ERROR_INVALID_REST_CLIENT;
				}

				if (_userId == userId) {
					return;
				}

				Close ();

				_sessionId = client.session._subscriberId;
				_userId = userId;
				_restClient = client;

				_messageListener.Open (userId, client.session);
				_messageListener.OnDisconnected += MessageListenerDisconnected;
			});
        }

        public void Close()
        {
            IfOpened(() =>
            {
                _userId = null;
                _sessionId = null;
                _restClient = null;

                _messageListener.Close();
            });
        }

        public void Publish(String channel, 
                            String nick, 
                            String body, 
                            String data,
                            bool translate,
                            bool persist,
                            Action<FizzException> callback)
        {
            IfOpened(() =>
            {
                if (string.IsNullOrEmpty(channel))
                {
                    FizzUtils.DoCallback(ERROR_INVALID_CHANNEL, callback);
                    return;
                }

                try
                {
                    string path = string.Format(FizzConfig.API_PATH_MESSAGES, channel);
                    JSONClass json = new JSONClass();
                    json[FizzJsonChannelMessage.KEY_NICK] = nick;
                    json[FizzJsonChannelMessage.KEY_BODY] = body;
                    json[FizzJsonChannelMessage.KEY_DATA] = data;
                    json["translate"].AsBool = translate;
                    json["persist"].AsBool = persist;

                    _restClient.Post(FizzConfig.API_BASE_URL, path, json.ToString(), (response, ex) =>
                    {
                        FizzUtils.DoCallback(ex, callback);
                    });
                }
                catch (FizzException ex)
                {
                    FizzUtils.DoCallback(ex, callback);
                }
            });
        }

        public void Subscribe(String channel, Action<FizzException> callback)
        {
            IfOpened(() => 
            {
                if (channel == null || string.IsNullOrEmpty(channel))
                {
                    FizzUtils.DoCallback(ERROR_INVALID_CHANNEL, callback);
                    return;
                }

                string path = string.Format(FizzConfig.API_PATH_SUBSCRIBERS, channel);

                _restClient.Post(FizzConfig.API_BASE_URL, path, "", (resp, ex) =>
                {
                    FizzUtils.DoCallback(ex, callback);
                });
            });
        }

        public void Unsubscribe(String channel, Action<FizzException> callback)
        {
            IfOpened(() => 
            {
                if (channel == null || string.IsNullOrEmpty(channel))
                {
                    FizzUtils.DoCallback(ERROR_INVALID_CHANNEL, callback);
                    return;
                }

                string path = string.Format(FizzConfig.API_PATH_SUBSCRIBERS, channel);

                _restClient.Delete(FizzConfig.API_BASE_URL, path, "", (resp, ex) =>
                {
                    FizzUtils.DoCallback(ex, callback);
                });
            });
        }

        public void QueryLatest(String channel, int count, Action<IList<FizzChannelMessage>, FizzException> callback)
        {
            IfOpened(() => 
            {
                if (string.IsNullOrEmpty(channel))
                {
                    FizzUtils.DoCallback<IList<FizzChannelMessage>>(null, ERROR_INVALID_CHANNEL, callback);
                    return;
                }
                if (count < 0)
                {
                    FizzUtils.DoCallback<IList<FizzChannelMessage>>(null, ERROR_INVALID_MESSAGE_QUERY_COUNT, callback);
                    return;
                }
                if (count == 0)
                {
                    FizzUtils.DoCallback<IList<FizzChannelMessage>>(new List<FizzChannelMessage>(), null, callback);
                    return;
                }

                string path = string.Format(FizzConfig.API_PATH_MESSAGES, channel);
                _restClient.Get(FizzConfig.API_BASE_URL, path + "?count=" + count, (response, ex) =>
                {
                    if (ex != null)
                    {
                        FizzUtils.DoCallback<IList<FizzChannelMessage>>(null, ex, callback);
                    }
                    else
                    {
                        try
                        {
                            JSONArray messagesArr = JSONNode.Parse(response).AsArray;
                            IList<FizzChannelMessage> messages = new List<FizzChannelMessage>();
                            foreach (JSONNode message in messagesArr.Childs)
                            {
                                messages.Add(new FizzJsonChannelMessage(message));
                            }
                            FizzUtils.DoCallback<IList<FizzChannelMessage>>(messages, null, callback);
                        }
                        catch
                        {
                            FizzUtils.DoCallback<IList<FizzChannelMessage>>(null, ERROR_INVALID_RESPONSE_FORMAT, callback);
                        }
                    }
                });
            });
        }

        protected FizzMQTTChannelMessageListener CreateListener(string appId, IFizzActionDispatcher dispatcher)
        {
            return new FizzMQTTChannelMessageListener(appId, dispatcher);
        }

		private void MessageListenerDisconnected (FizzException ex) {
			if (ex != null && ex.Code == FizzError.ERROR_AUTH_FAILED) {
				IfOpened (() => {
					_restClient.FetchSessionToken (tokenEx => {
						_messageListener.Close ();
						_messageListener.Open (_userId, _restClient.session);
					});
				});
			}
		}

        private void IfOpened(Action callback)
        {
			if (!string.IsNullOrEmpty (_userId))
            {
				FizzUtils.DoCallback (callback);
            }
            else
            {
                FizzLogger.W("Chat client should be opened before usage.");
            }
        }

		private void IfClosed(Action callback)
		{
			if (string.IsNullOrEmpty (_userId)) {
				FizzUtils.DoCallback (callback);
			} else {
				FizzLogger.W ("Chat client should be closed before opening.");
			}
		}
    }
}
