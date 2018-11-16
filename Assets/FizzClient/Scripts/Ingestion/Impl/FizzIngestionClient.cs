using System;
using System.Text;
using System.IO;
using System.Collections.Generic;

using Fizz.Common;
using Fizz.Common.Json;

namespace Fizz.Ingestion.Impl
{
    public class FizzIngestionClient : IFizzIngestionClient
    {
        static readonly int LOG_ROLL_INTERVAL = 5 * 1000;
        static readonly int EVENT_VER = 1;
#if UNITY_ANDROID
        static readonly string PLATFORM = "android";
#elif UNITY_IOS
        static readonly string PLATFORM = "ios";
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        static readonly string PLATFORM = "mac_osx";
#else
        static readonly string PLATFORM = "windows";
#endif
        static readonly FizzException ERROR_INVALID_CLIENT = new FizzException(FizzError.ERROR_BAD_ARGUMENT, "invalid_client");

        IFizzAuthRestClient _client;
        string _userId;
        long _timeOffset;
        long _startTime;
        string _sessionId;
        IFizzEventLog _eventLog;
        IFizzActionDispatcher _dispatcher;
        Action _onLogEmpty;

        public string BuildVer { get; set; }
        public string CustomDimesion01 { get; set; }
        public string CustomDimesion02 { get; set; }
        public string CustomDimesion03 { get; set; }

        public FizzIngestionClient(IFizzEventLog eventLog, IFizzActionDispatcher dispatcher)
        {
            if (eventLog == null)
            {
                throw new FizzException(FizzError.ERROR_BAD_ARGUMENT, "invalid_event_log");
            }
            if (dispatcher == null)
            {
                throw new FizzException(FizzError.ERROR_BAD_ARGUMENT, "invalid_dispatcher");
            }

            _eventLog = eventLog;
            _dispatcher = dispatcher;
        }

        public void Open(string userId, long curServerTS, IFizzAuthRestClient client)
        {
            if (_userId != null)
            {
                FizzLogger.W("Please close instance before re-opening");
                return;
            }

            if (userId == null)
            {
                throw FizzException.ERROR_INVALID_USER_ID;
            }
            if (client == null)
            {
                throw ERROR_INVALID_CLIENT;
            }


            _client = client;
            _userId = userId;
            _timeOffset = FizzUtils.Now() - curServerTS;
            _startTime = FizzUtils.Now();
            _sessionId = Guid.NewGuid().ToString();

            SessionStarted();

            _dispatcher.Delay(LOG_ROLL_INTERVAL, () => Flush());
        }

        public void Close(Action callback)
        {
            IfOpened(() =>
            {
                SessionEnded();
                Flush();

                _onLogEmpty += () =>
                {
                    _userId = null;
                    _client = null;
                    _sessionId = null;
                    _onLogEmpty = null;
                };
            });
        }

        public void ProductPurchased(string productId, double amount, string currency)
        {
            IfOpened(() => 
            {
                FizzLogger.D("Product Purchased => id:" + productId + " amount:" + amount + " currency:" + currency);

                JSONClass fields = new JSONClass();

                if (productId != null)
                {
                    fields["product_id"] = productId;
                }
                if (currency != null)
                {
                    fields["currency"] = currency;
                }
                fields["amount"].AsDouble = amount;

                _eventLog.Put(BuildEvent(FizzEventType.product_purchased, fields)); 
            });
        }

        public void TextMessageSent(string channelId, string content, string senderNick)
        {
            IfOpened(() =>
            {
                FizzLogger.D("Text Message Sent => channel:" + channelId + " content:" + content + " nick:" + senderNick);

                JSONClass fields = new JSONClass();

                if (channelId != null)
                {
                    fields["channel_id"] = channelId;
                }
                if (content != null)
                {
                    fields["content"] = content;
                }
                if (senderNick != null)
                {
                    fields["nick"] = senderNick;
                }

                _eventLog.Put(BuildEvent(FizzEventType.text_msg_sent, fields)); 
            });
        }

        private void SessionStarted()
        {
            FizzLogger.D("Session Started");

            _eventLog.Put(BuildEvent(FizzEventType.session_started, null));   
        }

        private void SessionEnded()
        {
            FizzLogger.D("Session Ended");

            JSONClass fields = new JSONClass();

            fields["duration"].AsDouble = FizzUtils.Now() - _startTime;

            _eventLog.Put(BuildEvent(FizzEventType.session_ended, fields));
        }

        private FizzEvent BuildEvent(FizzEventType type, JSONNode fields)
        {
            try
            {
                return new FizzEvent(
                    _userId,
                    type,
                    EVENT_VER,
                    _sessionId,
                    FizzUtils.Now() + _timeOffset,
                    PLATFORM,
                    BuildVer,
                    CustomDimesion01, CustomDimesion02, CustomDimesion03,
                    fields
                );
            }
            catch (Exception ex)
            {
                FizzLogger.E("Invalid event encountered: " + ex.Message);
                return null;
            }
        }

        private void IfOpened(Action onInit)
        {
            if (_userId == null)
            {
                FizzLogger.W("Ingestion must be opened before use.");
            }
            else
            {
                onInit.Invoke();
            }
        }

        public void Flush()
        {
            _eventLog.Read(128, items =>
            {
                if (items.Count <= 0)
                {
                    FizzUtils.DoCallback(_onLogEmpty);
                    return;
                }

                PostEvents(items, (response, ex) => 
                {
                    bool rollLog = true;

                    if (ex != null)
                    {
                        if (ex.Code == FizzError.ERROR_REQUEST_FAILED)
                        {
                            FizzLogger.W("Failed to submit events to service");
                            rollLog = false;
                        }
                        else
                        if (ex.Code == FizzError.ERROR_INVALID_REQUEST)
                        {
                            FizzLogger.E("Submission of some events failed: " + ex.Message);
                        }
                    }

                    if (rollLog)
                    {
                        _eventLog.RollTo(items[items.Count - 1]);
                    }

                    _dispatcher.Delay(LOG_ROLL_INTERVAL, () => Flush());
                });
            }
          );
        }

        private void PostEvents(List<FizzEvent> events,
                                Action<string, FizzException> callback)
        {
            _client.Post(
                FizzConfig.API_BASE_URL,
                FizzConfig.API_PATH_EVENTS,
                ParseEvents(events),
                callback
            );
        }

        private string ParseEvents(List<FizzEvent> items)
        {
            JSONArray events = new JSONArray();

            foreach (FizzEvent item in items)
            {
                events.Add(ParseEvent(item));
            }

            return events.ToString();
        }

        private static string StreamToString(Stream stream)
        {
            stream.Position = 0;
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        private JSONNode ParseEvent(FizzEvent item)
        {
            JSONClass json = new JSONClass();

            json["user_id"] = item.UserId;
            json["type"] = Enum.GetName(typeof(FizzEventType), item.Type);
            //json["ver"].AsInt = EVENT_VER;
            json.Add("ver", new JSONData(EVENT_VER));
            json["session_id"] = item.SessionId;
            json["time"].AsDouble = item.Time;

            if (item.Platform != null)
            {
                json["platform"] = item.Platform;
            }
            if (item.Build != null)
            {
                json["build"] = item.Build;
            }
            if (item.Custom01 != null)
            {
                json["custom_01"] = item.Custom01;
            }
            if (item.Custom02 != null)
            {
                json["custom_02"] = item.Custom02;
            }
            if (item.Custom03 != null)
            {
                json["custom_03"] = item.Custom03;
            }

            if (item.Fields != null) 
            {
                foreach (string key in item.Fields.Keys)
                {
                    json[key] = item.Fields[key];
                }   
            }

            return json;
        }
    }
}
