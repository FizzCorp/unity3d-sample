//
//  UIChatView.cs
//
//  Copyright (c) 2016 Fizz Inc
//
using System;
using System.Collections;
using System.Collections.Generic;
using Fizz;
using Fizz.Chat;
using Fizz.Common;
using Fizz.Common.Json;
using Fizz.UI.Components.Extentions;
using Fizz.UI.Components.Models;
using Fizz.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Fizz.UI.Components {
    /// <summary>
    /// User interface chat view.
    /// </summary>
    public class UIChatView : UIComponent, ICustomScrollRectDataSource {
        /// <summary>
        /// The background image.
        /// </summary>
        [SerializeField] Image backgroundImage;
        /// <summary>
        /// The table view.
        /// </summary>
        [SerializeField] CustomScrollRect scrollRect;
        /// <summary>
        /// The chat input.
        /// </summary>
        [SerializeField] UIChatInput chatInput;
        /// <summary>
        /// The options menu.
        /// </summary>
        [SerializeField] UIOptionsMenu optionsMenu;
        /// <summary>
        /// Spinner to show when fetching history
        /// </summary>
        [SerializeField] UISpinner spinner;
        /// <summary>
        /// Scroll Indicator button
        /// </summary>
        [SerializeField] Button scrollIndicator;

        protected override void Awake () {
            base.Awake ();

            Initialize ();
        }

        void OnEnable () {
            FizzService.Instance.OnChannelHistoryUpdated += OnChannelHistoryUpdated;
            FizzService.Instance.OnChannelMessage += OnChannelMessage;

            chatInput.onSend.AddListener (OnSendChatMessage);
            scrollIndicator.onClick.AddListener (OnScrollIndicator);

            scrollRect.onValueChanged.AddListener (OnScollValueChanged);
        }

        void OnDisable () {
            FizzService.Instance.OnChannelHistoryUpdated -= OnChannelHistoryUpdated;
            FizzService.Instance.OnChannelMessage -= OnChannelMessage;

            chatInput.onSend.RemoveListener (OnSendChatMessage);
            scrollIndicator.onClick.RemoveListener (OnScrollIndicator);

            scrollRect.onValueChanged.RemoveListener (OnScollValueChanged);
        }

        #region Public Methods

        /// <summary>
        /// Sets the data.
        /// </summary>
        /// <param name="room">Room.</param>
        public void SetData (FizzChannel room) {
            _data = room;
            _isAppTranslationEnabled = FizzService.Instance.IsTranslationEnabled;
            _userId = FizzService.Instance.UserId;

            StartCoroutine (LoadChatAsync (true));
            CancelInvoke ("RefreshScrollContent");
            InvokeRepeating ("RefreshScrollContent", 0.5f, 0.1f);
        }

        /// <summary>
        /// Shows the error.
        /// </summary>
        /// <param name="error">Error.</param>
        public void ShowError (string error) {
            chatInput.ShowError (error);
        }

        /// <summary>
        /// Hides the error.
        /// </summary>
        public void HideError () {
            chatInput.HideError ();
        }

        public void Reset () {
            chatInput.ResetText ();
            spinner.HideSpinner ();

            if (_chatCellModelList != null) {
                _chatCellModelList.Clear ();
                scrollRect.RefreshContent ();
            }

            if (_actionsLookUpDict != null) {
                _actionsLookUpDict.Clear ();
            }

            StopAllCoroutines ();
            CancelInvoke ();
        }

        #endregion

        #region Private Methods

        private void Initialize () {
            _chatCellModelList = new List<UIChatCellModel> ();
            _actionsLookUpDict = new Dictionary<string, UIChatCellModel> ();
            scrollRect.pullDirection = CustomScrollRect.PullDirection.Up;
            scrollRect.Initialize (this);
            scrollRect.RebuildContent ();
            LoadCellPrefabs ();
        }

        private void LoadCellPrefabs () {
            leftCellView = Utils.LoadPrefabs<UILeftChatCellView> ("Widgets/ChatCells/CellLeft");
            leftRepeatCellView = Utils.LoadPrefabs<UILeftRepeatChatCellView> ("Widgets/ChatCells/CellLeftRepeat");
            rightCellView = Utils.LoadPrefabs<UIRightChatCellView> ("Widgets/ChatCells/CellRight");
            rightRepeatCellView = Utils.LoadPrefabs<UIRightRepeatChatCellView> ("Widgets/ChatCells/CellRightRepeat");
            dateHeaderCellView = Utils.LoadPrefabs<UIChatDateHeaderCellView> ("Widgets/ChatCells/CellDateHeader");
        }

        private IEnumerator LoadChatAsync (bool scrollDown) {

            yield return new WaitForSeconds ((scrollDown) ? 0.26f : 0f);
            LoadChat (scrollDown);

            scrollRect.RefreshContent ();
            if (scrollDown) {
                if (_chatCellModelList.Count > 0) {
                    scrollRect.GoToScrollItem (_chatCellModelList.Count - 1);
                }
            }
        }

        private void RefreshScrollContent () {
            if (_isDirty) scrollRect.RefreshContent ();
            if (!_userScroll && _resetScroll && _chatCellModelList.Count > 0) scrollRect.GoToScrollItem (_chatCellModelList.Count - 1);
            _isDirty = false;
            _resetScroll = false;
        }

        private void LoadChat (bool scrollDown) {
            if (scrollDown) ResetScrollIndicator ();

            _lastAction = null;
            ResetOptionsMenu ();
            LoadMessages ();
        }

        private void LoadMessages () {
            _chatCellModelList.Clear ();
            _actionsLookUpDict.Clear ();

            IList<FizzChannelMessage> actionsList = _data.Messages;
            for (int index = 0; index < actionsList.Count; index++) {
                FizzChannelMessage action = actionsList[index];

                UIChatCellModel model = GetChatCellModelFromAction (action);
                model.Action.PublishState = UIChannelMessageState.Published;
                
                CheckForDateHeader(model.Action);

                _chatCellModelList.Add (model);
                AddActionInLookup (model);
            }
                

        }

        private void CheckForDateHeader (FizzUIMessage action) {
            bool shouldAddHeaderModel = false;
            if (_lastAction == null) {
                shouldAddHeaderModel = true;
                _lastAction = action;
            } else {
                DateTime last = Utils.GetDateTimeToUnixTime (_lastAction.Created);
                last = last.Date;
                DateTime current = Utils.GetDateTimeToUnixTime (action.Created);
                TimeSpan ts = current.Subtract (last);
                if (ts.Days > 0) {
                    shouldAddHeaderModel = true;
                }
                _lastAction = action;
            }

            if (shouldAddHeaderModel) {
                UIChatCellModel model = new UIChatCellModel ();
                model.Action = action;
                model.Type = UIChatCellModel.UIChatCellModelType.DateHeader;

                _chatCellModelList.Add (model);
            }
        }

        private void OnSendChatMessage (string message) {
            string _messageToSend = message.Trim ();
            if (!string.IsNullOrEmpty (_messageToSend)) {
                SendChatMessage (_messageToSend);
                chatInput.ResetText ();
            }
        }

        private void SendChatMessage (string messageStr) {
            if (_data == null) return;

            long now = FizzUtils.Now();
            JSONClass json = new JSONClass();
            json[FizzUIMessage.KEY_CLIENT_ID].AsDouble = now;
            string data = json.ToString();

            FizzChannelMessage message = new FizzChannelMessage(
                now, 
                FizzService.Instance.UserId, 
                FizzService.Instance.UserName, 
                _data.Id, 
                messageStr, 
                data, 
                null, 
                now);

            UIChatCellModel model = GetChatCellModelFromAction (message);
            AddNewAction (model, false, true);

            FizzService.Instance.Client.Chat.Publish(message.To, message.Nick, message.Body, message.Data, FizzService.Instance.IsTranslationEnabled, true, ex => { 
                if (ex == null) {
                    model.Action.PublishState = UIChannelMessageState.Sent;
                    AddNewAction(model);
                }
            });
            //IFIZZChatMessageAction _model = FIZZSDKWrapper.ChatMessageSendRequest (message, _data.RoomId, null);

            //UIChatCellModel model = GetChatCellModelFromAction (_model);
            //AddNewAction (model, false, true);
        }

        private void OnScollValueChanged (Vector2 val) {
            if (optionsMenu.isActiveAndEnabled) {
                optionsMenu.gameObject.SetActive (false);
            }

            float diff = scrollRect.ContentSize - (scrollRect.ViewportSize + scrollRect.content.anchoredPosition.y);
            _userScroll = (scrollRect.ContentSize > scrollRect.ViewportSize && diff > 50.0f);
            if (!_userScroll)
                scrollIndicator.gameObject.SetActive (_userScroll);
        }

        private void OnTranslateToggleClicked (int row) {
            scrollRect.RefreshContent ();
            int lastCellIndex = _chatCellModelList.Count - 1;
            if (row == lastCellIndex) {
                scrollRect.GoToScrollItem (lastCellIndex);
            }
        }

        private void OnScrollIndicator () {
            if (_userScroll) {
                scrollRect.StopMovement ();
                _userScroll = false;
                _resetScroll = true;
                scrollIndicator.gameObject.SetActive (false);
            }
        }

        private void AddNewAction (UIChatCellModel model, bool groupRefresh = false, bool hardRefresh = false, bool updateOnly = false) {
            if (model.Type == UIChatCellModel.UIChatCellModelType.ChatAction) {
                if (_data.Id.Equals (model.Action.To)) {
                    UIChatCellModel existingModel = GetActionFromLookup (model.Action);
                    if (existingModel != null) {
                        existingModel.Action = model.Action;
                        if (!groupRefresh)
                            _isDirty = true;
                    } else {
                        if (!updateOnly) {
                            CheckForDateHeader (model.Action);
                            _chatCellModelList.Add (model);
                            AddActionInLookup (model);
                            if (hardRefresh) {
                                scrollRect.RefreshContent ();
                                scrollRect.GoToScrollItem (_chatCellModelList.Count - 1);
                            } else {
                                // Should refresh content
                                _isDirty = true;
                                // Should reset scroll 
                                _resetScroll = true;
                                // show scroll indicator
                                scrollIndicator.gameObject.SetActive (_userScroll);
                            }
                        }
                    }
                }
            }
        }

        private void ResetOptionsMenu () {
            optionsMenu.gameObject.SetActive (false);
        }

        private void ResetScrollIndicator () {
            _userScroll = false;
            scrollIndicator.gameObject.SetActive (false);
        }

        private UIChatCellModel GetChatCellModelFromAction (FizzChannelMessage action) {
            var model = new UIChatCellModel
            {
                Action = new FizzUIMessage(action.Id, action.From, action.Nick, action.To, action.Body, action.Data, action.Translations, action.Created),
                Type = UIChatCellModel.UIChatCellModelType.ChatAction
            };
            return model;
        }

        private void AddActionInLookup (UIChatCellModel model) {
            
            if (model.Type == UIChatCellModel.UIChatCellModelType.ChatAction) {
                FizzUIMessage action = model.Action;
                if (!string.IsNullOrEmpty (action.Id.ToString ()) && !_actionsLookUpDict.ContainsKey (action.Id.ToString ())) {
                    _actionsLookUpDict.Add (action.Id.ToString (), model);
                } else if (!string.IsNullOrEmpty (action.AlternateId.ToString ()) && !_actionsLookUpDict.ContainsKey (action.AlternateId.ToString ())) {
                    _actionsLookUpDict.Add (action.AlternateId.ToString (), model);
                }
            }
        }

        private UIChatCellModel GetActionFromLookup (FizzUIMessage action) {
            UIChatCellModel model = null;
            if (!string.IsNullOrEmpty (action.AlternateId.ToString ()) && _actionsLookUpDict.ContainsKey (action.AlternateId.ToString ())) {
                _actionsLookUpDict.TryGetValue (action.AlternateId.ToString (), out model);
            } else if (!string.IsNullOrEmpty (action.Id.ToString ()) && _actionsLookUpDict.ContainsKey (action.Id.ToString ())) {
                _actionsLookUpDict.TryGetValue (action.Id.ToString (), out model);
            } 
            return model;
        }

        #endregion

        #region Room Action Event Listeners

        void OnChannelHistoryUpdated (string channelId)
        {
            if (_data != null && _data.Id.Equals (channelId))
                LoadChatAsync(true);
        }

        void OnChannelMessage (string channelId, FizzChannelMessage action) {
            if (_data != null && _data.Id.Equals (channelId)) {
                UIChatCellModel model = GetChatCellModelFromAction (action);
                model.Action.PublishState = UIChannelMessageState.Published;
                AddNewAction (model);
            }
        }

        #endregion

        public GameObject GetListItem (int index, int itemType, GameObject obj) {
            if (obj == null) {
                switch (itemType)
                {
                    case (int)UIChatActionType.YoursMessageAction:
                        obj = Instantiate(rightCellView.gameObject);
                        break;
                    case (int)UIChatActionType.YoursRepeatMessageAction:
                        obj = Instantiate(rightRepeatCellView.gameObject);
                        break;
                    case (int)UIChatActionType.TheirsMessageAction:
                        obj = Instantiate(leftCellView.gameObject);
                        break;
                    case (int)UIChatActionType.TheirsRepeatMessageAction:
                        obj = Instantiate(leftRepeatCellView.gameObject);
                        break;
                    case (int)UIChatActionType.DateHeader:
                        obj = Instantiate(dateHeaderCellView.gameObject);
                        break;
                }
            }

            UIChatCellModel model = _chatCellModelList[index];
            if (model.Type == UIChatCellModel.UIChatCellModelType.ChatAction) {
                FizzUIMessage action = model.Action;
                UIChatCellView chatCellView = obj.GetComponent<UIChatCellView> ();
                chatCellView.rowNumber = index;
                chatCellView.SetData (action, _isAppTranslationEnabled);

                if (itemType == (int) UIChatActionType.TheirsMessageAction) {
                    var leftCell = chatCellView as UILeftChatCellView;
                    leftCell.onTranslateTogglePressed = OnTranslateToggleClicked;
                } else if (itemType == (int) UIChatActionType.TheirsRepeatMessageAction) {
                    var leftRepeatCell = chatCellView as UILeftRepeatChatCellView;
                    leftRepeatCell.onTranslateTogglePressed = OnTranslateToggleClicked;
                } 

            } else if (model.Type == UIChatCellModel.UIChatCellModelType.DateHeader) {
                UIChatDateHeaderCellView dateHeader = obj.GetComponent<UIChatDateHeaderCellView> ();
                dateHeader.SetData (model.Action, _isAppTranslationEnabled);
            }

            return obj;
        }

        public int GetItemCount () {
            return _chatCellModelList.Count;
        }

        public int GetItemType (int index) {
            UIChatActionType actionType;
            UIChatCellModel model = _chatCellModelList[index];
            if (model.Type == UIChatCellModel.UIChatCellModelType.ChatAction) {

                FizzUIMessage lastAction = null;
                FizzUIMessage nextAction = null;
                FizzUIMessage chatAction = _chatCellModelList[index].Action;

                int lastIndex = index - 1;
                while (lastIndex > -1) {
                    if (_chatCellModelList[lastIndex].Type == UIChatCellModel.UIChatCellModelType.ChatAction) {
                        lastAction = _chatCellModelList[lastIndex].Action;
                        break;
                    } 
                    lastIndex--;
                }

                int nextIndex = index + 1;
                while (nextIndex < _chatCellModelList.Count) {
                    if (_chatCellModelList[nextIndex].Type == UIChatCellModel.UIChatCellModelType.ChatAction) {
                        nextAction = _chatCellModelList[nextIndex].Action;
                        break;
                    }
                    nextIndex++;
                }

                string senderId = chatAction.From;

                bool ownMessage = _userId.Equals (senderId);
                UIChatActionType cellType = UIChatActionType.TheirsMessageAction;

                cellType = ownMessage ? UIChatActionType.YoursMessageAction : UIChatActionType.TheirsMessageAction;

                if (!ownMessage && lastAction != null && chatAction.From.Equals (lastAction.From)) {
                    cellType = UIChatActionType.TheirsRepeatMessageAction;
                } else if (ownMessage && nextAction != null && chatAction.From.Equals (nextAction.From)) {
                    cellType = UIChatActionType.YoursRepeatMessageAction;
                }

                actionType = cellType;
            } else {
                actionType = UIChatActionType.DateHeader;
            }

            return (int) actionType;
        }

        private enum UIChatActionType {
            YoursMessageAction,
            YoursRepeatMessageAction,
            TheirsMessageAction,
            TheirsRepeatMessageAction,
            DateHeader
        }

        /// <summary>
        /// The left cell view.
        /// </summary>
        private UILeftChatCellView leftCellView;
        /// <summary>
        /// The left repeat cell view.
        /// </summary>
        private UILeftRepeatChatCellView leftRepeatCellView;
        /// <summary>
        /// The right cell view.
        /// </summary>
        private UIRightChatCellView rightCellView;
        /// <summary>
        /// The right repeat cell view.
        /// </summary>
        private UIRightRepeatChatCellView rightRepeatCellView;
        /// <summary>
        /// The chat date header cell view.
        /// </summary>
        private UIChatDateHeaderCellView dateHeaderCellView;

        /// <summary>
        /// The data.
        /// </summary>
        private FizzChannel _data;
        /// <summary>
        /// The last action.
        /// </summary>
        private FizzChannelMessage _lastAction;
        /// <summary>
        /// The chat actions.
        /// </summary>
        private List<UIChatCellModel> _chatCellModelList;
        /// <summary>
        /// The lookup dictionary for actions
        /// </summary>
        Dictionary<string, UIChatCellModel> _actionsLookUpDict;

        private string _userId = string.Empty;
        private bool _isDirty = false;
        private bool _resetScroll = false;
        private bool _userScroll = false;
        private bool _isAppTranslationEnabled = false;
    }
}
