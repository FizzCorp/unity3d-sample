using System;
using System.Collections.Generic;
using System.Linq;
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
                return channelId;
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
                    cached = true;
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
            try 
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
            catch (FizzException ex)
            {
                FizzLogger.E ("SubscribeAndQuery ex " + ex.Message);
            }
        }

        public bool FetchHistory (Action complete)
        {
            long beforeId = -1;
            if (_messageList.Count > 0)
                beforeId = _messageList.First ().Value.Id;

            if (beforeId == -1)
                return false;

            try
            {
                FizzService.Instance.Client.Chat.QueryLatest (Id, FizzService.QUERY_MESSAGES_COUNT, beforeId, (msgs, qEx) => {
                    if (qEx == null)
                    {
                        AddMessages(msgs);
                        FizzService.Instance.OnChannelHistoryUpdated.Invoke (Id);
                    }

                    if (complete != null)
                        complete.Invoke ();
                });
            }
            catch (FizzException ex)
            {
                FizzLogger.E ("FetchHistory ex " + ex.Message);
            }

            return true;
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