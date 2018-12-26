//
//  UITabBarButton.cs
//
//  Copyright (c) 2016 Fizz Inc
//
using UnityEngine;
using UnityEngine.UI;
using System;

namespace FIZZ.UI.Components
{
	/// <summary>
	/// User interface tab bar button.
	/// </summary>
	public class UIButtonBarItem : MonoBehaviour
	{
		/// <summary>
		/// The m text.
		/// </summary>
		[SerializeField] Text text;
		/// <summary>
		/// The background.
		/// </summary>
		[SerializeField] Image background;

		private Action<UIButtonBarItemModel> _callback;
		private Button _button;
		private UIButtonBarItemModel _item;

		public UIButtonBarItemModel Data {
			get { return _item; }
		}

		void Awake ()
		{
			_button = gameObject.GetComponent<Button> ();
		}

		void OnEnable ()
		{
			_button.onClick.AddListener (ButtonPressed);
		}

		void OnDisable ()
		{
			_button.onClick.RemoveListener (ButtonPressed);
		}

		#region Public Methods

		public void SetData (UIButtonBarItemModel data, Action<UIButtonBarItemModel> callback)
		{
			_item = data;
			text.text = _item.text;
			_callback = callback;
		}

		public void SetInteractable (bool interactable)
		{
			_button.interactable = interactable;
		}

		#endregion

		#region Private Methods

		private void ButtonPressed ()
		{
			if (_callback != null)
				_callback.Invoke (_item);
		}

		#endregion
	}
}

