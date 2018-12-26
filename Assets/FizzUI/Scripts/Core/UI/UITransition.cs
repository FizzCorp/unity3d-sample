//
//  UITransition.cs
//
//  Copyright (c) 2016 Fizz Inc
//
using UnityEngine;
using System;

namespace FIZZ.UI.Core
{
	public interface IUITransitable
	{
		void OnUIShow ();

		void OnUIHide ();

		void OnUIDisable ();

		void OnUIEnable ();

		void OnUIRefresh ();

		void OnUITransitionComplete ();

		RectTransform rect { get; }
	}

	public class UITransitionConfig
	{
		public Type type = typeof(UITransition);
		public float duration = 0.25f;

		public UITransitionConfig (Type inType, float inDuration)
		{
			type = inType;
			duration = inDuration;
		}
	}

	public class UITransitionContext
	{
		public IUITransitable from = null;
		public IUITransitable to = null;
		// public Dictionary<string,object> routeParams = null;
		public UITransitionConfig config;

		public UITransitionContext (UITransitionConfig inConfig, IUITransitable fromPanel = null, IUITransitable toPanel = null)
		{
			if (inConfig == null) {
				Debug.LogError ("Transition config must be defined to run a transition.");
			}
			config = inConfig;
			from = fromPanel;
			to = toPanel;
		}
	}

	public class UITransition : MonoBehaviour
	{
		public virtual void Do (UITransitionContext context, Action<string,UITransitionContext> onComplete)
		{
		}

		protected void EnableComponent (IUITransitable panel)
		{
			if (panel != null) {
				if (panel.rect != null) {
					Utils.SetInteractable (panel.rect.transform, true);
					panel.OnUIEnable ();

					if (!panel.rect.gameObject.activeSelf) {
						panel.rect.gameObject.SetActive (true);
						panel.OnUIShow ();
					}
				} else {
					Debug.LogError ("Trying to enable component with no RectTransform component.");
				}
			}
		}
	}

	public class UITransitionRegistry : IUITransitionRegistry
	{
		public UITransitionRegistry ()
		{
			sceneObject = new GameObject ("fizz_ui_object");
			UnityEngine.Object.DontDestroyOnLoad (sceneObject);
			RegisterTransition (typeof(UITransitionSlideOnTop));
			RegisterTransition (typeof(UITransitionSlideIn));
			RegisterTransition (typeof(UITransitionSlideOut));
		}

		public override bool RegisterTransition (Type type)
		{
			if (!type.IsSubclassOf (typeof(UITransition))) {
				Debug.LogError ("Only subclasses of UITransition can be registered with the UI transition registry.");
				return false;
			}
				
			Component component = sceneObject.GetComponent (type);
			if (component != null) {
				UnityEngine.Object.Destroy (component);
			}

			sceneObject.AddComponent (type);

			return true;
		}

		public override void UnregisterTransition (Type type)
		{
			Component component = sceneObject.GetComponent (type);
			if (component != null) {
				UnityEngine.Object.Destroy (component);
			}
		}

		public override UITransition this [Type type] {
			get { 
				if (sceneObject == null) {
					return null;
				}
				return sceneObject.GetComponent (type) as UITransition;
			}
		}

		private GameObject sceneObject = null;
	}
}