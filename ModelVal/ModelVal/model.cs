using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Collections;
namespace ModelVal
{
    public enum ModelType { EXPLICIT = 0, IMPLICIT = 2, TREND = 1 };

    class Model
    {
        /* this file reads a model from disk and passes the result to the scanner*/
        public static Model[] models;
        public static double error_level;
        static int n;
        ModelType type;
        int id;
        int seasonal;
        int freq;
        int len;
        double err;
        int nv;
        double[] values; //length is in len

        int nts;
        double[] ts;

        int nc;
        int[] children;
        int sparent;// (needed for seasonality compoments)
        int parent;// (needed for children)
        #region helper
        public virtual int Size()
        {
            if (seasonal == -1 && nv == 0) return ts.Length;
            if (seasonal == -1) return 2;
            return values.Length + models[seasonal].Size();
        }
        public int overallsize()
        {
            int size = this.Size();
            if (this.nc > 0) for (int i = 0; i < nc; i++) size += models[children[i]].overallsize();
            return size;
        }
        public int overallsize(int layers)
        {
            int size = this.Size();
            if (layers > 0)
                if (this.nc > 0) for (int i = 0; i < nc; i++) size += models[children[i]].overallsize(layers - 1);
            return size;
        }
        void PrintModel()
        {
            int i;
            Console.Write("---------------------\n");
            Console.Write("Id%d Type%d  len%d  Err%f freq%d\n", id, type, len, err, freq);
            Console.Write("Values(%d):", nv);
            for (i = 0; i < nv; i++) Console.Write("%f ", values[i]);
            Console.Write("\nTs(%d):", nts);
            for (i = 0; i < nts; i++) Console.Write("%f ", ts[i]);
            Console.Write("\nChildren(%d):", nc);
            for (i = 0; i < nc; i++) Console.Write("%d ", children[i]);
            Console.Write("\nSeasonal:%d", seasonal);

            Console.Write("---------------------\n");
        }
        static int getParent(int i)
        {
            int p = models[i].sparent;
            Debug.Assert(p != -1);

            if (models[p].len == -1) return getParent(p);
            else return p;
            //return -1;
        }
        static int getParent(Model pa)
        {
            int p = pa.sparent;
            Debug.Assert(p != -1);

            if (models[p].len == -1) return getParent(p);
            else return p;
            //return -1;
        }

        #endregion
        #region Eval
        // this is different than EvalProb(x, error) as it uses the model (i.e., it does not traverse the tree)
        double Eval(int x, ref double error)
        {
            double t, s;
            double xx, tt;
            //DModel *m= (DModel*)&(models[j]);
            if (seasonal == -1 && nv == 0)
            {
                return ts[x];
            }
            if (seasonal == -1)
            {
                return values[0] * x + values[1];
            }

            t = s = 0;
            if (type == ModelType.EXPLICIT) t = values[x / freq];
            else if (type == ModelType.IMPLICIT)
                t = values[0] * x / freq + values[1];
            else t = values[0] * x + values[1];
            Model ms = models[seasonal];
            tt = 0;
            s = ms.Eval(x % freq, ref tt);
            xx = s + t;
            //	Console.Write("Total  %lf\n",xx);
            error = err;
            return xx;
        }
        // this is different than EvalProb(x, error) as it uses the model (i.e., it does not traverse the tree)
        double EvalS(int x)
        {
            double s, tt;
            tt = s = 0;
            Model ms = models[seasonal];
            s = ms.Eval(x % freq, ref tt);
            return s;
        }
        double EvalProb(int x, double err)
        {
            double error = 0;
            double y = Eval(x, ref error);// no need to compute the value
            //elog(WARNING, "Model error%f requested error %f",error,err);
            if (err > error) return y; // found result within the error
            //elog(WARNING,"here");

            Model mm = null;

            if (nc <= 0) return y;
            int l = 0;
            l = children[0];
            mm = models[l];
            int llen = mm.len;

            int li = x / llen;

            if (li >= nc)
            {
                li = nc - 1;
                l = children[li];
                mm = models[l];

                llen = mm.len;
            }
            l = children[li];
            //	Console.Write("l %d li %d\n",l,li);
            Model c = models[l];
            return c.EvalProb(x % llen, err);
        }
        double GetValue(int x)
        {
            return EvalProb(x, error_level);
        }
        public static double GetValue(int i, int x)
        {
            return models[i].GetValue(x);
        }
        #endregion
        #region read model
        public static void LoadModules(string filename)
        {
            //   /home/khalefa/model/uk2.b
            TextReader f = new StreamReader(filename);
            int n, i;
            n = ReadInt(f);

            models = new Model[n];

            for (i = 0; i < n; i++)
            {
                models[i] = ReadModel(f, i);
                models[i].sparent = -1;
            }
            // experiemts
            Console.WriteLine(models[0].Size() + "\t" + models[0].overallsize(0));
            Console.WriteLine(models[0].Size() + "\t" + models[0].overallsize(1));
            Console.WriteLine(models[0].Size() + "\t" + models[0].overallsize(2));
            Console.WriteLine(models[0].Size() + "\t" + models[0].overallsize(3));
            Console.WriteLine(models[0].Size() + "\t" + models[0].overallsize(4));
            Console.WriteLine(models[0].Size() + "\t" + models[0].overallsize(5));
            Console.WriteLine(models[0].Size() + "\t" + models[0].overallsize(6));

            f.Close();
            // make another path to assign the parents
            for (i = 0; i < n; i++)
            {
                int s = models[i].seasonal;
                if (s != -1) models[s].sparent = i;
            }
            for (i = 0; i < n; i++)
            {
                Model m = models[i];
                for (int j = 0; j < m.nc; j++)
                {
                    models[m.children[j]].parent = i;
                }
            }
        }
        static string ReadString(System.IO.TextReader f)
        {
            string s = "";
            char c;
            for (; ; )
            {
                c = (char)f.Read();
                if (c == ' ') break;
                if (c == '\n') break;
                if (c == '\r') continue;
                s = s + c;
            }
            // s = s.Trim();
            Console.WriteLine(s);
            return s;
        }
        static int ReadInt(TextReader f)
        {
            string s = ReadString(f);
            return int.Parse(s);
        }
        static double ReadDouble(TextReader f)
        {
            return double.Parse(ReadString(f));
        }
        static int ReadInt(string s)
        {
            return int.Parse(s);
        }
        static double ReadDouble(string s)
        {
            return double.Parse(s);
        }
        static Model OldReadModel(TextReader f, int j)
        {
            Model m = new Model();

            int ti;
            double tf;
            int i;
            ti = ReadInt(f);
            m.id = ti;
            ti = ReadInt(f);

            if (ti == 0) m.type = ModelType.EXPLICIT;
            if (ti == 1) m.type = ModelType.TREND;
            if (ti == 2) m.type = ModelType.IMPLICIT;

            ti = ReadInt(f);
            m.len = ti;

            tf = ReadDouble(f);
            m.err = tf;

            ti = ReadInt(f);
            m.freq = ti;

            ti = ReadInt(f);
            m.seasonal = ti;

            //read v
            ti = ReadInt(f);
            m.nv = ti;
            m.values = new double[m.nv];
            for (i = 0; i < m.nv; i++)
            {
                tf = ReadDouble(f);
                m.values[i] = tf;
            }
            //read ts
            ti = ReadInt(f);
            m.nts = ti;
            m.ts = new double[m.nts];
            for (i = 0; i < m.nts; i++)
            {
                tf = ReadDouble(f);
                m.ts[i] = tf;
            }
            //read c
            ti = ReadInt(f);
            m.nc = ti;
            m.children = new int[m.nc];
            for (i = 0; i < m.nc; i++)
            {
                ti = ReadInt(f);
                m.children[i] = ti;
            }
            ReadString(f);
            return m;
        }
        static Model ReadModel(TextReader f, int j)
        {
            Model m = new Model();
            int i;
            string line = f.ReadLine();
            string[] p = line.Split(' ');
            int count = 0;
            for (i = 0; i < p.Length; i++)
            {
                if (p[i] != "") count++;
            }
            string[] parts = new string[count];
            int k = 0;
            for (i = 0; i < p.Length; i++)
            {
                if (p[i] != "") { parts[k] = p[i]; k++; }
            }

            int ti;
            double tf;
            k = 0;
            ti = ReadInt(parts[k++]);
            m.id = ti;
            ti = ReadInt(parts[k++]);

            if (ti == 0) m.type = ModelType.EXPLICIT;
            if (ti == 1) m.type = ModelType.TREND;
            if (ti == 2) m.type = ModelType.IMPLICIT;

            ti = ReadInt(parts[k++]);
            m.len = ti;

            tf = ReadDouble(parts[k++]);
            m.err = tf;

            ti = ReadInt(parts[k++]);
            m.freq = ti;

            ti = ReadInt(parts[k++]);
            m.seasonal = ti;

            //read v
            ti = ReadInt(parts[k++]);
            m.nv = ti;
            m.values = new double[m.nv];
            for (i = 0; i < m.nv; i++)
            {
                tf = ReadDouble(parts[k++]);
                m.values[i] = tf;
            }
            //read ts
            ti = ReadInt(parts[k++]);
            m.nts = ti;
            m.ts = new double[m.nts];
            for (i = 0; i < m.nts; i++)
            {
                tf = ReadDouble(parts[k++]);
                m.ts[i] = tf;
            }
            //read c
            ti = ReadInt(parts[k++]);
            m.nc = ti;
            m.children = new int[m.nc];
            for (i = 0; i < m.nc; i++)
            {
                ti = ReadInt(parts[k++]);
                m.children[i] = ti;
            }
            //ReadString(f);
            return m;
        }
        #endregion
        #region Similairaity
        // must everything be the same, 
        static bool comaprable(Model a, Model b)
        {
            // may need to add len and frequency too
            if (b.len != a.len) return false;
            if (a.type != b.type) return false;
            if ((a.seasonal == -1) && (b.seasonal == -1) && (a.nv == 0) && (b.nv == 0)) return true;
            if ((a.seasonal == -1) && (b.seasonal == -1)) return true;
            if ((a.seasonal != -1) && (b.seasonal != -1) && (a.nv > 0) && (b.nv > 0)) return true;
            return false;

        }
        static double simSeasonalOLD(Model a, Model b)
        {
            Debug.Assert(a.len == -1);
            Debug.Assert(b.len == -1);
            Debug.Assert(b.freq == a.freq);
            double sim_index;
            double a_scale, a_shift;
            double b_scale, b_shift;
            a_scale = 0; a_shift = 0;
            b_scale = 0; b_shift = 0;
            Model pa = models[getParent(a)];
            Model pb = models[getParent(b)];
            double[] aV = new double[pa.len];
            double[] bV = new double[pb.len];

            for (int i = 0; i < pa.len; i++)
            {
                aV[i] = pa.EvalS(i);
                bV[i] = pb.EvalS(i);
            }
            double[] tsa = Utils.normalize(aV, ref a_scale, ref a_shift);
            double[] tsb = Utils.normalize(bV, ref b_scale, ref b_shift);
            sim_index = Utils.sim(tsa, tsb);
            return sim_index;

            return 0;

        }
        static double simSeasonal(Model pa, Model pb)
        {
            double sim_index;
            double a_scale, a_shift;
            double b_scale, b_shift;
            a_scale = 0; a_shift = 0;
            b_scale = 0; b_shift = 0;
            double[] aV = new double[pa.len];
            double[] bV = new double[pb.len];

            for (int i = 0; i < pa.len; i++)
            {
                aV[i] = pa.EvalS(i);
                bV[i] = pb.EvalS(i);
            }
            double[] tsa = Utils.normalize(aV, ref a_scale, ref a_shift);
            double[] tsb = Utils.normalize(bV, ref b_scale, ref b_shift);
            sim_index = Utils.sim(tsa, tsb);
            return sim_index;
        }
        static double sim(Model a, Model b)
        {
            Debug.Assert(comaprable(a, b) == true);
            double sim_index;
            double a_scale, a_shift;
            double b_scale, b_shift;
            a_scale = 0; a_shift = 0;
            b_scale = 0; b_shift = 0;

            // determine whihch array to use
            if ((a.seasonal == -1) && (b.seasonal == -1) && (a.nv == 0) && (b.nv == 0))
            {
                // in this case, we use the ts of a and b
                double[] tsa = Utils.normalize(a.ts, ref a_scale, ref a_shift);
                double[] tsb = Utils.normalize(b.ts, ref b_scale, ref b_shift);
                sim_index = Utils.sim(tsa, tsb);
                return sim_index;
            }
            else if ((a.seasonal == -1) && (b.seasonal == -1))
            {
                // in this case, we use the ts of a and b
                double[] tsa = Utils.normalize(a.values, ref a_scale, ref a_shift);
                double[] tsb = Utils.normalize(b.values, ref b_scale, ref b_shift);
                sim_index = Utils.sim(tsa, tsb);
                return sim_index;
            }
            else if (a.type == ModelType.EXPLICIT)
            {
                double[] tsa = Utils.normalize(a.values, ref a_scale, ref a_shift);
                double[] tsb = Utils.normalize(b.values, ref b_scale, ref b_shift);
                sim_index = Utils.sim(tsa, tsb);
                sim_index += simSeasonal(a, b);
                return sim_index;
            }
            else if ((a.type == ModelType.IMPLICIT) || ((a.type == ModelType.TREND)))
            {
                double[] tsa = Utils.normalize(a.values, ref a_scale, ref a_shift);
                double[] tsb = Utils.normalize(b.values, ref b_scale, ref b_shift);
                sim_index = Utils.sim(tsa, tsb);// sim(models[a.seasonal], models[b.seasonal]);
                sim_index += simSeasonal(a, b);
                return sim_index;
            }

            return double.MaxValue;
        }
        public static ArrayList FindSimilairty()
        {
            ArrayList ar = new ArrayList();
            for (int i = 0; i < models.Length; i++)
            {
                for (int j = i + 1; j < models.Length; j++) // can be easily improved ( by starting from i+1)
                {
                    Model mi = models[i];
                    Model mj = models[j];
                    if (mi.len == -1) continue;
                    if (mj.len == -1) continue;
                    if (Model.comaprable(models[i], models[j]) == true)
                        if (mi.len > 192)
                        {
                            SimItem sm = new SimItem();
                            sm.id1 = i;
                            sm.id2 = j;
                            sm.sim = sim(models[i], models[j]);
                            sm.len = mi.len;
                            ar.Add(sm);
                            //Console.WriteLine("{0}\t{1}\t{2}\t{3}", i, j, sm.sim, sm.len);
                        }
                }
            }
            ar.Sort();
            return ar;
        }
        public static void compressSeasonal()
        {
            for (int i = 0; i < models.Length; i++)
            {
                for (int j = i + 1; j < models.Length; j++) // can be easily improved ( by starting from i+1)
                {
                    Model mi = models[i];
                    Model mj = models[j];
                    if (mi.len != -1) continue;
                    if (mj.len != -1)
                        continue;
                    Model pa = models[getParent(mi)];
                    Model pb = models[getParent(mj)];
                    if (pa.len == pb.len)
                        Console.WriteLine("{0}\t{1}\t{2}", i, j, simSeasonal(pa, pb));

                }
            }
        }

        #endregion

        static int ReplaceModel(int org, int replacment)
        {
            int s = models[org].overallsize();

            int org_parent = models[org].parent;
            Model parent = models[org_parent];
            //change the org_parent to 
            for (int i = 0; i < parent.nc; i++)
            {
                if (parent.children[i] == org) { parent.children[i] = replacment; break; }
            }
            return s;
        }
        internal static int compress(ArrayList ar, int l, int len)
        {
            int s = 0;
            for (int i = l; i < len + l; i++)
            {
                SimItem sm = (SimItem)ar[i];
                s += ReplaceModel(sm.id1, sm.id2);
            }
            return s;
        }
        public static void GradualCompression(ArrayList s, double ratio)
        {
            int newsize = Model.models[0].overallsize();
            for (int i = 0; i < s.Count; i++)
            {
                SimItem sm = (SimItem)s[i];
                if (sm.sim / sm.len > ratio) break;
                int size = Model.compress(s, i, 1);
                newsize -= size;
                Console.WriteLine("{0}\t{1}", i, size, newsize);
            }
        }

    }
}
