//
//  UIToastsView.cs
//
//  Copyright (c) 2016 Fizz Inc
//
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fizz.UI.Components {
    public class UIToastsView : MonoBehaviour {
		/// <summary>
		/// The toast.
		/// </summary>
		[SerializeField] UIToast toast;
		/// <summary>
		/// The toast layout.
		/// </summary>
		[SerializeField] VerticalLayoutGroup toastsLayout;

		public void Show (string toastMessage) {
			if (string.IsNullOrEmpty(toastMessage))
				return;

			transform.gameObject.SetActive (true);
			UIToast newToast = GetToast ();
			newToast.transform.SetParent (transform, false);
			newToast.Show (toastMessage);

			transform.SetAsLastSibling ();
		}

		private UIToast GetToast () {
			return transform.GetChild (0).GetComponent<UIToast> ();
		}
    }
}