using System.Threading.Tasks;

namespace Fizz.Common
{
    static class FizzTestUtils
    {
        public static Task FetchSessionToken(IFizzAuthRestClient client, string userId, string locale)
        {
            TaskCompletionSource<object> fetched = new TaskCompletionSource<object>();

            client.Open(userId, locale);
            client.FetchSessionToken(ex =>
            {
                if (ex != null)
                {
                    fetched.SetException(ex);
                }
                else
                {
                    fetched.SetResult(null);
                }
            });

            return fetched.Task;
        }
    }
}
