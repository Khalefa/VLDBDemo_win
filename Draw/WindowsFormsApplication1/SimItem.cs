using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace M
{
    class SimItem : IComparable
    {
        public int id1;
        public int id2;
        public double sim;
        public int len;
        /*
        public int CompareTo(object obj)
        {
            SimItem o = (SimItem)obj;
            if (sim > o.sim) return 1;
            else if (sim == o.sim)
            {
                if (len < o.len) return -1;
                else  return 1;
            }
            else return -1;                
        }
        */
        public int CompareTo(object obj)
        {
            SimItem o = (SimItem)obj;
            if (sim / len > o.sim / o.len) return 1;
            if (sim / len == o.sim / o.len)
            {
                if (len > o.len) return 1;
                else return -1;
            }
            else return -1;
        }
    }
}
