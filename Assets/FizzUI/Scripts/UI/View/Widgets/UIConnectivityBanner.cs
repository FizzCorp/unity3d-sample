//
//  UIConnectivityBanner.cs
//
//  Copyright (c) 2016 Fizz Inc
//
using FIZZ.UI.Core;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace FIZZ.UI.Components
{
	public class UIConnectivityBanner : UIComponent
	{
		[SerializeField] Image bannerImage;
		[SerializeField] Text messageLabel;

		Color connectedColor = new Color (75.0f / 255, 237.0f / 255, 145.0f / 255);
		Color disconnectedColor = new Color(110.0f / 255, 135.0f / 255, 156.0f / 255);

		#region Public Methods

		public void ShowBanner (bool animate)
		{
			messageLabel.text = Registry.localization.GetText ("General_SocketConnecting");

			if (animate && isActiveAndEnabled) {
				StartCoroutine (ShowBannerCoroutine ());
			} else {
				bannerImage.color = disconnectedColor;
				gameObject.SetActive (true);
			}
		}

		public void HideBanner (bool animate)
		{
			messageLabel.text = Registry.localization.GetText ("General_SocketConnected");

			if (animate && isActiveAndEnabled) {
				StartCoroutine (HideBannerCoroutine ());
			} else {
				bannerImage.color = connectedColor;
				gameObject.SetActive (false);
			}
		}

		#endregion

		#region Private Methods

		IEnumerator ShowBannerCoroutine ()
		{
			bannerImage.color = disconnectedColor;
			gameObject.SetActive (true);
			yield return null;
		}

		IEnumerator HideBannerCoroutine ()
		{
			bannerImage.color = connectedColor;
			yield return new WaitForSeconds (0.5f);
			gameObject.SetActive (false);
		}

		#endregion
	}
}