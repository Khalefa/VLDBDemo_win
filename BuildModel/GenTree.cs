using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using System.Xml.Serialization;
namespace ModelGen

{
   public  class GenTree
    {
        public static int[] freq = { 17520, 24 * 4};
        public static double[] errors = {700, 1000, 2000 };
        /*public static string dir = "C:/VLDBDemo_win/data/d/";
        public static string file = "uk3.txt";*/
        public static string dir = "C:/VLDBDemo_win/data/n/org/";
        public static string file = "uk.txt";
        static void Serlialize(int st, ModelTree t, double[] uk)
        {
            bool append = st > 0;
            StreamWriter sw = new StreamWriter(dir + "m.txt", append);

            t.ToFile(sw);
            sw.Close();
            Global.id = 0;
            t.Set();
            StreamWriter mm = new StreamWriter(dir + "b.txt");
            t.SerializeAll();
            mm.WriteLine(Global.id);
            for (int i = 0; i < Global.id; i++)
                mm.WriteLine(Global.ht[i]);
            mm.Close();

        }
        
        static public void BuildNiceTree()
        {
            ModelTree t = buildLevel0();

            double[] data = new double[t.len];
            t.Eval(10000);
            t.EvalProb(10000, 12);
            t.EvalProb(10000, 1);
            for (int i = 0; i < t.len; i++) data[i] = t.EvalProb(i, 1);

            Serlialize(0, t, data);
        }

        static ModelTree buildLevel0()
        {
            Global.id = 0;
            Model m = new Model();
            m.id = Global.id++;
            m.len = 1000 * 10 * 100;
            m.freq = 1000 * 10;
            m.error = 100;
            m.type = ModelType.Explicit;
            m.values = new double[100];
            Random r = new Random();
            for (int i = 0; i < 10; i++)
                m.values[i] = r.Next(100);
            ModelTree t = m.construct();
            t.seasonal = SetSeasonal();
            t.children = buildLevel1();

            return t;
        }
        static ModelTree[] buildLevel1()
        {
            ModelTree[] ms = new ModelTree[10];
            for (int j = 0; j < 10; j++)
            {
                Model m = new Model();
                m.id = Global.id++;
                m.len = 1000 * 10;
                m.freq = 1000; //leaf node
                m.error = 10;
                m.type = ModelType.Explicit;
                m.values = new double[10];
                Random r = new Random();
                for (int i = 0; i < 10; i++)
                    m.values[i] = r.Next(100);

                //children
                ms[j] = m.construct();
            }
            Model seasonal = SetSeasonal();
            for (int j = 0; j < 10; j++)
            {
                ModelTree m = ms[j];
                m.seasonal = seasonal;
                m.children = buildLevel2();
            }

            return ms;
        }
        static ModelTree[] buildLevel2()
        {
            ModelTree[] ms = new ModelTree[10];
            for (int j = 0; j < 10; j++)
            {
                Model m = new Model();
                m.id = Global.id++;
                m.len = 1000;
                m.freq = 100; //leaf node
                m.error = 10;
                m.type = ModelType.Explicit;
                m.values = new double[10];
                Random r = new Random();
                for (int i = 0; i < 10; i++)
                    m.values[i] = r.Next(100);

                //children
                m.seasonal = null;
                ms[j] = m.construct();
            }

            return ms;
        }
        static Model SetSeasonal()
        {
            Model s = new Model();
            s.id = Global.id++;
            s.freq = 100;
            s.values = new double[100];
            for (int i = 0; i < 50; i++) { s.values[i] = 2 * i; s.values[99 - i] = i; }
            s.len = -1;
            s.seasonal = SetbaseSeasonal();

            return s;
        }
        static Model SetbaseSeasonal()
        {
            double[] x = new double[100];
            Random r = new Random();
            for (int i = 0; i < 100; i++) x[i] = r.Next(100);
            int[] f = { 100, 20 };
            TimeSeries ts = new TimeSeries(x, f);
            Model s = new Model();
            s.ts = ts;
            s.len = -1;
            s.id = Global.id++;
            return s;
        }

        public static void build( int st, int n)
        {
            double[] uk = utils.File.ReadData(dir + file, st, n);
            TimeSeries ts = new TimeSeries(uk, freq);
            BuildModel.ts = ts;
            ModelTree t= BuildModel.BTree(errors);
          /*  ModelTree  t= new ModelTree(ts, errors,0);
            t.BuildTree();
            t.Clean();*/
            Global.id = 0;
            t.Set();
            
            //t.Improvetree();
            Global.id = 0;
            t.Set();
            Serlialize(st, t, uk);
        }

        
    }
}
