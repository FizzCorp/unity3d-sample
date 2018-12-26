//
//  UIToggle.cs
//
//  Copyright (c) 2016 Fizz Inc
//
using UnityEngine;
using UnityEngine.UI;
using Fizz.UI.Core;
using UnityEngine.Events;

namespace Fizz.UI.Components
{
	/// <summary>
	/// User interface toggle.
	/// </summary>
	public class UIToggle : UIComponent
	{
		/// <summary>
		/// The slider.
		/// </summary>
		[SerializeField] Slider slider;
		/// <summary>
		/// The background.
		/// </summary>
		[SerializeField] Image background;
		/// <summary>
		/// The handle.
		/// </summary>
		[SerializeField] Image handle;
		/// <summary>
		/// The on color.
		/// </summary>
		[SerializeField] Color on;
		/// <summary>
		/// The off color.
		/// </summary>
		[SerializeField] Color off;

		public ValueChangeEvent onValueChanged;

		private bool _isOn = false;

		public bool isOn {
			get {
				return _isOn;
			}
			set {
				_isOn = value;
				UpdateView (true);
			}
		}

		protected override void Awake ()
		{
			base.Awake ();
		}

		void OnEnable ()
		{
			slider.onValueChanged.AddListener (OnSliderValueChange);
			// handle.GetComponent<Button> ().onClick.AddListener (OnHandlePressed);
			UpdateView (true);
		}

		void OnDisable ()
		{
			slider.onValueChanged.RemoveListener (OnSliderValueChange);
			// handle.GetComponent<Button> ().onClick.RemoveListener (OnHandlePressed);
		}

		private void OnSliderValueChange (float val)
		{
			if (Mathf.RoundToInt (val) == 0) {
				_isOn = false;
				UpdateView (false);
				if (onValueChanged != null)
					onValueChanged.Invoke (_isOn);
			} else {
				_isOn = true;
				UpdateView (false);
				if (onValueChanged != null)
					onValueChanged.Invoke (_isOn);
			}
		}

		private void OnHandlePressed ()
		{
			if (_isOn)
				OnSliderValueChange (0);
			else 
				OnSliderValueChange (1);
		}

		private void UpdateView (bool change)
		{
			if (_isOn) {
				if (change) slider.value = 1;
				background.CrossFadeColor (on, 0.1f, true, true);
			} else {
				if (change) slider.value = 0;
				background.CrossFadeColor (off, 0.1f, true, true);
			}
		}

		[System.Serializable]
		public class ValueChangeEvent : UnityEvent<bool> 
		{
			
		}
	}
}

