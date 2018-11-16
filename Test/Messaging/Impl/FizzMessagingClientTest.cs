using NUnit.Framework;
using System;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fizz.Threading;
using Fizz.Common;

namespace Fizz.Messaging.Impl
{
    /*
    [TestFixture()]
    class FizzMessagingClientTest
    {
        private static readonly int INVALID_CHANNEL_ID_LEN = 65;
        private static readonly int INVALID_MESSAGE_TYPE_LEN = 33;
        private static readonly int INVALID_QUERY_MESSAGE_COUNT = 51;

        [Test()]
        public void ClientCreationTests()
        {
            var missingAppIdEx = Assert.Throws<FizzException>(() => new FizzMessagingClient(null, new FizzMockActionDispatcher()));
            Assert.AreEqual(missingAppIdEx.Code, FizzError.ERROR_BAD_ARGUMENT);
            Assert.AreEqual(missingAppIdEx.Reason, "invalid_app_id");

            var emptyAppIdEx = Assert.Throws<FizzException>(() => new FizzMessagingClient("", new FizzMockActionDispatcher()));
            Assert.AreEqual(emptyAppIdEx.Code, FizzError.ERROR_BAD_ARGUMENT);
            Assert.AreEqual(emptyAppIdEx.Reason, "invalid_app_id");

            var missingDispatcherEx = Assert.Throws<FizzException>(() => new FizzMessagingClient("appA", null));
            Assert.AreEqual(missingDispatcherEx.Code, FizzError.ERROR_BAD_ARGUMENT);
            Assert.AreEqual(missingDispatcherEx.Reason, "invalid_dispatcher");
        }

        [Test()]
        public void ClientOpenTests()
        {
            FizzMessagingClient client = new FizzMessagingClient("appA", new FizzMockActionDispatcher());

            var missingUserIdEx = Assert.Throws<FizzException>(() => client.Open(null, new FizzMockAuthRestClient()));
            Assert.AreEqual(missingUserIdEx.Code, FizzError.ERROR_BAD_ARGUMENT);
            Assert.AreEqual(missingUserIdEx.Reason, "invalid_user_id");

            var emptyUserIdEx = Assert.Throws<FizzException>(() => client.Open("", new FizzMockAuthRestClient()));
            Assert.AreEqual(emptyUserIdEx.Code, FizzError.ERROR_BAD_ARGUMENT);
            Assert.AreEqual(emptyUserIdEx.Reason, "invalid_user_id");

            var missingClientEx = Assert.Throws<FizzException>(() => client.Open("userA", null));
            Assert.AreEqual(missingClientEx.Code, FizzError.ERROR_BAD_ARGUMENT);
            Assert.AreEqual(missingClientEx.Reason, "invalid_rest_client");
        }

        [Test()]
    #if !(E2E)
        [Ignore("Ignoring publish test")]
    #endif
        public async Task PublishAndNotificationTest()
        {
            TaskCompletionSource<object> messageReceived = new TaskCompletionSource<object>();
            string user = "userA";
            string channel = Guid.NewGuid().ToString();
            string content = "test message 1";

            IFizzActionDispatcher dispatcher = new FizzMockActionDispatcher();
            FizzAuthRestClient restClient = new FizzAuthRestClient("appA", dispatcher);
            FizzChannelMessage message1 = BuildMessage(1, user, "text", content);
            FizzMessagingClient client = new FizzMessagingClient("appA", dispatcher);

            client.Listener.OnMessageReceived += (c, message) => 
            {
                ValidateMessages(message1, message);
                messageReceived.SetResult(null);
            };

            await FizzTestUtils.FetchSessionToken(restClient, user);

            client.Open(user, restClient);

            await ConnectClient(client);

            await Subscribe(client, channel);

            await Publish(client, channel, message1.Type, content);

            await messageReceived.Task;
        }

        [Test()]
    #if !(E2E)
        [Ignore("Ignoring publish test")]
    #endif
        public async Task PublishAndQueryTest()
        {
            TaskCompletionSource<object> messageReceived = new TaskCompletionSource<object>();

            string appId = "appA";
            string user = "userA";
            string channel = Guid.NewGuid().ToString();
            string type = "text";

            FizzChannelMessage message1 = BuildMessage(1, user, type, "test message 1");
            FizzChannelMessage message2 = BuildMessage(2, user, type, "test message 2");
            FizzChannelMessage message3 = BuildMessage(3, user, type, "test message 3");

            IFizzActionDispatcher dispatcher = new FizzMockActionDispatcher();
            IFizzAuthRestClient restClient = new FizzAuthRestClient(appId, dispatcher);
            FizzMessagingClient client = new FizzMessagingClient(appId, dispatcher);

            int receivedMessages = 0;
            client.Listener.OnMessageReceived += (c, message) =>
            {
                if (++receivedMessages >= 3)
                {
                    messageReceived.SetResult(null);
                }
            };

            await FizzTestUtils.FetchSessionToken(restClient, user);

            client.Open(user, restClient);

            await ConnectClient(client);

            await Subscribe(client, channel);

            await Publish(client, channel, message1.Type, message1.Data);

            await Publish(client, channel, message2.Type, message2.Data);

            await Publish(client, channel, message3.Type, message3.Data);

            await messageReceived.Task;

            TaskCompletionSource<object> queried = new TaskCompletionSource<object>();
            client.QueryLatest(channel, 2, (messages, ex) => 
            {
                Assert.AreEqual(2, messages.Count);
                ValidateMessages(message2, messages[0]);
                ValidateMessages(message3, messages[1]);
                queried.SetResult(null);
            });
            await queried.Task;
        }

        [Test(), Timeout(5000)]
        public async Task PublishMissingMessageTypeTest()
        {
            TaskCompletionSource<object> completed = new TaskCompletionSource<object>();

            FizzMessagingClient client = new FizzMessagingClient("appA", new FizzMockActionDispatcher());

            client.Open("userA", new FizzMockAuthRestClient());

            client.Publish("channelA", null, new Dictionary<string, string>() { { "content", "message 1" } },
            ex =>
            {
                Assert.IsNotNull(ex);
                Assert.AreEqual(ex.Code, FizzError.ERROR_BAD_ARGUMENT);
                Assert.AreEqual(ex.Reason, "invalid_message_type");
                completed.SetResult(null);
            });

            await completed.Task;
        }

        [Test(), Timeout(5000)]
        public async Task PublishEmptyMessageTypeTest()
        {
            TaskCompletionSource<object> completed = new TaskCompletionSource<object>();

            FizzMessagingClient client = new FizzMessagingClient("appA", new FizzMockActionDispatcher());

            client.Open("userA", new FizzMockAuthRestClient());

            client.Publish("channelA", "", new Dictionary<string, string>() { { "content", "message 1" } },
            ex =>
            {
                Assert.IsNotNull(ex);
                Assert.AreEqual(ex.Code, FizzError.ERROR_BAD_ARGUMENT);
                Assert.AreEqual(ex.Reason, "invalid_message_type");
                completed.SetResult(null);
            });

            await completed.Task;
        }

        [Test(), Timeout(5000)]
    #if !(E2E)
        [Ignore("Ignoring PublishLargeMessageTypeTest")]
    #endif
        public async Task PublishLargeMessageTypeTest()
        {
            TaskCompletionSource<object> completed = new TaskCompletionSource<object>();

            string app = "appA";
            string user = "userA";

            IFizzActionDispatcher dispatcher = new FizzMockActionDispatcher();

            IFizzAuthRestClient restClient = new FizzAuthRestClient(app, dispatcher);

            FizzMessagingClient client = new FizzMessagingClient(app, new FizzMockActionDispatcher());

            await FizzTestUtils.FetchSessionToken(restClient, user);

            client.Open(user, restClient);

            client.Publish(
                "channelA", 
                RandomString(INVALID_MESSAGE_TYPE_LEN), 
                new Dictionary<string, string>() { { "content", "message 1" } },
                ex =>
                {
                    Assert.IsNotNull(ex);
                    Assert.AreEqual(ex.Code, FizzError.ERROR_BAD_ARGUMENT);
                    Assert.AreEqual(ex.Reason, "invalid_message_type");
                    completed.SetResult(null);
                }
            );

            await completed.Task;
        }

        [Test(), Timeout(5000)]
        public async Task PublishMissingChannelTest()
        {
            TaskCompletionSource<object> completed = new TaskCompletionSource<object>();

            FizzMessagingClient client = new FizzMessagingClient("appA", new FizzMockActionDispatcher());

            client.Open("userA", new FizzMockAuthRestClient());

            client.Publish(null, "text", new Dictionary<string, string>() { { "content", "message 1" } },
            ex =>
            {
                Assert.IsNotNull(ex);
                Assert.AreEqual(ex.Code, FizzError.ERROR_BAD_ARGUMENT);
                Assert.AreEqual(ex.Reason, "invalid_channel_id");
                completed.SetResult(null);
            });

            await completed.Task;
        }

        [Test(), Timeout(5000)]
        public async Task PublishEmptyChannelTest()
        {
            TaskCompletionSource<object> completed = new TaskCompletionSource<object>();

            FizzMessagingClient client = new FizzMessagingClient("appA", new FizzMockActionDispatcher());

            client.Open("userA", new FizzMockAuthRestClient());

            client.Publish("", "text", new Dictionary<string, string>() { { "content", "message 1" } },
            ex =>
            {
                Assert.IsNotNull(ex);
                Assert.AreEqual(ex.Code, FizzError.ERROR_BAD_ARGUMENT);
                Assert.AreEqual(ex.Reason, "invalid_channel_id");
                completed.SetResult(null);
            });

            await completed.Task;
        }

        [Test(), Timeout(5000)]
    #if !(E2E)
        [Ignore("Ignoring PublisLargeChannelTest")]
    #endif
        public async Task PublisLargeChannelTest()
        {
            TaskCompletionSource<object> completed = new TaskCompletionSource<object>();

            string app = "appA";
            string user = "userA";

            IFizzActionDispatcher dispatcher = new FizzMockActionDispatcher();

            IFizzAuthRestClient restClient = new FizzAuthRestClient(app, dispatcher);

            FizzMessagingClient client = new FizzMessagingClient(app, new FizzMockActionDispatcher());

            await FizzTestUtils.FetchSessionToken(restClient, user);

            client.Open(user, restClient);

            client.Publish(
                RandomString(INVALID_CHANNEL_ID_LEN), 
                "text", 
                new Dictionary<string, string>() { { "content", "message 1" } },
                ex =>
                {
                    Assert.IsNotNull(ex);
                    Assert.AreEqual(ex.Code, FizzError.ERROR_BAD_ARGUMENT);
                    Assert.AreEqual(ex.Reason, "invalid_channel_id");
                    completed.SetResult(null);
                }
            );

            await completed.Task;
        }

        [Test(), Timeout(5000)]
        public async Task PublishInvalidMessageDataTest()
        {
            TaskCompletionSource<object> completed = new TaskCompletionSource<object>();

            string app = "appA";
            string user = "userA";

            IFizzActionDispatcher dispatcher = new FizzMockActionDispatcher();

            IFizzAuthRestClient restClient = new FizzAuthRestClient(app, dispatcher);

            FizzMessagingClient client = new FizzMessagingClient(app, new FizzMockActionDispatcher());

            await FizzTestUtils.FetchSessionToken(restClient, user);

            client.Open(user, restClient);

            client.Publish("channelA", "text", null,
            ex =>
            {
                Assert.IsNotNull(ex);
                Assert.AreEqual(ex.Code, FizzError.ERROR_BAD_ARGUMENT);
                Assert.AreEqual(ex.Reason, "invalid_message_data");
                completed.SetResult(null);
            });

            await completed.Task;
        }

        [Test(), Timeout(60000)]
    #if !(E2E)
        [Ignore("ignoring PublishMessageDataWithUnconstrainedItemsTest")]
    #endif
        public async Task PublishMessageDataWithUnconstrainedItemsTest()
        {
            TaskCompletionSource<object> completed = new TaskCompletionSource<object>();

            string app = "appA";
            string user = "userA";

            IFizzActionDispatcher dispatcher = new FizzMockActionDispatcher();

            IFizzAuthRestClient restClient = new FizzAuthRestClient(app, dispatcher);

            FizzMessagingClient client = new FizzMessagingClient(app, new FizzMockActionDispatcher());
            Dictionary<string, string> data = new Dictionary<string, string>();

            for (int di = 0; di < 33; di++)
            {
                data["key" + di] = "value" + di;
            }

            await FizzTestUtils.FetchSessionToken(restClient, user);

            client.Open(user, restClient);

            client.Publish("channelA", "text", data, ex =>
            {
                Assert.IsNotNull(ex);
                Assert.AreEqual(ex.Code, FizzError.ERROR_BAD_ARGUMENT);
                Assert.AreEqual(ex.Reason, "invalid_message_data");
                completed.SetResult(null);
            });

            await completed.Task;
        }

        [Test(), Timeout(5000)]
    #if !(E2E)
        [Ignore("ignoring publish empty message data test")]
    #endif
        public async Task PublishEmptyMessageDataTest()
        {
            TaskCompletionSource<object> completed = new TaskCompletionSource<object>();

            string app = "appA";
            string user = "userA";

            IFizzActionDispatcher dispatcher = new FizzMockActionDispatcher();

            IFizzAuthRestClient restClient = new FizzAuthRestClient(app, dispatcher);

            FizzMessagingClient client = new FizzMessagingClient(app, new FizzMockActionDispatcher());

            await FizzTestUtils.FetchSessionToken(restClient, user);

            client.Open(user, restClient);

            client.Publish("channelA", "text", new Dictionary<string, string> {},
            ex =>
            {
                Assert.IsNull(ex);
                completed.SetResult(null);
            });

            await completed.Task;
        }

        [Test(), Timeout(5000)]
        public async Task SubscribeMissingChannelTest()
        {
            TaskCompletionSource<object> completed = new TaskCompletionSource<object>();
            FizzMessagingClient client = new FizzMessagingClient("appA", new FizzMockActionDispatcher());

            client.Open("userA", new FizzMockAuthRestClient());
            client.Subscribe(null, ex => 
            {
                Assert.IsNotNull(ex);
                Assert.AreEqual(ex.Code, FizzError.ERROR_BAD_ARGUMENT);
                Assert.AreEqual(ex.Reason, "invalid_channel_id");
                completed.SetResult(null);
            });

            await completed.Task;
        }

        [Test(), Timeout(5000)]
        public async Task SubscribeEmptyChannelTest()
        {
            TaskCompletionSource<object> completed = new TaskCompletionSource<object>();
            FizzMessagingClient client = new FizzMessagingClient("appA", new FizzMockActionDispatcher());

            client.Open("userA", new FizzMockAuthRestClient());
            client.Subscribe("", ex =>
            {
                Assert.IsNotNull(ex);
                Assert.AreEqual(ex.Code, FizzError.ERROR_BAD_ARGUMENT);
                Assert.AreEqual(ex.Reason, "invalid_channel_id");
                completed.SetResult(null);
            });

            await completed.Task;
        }

        [Test(), Timeout(5000)]
    #if !(E2E)
        [Ignore("Ignoring SubscribeLargeChannelTest")]
    #endif
        public async Task SubscribeLargeChannelTest()
        {
            TaskCompletionSource<object> completed = new TaskCompletionSource<object>();

            string app = "appA";
            string user = "userA";

            IFizzActionDispatcher dispatcher = new FizzMockActionDispatcher();

            IFizzAuthRestClient restClient = new FizzAuthRestClient(app, dispatcher);

            FizzMessagingClient client = new FizzMessagingClient("appA", new FizzMockActionDispatcher());

            await FizzTestUtils.FetchSessionToken(restClient, user);

            client.Open(user, restClient);
            client.Subscribe(RandomString(INVALID_CHANNEL_ID_LEN), ex =>
            {
                Assert.IsNotNull(ex);
                Assert.AreEqual(ex.Code, FizzError.ERROR_BAD_ARGUMENT);
                Assert.AreEqual(ex.Reason, "invalid_channel_id");
                completed.SetResult(null);
            });

            await completed.Task;
        }

        [Test(), Timeout(5000)]
        public async Task UnsubscribeMissingChannelTest()
        {
            TaskCompletionSource<object> completed = new TaskCompletionSource<object>();
            FizzMessagingClient client = new FizzMessagingClient("appA", new FizzMockActionDispatcher());

            client.Open("userA", new FizzMockAuthRestClient());
            client.Unsubscribe(null, ex =>
            {
                Assert.IsNotNull(ex);
                Assert.AreEqual(ex.Code, FizzError.ERROR_BAD_ARGUMENT);
                Assert.AreEqual(ex.Reason, "invalid_channel_id");
                completed.SetResult(null);
            });

            await completed.Task;
        }

        [Test(), Timeout(5000)]
        public async Task UnsubscribeEmptyChannelTest()
        {
            TaskCompletionSource<object> completed = new TaskCompletionSource<object>();
            FizzMessagingClient client = new FizzMessagingClient("appA", new FizzMockActionDispatcher());

            client.Open("userA", new FizzMockAuthRestClient());
            client.Unsubscribe("", ex =>
            {
                Assert.IsNotNull(ex);
                Assert.AreEqual(ex.Code, FizzError.ERROR_BAD_ARGUMENT);
                Assert.AreEqual(ex.Reason, "invalid_channel_id");
                completed.SetResult(null);
            });

            await completed.Task;
        }

        [Test(), Timeout(5000)]
#if !(E2E)
        [Ignore("Ignoring UnsubscribeLargeChannelTest")]
#endif
        public async Task UnsubscribeLargeChannelTest()
        {
            TaskCompletionSource<object> completed = new TaskCompletionSource<object>();

            string app = "appA";
            string user = "userA";

            IFizzActionDispatcher dispatcher = new FizzMockActionDispatcher();

            IFizzAuthRestClient restClient = new FizzAuthRestClient(app, dispatcher);

            FizzMessagingClient client = new FizzMessagingClient("appA", new FizzMockActionDispatcher());

            await FizzTestUtils.FetchSessionToken(restClient, user);

            client.Open(user, restClient);
            client.Unsubscribe(RandomString(INVALID_CHANNEL_ID_LEN), ex =>
            {
                Assert.IsNotNull(ex);
                Assert.AreEqual(ex.Code, FizzError.ERROR_BAD_ARGUMENT);
                Assert.AreEqual(ex.Reason, "invalid_channel_id");
                completed.SetResult(null);
            });

            await completed.Task;
        }

        [Test(), Timeout(5000)]
        public async Task QueryMissingChannelTest()
        {
            TaskCompletionSource<object> completed = new TaskCompletionSource<object>();
            FizzMessagingClient client = new FizzMessagingClient("appA", new FizzMockActionDispatcher());

            client.Open("userA", new FizzMockAuthRestClient());
            client.QueryLatest(null, 1, (messages, ex) =>
            {
                Assert.IsNotNull(ex);
                Assert.AreEqual(ex.Code, FizzError.ERROR_BAD_ARGUMENT);
                Assert.AreEqual(ex.Reason, "invalid_channel_id");
                completed.SetResult(null);
            });

            await completed.Task;
        }

        [Test(), Timeout(5000)]
        public async Task QueryEmptyChannelTest()
        {
            TaskCompletionSource<object> completed = new TaskCompletionSource<object>();
            FizzMessagingClient client = new FizzMessagingClient("appA", new FizzMockActionDispatcher());

            client.Open("userA", new FizzMockAuthRestClient());
            client.QueryLatest("", 1, (messages, ex) =>
            {
                Assert.IsNotNull(ex);
                Assert.AreEqual(ex.Code, FizzError.ERROR_BAD_ARGUMENT);
                Assert.AreEqual(ex.Reason, "invalid_channel_id");
                completed.SetResult(null);
            });

            await completed.Task;
        }

        [Test(), Timeout(5000)]
#if !(E2E)
        [Ignore("Ignoring QueryLargeChannelTest")]
#endif
        public async Task QueryLargeChannelTest()
        {
            TaskCompletionSource<object> completed = new TaskCompletionSource<object>();

            string app = "appA";
            string user = "userA";

            IFizzActionDispatcher dispatcher = new FizzMockActionDispatcher();

            IFizzAuthRestClient restClient = new FizzAuthRestClient(app, dispatcher);

            FizzMessagingClient client = new FizzMessagingClient("appA", new FizzMockActionDispatcher());

            await FizzTestUtils.FetchSessionToken(restClient, user);

            client.Open(user, restClient);
            client.QueryLatest(RandomString(INVALID_CHANNEL_ID_LEN), 1, (messages, ex) =>
            {
                Assert.IsNotNull(ex);
                Assert.AreEqual(ex.Code, FizzError.ERROR_BAD_ARGUMENT);
                Assert.AreEqual(ex.Reason, "invalid_channel_id");
                completed.SetResult(null);
            });

            await completed.Task;
        }

        [Test(), Timeout(5000)]
        public async Task QueryWithNegativeCountTest()
        {
            TaskCompletionSource<object> completed = new TaskCompletionSource<object>();
            FizzMessagingClient client = new FizzMessagingClient("appA", new FizzMockActionDispatcher());

            client.Open("userA", new FizzMockAuthRestClient());
            client.QueryLatest("channelA", -1, (messages, ex) =>
            {
                Assert.IsNotNull(ex);
                Assert.AreEqual(ex.Code, FizzError.ERROR_BAD_ARGUMENT);
                Assert.AreEqual(ex.Reason, "invalid_query_count");
                completed.SetResult(null);
            });

            await completed.Task;
        }

        [Test(), Timeout(5000)]
        public async Task QueryWithZeroCountTest()
        {
            TaskCompletionSource<object> completed = new TaskCompletionSource<object>();
            FizzMessagingClient client = new FizzMessagingClient("appA", new FizzMockActionDispatcher());

            client.Open("userA", new FizzMockAuthRestClient());
            client.QueryLatest("channelA", 0, (messages, ex) =>
            {
                Assert.IsNull(ex);
                Assert.AreEqual(messages.Count, 0);
                completed.SetResult(null);
            });

            await completed.Task;
        }

        [Test(), Timeout(5000)]
    #if !(E2E)
        [Ignore("Ignoring QueryWithLargeMessageCountTest")]
    #endif
        public async Task QueryWithLargeMessageCountTest()
        {
            TaskCompletionSource<object> completed = new TaskCompletionSource<object>();

            string app = "appA";
            string user = "userA";

            IFizzActionDispatcher dispatcher = new FizzMockActionDispatcher();

            IFizzAuthRestClient restClient = new FizzAuthRestClient(app, dispatcher);

            FizzMessagingClient client = new FizzMessagingClient(app, new FizzMockActionDispatcher());

            await FizzTestUtils.FetchSessionToken(restClient, user);

            client.Open(user, restClient);
            client.QueryLatest("channelA", INVALID_QUERY_MESSAGE_COUNT, (messages, ex) =>
            {
                Assert.IsNotNull(ex);
                Assert.AreEqual(ex.Code, FizzError.ERROR_BAD_ARGUMENT);
                Assert.AreEqual(ex.Reason, "invalid_query_count");
                completed.SetResult(null);
            });

            await completed.Task;
        }

        private void ValidateMessages(FizzChannelMessage lhs, FizzChannelMessage rhs)
        {
            Assert.AreEqual(lhs.From, rhs.From);
            Assert.AreEqual(lhs.Type, rhs.Type);

            foreach (string key in lhs.Data.Keys)
            {
                Assert.AreEqual(lhs.Data[key], rhs.Data[key]);
            }
        }

        private FizzChannelMessage BuildMessage(int id, string from, string type, string content)
        {
            return new FizzChannelMessage(
                1,
                from,
                type,
                new Dictionary<string, string>() { { "content", content } },
                0
            );
        }

        private Task ConnectClient(FizzMessagingClient client)
        {
            TaskCompletionSource<object> connected = new TaskCompletionSource<object>();

            client.Listener.OnConnected += syncRequired =>
            {
                connected.SetResult(null);
            };

            return connected.Task;
        }

        private Task Subscribe(FizzMessagingClient client, string channel)
        {
            TaskCompletionSource<object> subscribed = new TaskCompletionSource<object>();

            client.Subscribe(channel, ex =>
            {
                Assert.IsNull(ex);
                subscribed.SetResult(null);
            });

            return subscribed.Task;
        }

        private Task Publish(FizzMessagingClient client, string channel, string type, IDictionary<string, string> data)
        {
            TaskCompletionSource<object> published = new TaskCompletionSource<object>();

            client.Publish(channel, type, data,
            ex =>
            {
                Assert.IsNull(ex);
                published.SetResult(null);
            });

            return published.Task;
        }

        private Task Publish(FizzMessagingClient client, string channel, string type, string content)
        {
            TaskCompletionSource<object> published = new TaskCompletionSource<object>();

            client.Publish(channel, type, new Dictionary<string, string>() { { "content", content } },
            ex =>
            {
                Assert.IsNull(ex);
                published.SetResult(null);
            });

            return published.Task;
        }

        private string RandomString(int size)
        {
            Random random = new Random();
            StringBuilder builder = new StringBuilder();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }
    }
    */
}
