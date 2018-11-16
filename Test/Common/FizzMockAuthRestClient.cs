using System;

namespace Fizz.Common
{
    class FizzMockAuthRestClient: IFizzAuthRestClient
    {
        public Action<string, string, string> OnPost { get; set; }
        public Action<string, string, string> OnDelete { get; set; }
        public Action<string, string> OnGet { get; set; }

        public FizzMockAuthRestClient()
        {
            session = new Session(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), 12345L);
        }

        public void Open(string userId, string locale)
        {
        }

        public void Close()
        {
        }

        public void FetchSessionToken(Action<FizzException> callback)
        {
            FizzUtils.DoCallback(null, callback);
        }

        public void Post(string host, string path, string json, Action<string, FizzException> callback)
        {
            if (OnPost != null) 
            {
                OnPost.Invoke(host, path, json);
            }
        }

        public void Delete(string host, string path, string json, Action<string, FizzException> callback)
        {
            if (OnDelete != null)
            {
                OnDelete.Invoke(host, path, json);
            }
        }

        public void Get(string host, string path, Action<string, FizzException> callback)
        {
            if (OnGet != null)
            {
                OnGet.Invoke(host, path);
            }
        }

        public Session session
        {
            get;
            set;
        }
    }
}
