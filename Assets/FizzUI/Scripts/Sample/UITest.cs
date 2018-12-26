//
//  UITest.cs
//
//  Copyright (c) 2016 Fizz Inc
//

using FIZZ.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace FIZZ.UI.Demo {
    public class UITest : MonoBehaviour {
        [SerializeField] Button fizzButton;
        [SerializeField] Button connectButton;
        [SerializeField] Button disconnectButton;
        [SerializeField] Animator connectingAnim;
        [SerializeField] Text internalUserIdLabel;
        [SerializeField] bool isDebug = false;

        private string KEY_INTERNAL_USER_ID = "internalUserId";

        void Awake () {
            connectButton.gameObject.SetActive (isDebug);
            disconnectButton.gameObject.SetActive (isDebug);
            if (isDebug) {
                connectButton.interactable = false;
                disconnectButton.interactable = false;
            }
        }

        void OnEnable () {
            fizzButton.onClick.AddListener (OnFIZZButtonPressed);
            connectButton.onClick.AddListener (OnConnectButtonPressed);
            disconnectButton.onClick.AddListener (OnDisconnectButtonPressed);
        }

        void OnDisable () {
            fizzButton.onClick.RemoveListener (OnFIZZButtonPressed);
            connectButton.onClick.RemoveListener (OnConnectButtonPressed);
            disconnectButton.onClick.RemoveListener (OnDisconnectButtonPressed);
        }

        void Start () {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();

            Initialize ();
        }

        void OnConnectButtonPressed () {
            Initialize ();
            connectButton.interactable = false;
            disconnectButton.interactable = false;
        }

        void OnDisconnectButtonPressed () {
            FizzService.Instance.Close();

            connectButton.interactable = true;
			disconnectButton.interactable = false;
        }

        void OnFIZZButtonPressed () {
			FizzUI.Instance.LaunchFizz ();
        }

        void Initialize () {
            if (PlayerPrefs.GetString (KEY_INTERNAL_USER_ID, string.Empty).Equals (string.Empty))
            {
                ShowInternalUserIdDialogue();
            }
            else 
            {
                InitializeFizz(PlayerPrefs.GetString(KEY_INTERNAL_USER_ID));
            }
        }

        void ShowInternalUserIdDialogue () {
            string _title = Registry.localization.GetText ("General_InternalIDInput");
            string _placeholder = Registry.localization.GetText ("internal id");
            string _leftButtonText = Registry.localization.GetText ("Login");

            if (FizzUI.Instance.AlertView != null) {
                FizzUI.Instance.AlertView.InputPopup (_title, _placeholder, false, _leftButtonText, "", delegate (string userId, int buttonId) {
                    PlayerPrefs.SetString (KEY_INTERNAL_USER_ID, userId);
                    PlayerPrefs.Save ();
                    InitializeFizz(userId);
                });
            }
        }

        void ShowConnectingBanner () {
            connectingAnim.gameObject.SetActive (true);
        }

        void HideConnectingBanner () {
            connectingAnim.gameObject.SetActive (false);
        }

        void InitializeFizz(string userId) {
            ShowConnectingBanner();
            // FizzService.Instance.Open(userId, Core.Utils.GetSystemLanguage(), () => {
            //     _updateConnectingBannerFrameCount = 2;

            //     disconnectButton.interactable = true;
            // });
        }

        int _updateConnectingBannerFrameCount = -1;
        void Update () {
            if (_updateConnectingBannerFrameCount != -1) {
				if (_updateConnectingBannerFrameCount < 1) {
					HideConnectingBanner ();
				}
				_updateConnectingBannerFrameCount--;
			}
        }
    }
}