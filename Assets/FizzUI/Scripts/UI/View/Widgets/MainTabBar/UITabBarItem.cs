//
//  UIMainTabBarItem.cs
//
//  Copyright (c) 2016 Fizz Inc
//
using UnityEngine;
using UnityEngine.UI;
using System;
using Fizz.UI.Core;

namespace Fizz.UI.Components
{
	/// <summary>
	/// User interface main tab bar item.
	/// </summary>
	public class UITabBarItem : MonoBehaviour
	{
		/// <summary>
		/// The image.
		/// </summary>
		[SerializeField] Image image;
		/// <summary>
		/// The text.
		/// </summary>
		[SerializeField] Text text;

		public UITransitableComponent View {
			get { return _component; }
		}

		public string Id {
			get { return _id; }
		}

		private UITransitableComponent _component;
		private Action<UITabBarItem> _callback;
		private Button _button;
		private string _id;

		void Awake ()
		{
			_button = GetComponent<Button> ();
		}

		void OnEnable ()
		{
			_button.onClick.AddListener (ButtonPressed);
		}

		void OnDisable ()
		{
			_button.onClick.RemoveListener (ButtonPressed);
		}

		public void SetupButton (string id, string spritePath, string text, UITransitableComponent view, Action<UITabBarItem> callback)
		{
			this._id = id;
			this.text.text = text;
			this._component = view;
			this._callback = callback;

			Sprite sp = Utils.LoadSprite (spritePath);
			if (sp != null) {
				Vector3 size = sp.bounds.size;
				this.image.GetComponent<AspectRatioFitter> ().aspectRatio = size.x / size.y;
				this.image.sprite = sp;
			}
		}

		public void SetInteractable (bool interactable)
		{
			_button.interactable = interactable;
		}

		public void SetColor (Color clr)
		{
			text.color = clr;
			image.color = clr;
		}

		private void ButtonPressed ()
		{
			if (_callback != null)
				_callback.Invoke (this);
		}
	}
}