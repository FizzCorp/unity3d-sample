using System;
using System.Collections.Generic;
using Fizz.Common;

namespace Fizz.Chat
{
    public interface IFizzChatClient
    {
        IFizzChannelMessageListener Listener { get; }

        void Publish(String channel, 
                     String nick, 
                     String body, 
                     String data, 
                     bool translate, 
                     bool persist, 
                     Action<FizzException> callback);
        void QueryLatest(String channel, 
                         int count, 
                         Action<IList<FizzChannelMessage>, FizzException> callback);
        void Subscribe(String channel, Action<FizzException> callback);
        void Unsubscribe(String channel, Action<FizzException> callback);
    }
}