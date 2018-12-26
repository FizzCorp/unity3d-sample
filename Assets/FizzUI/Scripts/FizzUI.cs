//
//  FIZZUI.cs
//
//  Copyright (c) 2016 Fizz Inc
//

using Fizz.UI.Components;
using Fizz.UI.Core;
using UnityEngine;
using UnityEngine.Events;

namespace Fizz.UI {
    public class FizzUI : MonoBehaviour {

        [SerializeField] bool dontDestroyOnLoad = true;
        /// <summary>
        /// Fizz UI close event
        /// </summary>
        /// <returns></returns>
        public UnityEvent onFizzClose;

        public static FizzUI Instance { get; private set; } = null;

        public UIMainView MainView { get; private set; }

        public UIGameChatView GameChatView { get; private set; }

        public UIAlertPopup AlertView { get; private set; }

        public UIActionMenu ActionMenu { get; private set; }

        public UIToastsView ToastView { get; private set; }

        public UICanvasScaler CanvasScaler { get; private set; }

        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else if (Instance != this)
                Destroy(gameObject);

            if (dontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);

            Initialize();
        }

        #region Public Methods

        public void LaunchFizz()
        {
            if (MainView != null)
            {
                var config = new UITransitionSlideIn.Config(0.25f, UITransitionSlide.SlideDirection.bottom);
                MainView.Show(RouterHistoryMode.pushState, config);
                isUIActive = true;
            }
            else
            {
                Debug.Log("STATUS_ERROR: mainView == null In Ack");
            }
        }

        public void Close () {
            Registry.router.HideAll ();
            isUIActive = false;

            if (onFizzClose != null) {
                onFizzClose.Invoke ();
            }
        }

        #endregion

        #region Methods

        void Initialize () {
            if (!isInitialized) {
                foreach (Transform child in transform) {
                    if (child.CompareTag (FIZZ_MainView)) {
                        MainView = child.GetComponent<UIMainView> ();
                        foreach (Transform mainViewChild in child) {
                            if (mainViewChild.CompareTag (FIZZ_MainViewContainer)) {
                                foreach (Transform mainViewContainerChild in mainViewChild) {
                                    if (mainViewContainerChild.CompareTag (FIZZ_GameChatView)) {
                                        GameChatView = mainViewContainerChild.GetComponent<UIGameChatView> ();
                                    }
                                }
                            }
                        }
                    } else if (child.CompareTag (FIZZ_AlertPopup)) {
                        AlertView = child.GetComponent<UIAlertPopup> ();
                    } else if (child.CompareTag (FIZZ_ActionMenu)) {
                        ActionMenu = child.GetComponent<UIActionMenu> ();
                    } else if (child.CompareTag (FIZZ_ToastView)) {
                        ToastView = child.GetComponent<UIToastsView> ();
                    }
                }
                isInitialized = true;

                CanvasScaler = gameObject.GetComponent<UICanvasScaler> ();
            }
        }

        #endregion

        bool isUIActive = false;
        bool isInitialized = false;

        const string FIZZ_MainView = "FIZZ_MainView";
        const string FIZZ_MainViewContainer = "FIZZ_MainViewContainer";
        const string FIZZ_GameChatView = "FIZZ_GameChatView";
        const string FIZZ_ActionMenu = "FIZZ_ActionMenu";
        const string FIZZ_AlertPopup = "FIZZ_AlertPopup";
        const string FIZZ_ToastView = "FIZZ_ToastView";
    }
}