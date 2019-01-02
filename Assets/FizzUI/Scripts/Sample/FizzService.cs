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

        public IFizzClient Client { get; } = new FizzClient(APP_ID, APP_SECRET);

        public bool IsConnected { get; private set; } = false;

        public bool IsTranslationEnabled { get; private set; } = false;

        public string UserId { get; private set; } = "090078601";

        public string UserName { get; private set; } = "Squid";

        public List<FizzChannel> Channels { get; private set; } = new List<FizzChannel>();

        public Action<string> OnChannelHistoryUpdated;
        public Action<string, FizzChannelMessage> OnChannelMessage;

        List<TestChannelMeta> metaChannelList;
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
            Client.Update();
        }

        public void Open (string userId, string userName, string locale, bool tranlation, List<TestChannelMeta> channelList, Action<bool> onDone)
        {
            UserId = userId;
            UserName = userName;
            metaChannelList = channelList;
            IsTranslationEnabled = tranlation;
            Client.Open(userId, locale, FizzServices.All, ex =>
            {
                if (ex == null) {
                    Client.Chat.Listener.OnConnected += Listener_OnConnected;
                    Client.Chat.Listener.OnDisconnected += Listener_OnDisconnected;
                    Client.Chat.Listener.OnMessagePublished += Listener_OnMessagePublished;
                }

                onDone(ex == null);
            });
        }

        public void Close()
        {
            if (Client != null) {
                Client.Chat.Listener.OnConnected -= Listener_OnConnected;
                Client.Chat.Listener.OnDisconnected -= Listener_OnDisconnected;
                Client.Chat.Listener.OnMessagePublished -= Listener_OnMessagePublished;
            }

            Client.Close(ex =>
            {
                IsConnected = false;
            });
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
            
            FizzService.Instance.OnChannelMessage.Invoke (msg.To, msg);
        }

        void Listener_OnDisconnected(FizzException obj)
        {
            IsConnected = false;
        }

        void Listener_OnConnected(bool syncRequired)
        {
            IsConnected = true;

            if (!syncRequired) 
                return;

            Channels.Clear();
            channelLoopup.Clear();

            foreach (TestChannelMeta channelMeta in metaChannelList)
            {
                if (channelLoopup.ContainsKey (channelMeta.channelId))
                    continue;
                    
                FizzChannel globalChannel = new FizzChannel(channelMeta.channelId, channelMeta.channelName);
                Channels.Add(globalChannel);
                channelLoopup.Add(globalChannel.Id, globalChannel);
            }
        }

        private void OnApplicationQuit()
        {
            Close();
        }
    }
}
