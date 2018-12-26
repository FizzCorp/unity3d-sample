//
//  UIMainView.cs
//
//  Copyright (c) 2016 Fizz Inc
//
using FIZZ.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace FIZZ.UI.Components {
    /// <summary>
    /// MainView which contains topbar with title, back and settings button and navigation tabbar.
    /// </summary>
    public class UIMainView : UITransitableComponent {
        /// <summary>
        /// The main view background.
        /// </summary>
        [SerializeField] Image backgroundImage;
        /// <summary>
        /// Title label.
        /// </summary>
        [SerializeField] Text titleLabel;
        /// <summary>
        /// The close button.
        /// </summary>
        [SerializeField] Button closeButton;
        /// <summary>
        /// The top bar background.
        /// </summary>
        [SerializeField] Image topBarBackgroundImage;
        /// <summary>
        /// The main tab bar.
        /// </summary>
        [SerializeField] UITabBar tabBar;

        const string KEY_GAMECHAT_VIEW = "gamechat";

        bool isBarLoaded = false;

        #region MonoBehaviour Methods

        protected override void Awake () {
            applyConfig = false;
            base.Awake ();
            Initialize ();
        }

        public override void OnEnable () {
            base.OnEnable ();
            
            closeButton.onClick.AddListener (CloseButtonHandler);
        }

        public override void OnDisable () {
            base.OnDisable ();
            
            closeButton.onClick.RemoveListener (CloseButtonHandler);
        }

        #endregion

        #region UITransitableComponent Methods

        public override void OnUIShow () {
            base.OnUIShow ();

            if (isBarLoaded && tabBar != null) {
                tabBar.Refresh ();
            } else {
                LoadTabBar ();
            }
        }

        public override void OnUIHide() {
            base.OnUIHide ();
            if (isBarLoaded && tabBar != null) {
                tabBar.Hide ();
            }
        }

        #endregion

        #region Private Methods

        void Initialize () {
            FizzUI.Instance.CanvasScaler.ApplySafeArea (gameObject.GetComponent<RectTransform> ());
        }

        void LoadTabBar () {
            if (isBarLoaded)
                return;

            if (FizzUI.Instance.GameChatView != null) {
                tabBar.AddTab (KEY_GAMECHAT_VIEW, "gamechatButton", Registry.localization.GetText ("TabItem_GameChat"), FizzUI.Instance.GameChatView);
            }

            tabBar.onTabChange.AddListener (TabbarTabChangeHandler);
            tabBar.Show ();

            isBarLoaded = true;
        }

        void CloseButtonHandler () {
            isBarLoaded = false;
            if (tabBar != null)
                tabBar.Reset ();
            FizzUI.Instance.Close ();
        }

        void TabbarTabChangeHandler (string tabId) {
            UpdateTitle (tabId);
        }

        void UpdateTitle (string id) {
            string titleText = string.Empty;
            switch (id) {
                case KEY_GAMECHAT_VIEW:
                    titleText = "Sample";
                    break;
                default:
                    break;
            }

            titleLabel.text = titleText;
        }

        #endregion
    }
}