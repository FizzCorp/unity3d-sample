//
//  UIComponent.cs
//
//  Copyright (c) 2016 Fizz Inc
//
using UnityEngine;

namespace Fizz.UI.Core
{
	public class UIComponent : MonoBehaviour
	{
		protected bool applyConfig = true;

		private RectTransform rectTransform;
		public RectTransform rect {
			get { 
				if (rectTransform == null)
					rectTransform = gameObject.GetComponent<RectTransform> ();
				return rectTransform;
			}
		}

		protected virtual void Awake ()
		{
			disableChildConfigurations ();
		}

		void disableChildConfigurations ()
		{
			if (!applyConfig) {
				return;
			}
			UIComponent[] children = gameObject.GetComponentsInChildren <UIComponent> ();
			foreach (UIComponent child in children) {
				child.applyConfig = false;
				child.disableChildConfigurations ();
			}
		}
	}
}