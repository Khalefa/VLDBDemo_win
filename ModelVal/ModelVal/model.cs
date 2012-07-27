using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ModelVal
{
    public enum ModelType { EXPLICIT = 0, IMPLICIT = 2, TREND = 1 };

    class Model
    {
        /* this file reads a model from disk and passes the result to the scanner*/
        /* this file reads a model from disk and passes the result to the scanner*/
        static Model[] models;
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
        double EvalProb( int x, double err)
        {
            double error = 0;
            double y = Eval(x, ref error);// no need to compute the value
            //elog(WARNING, "Model error%f requested error %f",error,err);
            if (err > error) return y; // found result within the error
            //elog(WARNING,"here");
            
            Model mm=null;

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
        static int ReadInt(TextReader f) {
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
             
            if(ti==0) m.type=ModelType.EXPLICIT;
            if(ti==1) m.type=ModelType.TREND;
            if(ti==2) m.type=ModelType.IMPLICIT;
            
            ti = ReadInt(f);            
            m.len = ti;

            tf=ReadDouble(f);
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
            for ( i = 0; i < p.Length; i++)
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

        public static void LoadModules()
        {
            //   /home/khalefa/model/uk2.b
            TextReader f = new StreamReader("C:/VLDBDemo_win/data/n/org/b.txt");
            int n, i;
            n = ReadInt(f);
            
            models = new Model[n];

            for (i = 0; i < n; i++)
            {
                models[i]= ReadModel(f, i);
            }
            f.Close();
            //	for(i=0;i<10;i++)
            //	elog(WARNING,"i %d v %f", i, GetValue(i));
        }

    }
}
