using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelGen
{
    public class Range:IComparable
    {
        public ArrayList children= new ArrayList();      
        public double s, e;
        public ModelTree t;
        public Range() { }
        public Range(double s, double e)
        { this.s = s; this.e = e; }

       public override string  ToString(){
           return "[" + s + "," + e + "]";
    }
       public double mid() { return (s + e )/ 2; }
       public static Range Combine(Range r, Range r2)
       {
           if (r.e  +1 == r2.s) return new Range(r.s, r2.e);
           else throw new Exception("could not combine this");
           return null;
       }

       public double len { get { return e - s+1; }  }
       public bool overlap(double y)
       {
           if (y >= s && y <= e) return true;
           return false;
       }

       public bool overlap(Range r)
       {
           if (r.s >= s && r.e <= e) return true;
           return false;
       }

       public int CompareTo(object obj)
       {
           double mid=(s+e )/2;
           Range r=(Range) obj;
           double mid_r=(r.s+r.e )/2;
           if (mid > mid_r) return 1;
           else if (mid < mid_r) return -1;
           return 0;
       }
       public ModelTree convert(TimeSeries ts)
       {
           TimeSeries ts_ = new TimeSeries(ts, this);
           Model m = new Model(ts_);
           ModelTree t = m.construct();
           t.range = this;
           t.children = new ModelTree[this.children.Count];
           int i = 0;
           foreach (Range r in this.children)
           {
               t.children[i] = r.convert(ts);
               i++;
           }
           return t;
       }
    }

    public class RTree
    {
        public Range root;
        public RTree(ArrayList ranges, TimeSeries ts)
        {
            ArrayList R_j = (ArrayList)ranges[ranges.Count - 1];
            root = (Range)R_j[0];
            ArrayList R_i;
            for (int i = ranges.Count - 1; i >= 0; i--)
            {
                R_i = (ArrayList)ranges[i];
                foreach (Range r_i in R_i)
                {
                    TimeSeries ts_ = new TimeSeries(ts, r_i);
                    Model m = new Model(ts_);
                    m.Clean();
                    m.Set();
                    ModelTree t = m.construct();
                    t.range = r_i;
                    t.children = null;                    
                    r_i.t = t;
                }
            }
            
            for (int i = ranges.Count - 2;i>=0 ; i--)
            {
                R_i = (ArrayList)ranges[i];
                R_j = (ArrayList)ranges[i+1];
             
                foreach (Range r_i in R_i)
                {
                    foreach (Range r_j in R_j)
                    {
                        if (r_j.overlap(r_i))
                            r_j.children.Add(r_i);
                    }
                }

            }
            for (int i = ranges.Count - 2; i >= 0; i--)
            {
                R_i = (ArrayList)ranges[i];
                R_j = (ArrayList)ranges[i + 1];

                foreach (Range r_i in R_i)
                {
                    foreach (Range r_j in R_j)
                    {
                        if (r_j.overlap(r_i))
                            if(r_j.len > r_i.len)
                            r_j.t.childs.Add(r_i.t);
                    }
                }
            }
            //convert childs to children
            for (int i = ranges.Count - 1; i >= 0; i--)
            {
                R_i = (ArrayList)ranges[i];
                foreach (Range r_i in R_i)
                {
                    if (r_i.t.childs.Count > 0)
                    {
                        r_i.t.children = new ModelTree[r_i.t.childs.Count];
                        int j = 0;
                        foreach (ModelTree t in r_i.t.childs)
                        {
                            r_i.t.children[j] = t;
                            j++;
                        }
                    }
                }

            }

        }
        
        
    }
}
