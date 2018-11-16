using System;
using System.Threading.Tasks;

using MQTTnet;
using MQTTnet.Client;

using Fizz.Chat.Impl;

namespace Fizz.Common
{
    class FizzMockMQTTConnection : IFizzMqttConnection
    {
        private static int nextId = 0;

        private readonly bool _isSessionPresent = false;
        private readonly String _clientId;
        private bool _connected = false;

        public FizzMockMQTTConnection(bool isSessionPresent, String clientId)
        {
            Id = System.Threading.Interlocked.Increment(ref nextId);
            _isSessionPresent = isSessionPresent;
            _clientId = clientId;
        }

        public int Id
        {
            get; private set;
        }

        public Action<int, object, MqttClientConnectedEventArgs> Connected { set; get; }
        public Action<int, object, MqttClientDisconnectedEventArgs> Disconnected { set; get; }
        public Action<int, object, MqttApplicationMessageReceivedEventArgs> MessageReceived { set; get; }

        public Task ConnectAsync()
        {
            _connected = true;

            if (Connected != null)
            {
                var args = new MqttClientConnectedEventArgs(_isSessionPresent);
                Connected.Invoke(Id, this, args);
            }

            var connected = new TaskCompletionSource<object>();
            connected.SetResult(null);
            return connected.Task;
        }

        public Task DisconnectAsync()
        {
            if (Disconnected != null)
            {
                var args = new MqttClientDisconnectedEventArgs(_connected, new Exception("disconnected"));
                _connected = false;
                Disconnected.Invoke(Id, this, args);
            }

            var disconnected = new TaskCompletionSource<object>();
            disconnected.SetResult(null);
            return disconnected.Task;
        }

        public void Publish(String topic, byte[] payload)
        {
            if (MessageReceived != null)
            {
                var message = new MqttApplicationMessage();
                message.Payload = payload;
                message.Topic = topic;
                message.Retain = false;
                var args = new MqttApplicationMessageReceivedEventArgs(_clientId, message);
                MessageReceived.Invoke(Id, this, args);
            }
        }
    }
}
