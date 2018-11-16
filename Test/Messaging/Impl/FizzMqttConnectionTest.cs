using NUnit.Framework;
using System;
using System.Threading.Tasks;
using Fizz.Common;
using Fizz.Threading;

namespace Fizz.Messaging.Impl
{
    /*
    [TestFixture()]
#if !(E2E)
    [Ignore("Ignoring MQTT connection test")]
#endif
    public class FizzMqttConnectionTest
    {
        private async Task<FizzMQTTConnection> createConnection()
        {
            const bool cleanSession = true;
            const bool retry = false;
            var created = new TaskCompletionSource<FizzMQTTConnection>();

            IFizzActionDispatcher dispatcher = new FizzMockActionDispatcher();
            FizzAuthRestClient restClient = new FizzAuthRestClient("appA", dispatcher);

            await FizzTestUtils.FetchSessionToken(restClient, "userA");

            return new FizzMQTTConnection(
                "userA",
                restClient.session._token,
                restClient.session._subscriberId, 
                retry, 
                cleanSession, 
                dispatcher
            );
        }

        [Test(), Timeout(5000)]
        public async Task ConnectDisconnectTest()
        {
            FizzMQTTConnection connection = await createConnection();
            TaskCompletionSource<object> connected = new TaskCompletionSource<object>();
            TaskCompletionSource<object> disconnected = new TaskCompletionSource<object>();

            connection.Connected += (id, sender, args) => {
                connected.SetResult(null);
            };
            connection.Disconnected += (id, sender, args) => {
                disconnected.SetResult(null);
            };

            await connection.ConnectAsync();
            await connection.DisconnectAsync();
            await connected.Task;
            await disconnected.Task;
        }

        [Test(), Timeout(5000)]
        public async Task MultipleConnectionHandlersTest()
        {
            FizzMQTTConnection connection = await createConnection();
            TaskCompletionSource<object> connected1 = new TaskCompletionSource<object>();
            TaskCompletionSource<object> connected2 = new TaskCompletionSource<object>();

            connection.Connected += (id, sender, args) => {
                connected1.SetResult(null);
            };
            connection.Connected += (id, sender, args) => {
                connected2.SetResult(null);
            };

            await connection.ConnectAsync();
            await connected1.Task;
            await connected2.Task;
        }

        [Test(), Timeout(5000)]
        public async Task MultipleConnectsSingleCallbackTest()
        {
            FizzMQTTConnection connection = await createConnection();
            TaskCompletionSource<object> connected = new TaskCompletionSource<object>();
            int connectCount = 0;

            connection.Connected += (id, sender, args) => {
                if (++connectCount >= 2)
                {
                    Assert.Fail();
                }
                connected.SetResult(null);
            };

            connection.ConnectAsync();
            await connection.ConnectAsync();
            await connected.Task;
        }

        [Test(), Timeout(5000)]
        public async Task MultipleDisconnectHandlersTest()
        {
            FizzMQTTConnection connection = await createConnection();
            TaskCompletionSource<object> disconnected1 = new TaskCompletionSource<object>();
            TaskCompletionSource<object> disconnected2 = new TaskCompletionSource<object>();

            connection.Disconnected += (id, sender, args) => {
                disconnected1.SetResult(null);
            };
            connection.Disconnected += (id, sender, args) => {
                disconnected2.SetResult(null);
            };

            await connection.ConnectAsync();
            await connection.DisconnectAsync();
            await disconnected1.Task;
            await disconnected2.Task;
        }

        [Test(), Timeout(5000)]
        public async Task SimultaneousConnectDisconnectTest()
        {
            FizzMQTTConnection connection = await createConnection();
            TaskCompletionSource<object> disconnected = new TaskCompletionSource<object>();
            int disconnectedCount = 0;
            object synclock = new object();

            connection.Connected += (id, sender, args) => {
                Console.WriteLine("Connected: " + id);
                Assert.Fail();
            };
            connection.Disconnected += (id, sender, args) => {
                lock (synclock)
                {
                    Console.WriteLine("Disconnected: " + id + " " + ++disconnectedCount + " times");
                    if (disconnectedCount <= 1)
                    {
                        disconnected.SetResult(null);
                    }
                }
            };

            connection.ConnectAsync();
            await connection.DisconnectAsync();
            await disconnected.Task;
        }
    }
    */
}
