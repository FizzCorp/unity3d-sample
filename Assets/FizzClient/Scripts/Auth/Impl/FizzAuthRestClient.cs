using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using Fizz.Common.Json;

namespace Fizz.Common
{
    public class FizzAuthRestClient: IFizzAuthRestClient
    {
        private static readonly FizzException ERROR_SESSION_CREATION_FAILED = new FizzException(FizzError.ERROR_REQUEST_FAILED, "session_creation_failed");
        private static readonly FizzException ERROR_INVALID_APP_SECRET = new FizzException(FizzError.ERROR_BAD_ARGUMENT, "invalid_app_secret");
        private static readonly FizzException ERROR_INVALID_LOCALE = new FizzException(FizzError.ERROR_BAD_ARGUMENT, "invalid_locale");

        private string _userId;
        private string _locale;
        private FizzSession _session;
        private IFizzRestClient _restClient;
        private IFizzSessionProvider _sessionClient;
        private Queue<Action<FizzException>> _requestQueue = new Queue<Action<FizzException>>();

        public FizzSession Session
        {
            get {
                return _session;
            }
        }

        public FizzAuthRestClient(IFizzSessionProvider sessionClient, IFizzRestClient restClient)
        {
            _restClient = restClient;
            _sessionClient = sessionClient;
        }

        public void Open(string userId, string locale)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw FizzException.ERROR_INVALID_APP_ID;
            }
            if (string.IsNullOrEmpty(locale))
            {
                throw ERROR_INVALID_LOCALE;
            }

            if (_userId == userId)
            {
                return;
            }

            _userId = userId;
            _locale = locale;
            _session = new FizzSession(null, null, 0);
        }

        public void Close()
        {
            _userId = null;
            _session = new FizzSession();
        }

        public void Post(string host, 
                         string path, 
                         string json,
                         Action<string, FizzException> callback)
        {
            _restClient.Post(host, path, json, AuthHeaders(), (response, ex) =>
            {
                ProcessResponse(response, ex, callback, () => 
                {
                    _restClient.Post(host, path, json, AuthHeaders(), callback);
                });
            });
        }

        public void Delete(string host, 
                           string path, 
                           string json, 
                           Action<string, FizzException> callback)
        {
            _restClient.Delete(host, path, json, AuthHeaders(), (response, ex) => 
            {
                ProcessResponse(
                    response, 
                    ex, 
                    callback, 
                    () => _restClient.Delete(host, path, json, AuthHeaders(), callback)
                );
            });    
        }

        public void Get(string host, 
                        string path, 
                        Action<string, FizzException> callback)
        {
            _restClient.Get(host, path, AuthHeaders(), (response, ex) => 
            {
                ProcessResponse(
                    response, 
                    ex, 
                    callback, 
                    () => _restClient.Get(host, path, AuthHeaders(), callback)
                );
            });
        }

        public void FetchSessionToken(Action<FizzException> onFetch)
        {
            _requestQueue.Enqueue(onFetch);
            if (_requestQueue.Count > 1)
            {
                return;
            }

            _sessionClient.FetchToken (_userId, _locale, (session, ex) => {
                if (ex != null)
                {
                    while (_requestQueue.Count > 0)
                    {
                        FizzUtils.DoCallback(ex, _requestQueue.Dequeue());
                    }
                }
                else 
                {
                    _session = session;
                    while (_requestQueue.Count > 0)
                    {
                        FizzUtils.DoCallback(null, _requestQueue.Dequeue());
                    }
                }
            });
        }
       
        private IDictionary<string,string> AuthHeaders()
        {
            if (string.IsNullOrEmpty(_session._token))
            {
                return null;
            }
            else
            {
                return FizzUtils.headers(_session._token);
            }
        }


        private void ProcessResponse(string response, 
                                     FizzException ex, 
                                     Action<string,FizzException> onResult,
                                     Action onRetry)
        {
            if (ex != null)
            {
                if (ex.Code == FizzError.ERROR_AUTH_FAILED)
                {
                    FetchSessionToken(authEx =>
                    {
                        if (authEx != null)
                        {
                            FizzUtils.DoCallback<string>(null, ex, onResult);
                        }
                        else
                        {
                            if (onRetry != null)
                            {
                                onRetry();
                            }
                        }
                    });
                }
                else
                {
                    FizzUtils.DoCallback<string>(null, ex, onResult);
                }
            }
            else
            {
                FizzUtils.DoCallback<string>(response, ex, onResult);
            }
        }
    }
}
