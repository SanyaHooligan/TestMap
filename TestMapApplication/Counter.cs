using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestMapApplication
{
    public class Counter
    {
        public int Limit { get; set; }
        public int Total { get; set; }

        public Counter(int passedLimit, int passedTotal)
        {
            Limit = passedLimit;
            Total = passedTotal;
        }

        public void Add(int x)
        {
            Total += x;
            if (Total >= Limit)
            {
                OnLimitReached(EventArgs.Empty);
            }
        }

        protected virtual void OnLimitReached(EventArgs e)
        {
            EventHandler handler = LimitReached;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler LimitReached;
    }
}
