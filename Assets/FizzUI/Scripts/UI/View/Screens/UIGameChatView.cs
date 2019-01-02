//
//  UIGameChatView.cs
//
//  Copyright (c) 2016 Fizz Inc
//
using System.Collections.Generic;
using Fizz;
using Fizz.UI.Core;
using UnityEngine;

namespace Fizz.UI.Components {
    /// <summary>
    /// User interface game chat view.
    /// </summary>
    public class UIGameChatView : UITransitableComponent {
        /// <summary>
        /// The tab bar.
        /// </summary>
        [SerializeField] UIButtonBar buttonBar;
        /// <summary>
        /// The chat view.
        /// </summary>
        [SerializeField] UIChatView chatView;

        private UIButtonBarItemModel selectedModelItem;

        #region MonoBehaviour Methods

        protected override void Awake () {
            base.Awake ();
            Initialize ();
        }

        #endregion

        #region Public Methods

        #endregion

        #region UITransitableComponent Methods

        public override void OnUIShow () {
            base.OnUIShow ();
            LoadRooms ();
        }

        public override void OnUIHide() {
            base.OnUIHide ();
        }

        public override void OnUIEnable () {
            base.OnUIEnable ();
            buttonBar.onBarItemPressed.AddListener (TabBarButtonHandler);

            FizzService.Instance.OnChannelHistoryUpdated += OnChannelHistoryUpdated;
        }

        public override void OnUIDisable () {
            base.OnUIDisable ();
            buttonBar.onBarItemPressed.RemoveListener (TabBarButtonHandler);

            FizzService.Instance.OnChannelHistoryUpdated -= OnChannelHistoryUpdated;
        }

        public override void OnUIRefresh () {
            base.OnUIRefresh ();
            LoadRooms ();
        }

        #endregion

        #region Private Methods

        private void Initialize () {

        }

        private void LoadRooms () {
            var items = new List<UIButtonBarItemModel> ();

            List<FizzChannel> fizzChannels = FizzService.Instance.Channels;

            foreach (FizzChannel channel in fizzChannels)
            {
                items.Add(new UIButtonBarItemModel { text = channel.Name, data = channel.Id });
            }

            buttonBar.ResetButtons ();
            buttonBar.SetupTabs (items, ((selectedModelItem != null) ? selectedModelItem.data : string.Empty), true);
        }

        private void TabBarButtonHandler (UIButtonBarItemModel selected) {
            if (selected != null && !string.IsNullOrEmpty (selected.data)) {

                FizzChannel channel = FizzService.Instance.GetChannelById(selected.data);
                if (selectedModelItem != null && !selectedModelItem.data.Equals(selected.data))
                {
                    chatView.Reset();
                }

                selectedModelItem = selected;
                chatView.SetData(channel);
            }
        }

        private void OnChannelHistoryUpdated (string channelId)
        {
            if (!string.IsNullOrEmpty (channelId))
            {
                FizzChannel channel = FizzService.Instance.GetChannelById (channelId);
                buttonBar.AddButton (new UIButtonBarItemModel { text = channel.Name, data = channel.Id });
            }
        }

        #endregion
    }
}