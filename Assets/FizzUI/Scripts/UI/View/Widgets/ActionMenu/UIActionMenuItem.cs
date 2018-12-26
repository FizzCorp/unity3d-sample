//
//  UIActionMenuItem.cs
//
//  Copyright (c) 2016 Fizz Inc
//
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Fizz.UI.Components
{
	public class UIActionMenuItem : MonoBehaviour
	{
		/// <summary>
		/// The m text.
		/// </summary>
		[SerializeField] Text text;

		private Action<UIActionMenuItemModel> _callback;
		private Button _button;
		private UIActionMenuItemModel _item;

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

		public void SetData (UIActionMenuItemModel data, Action<UIActionMenuItemModel> callback)
		{
			_item = data;
			text.text = _item.text;
			_callback = callback;
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

