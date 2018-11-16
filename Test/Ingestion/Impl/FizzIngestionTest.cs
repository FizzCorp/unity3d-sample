using NUnit.Framework;

using System;
using System.Threading.Tasks;

using Fizz.Common;
using Fizz.Common.Json;
using Fizz.Threading;

namespace Fizz.Ingestion.Impl
{
    [TestFixture()]
    public class FizzIngestionTest
    {
        [Test(), Timeout(5000)]
        public async Task EventsIngestionTest()
        {
            var ingestion = new FizzIngestionClient(
                new FizzInMemoryEventLog(), 
                new FizzMockActionDispatcher()
            );

            var checkpoint1 = new TaskCompletionSource<object>();
            var checkpoint2 = new TaskCompletionSource<object>();
            var checkpoint3 = new TaskCompletionSource<object>();
            var checkpoint4 = new TaskCompletionSource<object>();

            // User Info
            string userId = "userA";
            string buildVer = "build1234";
            string custom01 = "custom_dim_01";
            string custom02 = "custom_dim_02";
            string custom03 = "custom_dim_03";

            // Text Message Sent
            string content = "test message";
            string channelId = "global";
            string nick = "testNick";

            // Product Purchased
            string productId = "com.fizz.productA";
            double amount = 0.5;
            string currency = "usd";

            var client = new FizzMockAuthRestClient();
            client.OnPost += (host, path, body) => {
                JSONArray events = JSONNode.Parse(body).AsArray;
                for (int ei = 0; ei < events.Count; ei++)
                {
                    JSONNode node = events[ei];

                    Assert.AreEqual((string)node["user_id"], userId);
                    Assert.AreEqual((string)node["build"], buildVer);
                    Assert.AreEqual((string)node["custom_01"], custom01);
                    Assert.AreEqual((string)node["custom_02"], custom02);
                    Assert.AreEqual((string)node["custom_03"], custom03);

                    var type = (FizzEventType)Enum.Parse(typeof(FizzEventType), node["type"]);
                    if (type == FizzEventType.session_started)
                    {
                        checkpoint1.SetResult(null);
                    }
                    else
                    if (type == FizzEventType.text_msg_sent)
                    {
                        Assert.AreEqual((string)node["content"], content);
                        Assert.AreEqual((string)node["channel_id"], channelId);
                        Assert.AreEqual((string)node["nick"], nick);
                        checkpoint2.SetResult(null);
                    }
                    else
                    if (type == FizzEventType.product_purchased)
                    {
                        Assert.AreEqual((string)node["product_id"], productId);
                        Assert.AreEqual(node["amount"].AsDouble, amount);
                        Assert.AreEqual((string)node["currency"], currency);
                        checkpoint3.SetResult(null);
                    }
                    else
                    if (type == FizzEventType.session_ended)
                    {
                        checkpoint4.SetResult(null);
                    }
                }
            };

            ingestion.BuildVer = buildVer;
            ingestion.CustomDimesion01 = custom01;
            ingestion.CustomDimesion02 = custom02;
            ingestion.CustomDimesion03 = custom03;

            ingestion.Open(userId, client.session._serverTS, client);
            ingestion.TextMessageSent(channelId, content, nick);
            ingestion.ProductPurchased(productId, amount, currency);
            ingestion.Close(null);

            await checkpoint1.Task;
            await checkpoint2.Task;
            await checkpoint3.Task;
            await checkpoint4.Task;
        }

        [Test(), Timeout(5000)]
        public void InvalidInputTest()
        {
            var ex = Assert.Throws<FizzException>(() => new FizzIngestionClient(null, new FizzMockActionDispatcher()));
            Assert.AreEqual(ex.Message, "invalid_event_log");

            ex = Assert.Throws<FizzException>(() => new FizzIngestionClient(new FizzInMemoryEventLog(), null));
            Assert.AreEqual(ex.Message, "invalid_dispatcher");


            var ingestion = new FizzIngestionClient(
                new FizzInMemoryEventLog(), 
                new FizzMockActionDispatcher()
            );

            ex = Assert.Throws<FizzException>(() => ingestion.Open(null, FizzUtils.Now(), new FizzMockAuthRestClient()));
            Assert.AreEqual(ex.Message, "invalid_user_id");

            ex = Assert.Throws<FizzException>(() => ingestion.Open("userA", FizzUtils.Now(), null));
            Assert.AreEqual(ex.Message, "invalid_client");
        }
    }
}
