//
//  UIToast.cs
//
//  Copyright (c) 2016 Fizz Inc
//
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace FIZZ.UI.Components
{
	/// <summary>
	/// User interface toast.
	/// </summary>
	public class UIToast : MonoBehaviour
	{
		/// <summary>
		/// The text.
		/// </summary>
		[SerializeField] Text label;

		private CanvasGroup _canvasGroup;
		private float _fadeOutSpeed = 3f;
		private float _visibleTime = 2.0f;
		private bool _fadeOut = false;

		void Awake ()
		{
			_canvasGroup = GetComponent<CanvasGroup> ();
		}

		void Update ()
		{
			if (_fadeOut) {
				if (_canvasGroup.alpha > 0.01f) {
					_canvasGroup.alpha = Mathf.Lerp (_canvasGroup.alpha, 0, _fadeOutSpeed * Time.deltaTime);
				} else {
					_fadeOut = false;
					_canvasGroup.alpha = 0;
					transform.gameObject.SetActive (false);
				}
			}
		}

		public void Show (string text)
		{
			if (string.IsNullOrEmpty(text))
				return;
				
			transform.gameObject.SetActive (true);
			_canvasGroup.alpha = 1.0f;
			_fadeOut = false;
			label.text = text;
			transform.SetAsLastSibling ();
			StopAllCoroutines ();
			StartCoroutine (StartAnimate ());
		}

		private IEnumerator StartAnimate ()
		{
			yield return new WaitForSeconds (_visibleTime);
			_fadeOut = true;
		}
	}
}

