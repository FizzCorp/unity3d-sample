using System;
using System.Collections.Generic;
using Fizz;
using Fizz.Chat;
using Fizz.UI.Demo;
using UnityEngine;

namespace Fizz {
    public class FizzService : MonoBehaviour {

        private static string APP_ID = "751326fc-305b-4aef-950a-074c9a21d461";
        private static string APP_SECRET = "5c963d03-64e6-439a-b2a9-31db60dd0b34";

        public static int QUERY_MESSAGES_COUNT = 30;

        public static FizzService Instance { get; private set; } = null;

        public IFizzClient Client { get; private set; } = new FizzClient(APP_ID, APP_SECRET);

        public bool IsConnected { get; private set; } = false;

        public bool IsTranslationEnabled { get; private set; } = false;

        public string UserId { get; private set; } = "123456789";

        public string UserName { get; private set; } = "Squid";

        public List<FizzChannel> Channels { get; private set; } = new List<FizzChannel>();

        public Action<bool> OnConnected;
        public Action<FizzException> OnDisconnected;

        public Action<string, FizzChannelMessage> OnChannelMessage;
        public Action<string, FizzChannelMessage> OnChannelMessageUpdate;
        public Action<string, FizzChannelMessage> OnChannelMessageDelete;

        public Action<string> OnChannelHistoryUpdated;

        private Dictionary<string, FizzChannel> channelLoopup = new Dictionary<string, FizzChannel>();

        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else if (Instance != this)
                Destroy(gameObject);
                
            DontDestroyOnLoad(gameObject);
        }

        void Update()
        {
            if (Client != null)
            {
                Client.Update();
            }
        }

        public void Open (string userId, string userName, string locale, bool tranlation, List<TestChannelMeta> channelList, Action<bool> onDone)
        {
            UserId = userId;
            UserName = userName;
            AddChannelsInternal(channelList);
            IsTranslationEnabled = tranlation;
            Client.Open(userId, locale, FizzServices.All, ex =>
            {
                if (ex == null) {

                    Client.Chat.Listener.OnConnected += Listener_OnConnected;
                    Client.Chat.Listener.OnDisconnected += Listener_OnDisconnected;
                    Client.Chat.Listener.OnMessageUpdated += Listener_OnMessageUpdated;
                    Client.Chat.Listener.OnMessageDeleted += Listener_OnMessageDeleted;
                    Client.Chat.Listener.OnMessagePublished += Listener_OnMessagePublished;
                } 

                if (onDone != null)
                    onDone(ex == null); 
            });
        }

        public void Close()
        {
            if (Client != null)
            {
                if (Client.Chat != null) {
                    Client.Chat.Listener.OnConnected -= Listener_OnConnected;
                    Client.Chat.Listener.OnDisconnected -= Listener_OnDisconnected;
                    Client.Chat.Listener.OnMessageUpdated -= Listener_OnMessageUpdated;
                    Client.Chat.Listener.OnMessageDeleted -= Listener_OnMessageDeleted;
                    Client.Chat.Listener.OnMessagePublished -= Listener_OnMessagePublished;
                }

                Client.Close(ex =>
                {
                    IsConnected = false;
                });

            }

            Channels.Clear();
            channelLoopup.Clear();
        }

        public void AddChannel(TestChannelMeta channel)
        {
            if (channel == null)
                return;

            if (channelLoopup.ContainsKey(channel.channelId))
                return;

            FizzChannel fizzChannel = AddChannelInternal(channel);

            if (IsConnected)
            {
                fizzChannel.SubscribeAndQuery();
            }
        }

        public void RemoveChannel(string channelId)
        {
            if (string.IsNullOrEmpty(channelId))
                return;

            try
            {
                if (channelLoopup.ContainsKey(channelId))
                {
                    FizzChannel fizzChannel = channelLoopup[channelId];
                    channelLoopup.Remove(channelId);
                    Channels.Remove(fizzChannel);
                    fizzChannel.Unsubscribe(ex => { });
                }
            }
            catch
            {

            }
        }

        public void PublishMessage(string channel, string nick, string body, Dictionary<string, string> data, bool translate, bool persist, Action<FizzException> callback)
        {
            if (Client != null) 
            {
                Client.Chat.PublishMessage (channel, nick, body, data, translate, persist, ex => {
                    if (ex == null) {
                        Client.Ingestion.TextMessageSent (channel, body, nick);
                    }
                    if (callback != null) {
                        callback.Invoke (ex);
                    }
                });
            }
        }

        public void UpdateMessage(string channel, long messageId, string nick, string body, Dictionary<string, string> data, bool translate, bool persist, Action<FizzException> callback)
        {
            if (Client != null)
            {
                Client.Chat.UpdateMessage(channel, messageId, nick, body, data, translate, persist, callback);
            }
        }

        public void DeleteMessage(string channelId, long messageId, Action<FizzException> callback)
        {
            if (Client != null)
            {
                Client.Chat.DeleteMessage(channelId, messageId, callback);
            }
        }

        public FizzChannel GetChannelById (string id)
        {
            if (channelLoopup.ContainsKey(id))
                return channelLoopup[id];

            return null;
        }

        void Listener_OnMessagePublished(Fizz.Chat.FizzChannelMessage msg)
        {
            if (channelLoopup.ContainsKey(msg.To))
            {
                channelLoopup[msg.To].AddMessage(msg);
            }
            
            if (OnChannelMessage != null)
            {
                OnChannelMessage.Invoke (msg.To, msg);
            }
        }

        void Listener_OnMessageDeleted (FizzChannelMessage msg)
        {
            if (channelLoopup.ContainsKey (msg.To))
            {
                channelLoopup[msg.To].RemoveMessage(msg);
            }

            if (OnChannelMessageDelete != null)
            {
                OnChannelMessageDelete.Invoke(msg.To, msg);
            }
        }

        void Listener_OnMessageUpdated (FizzChannelMessage msg)
        {
            if (channelLoopup.ContainsKey (msg.To))
            {
                channelLoopup[msg.To].UpdateMessage(msg);
            }

            if (OnChannelMessageUpdate != null)
            {
                OnChannelMessageUpdate.Invoke(msg.To, msg);
            }
        }

        void Listener_OnDisconnected(FizzException obj)
        {
            IsConnected = false;

            if (OnDisconnected != null)
            {
                OnDisconnected.Invoke(obj);
            }
        }

        void Listener_OnConnected(bool syncRequired)
        {
            IsConnected = true;

            if (OnConnected != null)
            {
                OnConnected.Invoke(syncRequired);
            }

            if (!syncRequired)
                return;

            foreach (FizzChannel channel in Channels)
            {
                channel.SubscribeAndQuery();
            }
        }

        void AddChannelsInternal(List<TestChannelMeta> channelList)
        {
            foreach (TestChannelMeta meta in channelList)
            {
                AddChannelInternal(meta);
            }
        }

        FizzChannel AddChannelInternal(TestChannelMeta channelMeta)
        {
            if (channelLoopup.ContainsKey(channelMeta.channelId))
                return null;

            FizzChannel channel = new FizzChannel(channelMeta.channelId, channelMeta.channelName);
            Channels.Add(channel);
            channelLoopup.Add(channel.Id, channel);
            return channel;
        }

        void OnApplicationQuit()
        {
            Close();
        }
    }
}
