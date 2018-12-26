//
//  UIErrorHelper.cs
//
//  Copyright (c) 2016 Fizz Inc
//

using Fizz;

namespace FIZZ.UI.Core {
    public static class UIErrorHelper {
        public static bool IsAnyGeneralArror (FizzException error) {
            if (error == null) {
                return false;
            } else if (error.Code == FizzError.ERROR_AUTH_FAILED) {
                return true;
            }
            return false;
        }

        public static string ParseGeneralError (FizzException error) {
            return string.Empty;
        }
    }
}