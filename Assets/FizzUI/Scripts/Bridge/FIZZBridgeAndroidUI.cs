#if UNITY_ANDROID
using System;
// using FIZZ.Bridge.Android;
using UnityEngine;

namespace FIZZ.Bridge.UI.Android {

    internal class FIZZBridgeUIImpl {

        private static FIZZBridgeUIImpl s_instance = new FIZZBridgeUIImpl ();
        private AndroidJavaObject m_UnityJaveClassContext;

        private
        const string UnityKeyboard = "com.fizz.uicomponent.keyboard.dialogs.UnityEditBoxDialog";

        private AndroidJavaObject m_KeyboardClass;
        private AndroidJavaObject m_fizzKBJavaObject;

        private FIZZUnityKeyboardListener m_fizzUnityKeyboardListener;

        public static FIZZBridgeUIImpl Instance {
            get {
                return s_instance;
            }
        }

        private FIZZBridgeUIImpl () {
            InitNativeInstance ();
            m_fizzUnityKeyboardListener = new FIZZUnityKeyboardListener ();
        }

        private void InitNativeInstance () {
            AndroidJavaClass JavaClass = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
            m_UnityJaveClassContext = JavaClass.GetStatic<AndroidJavaObject> ("currentActivity");

            FIZZMainThreadExecutor.Init ();
        }

        private bool IsKeyboardClosed () {
            bool isKeyboardClosed = false;
            if (m_fizzKBJavaObject != null) {
                isKeyboardClosed = m_fizzKBJavaObject.Call<bool> ("isKeyboardClosed");
            }
            return isKeyboardClosed;
        }

        public void ResetAndroidKeyboardObject () {
            m_KeyboardClass = null;
        }

        public void GetKeyboardJavaObject () {
            if (m_fizzKBJavaObject == null) {
                m_fizzKBJavaObject = m_KeyboardClass.CallStatic<AndroidJavaObject> ("getKeyboardInstance");

                if (m_fizzKBJavaObject == null) {
                    throw new Exception ("Failed to instantiate Android keyboad Object");
                }
            }
        }

        public void ShowKeyboard (string text, string hintText) {
            m_KeyboardClass = new AndroidJavaClass (UnityKeyboard);

            if (m_KeyboardClass == null) {
                throw new Exception ("Failed to instantiate Android Keyboard UI");
            }
            m_KeyboardClass.CallStatic ("showKeyboard", m_UnityJaveClassContext, text, hintText, m_fizzUnityKeyboardListener);

        }

        public void ShowKeyboard (string text, string hintText, bool isEmojo) {
            m_KeyboardClass = new AndroidJavaClass (UnityKeyboard);

            if (m_KeyboardClass == null) {
                throw new Exception ("Failed to instantiate Android Keyboard UI");
            }
            m_KeyboardClass.CallStatic ("showKeyboard", m_UnityJaveClassContext, text, hintText, isEmojo, m_fizzUnityKeyboardListener);

        }

        public String GetText () {
            string text = string.Empty;
            if (!IsKeyboardClosed () && m_fizzKBJavaObject != null) {
                text = m_fizzKBJavaObject.Call<string> ("getText");
            }
            return text;
        }

        public float GetKeyboardHeight () {
            float height = 0.0f;
            if (!IsKeyboardClosed () && m_fizzKBJavaObject != null) {
                height = m_fizzKBJavaObject.Call<float> ("getHeight");
            }
            return height;
        }

        public bool IsKeyboardOpen () {
            bool isKeyboardOpen = false;
            if (m_fizzKBJavaObject != null) {
                isKeyboardOpen = m_fizzKBJavaObject.Call<bool> ("isKeyboardOpen");
            }
            return isKeyboardOpen;
        }

        public void HideKeyboard () {
            if (m_fizzKBJavaObject != null) {
                m_fizzKBJavaObject.Call ("hideKeyboard");
            }
        }

        public void SetKeyboardMessageLayoutGraphics (String mainLayoutBg, String editTextBg, String doneButtonBg) {
            if (m_fizzKBJavaObject != null) {
                m_fizzKBJavaObject.Call ("setMessageLayoutGraphics", mainLayoutBg, editTextBg, doneButtonBg);
            }
        }
    }
}
#endif