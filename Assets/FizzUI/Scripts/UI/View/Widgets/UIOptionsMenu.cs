//
//  UIOptionsMenu.cs
//
//  Copyright (c) 2016 Fizz Inc
//
using UnityEngine;
using System.Collections.Generic;
using System;


namespace Fizz.UI.Components
{
	/// <summary>
	/// User interface chat cell options menu.
	/// </summary>
	public class UIOptionsMenu : MonoBehaviour
	{
		[SerializeField] UIOptionsMenuButton botton;

		private Action<string> _callback;
		private List<GameObject> _options;

		void Awake ()
		{
			_options = new List<GameObject> ();
		}

		public void SetupMenu (List<UIOptionsMenuItem> items, Action<string> callback)
		{
			Reset ();
			
			_callback = callback;
			foreach (UIOptionsMenuItem item in items) {
				UIOptionsMenuButton button = Instantiate (botton);
				button.gameObject.SetActive (true);
				button.transform.SetParent (transform, false);
				button.transform.localScale = Vector3.one;

				button.SetupButton (item.id, item.name, ButtonPressed);
				_options.Add (button.gameObject);
			}
		}

		private void Reset ()
		{
			foreach (GameObject go in _options)
				Destroy (go);
			_options.Clear ();
		}

		private void ButtonPressed (string action)
		{
			_callback (action);
		}
	}

	public class UIOptionsMenuItem
	{
		public string id;
		public string name;
	}

}