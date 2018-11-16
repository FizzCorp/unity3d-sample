using System;
using System.Collections.Generic;

using Fizz.Common;

namespace Fizz.Ingestion.Impl
{
    public interface IFizzEventLog 
    {
        void Put(FizzEvent item);
        void Read(int count, Action<List<FizzEvent>> callback);
        void RollTo(FizzEvent item);
    }

    class FizzEventComparer : IComparer<FizzEvent>
    {
        public int Compare(FizzEvent lhs, FizzEvent rhs) 
        {
            return (int)(lhs.Id - rhs.Id);
        }
    }

    public class FizzInMemoryEventLog : IFizzEventLog
    {
        private readonly SortedSet<FizzEvent> log = new SortedSet<FizzEvent>(new FizzEventComparer());

        public void Put(FizzEvent item)
        {
            if (item == null)
            {
                FizzLogger.W("Empty item put in log");
                return;
            }

            log.Add(item);
        }

        public void Read(int count, Action<List<FizzEvent>> callback)
        {
            if (callback == null)
            {
                return;
            }

            List<FizzEvent> events = new List<FizzEvent>();

            foreach (FizzEvent item in log) 
            {
                events.Add(item);
                if (events.Count >= count) {
                    break;
                }
            }

            callback.Invoke(events);
        }

        public void RollTo(FizzEvent item)
        {
            log.RemoveWhere((FizzEvent obj) => obj.Id <= item.Id);
        }
    }
}
