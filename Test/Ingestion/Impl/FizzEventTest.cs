using NUnit.Framework;
using Fizz.Common;

namespace Fizz.Ingestion.Impl
{
    [TestFixture()]
    public class FizzEventTest
    {
        [Test()]
        public void FizzEventIdTest()
        {
            FizzEvent e1 = BuildEvent("u1", "s1");
            FizzEvent e2 = BuildEvent("u1", "s2");
            FizzEvent e3 = BuildEvent("u1", "s3");

            Assert.IsTrue(e1.Id < e2.Id && e1.Id < e3.Id);
        }

        [Test()]
        public void FizzEventCreationTest()
        {
            FizzEvent e = BuildEvent("u1", "s1");

            Assert.AreEqual(e.Build, "b1");
            Assert.AreEqual(e.Custom01, "c1");
            Assert.AreEqual(e.Custom02, "c2");
            Assert.AreEqual(e.Custom03, "c3");
            Assert.AreEqual(e.Fields, null);
            Assert.AreEqual(e.Platform, "ios");
            Assert.AreEqual(e.SessionId, "s1");
            Assert.AreEqual(e.Type, FizzEventType.session_started);
            Assert.AreEqual(e.UserId, "u1");
            Assert.AreEqual(e.Version, 1);
            Assert.IsTrue(e.Time > 0);
        }

        [Test()]
        public void InvalidInputTest()
        {
            var ex = Assert.Throws<FizzException>(() => BuildEvent(null, "s1"));
            Assert.AreEqual(ex.Message, "invalid_user_id");
            ex = Assert.Throws<FizzException>(() => BuildEvent("userA", null));
            Assert.AreEqual(ex.Message, "invalid_session_id");
        }

        private FizzEvent BuildEvent(string userId, string sessionId)
        {
            return new FizzEvent(
                userId,
                FizzEventType.session_started,
                1, 
                sessionId, 
                FizzUtils.Now(), 
                "ios", 
                "b1",  
                "c1", "c2", "c3",
                null
            );
        }
    }
}
