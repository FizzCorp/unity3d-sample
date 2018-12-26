//
//  FIZZUnityKeyboardListener.cs
//
//  Copyright (c) 2016 Fizz Inc
//
#if UNITY_ANDROID
using FIZZ.Bridge;

class FIZZUnityKeyboardListener : JavaInterfaceProxy {

    internal FIZZUnityKeyboardListener () : base ("com.fizz.uicomponent.keyboard.interfaces.UnityKeyboardListener") {

    }
    public void onCancel (string text) {
        FIZZMainThreadExecutor.Queue (() => {
            FIZZKeyboard.OnCancel (text);
        });
    }
    public void onDoneButtonPress (string text) {
        FIZZMainThreadExecutor.Queue (() => {
            FIZZKeyboard.OnDoneButtonPress (text);
        });
    }
    public void onKeyboardOpen (float height) {
        FIZZMainThreadExecutor.Queue (() => {
            FIZZKeyboard.GetKeyboardJavaObject ();
            FIZZKeyboard.OnKeyboardOpen (height);
        });
    }
    public void onKeyboardClose () {
        FIZZMainThreadExecutor.Queue (() => {
            FIZZKeyboard.ResetAndroidKeyboardObject ();
            FIZZKeyboard.OnKeyboardClose ();
        });
    }

    public void onEmojiClicked () {
        FIZZMainThreadExecutor.Queue (() => {
            FIZZKeyboard.ResetAndroidKeyboardObject ();
            FIZZKeyboard.OnEmojiClicked ();
        });
    }
}
#endif