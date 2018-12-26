#if UNITY_ANDROID
using UnityEngine;

namespace FIZZ.Bridge
{
    internal class JavaInterfaceProxy : AndroidJavaProxy
    {
        private static JavaInterfaceProxy comparingWho;

		internal JavaInterfaceProxy(string javaInterface) : base(javaInterface) {}
        protected bool equals(AndroidJavaObject other)
        {
            bool result = false;
            
            if(comparingWho != null)
            {
                result = comparingWho == this;
                comparingWho = null;
            }
            else
            {
                comparingWho = this;
                result = other.Call<bool>("equals", other);
            }
            
            return result;
        }

        protected string toString()
        {
            return GetType().Name;
        }
    }
}
#endif
