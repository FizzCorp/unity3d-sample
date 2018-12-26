//
//  UIOtherChatCellView.cs
//
//  Copyright (c) 2016 Fizz Inc
//
using System;
using Fizz.UI.Components.Models;
using UnityEngine;
using UnityEngine.UI;

namespace Fizz.UI.Components {
    /// <summary>
    /// User interface other chat cell view.
    /// </summary>
    public class UILeftChatCellView : UIChatCellView {
        /// <summary>
        /// Sender nick name label.
        /// </summary>
        [SerializeField] TextWithEmoji nickLabel;
        /// <summary>
        /// The translate toggle node.
        /// </summary>
        [SerializeField] RectTransform translateToggleNode;
        /// <summary>
        /// The translate toggle.
        /// </summary>
        [SerializeField] Button translateToggle;

        public Action<int> onTranslateTogglePressed;

        protected override void Awake () {
            base.Awake ();
        }

        void OnEnable () {
            // translateToggle.onClick.AddListener (ToggleTranslateClicked);
        }

        void OnDisable () {
            // translateToggle.onClick.RemoveListener (ToggleTranslateClicked);
        }

        #region Public Methods

        /// <summary>
        /// Set the data to populate ChatCellView.
        /// </summary>
        /// <param name="model">FIZZChatMessageAction. Please see <see cref="FIZZ.Actions.IFIZZChatMessageAction"/></param>
        public override void SetData (FizzUIMessage model, bool appTranslationEnabled) {
            base.SetData (model, appTranslationEnabled);

            nickLabel.text = _model.Nick;
            nickLabel.color = Utils.GetUserNickColor (_model.From);

            LoadChatMessageAction ();
        }

        #endregion

        #region Methods

        void LoadChatMessageAction () {
            messageLabel.gameObject.SetActive (true);

            nickLabel.text = _model.Nick;

            if (false) { //Utils.ShouldFixRTLMessage (_chatMessageAction)) {
                messageLabel.text = ArabicSupport.ArabicFixer.Fix (_model.GetActiveMessage ());
            } else {
                messageLabel.text = _model.GetActiveMessage ();
            }

            bool showTranslationToggle = _appTranslationEnabled;
            //bool showTranslationButton = _appTranslationEnabled && !_autoTranslationEnabled && FIZZSDKWrapper.IsTranslatableChatMessageAction (_chatMessageAction);

            //if (_chatMessageAction.TranslationState == FIZZChatMessageTranslationState.ChatMessageTranslationStateTranslated) {
            //    showTranslationToggle = true;
            //    showTranslationButton = false;
            //} else if (_chatMessageAction.TranslationState == FIZZChatMessageTranslationState.ChatMessageTranslationStateTranslationError) {
            //    showTranslationToggle = showTranslationButton = false;
            //} else if (_chatMessageAction.TranslationState == FIZZChatMessageTranslationState.ChatMessageTranslationStateNotTranslated) {
            //    showTranslationToggle = false;
            //    translateButtonText.text = Registry.localization.GetText ("Translation_Translate");
            //} else if (_chatMessageAction.TranslationState == FIZZChatMessageTranslationState.ChatMessageTranslationStateTranslating) {
            //    showTranslationToggle = false;
            //    showTranslationButton = true;
            //    translateButtonText.text = Registry.localization.GetText ("Translation_Translating");
            //} else {
            //    showTranslationToggle = showTranslationButton = false;
            //}

            translateToggleNode.gameObject.SetActive (showTranslationToggle);
        }

        public void ToggleTranslateClicked () {
            _model.ToggleTranslationState();
            messageLabel.text = _model.GetActiveMessage();
            onTranslateTogglePressed.Invoke(rowNumber);
        }

        #endregion
    }
}