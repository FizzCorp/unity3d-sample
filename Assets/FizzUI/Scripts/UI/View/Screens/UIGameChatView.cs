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
        }

        public override void OnUIDisable () {
            base.OnUIDisable ();
            buttonBar.onBarItemPressed.RemoveListener (TabBarButtonHandler);
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

            //List<FIZZRoom> _defaultRooms = FIZZSDKWrapper.GetRooms (FIZZRoomType.RoomTypeDefault);
            //_defaultRooms.Sort (new FIZZRoomComparer (FIZZRoomCompare.FIZZRoomCompareByName));
            //
            //foreach (FIZZRoom room in _defaultRooms) {
            //    if (room.IsEnabled && !room.IsDeleted && room.UserStatus == FIZZRoomUserStatus.RoomUserStatusJoined)
            //        items.Add (new UIButtonBarItemModel { text = room.Name, data = room.RoomId });
            //}

            //List<FIZZRoom> _gameRooms = FIZZSDKWrapper.GetRooms (FIZZRoomType.RoomTypeGameRoom);
            //_gameRooms.Sort (new FIZZRoomComparer (FIZZRoomCompare.FIZZRoomCompareByName));
            //foreach (FIZZRoom room in _gameRooms) {
            //    if (room.IsEnabled && !room.IsDeleted && room.UserStatus == FIZZRoomUserStatus.RoomUserStatusJoined)
            //        items.Add (new UIButtonBarItemModel { text = room.Name, data = room.RoomId });
            //}

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

        #endregion
    }
}