﻿//
//  UIExtentions.cs
//
//  Copyright (c) 2016 Fizz Inc
//
using UnityEngine.UI;
using UnityEngine;

namespace Fizz.UI.Components
{
	public static class UIExtentions
	{
		public static void IsActiveAndEnabled (this Button button, bool activeAndEnabled) {
			if (button != null && button.gameObject != null) {
				button.interactable = activeAndEnabled;
				button.gameObject.SetActive (activeAndEnabled);
				if (!activeAndEnabled) {
					button.onClick.RemoveAllListeners ();
				}
			}
		}

		public static void SetLocalizedText (this Text label, string key) {
			label.text = Core.Registry.localization.GetText (key);
		}

		public static void DestroyChildren(this RectTransform trans) {
            foreach (RectTransform child in trans) {
                GameObject.Destroy(child.gameObject);
            }
        }

        public static Transform AddChildFromPrefab(this Transform trans, Transform prefab, string name = null) {
            Transform childTrans = GameObject.Instantiate(prefab) as Transform;
            childTrans.SetParent(trans, false);
            if (!string.IsNullOrEmpty (name)) {
                childTrans.gameObject.name = name;
	        }
            return childTrans;
        }
	}

}