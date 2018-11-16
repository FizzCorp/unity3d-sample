using NUnit.Framework;
using System.Threading.Tasks;
using Fizz.Common;


namespace Fizz.Threading
{
    [TestFixture()]    
    public class FizzActionDispatcherTest
    {
        [Test(), Timeout(5000)]
        public async Task ActionTest()
        {
            var dispatcher = new FizzActionDispatcher();
            bool fired1 = false;
            bool fired2 = false;
            var checkpoint1 = new TaskCompletionSource<object>();
            var checkpoint2 = new TaskCompletionSource<object>();

            dispatcher.Post(() => 
            {
                Assert.IsFalse(fired1);
                Assert.IsFalse(fired2);

                fired1 = true;
                checkpoint1.SetResult(null);  
            });

            dispatcher.Post(() => 
            {
                Assert.IsTrue(fired1);
                Assert.IsFalse(fired2);
                fired2 = true;
                checkpoint2.SetResult(null);   
            });

            dispatcher.Process();

            await checkpoint1.Task;
            await checkpoint2.Task;
        }

        [Test(), Timeout(5000)]
        public void TimerTest()
        {
            var dispatcher = new FizzActionDispatcher();
            bool fired1 = false;
            bool fired2 = false;
            bool fired3 = false;

            long scheduledAt1 = FizzUtils.Now();
            dispatcher.Delay(10, () => 
            {
                long now = FizzUtils.Now();
                Assert.IsTrue(now >= scheduledAt1 + 10);
                Assert.IsFalse(fired2);
                Assert.IsFalse(fired3);
                fired1 = true;
            });

            long scheduledAt2 = FizzUtils.Now();
            dispatcher.Delay(1000, () =>
            {
                long now = FizzUtils.Now();
                Assert.IsTrue(now >= scheduledAt2 + 1000);
                Assert.IsTrue(fired1);
                Assert.IsTrue(fired3);
                fired2 = true;
            });

            long scheduledAt3 = FizzUtils.Now();
            dispatcher.Delay(100, () =>
            {
                long now = FizzUtils.Now();
                Assert.IsTrue(now >= scheduledAt3 + 100);
                Assert.IsTrue(fired1);
                Assert.IsFalse(fired2);
                fired3 = true;
            });

            while (!fired1 || !fired2 || !fired3)
            {
                dispatcher.Process();
            }
        }

        [Test(), Timeout(5000)]
        public async Task ReentrantTimerTest()
        {
            FizzActionDispatcher dispatcher = new FizzActionDispatcher();
            var checkpoint1 = new TaskCompletionSource<object>();
            var checkpoint2 = new TaskCompletionSource<object>();

            dispatcher.Delay(0, () =>
            {
                dispatcher.Delay(0, () => checkpoint2.SetResult(null));

                checkpoint1.SetResult(null);
            });

            dispatcher.Process();
            await checkpoint1.Task;
            dispatcher.Process();
            await checkpoint2.Task;
        }

        [Test(), Timeout(5000)]
        public async Task ReentrantActionTest()
        {
            FizzActionDispatcher dispatcher = new FizzActionDispatcher();
            var checkpoint1 = new TaskCompletionSource<object>();
            var checkpoint2 = new TaskCompletionSource<object>();

            dispatcher.Post(() =>
            {
                dispatcher.Post(() => checkpoint2.SetResult(null));

                checkpoint1.SetResult(null);
            });

            dispatcher.Process();
            await checkpoint1.Task;
            dispatcher.Process();
            await checkpoint2.Task;
        }
    }
}
