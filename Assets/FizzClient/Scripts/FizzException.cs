using System;

namespace Fizz
{
	public class FizzException: Exception
    {
        public static readonly FizzException ERROR_INVALID_APP_ID = new FizzException(FizzError.ERROR_BAD_ARGUMENT, "invalid_app_id");
        public static readonly FizzException ERROR_INVALID_USER_ID = new FizzException(FizzError.ERROR_BAD_ARGUMENT, "invalid_user_id");

        public FizzException(int code, string reason): base(reason)
        {
            Code = code;
            Reason = reason;
        }

        public int Code { get; private set; }

        public string Reason { get; private set; }
    }
}
