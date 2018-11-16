using NUnit.Framework;

using Fizz.Common;
using Fizz.Common.Json;

namespace Fizz.Chat.Impl
{
    [TestFixture()]
    class FizzTopicMessageTest
    {
        [Test(), Timeout(5000)]
        public void IntegrityTest()
        {
            JSONClass json = new JSONClass();
            long id = FizzUtils.Now();
            long created = FizzUtils.Now();

            json[FizzTopicMessage.KEY_ID].AsDouble = (double)id;
            json[FizzTopicMessage.KEY_TYPE] = "test";
            json[FizzTopicMessage.KEY_FROM] = "from";
            json[FizzTopicMessage.KEY_DATA] = "data";
            json[FizzTopicMessage.KEY_CREATED].AsDouble = (double)created;

            var m = new FizzTopicMessage(json.ToString());

            Assert.AreEqual(m.Id, id);
            Assert.AreEqual(m.Type, "test");
            Assert.AreEqual(m.From, "from");
            Assert.AreEqual(m.Data, "data");
            Assert.AreEqual(m.Created, created);
        }
    }
}
