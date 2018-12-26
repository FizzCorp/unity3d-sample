//
//  UIActionMenu.cs
//
//  Copyright (c) 2016 Fizz Inc
//
using System;
using System.Collections.Generic;
using FIZZ.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace FIZZ.UI.Components {
    /// <summary>
    /// User interface action menu.
    /// </summary>
    public class UIActionMenu : UITransitableComponent {
        /// <summary>
        /// The title.
        /// </summary>
        [SerializeField] TextWithEmoji title;
        /// <summary>
        /// The action button.
        /// </summary>
        [SerializeField] UIActionMenuItem actionButton;
        /// <summary>
        /// The action button container.
        /// </summary>
        [SerializeField] RectTransform actionButtonContainer;
        /// <summary>
        /// The overlay image.
        /// </summary>
        [SerializeField] Image overlayImage;

        private Action<UIActionMenuItemModel> _callback;
        private List<UIActionMenuItem> _actionsList;
        private List<UIActionMenuItemModel> _actionsData;

        protected override void Awake () {
            base.Awake ();
            FizzUI.Instance.CanvasScaler.ApplySafeArea (gameObject.GetComponent<RectTransform> ());
            _actionsList = new List<UIActionMenuItem> ();
        }

        public override void OnUIShow () {
            base.OnUIShow ();
            foreach (UIActionMenuItemModel action in _actionsData) {
                UIActionMenuItem newAction = Instantiate (actionButton);
                newAction.gameObject.SetActive (true);
                newAction.transform.SetParent (actionButtonContainer, false);
                newAction.transform.localScale = Vector3.one;

                newAction.SetData (action, OnActionPressed);
                _actionsList.Add (newAction);
            }
        }

        public override void OnUIHide () {
            base.OnUIHide ();
            _actionsData.Clear ();
            foreach (UIActionMenuItem item in _actionsList) {
                Destroy (item.gameObject);
            }
            _actionsList.Clear ();
        }

        public override void OnUIEnable () {
            base.OnUIEnable ();
        }

        public override void OnUIDisable () {
            base.OnUIDisable ();
        }

        public void Show (string titleTxt, List<UIActionMenuItemModel> actions, Action<UIActionMenuItemModel> callback) {
            if (string.IsNullOrEmpty (titleTxt)) {
                this.title.transform.parent.gameObject.SetActive (false);
            } else {
                this.title.transform.parent.gameObject.SetActive (true);
                this.title.text = titleTxt;
            }

            _callback = callback;
            _actionsData = actions;
            overlayImage.color = Color.clear;

            Popup ();
            Invoke ("EnableOverlayImage", 0.25f);
        }

        public void OnCancelButtonPressed () {
            CloseUI ();
        }

        private void OnActionPressed (UIActionMenuItemModel model) {
            CloseUI ();
            if (_callback != null) {
                _callback.Invoke (model);
                _callback = null;
            }
        }

        private void EnableOverlayImage () {
            overlayImage.color = new Color (0f, 0f, 0f, 0.5f);
        }

        private void CloseUI () {
            overlayImage.color = new Color (0f, 0f, 0f, 0f);
            UITransitionSlideOut.Config config = new UITransitionSlideOut.Config (0.1f, UITransitionSlide.SlideDirection.bottom);
            Hide (config);
        }
    }

    public class UIActionMenuItemModel {
        public string text;
        public string id;
    }
}