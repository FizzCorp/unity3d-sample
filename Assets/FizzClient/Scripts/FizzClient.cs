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
        FizzClientState State { get; }
    }

    public class FizzClient: IFizzClient
    {
        readonly string _appId;
        readonly FizzChatClient _chat;
        readonly IFizzRestClient _restClient;
        readonly IFizzAuthRestClient _authClient;
        readonly IFizzSessionProvider _sessionClient;
        readonly FizzIngestionClient _ingestionClient;
        readonly FizzActionDispatcher _dispatcher = new FizzActionDispatcher();
        
        string _userId;
        FizzClientState _state;

        public FizzClient(string appId, string appSecret)
        {
            if (string.IsNullOrEmpty(appId))
            {
                throw FizzException.ERROR_INVALID_APP_ID;
            }

            _appId = appId;
            _chat = new FizzChatClient(appId, _dispatcher);
            _restClient = new FizzRestClient (_dispatcher);
            _sessionClient = new FizzIdSecretSessionProvider (appId, appSecret, _restClient);
            _authClient = new FizzAuthRestClient(_sessionClient, _restClient);
            _ingestionClient = new FizzIngestionClient(new FizzInMemoryEventLog(), _dispatcher);
        }

        public FizzClient (string appId, IFizzSessionProvider sessionClient)
        {
            if (string.IsNullOrEmpty(appId))
            {
                throw FizzException.ERROR_INVALID_APP_ID;
            }
            _appId = appId;

            _sessionClient = sessionClient;
            _chat = new FizzChatClient(appId, _dispatcher);
            _restClient = new FizzRestClient (_dispatcher);
            _authClient = new FizzAuthRestClient(_sessionClient, _restClient);
            _ingestionClient = new FizzIngestionClient(new FizzInMemoryEventLog(), _dispatcher);
        }

        public void Open(string userId, string locale, FizzServices services, Action<FizzException> callback)
        {
            try
            {
                _authClient.Open(userId, locale);
                _authClient.FetchSessionToken(ex =>
                {
                    if (ex == null)
                    {
                        if (services.HasFlag(FizzServices.Chat))
                        {
                            _chat.Open(userId, _authClient);
                        }
                        if (services.HasFlag(FizzServices.Analytics))
                        {
                            _ingestionClient.Open(userId, _authClient.Session._serverTS, _authClient);
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
                Close(() => { FizzUtils.DoCallback(null, callback); });
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
                return _userId != null ? _ingestionClient : null;
            }
        }

        public FizzClientState State
        {
            get
            {
                return _state;
            }
        }
        
        private void Close(Action callback)
        {
            _ingestionClient.Close(() =>
            {
                _chat.Close();
                _authClient.Close();
                _userId = null;
                _state = FizzClientState.Closed;

                callback();
            });
        }
    }
}
