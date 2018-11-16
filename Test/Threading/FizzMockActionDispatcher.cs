using System;
using Fizz.Common;

namespace Fizz.Threading
{
    public class FizzMockActionDispatcher: IFizzActionDispatcher
    {
        public void Post(Action action)
        {
            if (action != null)
            {
                action();
            }
        }

        public void Delay(int delayMS, Action action)
        {
            
        }
    }
}
