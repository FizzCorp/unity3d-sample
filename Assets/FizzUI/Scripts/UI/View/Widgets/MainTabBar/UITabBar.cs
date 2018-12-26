//
//  UITabBar.cs
//
//  Copyright (c) 2016 Fizz Inc
//
using UnityEngine;
using Fizz.UI.Core;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Fizz.UI.Components
{
	/// <summary>
	/// User interface main tab bar.
	/// </summary>
	public class UITabBar : UIComponent
	{
		/// <summary>
		/// The background.
		/// </summary>
		[SerializeField] Image background;
		/// <summary>
		/// The item template.
		/// </summary>
		[SerializeField] UITabBarItem itemTemplate;
		/// <summary>
		/// The tabbar item container.
		/// </summary>
		[SerializeField] HorizontalLayoutGroup itemContainer;

		public TabChangeEvent onTabChange;

		private List<UITabBarItem> _items = new List<UITabBarItem> ();
		private UIRouter _router = new UIRouter ();
		private Color _normalColor;
		private Color _selectionColor;
		private UITabBarItem _current;

		protected override void Awake ()
		{
			base.Awake ();

			Initialize ();
		}

		#region Public Methods

		//public void UpdateConfiguration (UIMainTabBarConfig config)
		//{
		//	background.UpdateConfiguration (config.background);
		//	_normalColor = config.tabNormalColor;
		//	_selectionColor = config.tabPressedColor;
		//}

		public void AddTab (string id, string spritePath, string text, UITransitableComponent view)
		{
			if (_items.Count < 10) {
				UITabBarItem _tab = Instantiate (itemTemplate);
				_tab.gameObject.SetActive (true);
				_tab.transform.SetParent (itemContainer.transform, false);
				_tab.transform.localScale = Vector3.one;
				_tab.SetupButton (id, spritePath, text, view, TabBarItemPressed);
				_tab.SetColor (_normalColor);
				_items.Add (_tab);
			}
		}

		public void Show ()
		{
			if (_items.Count > 0)
				TabBarItemPressed (_items [0]);
		}

		public void Reset ()
		{
			_router.HideAll ();

			if (_items != null) {
				foreach (UITabBarItem item in _items) {
					Object.Destroy(item.gameObject);
				}
				_items.Clear();
			}
		}

		public void Refresh ()
		{
			if (_current != null) {
				TabBarItemPressed (_current);
			}
		}

		public void Hide () {
			_router.HideAll ();
		}

		#endregion

		#region Private Methods

		private void Initialize ()
		{
			//UpdateConfiguration (UIConfig.mainTabBarConfig);
		}

		private void TabBarItemPressed (UITabBarItem button)
		{
			int lastIndex = -1;
			int currentIndex = -1;

			if (_current != null) {
				_current.SetInteractable (true);
				_current.SetColor (_normalColor);
				lastIndex = _items.FindIndex (a => a.Id.Equals (_current.Id));
			}

			_current = button;
			_current.SetInteractable (false);
			_current.SetColor (_selectionColor);
			currentIndex = _items.FindIndex (a => a.Id.Equals (_current.Id));

			UITransitionSlideIn.Config config = new UITransitionSlideIn.Config (0f, (currentIndex > lastIndex)? UITransitionSlide.SlideDirection.right : UITransitionSlide.SlideDirection.left);
			_router.Show (_current.View, RouterHistoryMode.replaceState, config);

			if (onTabChange != null) {
				onTabChange.Invoke (_current.Id);
			}
		}

		#endregion

		[System.Serializable]
		public class TabChangeEvent : UnityEvent<string>
		{
			
		}
	}
}