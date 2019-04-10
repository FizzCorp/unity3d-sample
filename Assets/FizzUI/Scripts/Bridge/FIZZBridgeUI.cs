
#if UNITY_IOS && !UNITY_EDITOR
using FIZZ.Bridge.UI.iOS;
#elif UNITY_ANDROID && !UNITY_EDITOR
using FIZZ.Bridge.UI.Android;
#else
using FIZZ.Bridge.UI.Mock;
#endif

namespace Fizz.UI.Bridge
{
    internal static class FIZZBridgeUI
	{
		internal static FIZZBridgeUIImpl Instance {
			get { 
				return FIZZBridgeUIImpl.Instance;
			}
		}
	}
}