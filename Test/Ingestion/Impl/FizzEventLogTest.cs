using NUnit.Framework;
using System.Threading.Tasks;
using Fizz.Common;

namespace Fizz.Ingestion.Impl
{
    [TestFixture()]
    public class FizzEventLogTest
    {
        [Test()]
        public async Task EventLogTest()
        {
            FizzEvent e1 = BuildEvent("u1", "s1");
            FizzEvent e2 = BuildEvent("u1", "s2");
            FizzEvent e3 = BuildEvent("u1", "s3");
            FizzEvent e4 = BuildEvent("u1", "s4");
            FizzEvent e5 = BuildEvent("u1", "s5");

            FizzInMemoryEventLog log = new FizzInMemoryEventLog();
            log.Put(e1);
            log.Put(e2);
            log.Put(e3);
            log.Put(e4);
            log.Put(e5);

            TaskCompletionSource<object> checkpoint1 = new TaskCompletionSource<object>();
            log.Read(3, events =>
            {
                Assert.AreEqual(events.Count, 3);
                Assert.AreEqual(events[0].SessionId, e1.SessionId);
                Assert.AreEqual(events[1].SessionId, e2.SessionId);
                Assert.AreEqual(events[2].SessionId, e3.SessionId);

                checkpoint1.SetResult(null);
            });
            await checkpoint1.Task;

            log.RollTo(e2);

            TaskCompletionSource<object> checkpoint2 = new TaskCompletionSource<object>();
            log.Read(3, events =>
            {
                Assert.AreEqual(events.Count, 3);
                Assert.AreEqual(events[0].SessionId, e3.SessionId);
                Assert.AreEqual(events[1].SessionId, e4.SessionId);
                Assert.AreEqual(events[2].SessionId, e5.SessionId);

                checkpoint2.SetResult(null);
            });
            await checkpoint2.Task;
        }

        private FizzEvent BuildEvent(string userId, string sessionId)
        {
            return new FizzEvent(
                userId,
                FizzEventType.session_started,
                1, sessionId, FizzUtils.Now(), "ios", "b1", null, null, null, null);
        }
    }
}
