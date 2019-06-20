using System;
#if UNITY_IOS
using System.Runtime.InteropServices;
#endif
using UnityEngine;

namespace Fizz.UI.Bridge {
    //
    // Summary:
    //     ///
    //     Interface into the native iPhone, Android
    //     on-screen keyboards - it is not available on other platforms.
    //     ///
    public class FIZZTouchScreenKeyboard {
        public FIZZTouchScreenKeyboard (string text, FIZZTouchScreenKeyboardType keyboardType, bool autocorrection, bool multiline, bool secure, bool alert, string textPlaceholder) {
#if UNITY_IOS
            FIZZUnityKeyboard_Create ((int) keyboardType, autocorrection ? 1 : 0, multiline ? 1 : 0, secure ? 1 : 0, alert ? 1 : 0, text, textPlaceholder);
#endif
            }

            ~FIZZTouchScreenKeyboard () {

            }

        public static void SetCustomMessage (bool custom, Action<string> onMessage, Action onSticker) {
            if (custom) {
#if UNITY_IOS
                _messageCallback = onMessage;
                _stickerCallback = onSticker;
                FIZZUnityKeyboard_CustomMessageInput (custom, MessageCallback, StickerCallback);
#endif
            } else {
#if UNITY_IOS
                _messageCallback = null;
#endif
            }
        }

        public static Rect area {
            get {
#if UNITY_IOS
                float x;
                float y;
                float w;
                float h;
                FIZZUnityKeyboard_GetRect (out x, out y, out w, out h);
                return new Rect (x, y, w, h);
#else
                return new Rect(0, 0, 0, 0);
#endif

            }
        }

        public static bool hideInput {
            get {
#if UNITY_IOS
                return (FIZZUnityKeyboard_IsInputHidden () == 1);
#else
                return false;
#endif
            }

            set {
#if UNITY_IOS
                FIZZUnityKeyboard_SetInputHidden (value ? 1 : 0);
#endif
            }
        }

        public static bool isSupported {
            get {
                RuntimePlatform platform = Application.platform;
                bool result;
                switch (platform) {
                    case RuntimePlatform.IPhonePlayer:
                        result = true;
                        break;
                    default:
                        result = false;
                        break;
                }
                return result;
            }
        }

        public static bool visible {
            get {
#if UNITY_IOS
                return (FIZZUnityKeyboard_IsActive () == 1) && (area.height > 0);
#else
                return false;
#endif
            }
        }
        public int targetDisplay { get; set; }

        public bool wasCanceled {
            get {
#if UNITY_IOS
                return FIZZUnityKeyboard_WasCanceled () == 1;
#else
                return false;
#endif
            }
        }

        public string text {
            get {
#if UNITY_IOS
                return Marshal.PtrToStringAuto (FIZZUnityKeyboard_GetText ());
#else
                return string.Empty;
#endif
            }
            set {
#if UNITY_IOS
                FIZZUnityKeyboard_SetText (value);
#endif
            }
        }

        public bool done {
            get {
#if UNITY_IOS
                return FIZZUnityKeyboard_IsDone () == 1;
#else
                return false;
#endif
            }
        }
        public bool active {
            get {
#if UNITY_IOS
                return (FIZZUnityKeyboard_IsActive () == 1);
#else
                return false;
#endif
            }

            set {
                if (!value)
                {
#if UNITY_IOS
                    FIZZUnityKeyboard_Hide();
#endif
                }
            }
        }

        public static FIZZTouchScreenKeyboard Open (string text, FIZZTouchScreenKeyboardType keyboardType) {
            return FIZZTouchScreenKeyboard.Open (text, keyboardType);
        }

        public static FIZZTouchScreenKeyboard Open (string text, FIZZTouchScreenKeyboardType keyboardType, bool autocorrection, bool multiline, bool secure, bool alert) {
            return FIZZTouchScreenKeyboard.Open (text, keyboardType, autocorrection, multiline, secure, alert);
        }

        public static FIZZTouchScreenKeyboard Open (string text, FIZZTouchScreenKeyboardType keyboardType, bool autocorrection, bool multiline, bool secure) {
            return FIZZTouchScreenKeyboard.Open (text, keyboardType, autocorrection, multiline, secure);
        }

        public static FIZZTouchScreenKeyboard Open (string text, FIZZTouchScreenKeyboardType keyboardType, bool autocorrection, bool multiline) {
            return FIZZTouchScreenKeyboard.Open (text, keyboardType, autocorrection, multiline);
        }

        public static FIZZTouchScreenKeyboard Open (string text, FIZZTouchScreenKeyboardType keyboardType, bool autocorrection) {
            return FIZZTouchScreenKeyboard.Open (text, keyboardType, autocorrection);
        }

        public static FIZZTouchScreenKeyboard Open (string text) {
            return FIZZTouchScreenKeyboard.Open (text);
        }

        public static FIZZTouchScreenKeyboard Open (string text, FIZZTouchScreenKeyboardType keyboardType = FIZZTouchScreenKeyboardType.Default, bool autocorrection = true, bool multiline = false, bool secure = false, bool alert = false, string textPlaceholder = "") {
            FIZZTouchScreenKeyboard keyboard = new FIZZTouchScreenKeyboard (text, keyboardType, autocorrection, multiline, secure, alert, textPlaceholder);
#if UNITY_IOS
            FIZZUnityKeyboard_Show ();
#endif
            return keyboard;
        }

#if UNITY_IOS

        [DllImport ("__Internal")]
        private static extern void FIZZUnityKeyboard_Create (int keyboardType, int autocorrection, int multiline, int secure, int alert, [MarshalAs (UnmanagedType.LPStr)] string text, [MarshalAs (UnmanagedType.LPStr)] string placeholder);

        [DllImport ("__Internal")]
        private static extern void FIZZUnityKeyboard_Show ();

        [DllImport ("__Internal")]
        private static extern void FIZZUnityKeyboard_Hide ();

        [DllImport ("__Internal")]
        private static extern void FIZZUnityKeyboard_SetText ([MarshalAs (UnmanagedType.LPStr)] string text);

        [DllImport ("__Internal")]
        private static extern IntPtr FIZZUnityKeyboard_GetText ();

        [DllImport ("__Internal")]
        private static extern int FIZZUnityKeyboard_IsActive ();

        [DllImport ("__Internal")]
        private static extern int FIZZUnityKeyboard_IsDone ();

        [DllImport ("__Internal")]
        private static extern int FIZZUnityKeyboard_WasCanceled ();

        [DllImport ("__Internal")]
        private static extern void FIZZUnityKeyboard_SetInputHidden (int hidden);

        [DllImport ("__Internal")]
        private static extern int FIZZUnityKeyboard_IsInputHidden ();

        [DllImport ("__Internal")]
        private static extern void FIZZUnityKeyboard_GetRect (out float x, out float y, out float w, out float h);

        private delegate void MessageDelegate (string message);
        private delegate void StickerDelegate ();

        [DllImport ("__Internal")]
        private static extern void FIZZUnityKeyboard_CustomMessageInput (bool enable, MessageDelegate messageDelegate, StickerDelegate stickerDelegate);

        private static System.Action<string> _messageCallback;
        [AOT.MonoPInvokeCallback (typeof (MessageDelegate))]
        private static void MessageCallback (string message) {
            if (_messageCallback != null)
                _messageCallback.Invoke (message);
        }

        private static System.Action _stickerCallback;
        [AOT.MonoPInvokeCallback (typeof (StickerDelegate))]
        private static void StickerCallback () {
            if (_stickerCallback != null)
                _stickerCallback.Invoke ();
        }

#endif
    }

    public enum FIZZTouchScreenKeyboardType {
        Default = 0,
        ASCIICapable = 1,
        NumbersAndPunctuation = 2,
        URL = 3,
        NumberPad = 4,
        PhonePad = 5,
        NamePhonePad = 6,
        EmailAddress = 7,
        NintendoNetworkAccount = 8
    }
}