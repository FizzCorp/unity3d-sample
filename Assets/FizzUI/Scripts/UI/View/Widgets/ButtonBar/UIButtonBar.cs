//
//  UIButtonBar.cs
//
//  Copyright (c) 2016 Fizz Inc
//
using UnityEngine;
using System.Collections.Generic;
using Fizz.UI.Core;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;

namespace Fizz.UI.Components
{
	/// <summary>
	/// User interface tab bar.
	/// </summary>
	public class UIButtonBar : UIComponent
	{
		/// <summary>
		/// The background.
		/// </summary>
		[SerializeField] Image background;
		/// <summary>
		/// The button prefab.
		/// </summary>
		[SerializeField] UIButtonBarItem button;
		/// <summary>
		/// The buttons contianer.
		/// </summary>
		[SerializeField] HorizontalLayoutGroup container;
		/// <summary>
		/// The on bar item pressed.
		/// </summary>
		public ButtonBarItemPressedEvent onBarItemPressed;

		private Dictionary<string, UIButtonBarItem> _buttons;
		private int _reloadContainerLayout = -1;
		private UIButtonBarItemModel _currentTabData;
		private VerticalLayoutGroup _backgroundVLG;

		protected override void Awake ()
		{
			base.Awake ();

			Initialize ();
		}

		void LateUpdate ()
		{
			if (_reloadContainerLayout != -1) {
				if (_reloadContainerLayout < 1) {
					UpdateLayout ();
				}

				_reloadContainerLayout--;
			}
		}

		#region Public Methods

//		public void UpdateConfiguration (UITabBarConfig config)
//		{
//			background.color = Color.white;
//			Button uiButton = button.GetComponent<Button> ();

//			Text textComponent = uiButton.GetComponentInChildren<Text> ();
//			if (textComponent != null) {
//				textComponent.color = new Color (176f / 255, 182f / 255, 188f / 255);
//				textComponent.font = Utils.LoadFont ("SourceSansPro-Regular");
//				textComponent.fontSize = 25;
//			}

//			if (uiButton.image != null) {
//				uiButton.image.color = new Color (176f / 255, 182f / 255, 188f / 255);
//			}

//			if (uiButton.transition == Selectable.Transition.ColorTint) {
//				ColorBlock transitionColor = uiButton.colors;
//				transitionColor.normalColor = new Color (176f / 255, 182f / 255, 188f / 255);
//				transitionColor.highlightedColor = new Color (176f / 255, 182f / 255, 188f / 255);
//				transitionColor.pressedColor = new Color (176f / 255, 182f / 255, 188f / 255);
//				transitionColor.disabledColor = Color.black;
//				uiButton.colors = transitionColor;
//			} else if (uiButton.transition == Selectable.Transition.SpriteSwap) {
//				SpriteState spriteSwapState = new SpriteState ();
//				spriteSwapState.pressedSprite = Utils.LoadSprite ("");
//				uiButton.spriteState = spriteSwapState;
//			}
////			background.UpdateConfiguration (config.backgroundConfig);
////			button.GetComponent<Button> ().UpdateConfiguration (config.tabButtonConfig);
		//}

		/// <summary>
		/// Setups the tabs.
		/// </summary>
		/// <param name="items">Items.</param>
		/// <param name="callback">Callback.</param>
		public void SetupTabs (List<UIButtonBarItemModel> items, string selected, bool variableWidth)
		{
			bool isOneSelected = false;
			for (int index = 0; index < items.Count; index++) {
				_AddButton (items [index]);

				if (!string.IsNullOrEmpty (selected) && items [index].data.Equals (selected) && !isOneSelected) {
					TabButtonPressed (items [index]);
					isOneSelected = true;
				}
			}

			if (!isOneSelected && items.Count > 0) {
				TabButtonPressed (items [0]);
			}

			_reloadContainerLayout = 1;
		}

		/// <summary>
		/// Adds the button.
		/// </summary>
		/// <param name="_item">Item.</param>
		public void AddButton (UIButtonBarItemModel _item)
		{
			ResetLayout ();
			_AddButton (_item);
			_reloadContainerLayout = 1;
		}

		/// <summary>
		/// Removes the button.
		/// </summary>
		/// <param name="data">Data.</param>
		public void RemoveButton (UIButtonBarItemModel _item)
		{
			ResetLayout ();
			_RemoveButton (_item);
			_reloadContainerLayout = 1;
		}

		/// <summary>
		/// Resets the buttons.
		/// </summary>
		public void ResetButtons ()
		{
			_currentTabData = null;
			if (_buttons == null)
				return;
			foreach (KeyValuePair<string, UIButtonBarItem> pair in _buttons) {
				UIButtonBarItem button = pair.Value;
				if (button != null) {
					Destroy (button.gameObject);
				}
			}
			_buttons.Clear ();
		}

		#endregion

		#region Private Method(s)

		private void Initialize ()
		{
			_currentTabData = null;
			_buttons = new Dictionary<string, UIButtonBarItem> ();
			_backgroundVLG = button.GetComponentInChildren<VerticalLayoutGroup> ();

			//UpdateConfiguration (UIConfig.tabBarConfig);
		}

		private void UpdateLayout ()
		{
			Rect barSize = transform.GetComponent<RectTransform> ().rect;
			Rect containerSize = container.GetComponent<RectTransform> ().rect;
			if (containerSize.width > barSize.width) {
				ResetLayout ();
			} else {
				float elementSize = barSize.width / _buttons.Count;
				foreach (KeyValuePair<string, UIButtonBarItem> pair in _buttons) {
					UIButtonBarItem barButton = pair.Value;
					LayoutElement layoutElement = barButton.GetComponent<LayoutElement> ();
					layoutElement.preferredWidth = elementSize;
				}

				RectTransform cRect = container.GetComponent<RectTransform> ();
				cRect.anchorMin = Vector2.zero;
				cRect.anchorMax = Vector2.one;
				cRect.sizeDelta = Vector2.zero;
				cRect.offsetMin = Vector2.zero;
				cRect.offsetMax = Vector2.zero;

				container.childForceExpandHeight = true;
				container.childForceExpandWidth = true;
				container.spacing = 0;
				container.padding = new RectOffset (0, 0, 0, 0);

				_backgroundVLG.padding = new RectOffset (0, 0, 0, 0);

				container.GetComponent<ContentSizeFitter> ().horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
				container.SetLayoutHorizontal ();
			}
		}

		private void TabButtonPressed (UIButtonBarItemModel data)
		{
			if (_currentTabData != null) {
				UIButtonBarItem currentButton = _buttons [_currentTabData.data];
				currentButton.SetInteractable (true);
			}

			_currentTabData = data;
			if (onBarItemPressed != null)
				onBarItemPressed.Invoke (data);

			UIButtonBarItem barButton = _buttons [data.data];
			barButton.SetInteractable (false);
		}

		private void ResetLayout ()
		{
			foreach (KeyValuePair<string, UIButtonBarItem> pair in _buttons) {
				UIButtonBarItem barButton = pair.Value;
				LayoutElement layoutElement = barButton.GetComponent<LayoutElement> ();
				layoutElement.preferredWidth = -1;
			}

			RectTransform cRect = container.GetComponent<RectTransform> ();
			cRect.anchorMin = Vector2.zero;
			cRect.anchorMax = new Vector2 (0, 1);
			cRect.sizeDelta = Vector2.zero;
			cRect.offsetMin = Vector2.zero;
			cRect.offsetMax = Vector2.zero;

			container.childForceExpandHeight = true;
			container.childForceExpandWidth = false;
			container.spacing = 40;
			container.padding = new RectOffset (40, 40, 0, 0);

			_backgroundVLG.padding = new RectOffset (0, 0, 0, 0);

			container.GetComponent<ContentSizeFitter> ().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
			container.SetLayoutHorizontal ();
		}

		private bool _AddButton (UIButtonBarItemModel _item)
		{
			bool _added = false;
			if (!_buttons.ContainsKey (_item.data)) {
				UIButtonBarItem _button = Instantiate (button);
				_button.gameObject.SetActive (true);
				_button.transform.SetParent (container.transform, false);
				_button.transform.localScale = Vector3.one;
				_button.SetData (_item, TabButtonPressed);
				_buttons.Add (_item.data, _button);
				_added = true;
			}
			return _added;
		}

		private bool _RemoveButton (UIButtonBarItemModel _item)
		{
			bool _removed = false;
			if (!string.IsNullOrEmpty (_item.data)) {
				if (_buttons.ContainsKey (_item.data)) {
					Destroy (_buttons [_item.data].gameObject);
					_buttons.Remove (_item.data);
					_removed = true;
					if (_currentTabData.data.Equals (_item.data)) {
						_currentTabData = null;
						if (_buttons.Count > 0) {
							KeyValuePair<string, UIButtonBarItem> pair = _buttons.First();
							UIButtonBarItemModel item = pair.Value.Data;
							TabButtonPressed(item);
						}
					}
				}
			}
			return _removed;
		}

		#endregion

		[System.Serializable]
		public class ButtonBarItemPressedEvent : UnityEvent<UIButtonBarItemModel>
		{

		}
	}

	public class UIButtonBarItemModel
	{
		public string text;
		public string data;
	}
}