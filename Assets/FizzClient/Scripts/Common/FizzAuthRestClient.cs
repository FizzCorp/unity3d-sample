using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using Fizz.Common.Json;
using Fizz.Threading;

namespace Fizz.Common
{
    public struct Session
    {
        public readonly string _token;
        public readonly string _subscriberId;
        public readonly long _serverTS;

        public Session(string token, string subscriberId, long serverTS)
        {
            _token = token;
            _subscriberId = subscriberId;
            _serverTS = serverTS;
        }
    }

    public interface IFizzAuthRestClient
    {
        void Open(string userId, string locale);
        void Close();
        void FetchSessionToken(Action<FizzException> callback);
        void Post(string host, string path, string json, Action<string, FizzException> callback);
        void Delete(string host, string path, string json, Action<string, FizzException> callback);
        void Get(string host, string path, Action<string, FizzException> callback);

        Session session { get; }
    }

    public class FizzAuthRestClient: FizzRestClient, IFizzAuthRestClient
    {
        private static readonly FizzException ERROR_SESSION_CREATION_FAILED = new FizzException(FizzError.ERROR_REQUEST_FAILED, "session_creation_failed");
        private static readonly FizzException ERROR_INVALID_APP_SECRET = new FizzException(FizzError.ERROR_BAD_ARGUMENT, "invalid_app_secret");
        private static readonly FizzException ERROR_INVALID_LOCALE = new FizzException(FizzError.ERROR_BAD_ARGUMENT, "invalid_locale");

        private readonly string _appId;
        private readonly string _appSecret;
        private string _userId;
        private string _locale;
        private Session _session;
        private Queue<Action<FizzException>> _requestQueue = new Queue<Action<FizzException>>();

        public Session session
        {
            get {
                return _session;
            }
        }

        public FizzAuthRestClient(string appId, 
                                  string appSecret, 
                                  IFizzActionDispatcher dispatcher): base(dispatcher)
        {
            if (string.IsNullOrEmpty(appId))
            {
                throw FizzException.ERROR_INVALID_APP_ID;
            }
            if (string.IsNullOrEmpty(appSecret))
            {
                throw ERROR_INVALID_APP_SECRET;
            }

            _appId = appId;
            _appSecret = appSecret;
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
            _session = new Session(null, null, 0);
        }

        public void Close()
        {
            _userId = null;
            _session = new Session();
        }

        public void Post(string host, 
                         string path, 
                         string json,
                         Action<string, FizzException> callback)
        {
            Post(host, path, json, AuthHeaders(), (response, ex) =>
            {
                ProcessResponse(response, ex, callback, () => 
                {
                    Post(host, path, json, AuthHeaders(), callback);
                });
            });
        }

        public void Delete(string host, 
                           string path, 
                           string json, 
                           Action<string, FizzException> callback)
        {
            Delete(host, path, json, AuthHeaders(), (response, ex) => 
            {
                ProcessResponse(
                    response, 
                    ex, 
                    callback, 
                    () => Delete(host, path, json, AuthHeaders(), callback)
                );
            });    
        }

        public void Get(string host, 
                        string path, 
                        Action<string, FizzException> callback)
        {
            Get(host, path, AuthHeaders(), (response, ex) => 
            {
                ProcessResponse(
                    response, 
                    ex, 
                    callback, 
                    () => Get(host, path, AuthHeaders(), callback)
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

            JSONClass node = new JSONClass();
            node["app_id"] = _appId;
            node["user_id"] = _userId;
            node["locale"] = _locale;

            string body = node.ToString();
            string digest = GenerateHmac(body, _appSecret);
            var headers = new Dictionary<string, string>() { { "Authorization", "HMAC-SHA256 " + digest } };

            Post(FizzConfig.API_BASE_URL, FizzConfig.API_PATH_SESSIONS, body, headers, (response, ex) =>
            {
                if (ex != null)
                {
                    while (_requestQueue.Count > 0)
                    {
                        FizzUtils.DoCallback(ex, _requestQueue.Dequeue());
                    }
                }
                else
                {
                    try
                    {
                        _session = ParseSession(JSONNode.Parse(response));
                        while (_requestQueue.Count > 0)
                        {
                            FizzUtils.DoCallback(null, _requestQueue.Dequeue());
                        }
                    }
                    catch (Exception responseEx)
                    {
                        FizzLogger.E(responseEx.Message);
                        while (_requestQueue.Count > 0)
                        {
                            FizzUtils.DoCallback(ERROR_SESSION_CREATION_FAILED, _requestQueue.Dequeue());
                        }
                    }
                }
            });
        }

        private Session ParseSession(JSONNode json) 
        {
            string token = json["token"];
            string subId = json["subscriber_id"];
            long now = 0;
            long.TryParse(json["now_ts"], out now);

            return new Session(token, subId, now);
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

        private string GenerateHmac(string json, string secretKey)
        {
            var encoding = new System.Text.UTF8Encoding();
            var body = encoding.GetBytes(json);
            var key = encoding.GetBytes(secretKey);

            using (var encoder = new HMACSHA256(key))
            {
                byte[] hashmessage = encoder.ComputeHash(body);
                return System.Convert.ToBase64String(hashmessage);
            }
        }
    }
}
