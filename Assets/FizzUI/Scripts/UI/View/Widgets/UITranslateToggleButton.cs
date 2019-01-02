using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fizz.UI.Components {
	public class UITranslateToggleButton : MonoBehaviour {

		[SerializeField] Image orignalImage;
		[SerializeField] Image translateImage;

		Button _button;
		bool _isOriginal;

		void Awake ()
		{
			_button = gameObject.GetComponent<Button> ();
		}

		public void Configure (Models.UITranslationState state)
		{
			orignalImage.gameObject.SetActive (state == Models.UITranslationState.Original);
			translateImage.gameObject.SetActive (state != Models.UITranslationState.Original);
		}

		public void ShowOriginal ()
		{
			_isOriginal = true;
			orignalImage.gameObject.SetActive (true);
			translateImage.gameObject.SetActive (false);
		}

		public void ShowTranslate ()
		{
			_isOriginal = false;
			orignalImage.gameObject.SetActive (false);
			translateImage.gameObject.SetActive (true);
		}

		public void Toggle ()
		{
			_isOriginal = !_isOriginal;
			orignalImage.gameObject.SetActive (_isOriginal);
			translateImage.gameObject.SetActive (!_isOriginal);
		}
	}
}