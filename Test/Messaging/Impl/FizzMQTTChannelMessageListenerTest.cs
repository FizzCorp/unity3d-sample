using NUnit.Framework;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Fizz.Common;
using Fizz.Threading;

namespace Fizz.Messaging.Impl
{
    /*
    class FizzMockMQTTChannelMessageListener: FizzMQTTChannelMessageListener
    {
        private readonly bool _isSessionPresent = false;
        public FizzMockMQTTChannelMessageListener(string appId, bool isSessionPresent)
            : base(appId, new FizzMockActionDispatcher())
        {
            _isSessionPresent = isSessionPresent;
        }

        override protected IFizzMqttConnection CreateConnection() 
        {
            return new FizzMockMQTTConnection(_isSessionPresent, _session._subscriberId);
        }

        public IFizzMqttConnection Connection
        {
            get {
                return _connection;
            }
        }
    }

    [TestFixture()]
    public class FizzMQTTChannelMessageListenerTest
    {
        const string appId = "appA";
        const string channel = "channelA";

        [Test()]
        public async Task OpenCloseTest()
        {
            var listener = new FizzMockMQTTChannelMessageListener("appA", false);
            var connected = new TaskCompletionSource<object>();
            var disconnected = new TaskCompletionSource<object>();

            listener.OnConnected += (syncRequired =>
            {
                Assert.IsTrue(syncRequired);
                connected.SetResult(null);
            });

            listener.OnDisconnected += () => 
            {
                disconnected.SetResult(null);
            };

            listener.Open("userA", new Session("mock_token", "_session_1", 0));
            listener.Close();
            await connected.Task;
            await disconnected.Task;
        }

        [Test()]
        public async Task MultipleOpenTest()
        {
            var listener = new FizzMockMQTTChannelMessageListener("appA", false);
            var connected = new TaskCompletionSource<object>();
            var openCount = 0;

            listener.OnConnected += (syncRequired =>
            {
                Assert.IsTrue(syncRequired);

                if (++openCount > 1)
                {
                    Assert.Fail();
                }
                connected.SetResult(null);
            });

            listener.Open("userA", new Session("mock_token", "_session_1", 0));
            listener.Open("userA", new Session("mock_token", "_session_1", 0));
            listener.Open("userA", new Session("mock_token", "_session_1", 0));
            await connected.Task;
            await Task.Delay(500);
        }

        [Test()]
        public void MultipleCloseWithoutOpenTest()
        {
            var listener = new FizzMockMQTTChannelMessageListener("appA", false);
            var disconnected = new TaskCompletionSource<object>();

            listener.OnDisconnected += (() =>
            {
                Assert.Fail();
            });

            listener.Close();
            listener.Close();
            listener.Close();
        }

        [Test()]
        public async Task MultipleCloseWithOpenTest()
        {
            var listener = new FizzMockMQTTChannelMessageListener("appA", false);
            var disconnected = new TaskCompletionSource<object>();
            var count = 0;

            listener.OnDisconnected += (() =>
            {
                if (++count > 1)
                {
                    Assert.Fail();
                }
                disconnected.SetResult(null);
            });

            listener.Open("userA", new Session("mock_token", "_session_1", 0));
            listener.Close();
            listener.Close();
            listener.Close();
            await disconnected.Task;
        }

        [Test()]
        public async Task ValidateSyncRequiredTest()
        {
            var sessionPresent = false;
            var listener = new FizzMockMQTTChannelMessageListener("appA", sessionPresent);
            var connected = new TaskCompletionSource<object>();

            listener.OnConnected += (syncRequired => {
                Assert.IsTrue(syncRequired);
                connected.SetResult(null);
            });

            listener.Open("userA", new Session("mock_token", "_session_1", 0));
            await connected.Task;
        }

        [Test()]
        public async Task ValidateSyncNotRequiredTest()
        {
            var sessionPresent = true;
            var listener = new FizzMockMQTTChannelMessageListener("appA", sessionPresent);
            var connected = new TaskCompletionSource<object>();

            listener.OnConnected += (syncRequired => {
                Assert.IsFalse(syncRequired);
                connected.SetResult(null);
            });

            listener.Open("userA", new Session("mock_token", "_session_1", 0));
            await connected.Task;
        }

        [Test()]
        public async Task ParseValidMessageTest()
        {
            var listener = new FizzMockMQTTChannelMessageListener(appId, true);
            var messageReceived = new TaskCompletionSource<object>();
            var message = BuildTextMessage(1, "userA", "test message 1");

            listener.OnMessageReceived += (c, m) =>
            {
                Assert.AreEqual(channel, c);
                Validate(m, message);
                messageReceived.SetResult(null);
            };

            listener.Open("userA", new Session("mock_token", "_session_1", 0));

            PublishMessage(listener, channel, message);

            await messageReceived.Task;
        }

        [Test()]
        public async Task ParseInvalidMessageTest()
        {
            var listener = new FizzMockMQTTChannelMessageListener(appId, true);

            listener.OnMessageReceived += (c, m) => Assert.Fail();

            listener.Open("userA", new Session("mock_token", "_session_1", 0));

            var connection = listener.Connection as FizzMockMQTTConnection;
            connection.Publish(appId + "/" + channel, Encoding.UTF8.GetBytes("invalid_message_format"));

            await Task.Delay(100);
        }

        FizzJsonChannelMessage BuildTextMessage(long id, String from, String content)
        {
            return new FizzJsonChannelMessage(
                id, from, "text",
                new Dictionary<String, String>() {
                {"content", content}
                },
                Now()
            );    
        }

        void PublishMessage(FizzMockMQTTChannelMessageListener listener, 
                            String channelId, 
                            FizzJsonChannelMessage message)
        {
            var connection = listener.Connection as FizzMockMQTTConnection;
            var topic = appId + "/" + channelId;
            var payload = Encoding.UTF8.GetBytes(message.Serialize());
            connection.Publish(topic, payload);
        }

        void Validate(FizzChannelMessage lhs, FizzChannelMessage rhs)
        {
            Assert.AreEqual(lhs.Id, rhs.Id);
            Assert.AreEqual(lhs.From, rhs.From);
            Assert.AreEqual(lhs.Created, rhs.Created);

            foreach (String key in lhs.Data.Keys)
            {
                Assert.AreEqual(lhs.Data[key], rhs.Data[key]);
            }
        }

        private long Now()
        {
            return (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }
    }
    */
}
