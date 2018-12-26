//
//  UIAlertPopup.cs
//
//  Copyright (c) 2016 Fizz Inc
//
using System;
using FIZZ.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace FIZZ.UI.Components {
    /// <summary>
    /// User interface alert popup.
    /// </summary>
    public class UIAlertPopup : UITransitableComponent {
        /// <summary>
        /// The title.
        /// </summary>
        [SerializeField] Text title;
        /// <summary>
        /// The message.
        /// </summary>
        [SerializeField] Text message;
        /// <summary>
        /// The input field.
        /// </summary>
        [SerializeField] InputField inputField;
        /// <summary>
        /// The left button.
        /// </summary>
        [SerializeField] Button leftButton;
        /// <summary>
        /// The right button.
        /// </summary>
        [SerializeField] Button rightButton;
        /// <summary>
        /// The left button title.
        /// </summary>
        [SerializeField] Text leftButtonTitle;
        /// <summary>
        /// The right button title.
        /// </summary>
        [SerializeField] Text rightButtonTitle;
        /// <summary>
        /// The overlay image.
        /// </summary>
        [SerializeField] Image overlayImage;

        private Action _leftButtonCallback;
        private Action _rightButtonCallback;
        private Action<string, int> _inputCallback;
        private bool _isSingleButtonAlert = false;
        private bool _isInputAlert = false;
        private bool _isPasswordInput = false;

        protected override void Awake () {
            base.Awake ();
            FizzUI.Instance.CanvasScaler.ApplySafeArea (gameObject.GetComponent<RectTransform> ());
        }

        public override void OnEnable () {
            base.OnEnable ();
            inputField.onValidateInput += OnPasswordValidate;
        }

        public override void OnDisable () {
            base.OnDisable ();
            inputField.onValidateInput -= OnPasswordValidate;
        }

        public override void OnUIShow () {
            base.OnUIShow ();
        }

        public override void OnUIHide () {
            base.OnUIHide ();
        }

        public override void OnUIEnable () {
            base.OnUIEnable ();

            leftButton.onClick.AddListener (OnLeftButtonPressed);
            rightButton.onClick.AddListener (OnRightButtonPressed);
        }

        public override void OnUIDisable () {
            base.OnUIDisable ();

            leftButton.onClick.RemoveListener (OnLeftButtonPressed);
            rightButton.onClick.RemoveListener (OnRightButtonPressed);
        }

        /// <summary>
        /// Singles the button popup.
        /// </summary>
        /// <param name="title">Title.</param>
        /// <param name="message">Message.</param>
        /// <param name="buttonTitle">Button title.</param>
        /// <param name="callback">Callback.</param>
        public void SingleButtonPopup (string title, string message, string buttonTitle, Action callback) {
            this.title.text = title;
            this.message.text = message;
            this.leftButtonTitle.text = buttonTitle;
            this._leftButtonCallback = callback;
            _isSingleButtonAlert = true;
            _isInputAlert = false;

            this.leftButton.gameObject.SetActive (true);
            this.rightButton.gameObject.SetActive (false);
            this.inputField.gameObject.SetActive (false);
            this.message.gameObject.SetActive (true);

            Popup ();
            Invoke ("EnableOverlayImage", 0.25f);
        }

        /// <summary>
        /// Shows the network unavailablity popup.
        /// </summary>
        public void ShowNetworkUnavailablityPopup () {
            string _title = Registry.localization.GetText ("General_Error");
            string _message = Registry.localization.GetText ("General_InternetUnavailable");
            string _buttonTitle = Registry.localization.GetText ("General_Ok");
            SingleButtonPopup (_title, _message, _buttonTitle, null);
        }

        public void ShowPrivilegedUserPopup () {
            string _title = Registry.localization.GetText ("General_Error");
            string _message = Registry.localization.GetText ("General_PrivilegedActionError");
            string _okButton = Registry.localization.GetText ("General_Ok");
            SingleButtonPopup (_title, _message, _okButton, null);
        }

        /// <summary>
        /// Twos the button popup.
        /// </summary>
        /// <param name="title">Title.</param>
        /// <param name="message">Message.</param>
        /// <param name="leftbuttonTitle">Leftbutton title.</param>
        /// <param name="leftButtonCallback">Left button callback.</param>
        /// <param name="rightbuttonTitle">Rightbutton title.</param>
        /// <param name="rightButtonCallback">Right button callback.</param>
        public void TwoButtonPopup (string title, string message, string leftbuttonTitle, Action leftButtonCallback, string rightbuttonTitle, Action rightButtonCallback) {
            this.title.text = title;
            this.message.text = message;
            this.leftButtonTitle.text = leftbuttonTitle;
            this.rightButtonTitle.text = rightbuttonTitle;
            this._leftButtonCallback = leftButtonCallback;
            this._rightButtonCallback = rightButtonCallback;
            _isSingleButtonAlert = false;
            _isInputAlert = false;

            this.rightButton.interactable = true;
            this.leftButton.gameObject.SetActive (true);
            this.rightButton.gameObject.SetActive (true);
            this.inputField.gameObject.SetActive (false);
            this.message.gameObject.SetActive (true);

            Popup ();
            Invoke ("EnableOverlayImage", 0.25f);
        }

        /// <summary>
        /// Inputs the popup.
        /// </summary>
        /// <param name="title">Title.</param>
        /// <param name="placeholder">Placeholder.</param>
        /// <param name="isPassword">If set to <c>true</c> is password.</param>
        /// <param name="leftbuttonTitle">Leftbutton title.</param>
        /// <param name="rightbuttonTitle">Rightbutton title.</param>
        /// <param name="inputCallback">Input callback.</param>
        public void InputPopup (string title, string placeholder, bool isPassword, string leftbuttonTitle, string rightbuttonTitle, Action<string, int> inputCallback) {
            this.title.text = title;
            this.inputField.placeholder.GetComponent<Text> ().text = placeholder;
            this.leftButtonTitle.text = leftbuttonTitle;
            this.rightButtonTitle.text = rightbuttonTitle;
            this.inputField.contentType = (isPassword) ? InputField.ContentType.Password : InputField.ContentType.Alphanumeric;
            this._inputCallback = inputCallback;

            _isSingleButtonAlert = false;
            _isInputAlert = true;
            _isPasswordInput = isPassword;

            //			this.rightButton.interactable = (!_isPasswordInput);
            this.leftButton.gameObject.SetActive (true);
            this.rightButton.gameObject.SetActive (true);
            this.inputField.gameObject.SetActive (true);
            this.message.gameObject.SetActive (false);

            this.inputField.text = string.Empty;

            Popup ();
            Invoke ("EnableOverlayImage", 0.25f);
        }

        #region Private Methods

        private void OnLeftButtonPressed () {
            Hide ();
            if (!_isSingleButtonAlert) {
                if (!_isInputAlert) {
                    if (_leftButtonCallback != null) {
                        _leftButtonCallback ();
                        _leftButtonCallback = null;
                    }
                } else {
                    if (_inputCallback != null) {
                        _inputCallback (inputField.text, 0);
                        _inputCallback = null;
                    }
                }
            } else {
                if (_leftButtonCallback != null) {
                    _leftButtonCallback ();
                    _leftButtonCallback = null;
                }
            }
        }

        private void OnRightButtonPressed () {
            Hide ();
            if (!_isSingleButtonAlert) {
                if (!_isInputAlert) {
                    if (_rightButtonCallback != null) {
                        _rightButtonCallback ();
                        _rightButtonCallback = null;
                    }
                } else {
                    if (_inputCallback != null) {
                        _inputCallback (inputField.text, 1);
                        _inputCallback = null;
                    }
                }
            }
        }

        private char OnPasswordValidate (string inputText, int addedCharIndex, char addedChar) {
            if (_isPasswordInput) {
                if (addedChar == ' ') {
                    addedChar = '\0';
                }
            }

            return addedChar;
        }

        private void Hide () {
            overlayImage.color = new Color (0f, 0f, 0f, 0f);
            UITransitionSlideOut.Config config = new UITransitionSlideOut.Config (0.25f, UITransitionSlide.SlideDirection.bottom);
            Hide (config);
        }

        private void EnableOverlayImage () {
            overlayImage.color = new Color (0f, 0f, 0f, 0.5f);
        }

        #endregion
    }
}