//
//  Registry.cs
//
//  Copyright (c) 2016 Fizz Inc
//
using System;

namespace Fizz.UI.Core
{
	public enum RouterHistoryMode
	{
		pushState,
		replaceState
	}

	public abstract class IUIRouter
	{
		public abstract void Show (IUITransitable panel, RouterHistoryMode mode, UITransitionConfig config);

		public abstract void Hide (IUITransitable panel, UITransitionConfig config);

		public abstract void HideTopmost (UITransitionConfig config);

		public abstract void HideAll ();
	}

	public abstract class IServiceLocalization
	{
		public abstract string GetText (string id);

		public abstract string Language { get; set; }

		public abstract string this [string id] { get; }
	}

	public abstract class IUITransitionRegistry
	{
		public abstract bool RegisterTransition (Type type);

		public abstract void UnregisterTransition (Type type);

		public abstract UITransition this [System.Type type] { get; }
	}

	public static class Registry
	{
		private static IServiceLocalization localizationInstance = new LocalizationService ();

		public static IServiceLocalization localization {
			get {
				return localizationInstance;
			}
			set {
				localizationInstance = value;
			}
		}

		private static IUIRouter routerInstance = new UIRouter ();

		public static IUIRouter router {
			get {
				return routerInstance;
			}
		}

		private static IUITransitionRegistry transitionRegistryInstance = new UITransitionRegistry ();

		public static IUITransitionRegistry transitionRegistry {
			get { 
				return transitionRegistryInstance;
			}
		}
	}
}