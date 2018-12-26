using System;

namespace FIZZ.Bridge.UI.Mock {
	internal class FIZZBridgeUIImpl {

		private static FIZZBridgeUIImpl s_instance = new FIZZBridgeUIImpl ();

		public static FIZZBridgeUIImpl Instance {
			get {
				return s_instance;
			}
		}

		public void GetKeyboardJavaObject () { }

		public void ResetAndroidKeyboardObject () {

		}

		public void ShowKeyboard (string text, string hintText) {

		}

		public void ShowKeyboard (string text, string hintText, bool isEmoji) {

		}

		public string GetText () {
			return string.Empty;
		}

		public float GetKeyboardHeight () {
			return 0.0f;
		}

		public bool IsKeyboardOpen () {
			return false;
		}

		public void HideKeyboard () {

		}

		public void SetKeyboardMessageLayoutGraphics (string mainLayoutBg, string editTextBg, string doneButtonBg) {

		}
	}
}
