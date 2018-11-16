using NUnit.Framework;

using Fizz.Common;

namespace Fizz.Chat
{
    [TestFixture()]
    class FizzChannelMessageTest
    {
        [Test(), Timeout(5000)]
        public void InvalidCreationTest()
        {
            var ex = Assert.Throws<FizzException>(() => new FizzChannelMessage(
                1, 
                null, 
                "nick", 
                "to", 
                "body", 
                "data", 
                null,
                FizzUtils.Now()
            ));
            Assert.AreEqual(ex.Message, "invalid_message_from");

            ex = Assert.Throws<FizzException>(() => new FizzChannelMessage(
                1, 
                "from", 
                "nick", 
                null, 
                "body", 
                "data", 
                null,
                FizzUtils.Now()
            ));
            Assert.AreEqual(ex.Message, "invalid_message_to");
        }

        [Test(), Timeout(5000)]
        public void IntegrityTest()
        {
            var now = FizzUtils.Now();
            var m = new FizzChannelMessage(
                1, 
                "from", 
                "nick", 
                "to", 
                "body", 
                "data", 
                null,
                now
            );

            Assert.AreEqual(m.Id, 1);
            Assert.AreEqual(m.From, "from");
            Assert.AreEqual(m.Nick, "nick");
            Assert.AreEqual(m.To, "to");
            Assert.AreEqual(m.Body, "body");
            Assert.AreEqual(m.Data, "data");
            Assert.AreEqual(m.Created, now);
        }
    }
}
