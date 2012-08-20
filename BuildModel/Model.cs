using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using stllib;
using System.IO;
namespace ModelGen
{

    public enum ModelType
    {
        Explicit, Implicit, Trend, PLA
    };

    public class Model
    {
        public int id;
        public TimeSeries ts;
        public ModelType type;

        public double[] errors;
        public double[] values;
        public double[] trend;
        public Model seasonal;

        public int freq;
        public int len;
        public double error;
        public int start;
        public Range error_range;
        public ModelTree construct()
        {
            ModelTree t = new ModelTree();
            t.range = new Range(0, len);
            t.len = len;
            t.values = values;
            t.seasonal = seasonal;
            t.freq = freq;
            t.type = type;
            t.error = error;
            t.children = null;
            t.id = id;
            t.ts = ts;
            return t;
        }
        #region code
        private void decompose(int n, int freq, double[] season_)
        {
            int swindow = 10 * n + 1;
            stllib.STL stl = new STL();
            double[] seaonal = new double[freq];
            int[] count = new int[freq];
            int limit = (int)Math.Ceiling((double)n / freq);
            this.freq = freq;
            this.len = n;
            unsafe
            {
                double* y = (double*)utils.Memory.Alloc(sizeof(double) * n);
                double* t = (double*)utils.Memory.Alloc(sizeof(double) * n);
                double* s = (double*)utils.Memory.Alloc(sizeof(double) * n);
                for (int i = 0; i < n; i++) y[i] = ts.data[i];
                stl.compute(y, n, freq, swindow, t, s);
                int k = 0;
                if (type == ModelType.Explicit || type == ModelType.Implicit)
                {
                    values = new double[limit];
                    for (int i = 0; i < limit; i++)
                    {
                        double t1 = 0;
                        double t2 = 0;
                        int l = 0;
                        for (int j = 0; j < freq; j++)
                        {
                            if (k == n) break;
                            l++;
                            k++;
                            t1 += ts.data[j + i * freq];
                            t2 += y[j + i * freq];
                        }
                        t1 /= l;
                        t2 /= l;
                        values[i] = (t2 + t1) / 2;
                    }
                }
                if (type == ModelType.Implicit)
                {
                    double[] x = ChebyshevReg.Solve(values);
                    values = new double[2];
                    values[0] = x[0];
                    values[1] = x[1];
                }
                else if (type == ModelType.Trend)
                {
                    double[] t_ = new double[n];
                    for (int i = 0; i < n; i++)
                    {
                        t_[i] = t[i];
                    }

                    double[] x = ChebyshevReg.Solve(t_);
                    values = new double[2];
                    values[0] = x[0];
                    values[1] = x[1];
                }
                //make seaonality perfect
                for (int i = 0; i < n; i++)
                {
                    seaonal[i % freq] += s[i];
                    count[i % freq]++;
                }
                for (int i = 0; i < freq; i++)
                {
                    season_[i] = seaonal[i % freq] / count[i % freq];
                }
                trend = new double[n];
                for (int i = 0; i < n; i++)
                {
                    trend[i] = t[i];
                }
                utils.Memory.Free(y);
                utils.Memory.Free(s);
                utils.Memory.Free(t);
            }

        }
        
        public Model(TimeSeries ts)
        {
            this.ts = ts;
            seasonal = null;
            values = null;
            errors = null;

            ModelType best = ModelType.Trend;
            double min = double.MaxValue;
            foreach (ModelType t in Enum.GetValues(typeof(ModelType)))
            {
                this.type = t;
                this.Solve();
                this.CalcError();
                double rt = this.Size() * Error(Global.confidence);
                if (min > rt) { min = rt; best = t; }
            }
            type = best;
            this.Solve();
            this.CalcError();
            ComputeErrorRange();
            error = Error(Global.confidence);
            this.len = ts.Length;
        }
        static public Model ModelQuick(TimeSeries ts)
        {
            Model m = new Model();
            m.ts = ts;
            m.seasonal = null;
            m.values = null;
            m.errors = null;
                        
            m.type = ModelType.Explicit;
            m.Solve();
            m.CalcError();
           // m.ComputeErrorRange();
            m.error = m.Error(Global.confidence);
            m.len = ts.Length;
            return m;
        }

        public int countError(double error)
        {
            int count = 0;
            CalcError();

            for (int i = 0; i < ts.Length; i++)
            {
                if (Math.Abs(errors[i]) < error) count++;
            }

            return count;
        }
        public Model()
        {
            ts = null;
            seasonal = null;
            values = null;
            errors = null;
        }
        public void Solve()
        {
            if (type == ModelType.PLA)
            {
                values = new double[2];
                double asum = 0;
                double bsum = 0;

                for (int i = 0; i < this.ts.Length; i++)
                {
                    asum += ((i + 1) - ((ts.Length + 1) / 2)) * ts.data[i];
                    bsum += ((i + 1) - ((2 * ts.Length + 1) / 3)) * ts.data[i];
                }
                values[0] = 12 * asum / ts.Length / (ts.Length + 1) / (ts.Length - 1);
                values[1] = 6 * bsum / ts.Length / (1 - ts.Length);
                return;
            }

            int n = ts.Length;
            int l = 0;
            freq = ts.freq[0];
            while (freq > ts.Length)
            {
                if (l == ts.freq.Length)
                {
                    return;
                }
                freq = ts.freq[l++];

            }

            if (freq == 0)
            {
                //use regression
                values = ChebyshevReg.Solve(ts.data);
                seasonal = null;
            }
            else
            {
                double[] season_ = new double[freq];
                decompose(n, freq, season_);
                seasonal = new Model();
                int[] f;
                if (ts.freq.Length == 1) f = null;
                else
                {
                    f = new int[ts.freq.Length - 1];
                    for (int i = 0; i < ts.freq.Length - 1; i++) f[i] = ts.freq[i + 1];
                }
                seasonal.ts = new TimeSeries(season_, f);

                if (f != null)
                    seasonal.Solve();
            }

        }

        public virtual void Clean()
        {
            errors = null;
            trend = null;
            if ((seasonal != null) && (values != null)) { ts = null; }
            if (seasonal != null) seasonal.Clean();
        }
        public double Eval(int x)
        {
            if (type == ModelType.PLA) return values[0] * (x - start) + values[1];
            if (seasonal == null && values == null) return ts.data[x];

            if (seasonal == null) return values[0] * x + values[1];
            double t, s;
            t = s = 0;
            if (type == ModelType.Explicit)
                t = values[x / freq];
            else if (type == ModelType.Implicit)
                t = values[0] * x / freq + values[1];
            else t = values[0] * x + values[1];

            s = seasonal.Eval(x % freq);
            return s + t;
        }

        private void ComputeErrorRange()
        {
            double[] e = new double[ts.Length];
            for (int i = 0; i < ts.Length; i++)
            {
                e[i] = ts.data[i] - Eval(i);// /ts.data[i] * 100;
            }
            Array.Sort(e);
            error_range = new Range(e[0], e[ts.Length - 1]);
        }

        private void CalcError()
        {
            errors = new double[ts.Length];
            for (int i = 0; i < ts.Length; i++)
            {
                errors[i] = Math.Abs(ts.data[i] - Eval(i));// /ts.data[i] * 100;
            }
            Array.Sort(errors);
        }
        public double AvgError()
        {
            CalcError();
            double error = 0;
            for (int i = 0; i < ts.Length; i++)
            {
                error += errors[i];
            }

            return error / ts.Length;
        }
        public double MaxError()
        {
            CalcError();
            return errors[ts.Length - 1];
        }
        public double Error(double x)
        {
            CalcError();

            return errors[(int)(x * (ts.Length - 1))];
        }
        public virtual int Size()
        {
            if (seasonal == null && values == null && ts == null) return 2;
            if (seasonal == null && values == null) return ts.data.Length+2;
            if (seasonal == null) return 2+2;
            return values.Length + seasonal.Size()+2;
        }
        public void Print()
        {
            Console.WriteLine("{");
            ts.Print();
            Console.Write("Season:");
            if (seasonal == null) Console.WriteLine("null");
            else seasonal.Print();
            if (values != null)
            {
                Console.Write("values[" + values.Length + "]");
                Console.WriteLine(values[0] + "\t" + values[1]);
            }
            else Console.WriteLine("values:null");

            Console.WriteLine("Error:" + AvgError() + "% Size:" + Size());
            Console.WriteLine("}");
        }
        public void PrintShort()
        {
            Console.WriteLine(ts.Length + "\t" + String.Format("{0:00.00}", Error(0.5)) + "% " + "\t" + String.Format("{0:00.00}", Error(Global.confidence)) + "% " + "\t" + String.Format("{0:00.00}", MaxError()) + "%\t" + Size());
        }
        public void Save()
        {
            utils.File.WriteData("c:/data/d1", ts.freq[0], ts.data, trend, seasonal.ts.data, errors);
        }
        #endregion
        public virtual void Set()
        {            
            id = Global.id;
            Global.id++;
            if (seasonal != null) seasonal.Set();           
        }
        public string ToString(string h)
        {
            String s = h + "len:" + len + "freq: " + freq + " Values:";
            if (values != null)
            {
                for (int i = 0; i < values.Length; i++)
                    s = s + values[i] + " " + values[i];
            }
            else s = s + "null";
            s = s + "\n" + h;
            if (seasonal != null)
                s = s + "\tseasonal: " + seasonal.ToString("");
            else s = s + "\tseasonal: null";
            s = s + "\n" + h;
            if (seasonal == null && values == null)
                s = s + "ts:" + ts.ToString();
            s = s + "\n";
            return s;
        }
        public int Type()
        {
            if (type == ModelType.Explicit) return 0;
            if (type == ModelType.Trend) return 1;
            if (type == ModelType.Trend) return 2;
            return 3;
        }
        public string Serialize()
        {
            string m = "";
            string s = "" + "";
            string c = "";
            string v = "";

            int l = 0;
            if (id == 9171)
                m = m;
            int l_ts = 0;
            int c_count = 0;

            if (values != null) l = values.Length;
            if (ts != null) l_ts = ts.Length;
            if (seasonal == null) s = "-1";
            else s = "" + seasonal.id;
            c = "" + "0"; c_count = 0;


            string ts_ = "" + l_ts + " ";
            v = l + " ";
            m = "" + id + " " + Type() + " " + -1 + " " + "" + error + " " + "" + freq;
            for (int i = 0; i < l; i++) v += "" + values[i] + " ";
            for (int i = 0; i < l_ts; i++) ts_ += "" + ts.data[i] + " ";

            if (seasonal != null)
            {
                if (Global.ht.ContainsKey(seasonal.id) == false)

                    Global.ht.Add(seasonal.id, seasonal.Serialize());
            }

            return m + " " + s + " " + v + " " + ts_ + " " + c;
        }

/*        public string Serialize()
        {
            string m = "";
            string s = "";
            string v = "";

            int l = 0;
            int l_ts = 0;

            if (values != null) l = values.Length;
            if (ts != null) l_ts = ts.Length;
            if (seasonal == null) s = "-1";
            else { s = "" + seasonal.id;
                if(Global.ht.ContainsKey(seasonal.id)==false)
            Global.ht.Add(seasonal.id, seasonal.Serialize());
            }

            string ts_ = "" + l_ts + " ";
            v = l + " ";
            m = "" + id + " " + Type() + " " + len + " " + "" + error + " " + "" + freq;
            for (int i = 0; i < l; i++) v += "" + values[i] + " ";
            for (int i = 0; i < l_ts; i++) ts_ += "" + ts.data[i] + " ";

            return m + " " + s + " " + v + " " + ts_;
        }
  */      
    }
    class ModelSet
    {
        TimeSeries ts;
        public Model[] models;
        double[] errors;
        int len;
        public void getModels(int len, double error, ref int length)
        {
            length = 0;
            int kk = (int)Math.Floor((double)ts.Length / len);

            models = new Model[kk];
            int x = 0;
            this.len = len;
            for (int i = 0; i < kk; i++)
            {
                if (i == kk - 1) len = ts.Length - x;
                int s = x;
                double[] u = new double[len];
                for (int j = 0; j < len; j++) u[j] = ts.data[x++];
                TimeSeries t = new TimeSeries(u, ts.freq);
                Model m = new Model(t);
                length += m.countError(error);
                m.start = s;
                models[i] = m;
            }

        }

        public ModelSet(TimeSeries ts)
        {
            this.ts = ts;
        }
        public void Solve()
        {
            if (models == null) return;
            for (int i = 0; i < models.Length; i++)
            {
                models[i].Solve();
            }
        }
        public double Eval(int x)
        {
            if (models == null) throw new Exception("Empty");
            int llen = len;
            int l = x / len;
            if (l >= models.Length)
            {
                l = models.Length - 1;
                llen = models[l].len;
            }

            return models[l].Eval(x % llen);
        }
        public int Size()
        {
            if (models == null) throw new Exception("Empty");
            int pieces = models.Length;

            int size = 0;
            for (int i = 0; i < pieces; i++)
            {
                size += models[i].Size();
            }
            return size;
        }
        public Range errorRange()
        {
            errors = new double[ts.Length];
            for (int i = 0; i < ts.Length; i++)
            {
                errors[i] = (ts.data[i] - Eval(i));// ts.data[i] * 100;
            }
            Array.Sort(errors);

            return new Range(errors[0], errors[ts.Length - 1]);
        }
        private void CalcError()
        {
            errors = new double[ts.Length];
            for (int i = 0; i < ts.Length; i++)
            {
                errors[i] = Math.Abs(ts.data[i] - Eval(i));// ts.data[i] * 100;
            }
            Array.Sort(errors);
        }
        public double AvgError()
        {
            CalcError();
            double error = 0;
            for (int i = 0; i < ts.Length; i++)
            {
                error += errors[i];
            }

            return error / ts.Length;
        }
        public double MaxError()
        {
            CalcError();

            return errors[ts.Length - 1];
        }
        public double Error(double x)
        {
            CalcError();

            return errors[(int)(x * (ts.Length - 1))];
        }
        public void PrintShort()
        {
            Console.WriteLine(ts.Length + "\t" + String.Format("{0:00.00}", Error(0.5)) + "% " + "\t" + String.Format("{0:00.00}", Error(Global.confidence)) + "% " + "\t" + String.Format("{0:00.00}", MaxError()) + "%\t" + Size());
        }
        public double Cost()
        {
            return Size() * Error(Global.confidence);
        }

    }
    public class BuildModel
    {
        static public TimeSeries ts;

        static public ModelTree BTree(double[] error_level)
        {
            int period = ts.freq[ts.freq.Length - 1];
            int n = ts.Length / period;
            ArrayList ranges = new ArrayList();
            for (int i = 0; i < n; i++)
            {
                Range r = new Range(i * period, (i + 1) * period - 1);
                r.computed = 1;
                r.matched=1; 
                ranges.Add(r);
            }
            ArrayList all_ranges = new ArrayList();
            //all_ranges.Add(ranges); 
            ArrayList nr;
            foreach (double i in error_level)
            {
                nr = Level(ranges, i);
                all_ranges.Add(nr);
                ranges = nr;
            }
            ArrayList top = (ArrayList)all_ranges[all_ranges.Count - 1];
            if (top.Count == 0)
            {
                top.Add(new Range(0, ts.Length - 1));
                all_ranges[all_ranges.Count - 1] = top;
            }
            top = (ArrayList)all_ranges[all_ranges.Count - 1];
            if (top.Count != 1)
            {                
                ArrayList a=new ArrayList();
                a.Add(new Range(0, ts.Length - 1));
                all_ranges.Add(a);
            }
            RTree rt = new RTree(all_ranges,ts);
            return rt.root.t;

        }

        static ArrayList combine(ArrayList ranges)
        {
            ArrayList r = new ArrayList();
            int i;
            int last_added = 0;
            for (i = 0; i < ranges.Count - 1; i = i + 2)
            {
                Range r1 = (Range)ranges[i];
                Range r2 = (Range)ranges[i + 1];
                last_added = i + 1;
                Range t = Range.Combine(r1, r2);
                //if
                //    r.Add(t);
                //else { r.Add(r1); r.Add(r2); }
                 if (r1.matched == 1 && r2.matched == 1)
                
                     if(t!=null)r.Add(t);
            }
            // I am not sure if this is ever needed but it would not harm
            if (last_added < ranges.Count - 1)
                r.Add(ranges[last_added + 1]);
            return r;
        }
        static ArrayList Level(ArrayList ranges, double errror_level)
        {
            ArrayList tss = null;
            ArrayList R = new ArrayList();
            foreach (Range r in ranges)
                R.Add(new Range(r.s, r.e));
            ArrayList X = ranges;

            // should ont use computed and matched
            for (; ; )
            {
                int count = 0;
                tss = ts.Divide(X);
                foreach (TimeSeries t in tss)
                {
                    if (t.r.computed == 1) {if (t.r.matched== 1) count++; continue; }
                    t.r.computed = 1;
                    t.r.matched = 0;
                    Model m;
                    if(Global.quick)
                        m=Model.ModelQuick(t);
                    else
                     m = new Model(t);// 
                    if (m.error < errror_level)
                    {
                        t.r.matched = 1;
                        bool ignore = false; // may be also be called as do not add to the 
                        count++;
                        //remove ranges
                        ArrayList todel = new ArrayList();

                        foreach (Range r in R)
                        {
                            if ((t.r.overlap(r)) && ((int)t.r.len != (int)r.len)) todel.Add(r);
                            else if (t.r.overlap(r)) ignore = true;
                        }
                        foreach (Range r in todel)
                            R.Remove(r);
                       if(ignore==false) R.Add(t.r);
                    }
                }
                if (count == 0) break;
                //if (count == 1) break;
               X.Sort();
               X = combine(X);
                // we need to deset matched or not 
            }
            return R;
        }
    
    }
    public class ModelTree : Model, IComparable
    {
        public ModelTree[] children;
        public ModelTree parent;
        public ArrayList childs= new ArrayList();
        public Range range;
        public void convertchildstochildren()
        {
            if (childs.Count > 0)
            {
                children = new ModelTree[childs.Count];
                int j = 0;
                childs.Sort();
                foreach (ModelTree t in childs)
                {
                    children[j] = t;
                    j++;
                }
            }
        }
        public void removeChild(int id_)
        {
            for (int i = 0; i < childs.Count; i++)
            {
                ModelTree child = (ModelTree)childs[i];
                if (child.id == id_) childs.Remove(child);
            }
            convertchildstochildren();
                
        }
        public void Improvetree()
        {
            foreach (ModelTree t in childs)
            {

                if (t.error > error)
                { // the model is not needed so clean every then
                    t.Clean();
                    t.ts = null;
                    t.values = null;
                    t.seasonal = null;

                    t.Improvetree();
                }
            }  
          
        }

        public virtual void Set()
        {
            base.Set();
            if (children != null)
                foreach (ModelTree c in children) c.Set();
        }
        void setModels(int len, double[] errors, int shift, int done)
        {
            double[] newerrors = null;
            if (errors.Length > 1)
            {
                newerrors = new double[errors.Length - 1];
                for (int i = 0; i < errors.Length - 1; i++)
                {
                    newerrors[i] = errors[i];
                }
            }
            int kk = (int)Math.Floor((double)ts.Length / len);

            children = new ModelTree[kk];
            int x = 0;

            for (int i = 0; i < kk; i++)
            {
                if (i == kk - 1) len = ts.Length - x;
                int s = x;
                double[] u = new double[len];
                for (int j = 0; j < len; j++) u[j] = ts.data[x++];
                TimeSeries t = new TimeSeries(u, ts.freq);
                ModelTree m = null;
                if (shift == 1)
                    m = new ModelTree(t, newerrors, s);
                else
                    m = new ModelTree(t, errors, s);
                if (done == 0)
                {
                    m.BuildTree();
                }
                children[i] = m;
                m.start = s;
            }
        }
        public void BuildTree()
        {
            if (errors == null) return;
            int i = 0;
            int max_branching = int.MaxValue;
            double current_error = errors[errors.Length - 1];
            int f = 0;
            int length = ts.Length;
            int len = 0;
            int best_f = 0;
            double best_cost = double.MaxValue;

            for (i = 0; i < ts.freq.Length; i++)
            {
                f = ts.freq[i];
                if (f == 0) continue;
                for (; ; )
                {
                    ModelSet s = new ModelSet(ts);
                    s.getModels(f, current_error, ref len);
                    //
                    if ((len >= Global.confidence * length) && (s.models.Length <= max_branching))
                    {
                        double cost = s.Cost();
                        if (cost < best_cost)
                        {
                            best_f = f;
                            best_cost = cost;
                        }

                    }
                    else break;

                    f = f * 2;
                    if (len == 0) break;
                    if ((i + 1 < ts.freq.Length) && (f > ts.freq[i + 1])) break;
                }
            }

            Console.WriteLine(best_f);
            if (best_f != 0) { setModels(best_f, errors, 1, 0); }
            else
            {
                int done = 0;
                for (; ; )
                {
                    ModelSet s = new ModelSet(ts);
                    s.getModels(f, current_error, ref len);
                    if (s.models.Length >= max_branching)
                    {
                        f = f * 2;
                    }
                    else { best_f = f; if (s.models.Length == 1) done = 1; break; }
                }

                setModels(best_f, errors, 0, done);
            }
            //Set(); 
        }
        public ModelTree() { }
        public ModelTree(TimeSeries ts, double[] errors = null, int start = 0)
            : base(ts)
        {
            this.range = new Range(start, start + ts.Length - 1);
            this.errors = errors;
            this.children = null;
        }
        public override int Size()
        {
            int size = base.Size();
            size += 2;

            return size;
        }
        public override void Clean()
        {
            base.Clean();
            if (children != null)
            {
                foreach (ModelTree m in children) m.Clean();
            }
        }
        public void ToFile(StreamWriter sw, int indent = 0)
        {
            string h = indent + " ";
            string hh = indent + " ";
            for (int i = 0; i < indent; i++) { h += "\t"; hh += "\t"; }
            int k = 0;
            if (children != null)
            {
                k = children.Length;
            }
            string s = h + "Model Error:" + error + range.ToString() + " " + Size() + " " + type + " " + k + " ";
            //string s_mo =  "\t" + this.ToString(hh)+ "\n";
            //s =  s + s_mo;
            sw.WriteLine(s);
            if (children == null)
            {
                return;
            }

            for (int i = 0; i < k; i++)
            {
                children[i].ToFile(sw, indent + 1);
            }

        }

      /*  public new void Serialize()
        {
            string c = "";
            int c_count = 0;
            if (children == null) { c = "" + "0"; c_count = 0; }
            else { c = "" + children.Length + " "; c_count = children.Length; }

            for (int i = 0; i < c_count; i++) c += "" + children[i].id + " ";

            if (seasonal != null)
            {                
                    seasonal.Serialize();
            }
            string to_str = base.Serialize() + " " + c;
            Global.ht.Add(this.id, to_str);
        }*/
        public string Serialize()
        {
            string m = "";
            string s = "";
            string c = "";
            string v = "";

            int l = 0;
            int l_ts = 0;
            int c_count = 0;

            if (values != null) l = values.Length;
            if (ts != null) l_ts = ts.Length;
            if (seasonal == null) s = "-1";
            else s = "" + seasonal.id;
            if (children == null) { c = "" + "0"; c_count = 0; }
            else { c = "" + children.Length + " "; c_count = children.Length; }

            string ts_ = "" + l_ts + " ";
            v = l + " ";
            m = "" + id + " " + Type() + " " + len + " " + "" + error + " " + "" + freq;
            for (int i = 0; i < l; i++) v += "" + values[i] + " ";
            for (int i = 0; i < l_ts; i++) ts_ += "" + ts.data[i] + " ";
            for (int i = 0; i < c_count; i++) c += "" + children[i].id + " ";

            if (seasonal != null)
            {
                string ss=seasonal.Serialize();
                if (Global.ht.ContainsKey(seasonal.id) == false)
                    Global.ht.Add(seasonal.id, ss);
            }
            return m + " " + s + " " + v + " " + ts_ + " " + c;
        }
        public void SerializeAll()
        {
            string ss=this.Serialize();
            if(Global.ht.ContainsKey(id)==false)
            Global.ht.Add(this.id, ss);
            if (children != null)
                foreach (ModelTree m in children) m.SerializeAll();
        }

        public double EvalProb(int x, double err)
        {
            double e = base.error;
            double y = Eval(x);// no need to compute the value

            //	printf("Totalhe  %lf  error %lf\n",y,e);
            if (err > error) return y; // found result within the error
            //find child

            //Model* m = (Model*)&(models[j]);
            if (this.children == null) return -1;
            if (this.children.Length <= 0) return -1;
            Model mm = this.children[0];
            int llen = mm.len;

            int li = x / llen;

            if (li >= this.children.Length)
            {
                li = this.children.Length - 1;
                mm = this.children[li];
                llen = mm.len;
            }

            //	printf("l %d li %d\n",l,li);
            return children[li].EvalProb(x % llen, err);
        }

        public int CompareTo(object obj)
        {
            ModelTree tt=(ModelTree) obj;

            return this.range.CompareTo(tt.range);
        }

    }
}
