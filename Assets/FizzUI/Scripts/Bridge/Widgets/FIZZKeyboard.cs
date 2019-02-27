using System;
using Fizz.UI.Bridge;

public class FIZZKeyboard {

    public static Action<string> OnCancel = delegate { };

    public static Action<string> OnDoneButtonPress = delegate { };

    public static Action<float> OnKeyboardOpen = delegate { };

    public static Action OnKeyboardClose = delegate { };

    public static Action OnEmojiClicked = delegate { };

    public FIZZKeyboard () {

    }

    public static void GetKeyboardJavaObject () {
        FIZZBridgeUI.Instance.GetKeyboardJavaObject ();
    }

    public static void ResetAndroidKeyboardObject () {
        FIZZBridgeUI.Instance.ResetAndroidKeyboardObject ();
    }

    public static void ShowKeyboard (string text, string hintText) {
        FIZZBridgeUI.Instance.ShowKeyboard (text, hintText);
    }

    public static void ShowKeyboard (string text, string hintText, bool isEmoji) {
        FIZZBridgeUI.Instance.ShowKeyboard (text, hintText, isEmoji);
    }

    public static string GetText () {
        return FIZZBridgeUI.Instance.GetText ();
    }

    public static float GetKeyboardHeight () {
        return FIZZBridgeUI.Instance.GetKeyboardHeight ();
    }

    public static bool IsKeyboardOpen () {
        return FIZZBridgeUI.Instance.IsKeyboardOpen ();
    }

    public static void HideKeyboard () {
        FIZZBridgeUI.Instance.HideKeyboard ();
    }

    public static void SetKeyboardMessageLayoutGraphics (string mainLayoutBg, string editTextBg, string doneButtonBg) {
        FIZZBridgeUI.Instance.SetKeyboardMessageLayoutGraphics (mainLayoutBg, editTextBg, doneButtonBg);
    }
}