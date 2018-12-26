using System.Collections.Generic;
using Fizz.Chat;
using Fizz.Common;

namespace Fizz
{
    public class FizzChannel
    {
        private string channelId = string.Empty;
        private string channelName = string.Empty;
        private bool cached = false;
        private SortedList<long, FizzChannelMessage> _messageList = new SortedList<long, FizzChannelMessage>(new FizzChannelMessageComparer());
        IList<FizzChannelMessage> cachedMessageList = null;

        public string Id { 
            get { 
                return channelId + "_temp001"; 
            } 
        }

        public string Name {
            get {
                return channelName;
            }
        }

        public IList<FizzChannelMessage> Messages
        {
            get
            {
                if (!cached)
                {
                    cachedMessageList = _messageList.Values;
                }
                return cachedMessageList;
            }
        }

        public FizzChannel(string channelId, string channelName)
        {
            this.channelId = channelId;
            this.channelName = channelName;

            SubscribeAndQuery();
        }

        public void AddMessage(FizzChannelMessage message, bool notify = true)
        {
            if (_messageList.ContainsKey(message.Id))
                return;

            cached = false;
            _messageList.Add(message.Id, message);
        }

        public void AddMessages(IList<FizzChannelMessage> messages)
        {
            foreach (FizzChannelMessage message in messages)
            {
                AddMessage(message, false);
            }
        }

        private void SubscribeAndQuery()
        {
            FizzService.Instance.Client.Chat.Subscribe(Id, subEx =>
            {
                if (subEx != null)
                {
                    FizzLogger.E("Subscribe Error " + Id + " ex " + subEx.Message);
                }
                else
                {
                    FizzLogger.D("Subscribed " + Id);
                    FizzService.Instance.Client.Chat.QueryLatest(Id, FizzService.QUERY_MESSAGES_COUNT, (msgs, qEx) =>
                    {
                        if (qEx == null)
                        {
                            FizzLogger.D("QueryLatest " + msgs.Count);
                            Reset();
                            AddMessages(msgs);
                            FizzService.Instance.OnChannelHistoryUpdated.Invoke (Id);
                        }
                    });
                }
            });
        }

        public void Reset()
        {
            _messageList.Clear();
            cached = false;
        }
    }

    class FizzChannelMessageComparer : IComparer<long>
    {
        public int Compare(long lhs, long rhs)
        {
            return (int)(lhs - rhs);
        }
    }
}