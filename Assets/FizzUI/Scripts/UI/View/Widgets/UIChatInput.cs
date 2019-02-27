//
//  UIChatInput.cs
//
//  Copyright (c) 2016 Fizz Inc
//
using Fizz;
using Fizz.UI.Bridge;
using Fizz.UI.Components.Extentions;
using Fizz.UI.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Fizz.UI.Components {
    /// <summary>
    /// User interface chat input.
    /// </summary>
    public class UIChatInput : UIComponent {

        /// <summary>
        /// The background image.
        /// </summary>
        [SerializeField] Image backgroundImage;
        /// <summary>
        /// The input field for Editor.
        /// </summary>
        [SerializeField] InputFieldWithEmoji inputEditor;
        ///<summary>
        /// The input field for Mobile
        /// </summary>
        [SerializeField] TextWithEmoji inputMobile;
        /// <summary>
        /// The send button.
        /// </summary>
        [SerializeField] Button sendButton;
        /// <summary>
        /// The error node.
        /// </summary>
        [SerializeField] RectTransform errorNode;
        /// <summary>
        /// The error label.
        /// </summary>
        [SerializeField] Text errorLabel;
        /// <summary>
        /// The node to move.
        /// </summary>
        [SerializeField] CustomScrollRect nodeToMove;
        /// <summary>
        /// The node to move delta.
        /// </summary>
        [SerializeField] float nodeToMoveDelta;
        /// <summary>
        /// The on send.
        /// </summary>
        public SendEvent onSend;

        bool _moveUI = false;
        bool _handleTouches = false;
        string _placeholderText = string.Empty;
        private string _message = string.Empty;
        FIZZTouchScreenKeyboard _keyboard;
        CanvasGroup _canvasGroup;

        protected override void Awake () {
            applyConfig = false;
            base.Awake ();

            _canvasGroup = GetComponent<CanvasGroup> ();
            Initialize ();
        }

        void OnEnable () {
            bool isFizzConnected = FizzService.Instance.IsConnected;
            sendButton.interactable = isFizzConnected;

            sendButton.onClick.AddListener (OnSend);

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            inputEditor.interactable = isFizzConnected;
            inputEditor.onDone.AddListener (OnSend);
            inputEditor.onSelect.AddListener (OnInputFieldSelect);
            inputEditor.onDeselect.AddListener (OnInputFieldDeselect);
#else
            inputMobile.GetComponent<Button> ().interactable = isFizzConnected;
            inputMobile.GetComponent<Button> ().onClick.AddListener (ActivateKeyboard);
#endif
            try
            {
                if (FizzService.Instance.Client.State == FizzClientState.Opened)
                {
                    FizzService.Instance.Client.Chat.Listener.OnConnected += OnFizzConnected;
                    FizzService.Instance.Client.Chat.Listener.OnDisconnected += OnFizzDisconnected;
                }
            }
			catch (FizzException ex)
			{
                Common.FizzLogger.E ("UIChatInput ex " + ex.Message);
			}
        }

        void OnDisable () {
            sendButton.onClick.RemoveListener (OnSend);

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            inputEditor.onDone.RemoveListener (OnSend);
            inputEditor.onSelect.RemoveListener (OnInputFieldSelect);
            inputEditor.onDeselect.RemoveListener (OnInputFieldDeselect);
#else
            inputMobile.GetComponent<Button> ().onClick.RemoveListener (ActivateKeyboard);
#endif

            try
            {
                if (FizzService.Instance.Client.State == FizzClientState.Opened)
                {
                    FizzService.Instance.Client.Chat.Listener.OnConnected -= OnFizzConnected;
                    FizzService.Instance.Client.Chat.Listener.OnDisconnected -= OnFizzDisconnected;
                }
            }
			catch (FizzException ex)
			{
                Common.FizzLogger.E ("UIChatInput ex " + ex.Message);
			}

             #if UNITY_IPHONE
                DeactivateKeyboard ();
            #elif UNITY_ANDROID
                FIZZKeyboard.HideKeyboard();
            #endif
        }

        void OnApplicationPause (bool pauseState) {
            if (pauseState) {
#if UNITY_IPHONE
                DeactivateKeyboard ();
#elif UNITY_ANDROID
                //NOTHING to do anything. Android widget handle itself
                FIZZKeyboard.HideKeyboard();
#endif
            }
        }

        #region Public Methods

        public void ResetText () {
            _message = string.Empty;
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            inputEditor.ResetText ();
#else
            inputMobile.text = string.Empty;
            inputMobile.text = _placeholderText;
            ChangeInputFieldColorAlpha (false);
#endif
        }

        public void ShowError (string error) {
            _canvasGroup.interactable = false;
            errorNode.gameObject.SetActive (true);
            errorLabel.text = error;
        }

        public void HideError () {
            _canvasGroup.interactable = true;
            errorNode.gameObject.SetActive (false);
            errorLabel.text = string.Empty;
        }

        #endregion

        #region Methods

        void Initialize () {
            _placeholderText = Registry.localization.GetText ("Message_PlaceHolderTypeMsg");

            UpdatePlaceholderText ();

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            inputMobile.gameObject.SetActive (false);
            inputEditor.gameObject.SetActive (true);
#else
            inputMobile.gameObject.SetActive (true);
            inputEditor.gameObject.SetActive (false);
            ChangeInputFieldColorAlpha (false);
#endif
        }

        void OnSend () {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            onSend.Invoke (inputEditor.text);
            _message = string.Empty;
#else
            string messageText = string.Empty;
            if (!string.IsNullOrEmpty (_message) && !_message.Equals (_placeholderText)) {
                onSend.Invoke (_message);
                inputMobile.text = _placeholderText;
                _message = string.Empty;
                ChangeInputFieldColorAlpha (false);
            }
#endif
        }

        void OnInputFieldSelect () {
        }

        void OnInputFieldDeselect () {

        }

        void UpdatePlaceholderText () {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            if (inputEditor.placeholder != null) {
                Text placeHolderText = inputEditor.placeholder.GetComponent<Text> ();
                if (placeHolderText != null) {
                    placeHolderText.text = Registry.localization.GetText ("Message_PlaceHolderTypeMsg");
                }
            }
#else
            inputMobile.text = _placeholderText;
#endif
        }

        void ActivateKeyboard () {
#if UNITY_IPHONE 
            _handleTouches = true;
            FIZZTouchScreenKeyboard.SetCustomMessage (true, OnMessageCallback, OnStickerCallback);
            if (_keyboard == null) {
                string _initialText = (_message.Equals (_placeholderText)) ? string.Empty : _message;
                _keyboard = FIZZTouchScreenKeyboard.Open (_initialText, FIZZTouchScreenKeyboardType.Default, true, true, false, false, _placeholderText);
            } else {
                _keyboard.active = true;
            }

            DeviceOrientation deviceOrientation = FizzUI.Instance.CanvasScaler.Orientation;
            if ((deviceOrientation == DeviceOrientation.PortraitUpsideDown || deviceOrientation == DeviceOrientation.Portrait) &&
                nodeToMove != null) {
                _moveUI = true;
            }
#elif UNITY_ANDROID
            _handleTouches = true;

            RemoveAllAndroidKBListener ();
            FIZZKeyboard.OnDoneButtonPress = OnDoneButtonPress;
            FIZZKeyboard.OnCancel = OnCancel;
            FIZZKeyboard.OnKeyboardClose = OnKeyboardClose;
            FIZZKeyboard.OnKeyboardOpen = OnKeyboardOpen;
            FIZZKeyboard.OnEmojiClicked = OnEmojiClicked;
				
            string _initialText = (_message.Equals (_placeholderText)) ? string.Empty : _message;
            FIZZKeyboard.ShowKeyboard (_initialText, _placeholderText, false);
#endif
        }

        public void OnCancel (string text) {
            if ((text.Length > 0)) {
                _message = text;
                inputMobile.text = text;
                ChangeInputFieldColorAlpha (true);
            } else {
                _message = string.Empty;
                inputMobile.text = _placeholderText;
                ChangeInputFieldColorAlpha (false);
            }
        }

        public void OnDoneButtonPress (string text) {
            onSend.Invoke (text);
        }

        public void OnKeyboardOpen (float height) {
            FIZZKeyboard.GetKeyboardJavaObject ();
            MoveScrollNode (Vector2.up, height, FizzUI.Instance.CanvasScaler.ReferenceResolutionRatio);
        }
        public void OnKeyboardClose () {
            FIZZKeyboard.ResetAndroidKeyboardObject ();

            RemoveAllAndroidKBListener ();

            _handleTouches = false;
            MoveScrollNode (Vector2.down, rect.sizeDelta.y, 1);
        }

        public void OnEmojiClicked(){
            FIZZKeyboard.OnEmojiClicked = delegate { };
        }

        private void RemoveAllAndroidKBListener () {
            FIZZKeyboard.OnCancel = delegate { };
            FIZZKeyboard.OnDoneButtonPress = delegate { };
            FIZZKeyboard.OnKeyboardClose = delegate { };
            FIZZKeyboard.OnKeyboardOpen = delegate { };
        }

        void DeactivateKeyboard () {
#if UNITY_IPHONE
            _handleTouches = false;
            if (_keyboard != null) {
                string _keyboardText = _keyboard.text;
                if (_keyboardText.Length > 0) {
                    _message = _keyboardText;
                    inputMobile.text = _keyboardText;
                    ChangeInputFieldColorAlpha (true);
                } else {
                    _message = string.Empty;
                    inputMobile.text = _placeholderText;
                    ChangeInputFieldColorAlpha (false);
                }
                _keyboard.active = false;
                _keyboard.text = string.Empty;
                _keyboard = null;
            }

            FIZZTouchScreenKeyboard.SetCustomMessage (false, null, null);
            MoveScrollNode (Vector2.down, rect.sizeDelta.y, 1);
#endif
        }

        void OnMessageCallback (string message) {
            onSend.Invoke (message);
        }

        void OnStickerCallback () {
            DeactivateKeyboard ();
        }

        void ChangeInputFieldColorAlpha (bool active) {
            Color inputColor = inputMobile.color;
            inputMobile.color = new Color (inputColor.r, inputColor.g, inputColor.b, active ? 1.0f : 0.5f);
        }

        void MoveScrollNode (Vector2 direction, float height, float ratio) {
            DeviceOrientation deviceOrientation = FizzUI.Instance.CanvasScaler.Orientation;
            if ((deviceOrientation == DeviceOrientation.PortraitUpsideDown || deviceOrientation == DeviceOrientation.Portrait) && height > 0) {
                if (direction.Equals (Vector2.up)) {
                    Vector2 offsetMin = Vector2.up * ((height * ratio) - nodeToMoveDelta);
                    nodeToMove.GetComponent<RectTransform> ().offsetMin = offsetMin;
                } else if (direction.Equals (Vector2.down)) {
                    nodeToMove.GetComponent<RectTransform> ().offsetMin = Vector2.up * height;
                }

                nodeToMove.GoToLastScrollItem ();
            }
        }

        void MoveInputNode (float height) {
            rect.anchoredPosition = Vector2.up * (height - nodeToMoveDelta);
        }

        #endregion

        [System.Serializable]
        public class SendEvent : UnityEvent<string> {

        }

        void LateUpdate () {
#if UNITY_IPHONE || UNITY_ANDROID
            if (_moveUI && _keyboard.active && FIZZTouchScreenKeyboard.area.height > 0) {
                _moveUI = false;
                
                MoveScrollNode (Vector2.up, FIZZTouchScreenKeyboard.area.height, FizzUI.Instance.CanvasScaler.ReferenceResolutionRatio);
            }
#endif
        }

        void Update () {
            if (_handleTouches && Input.touchCount > 0) {
                Touch touch = Input.GetTouch (0);
                switch (touch.phase) {
                    case TouchPhase.Began:
                        DeactivateKeyboard ();
                        break;
                    default:
                        break;
                }
            }
        }

        void OnFizzConnected (bool syncReq) {
            sendButton.interactable = true;
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            inputEditor.interactable = true;
#else
            inputMobile.GetComponent<Button> ().interactable = true;
#endif
        }

        void OnFizzDisconnected (FizzException ex) {
            sendButton.interactable = false;
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            inputEditor.interactable = false;
#else
            inputMobile.GetComponent<Button> ().interactable = false;
#endif
        }

        void OnStickerBoardClose () {
            MoveInputNode (nodeToMoveDelta);
            MoveScrollNode (Vector2.down, rect.sizeDelta.y, 1);
        }
    }
}