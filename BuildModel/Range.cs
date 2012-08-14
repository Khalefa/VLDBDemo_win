using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelGen
{
    public class Range
    {
        public double s, e;
        public Range() { }
        public Range(double s, double e)
        { this.s = s; this.e = e; }

       public override string  ToString(){
           return "[" + s + "," + e + "]";
    }
    
    }
}
