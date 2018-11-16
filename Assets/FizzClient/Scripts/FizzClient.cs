using System;
using Fizz.Common;
using Fizz.Threading;
using Fizz.Chat;
using Fizz.Chat.Impl;
using Fizz.Ingestion;
using Fizz.Ingestion.Impl;

namespace Fizz
{
    public enum FizzClientState
    {
        Closed,
        Opened
    }

    [Flags]
    public enum FizzServices
    {
        Chat =      1 << 0,
        Analytics = 1 << 1,
        All = Chat | Analytics
    }

    public interface IFizzClient
    {
        void Open(string userId, string locale, FizzServices services, Action<FizzException> callback);
        void Close(Action<FizzException> callback);
        void Update();

        IFizzChatClient Chat { get; }
        IFizzIngestionClient Ingestion { get; }
    }

    public class FizzClient: IFizzClient
    {
        readonly FizzActionDispatcher _dispatcher = new FizzActionDispatcher();
        readonly FizzChatClient _chat;
        readonly FizzIngestionClient _ingestion;
        readonly string _appId;
        readonly IFizzAuthRestClient _restClient;
        string _userId;
        FizzClientState _state;

        public FizzClient(string appId, string appSecret)
        {
            if (string.IsNullOrEmpty(appId))
            {
                throw FizzException.ERROR_INVALID_APP_ID;
            }

            _appId = appId;
            _restClient = CreateRestClient(_appId, appSecret, _dispatcher);
            _chat = new FizzChatClient(appId, _dispatcher);
            _ingestion = new FizzIngestionClient(new FizzInMemoryEventLog(), _dispatcher);
        }

        public void Open(string userId, string locale, FizzServices services, Action<FizzException> callback)
        {
            if (_state != FizzClientState.Closed)
            {
                Close();
            }

            try
            {
                _restClient.Open(userId, locale);
                _restClient.FetchSessionToken(ex =>
                {
                    if (ex == null)
                    {
                        if (services.HasFlag(FizzServices.Chat))
                        {
                            _chat.Open(userId, _restClient);
                        }
                        if (services.HasFlag(FizzServices.Analytics))
                        {
                            _ingestion.Open(userId, _restClient.session._serverTS, _restClient);
                        }

                        _userId = userId;
                        _state = FizzClientState.Opened;
                        FizzUtils.DoCallback(null, callback);
                    }
                    else
                    {
                        FizzUtils.DoCallback(ex, callback);
                    }
                });
            }
            catch (FizzException ex)
            {
                FizzUtils.DoCallback(ex, callback);
            }
        }

        public void Close(Action<FizzException> callback)
        {
            try
            {
                Close();
                FizzUtils.DoCallback(null, callback);
            }
            catch (FizzException ex)
            {
                FizzUtils.DoCallback(ex, callback);
            }
        }

        public void Update()
        {
            _dispatcher.Process();
        }
        
        public IFizzChatClient Chat
        {
            get
            {
                return _userId != null ? _chat : null;
            }
        }

        public IFizzIngestionClient Ingestion
        {
            get
            {
                return _userId != null ? _ingestion : null;
            }
        }

        public FizzClientState State
        {
            get
            {
                return _state;
            }
        }

        private void Close()
        {
            _ingestion.Close(() => 
            {
                _chat.Close();
                _restClient.Close();
                _userId = null;
                _state = FizzClientState.Closed;
            });
        }

        protected IFizzAuthRestClient CreateRestClient(string appId, 
                                                       string appSecret, 
                                                       IFizzActionDispatcher dispatcher)
        {
            return new FizzAuthRestClient(appId, appSecret, dispatcher);
        }
    }
}
