//
//  UITransitableComponent.cs
//
//  Copyright (c) 2016 Fizz Inc
//
using UnityEngine;
using Fizz.UI.Components;
using Fizz;

namespace Fizz.UI.Core
{
	public class UITransitableComponent : UIComponent, IUITransitable
	{
		[SerializeField] UIConnectivityBanner connectivityBanner;

		public virtual void OnEnable ()
		{
			try
			{
				if (FizzService.Instance.Client.State == FizzClientState.Opened)
				{
					FizzService.Instance.Client.Chat.Listener.OnConnected += OnFizzConnected;
					FizzService.Instance.Client.Chat.Listener.OnDisconnected += OnFizzDisconnected;
				}
			}
			catch (FizzException ex)
			{
                Common.FizzLogger.E ("UITransitableComponent ex " + ex.Message);
			}
		}

		public virtual void OnDisable ()
		{
			try
			{
				if (FizzService.Instance.Client.State == FizzClientState.Opened)
				{
					FizzService.Instance.Client.Chat.Listener.OnConnected -= OnFizzConnected;
					FizzService.Instance.Client.Chat.Listener.OnDisconnected -= OnFizzDisconnected;
				}
			}
			catch (FizzException ex)
			{
                Common.FizzLogger.E ("UITransitableComponent ex " + ex.Message);
			}
        }

		public virtual void Popup ()
		{
			var config = new UITransitionSlideOnTop.Config (0.25f, UITransitionSlide.SlideDirection.bottom);
			Registry.router.Show (this, RouterHistoryMode.pushState, config);
		}

		public virtual void Show (RouterHistoryMode mode, UITransitionConfig config)
		{
			Registry.router.Show (this, mode, config);
		}

		public virtual void Hide (UITransitionConfig config)
		{
			Registry.router.Hide (this, config);
		}

		public virtual void OnUIShow ()
		{
			if (FizzService.Instance.IsConnected) {
				if (connectivityBanner != null)
					connectivityBanner.HideBanner (false);
			} else {
                OnFizzDisconnected (null);
			}
		}

		public virtual void OnUIHide ()
		{
			if (connectivityBanner != null) {
				connectivityBanner.HideBanner (false);
			}
		}

		public virtual void OnUIEnable ()
		{
		}

		public virtual void OnUIDisable ()
		{
		}

		public virtual void OnUIRefresh ()
		{
            if (FizzService.Instance.IsConnected) {
				if (connectivityBanner != null)
					connectivityBanner.HideBanner (false);
			} else {
                OnFizzDisconnected (null);
			}
		}

		public virtual void OnUITransitionComplete ()
		{
			
		}

		#region Connnection Listener

		private void OnFizzConnected (bool syncReq)
		{
			if (connectivityBanner != null) { 
				connectivityBanner.HideBanner (true);
			}

			OnUIRefresh ();
		}

        private void OnFizzDisconnected (FizzException ex)
		{
			if (connectivityBanner != null) { 
				connectivityBanner.ShowBanner (false);
			}
		}

		#endregion
	}
}