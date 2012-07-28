using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using utils;
using System.IO;
using System.Xml.Serialization;


namespace ConsoleApplication1
{
    class Program
    {
        static string dir = "C:/VLDBDemo_win/data/n/4m/";
        static string file = "uk3.txt";
        static void computeMM(ModelType type, int[] freq, string file, string fileo, int n = int.MaxValue)
        {
            double[] uk = utils.File.ReadData(file, n);
            TimeSeries t = new TimeSeries(uk, freq);
            Model m = new Model(t);
            m.Solve();
            m.PrintShort();
            m.Save();
            try
            {
                using (StreamWriter sw = new StreamWriter(fileo))
                {
                    for (int i = 0; i < uk.Length; i++)
                        sw.WriteLine(uk[i] + "\t" + m.Eval(i));
                }
            }
            catch (Exception e)
            {
                //Let the user know what went wrong.
                Console.WriteLine("The file could not be written:");
                Console.WriteLine(e.Message);
            }
        }
        /* static void computeMS(ModelType type, int[] freq, string file, string fileo, int n = int.MaxValue)
         {
             int pieces = 50;
             double[] uk = utils.File.ReadData(file,162000-1);
             TimeSeries t = new TimeSeries(uk, freq);
             ModelSet s = new ModelSet(t, type, pieces);
             s.Solve();
            
             s.PrintShort();
          */
        /*    for (int i = 0; i < pieces; i++)
            {
                double[] u = new double[uk.Length / pieces];
                for (int j = 0; j < uk.Length / pieces; j++) u[j] = uk[i * uk.Length / pieces + j];
                TimeSeries t = new TimeSeries(u, freq);
                MModel m = new MModel(t, type);
                m.Solve();
                m.PrintShort();
            }*/

        //}

        static void computeM(string file, string fileo)
        {
            double[] uk = utils.File.ReadData(file);
            int[] freq = { 17520, 24 * 4, 0 };
            int[] freq1 = { 17520, 24 * 7 * 4 };
            int[] freq2 = { 17520 };
            int[] freq3 = { 17520, 24 * 7 * 4, 24 * 4, 0 };

            TimeSeries t = new TimeSeries(uk, freq);
            TimeSeries t1 = new TimeSeries(uk, freq1);
            TimeSeries t2 = new TimeSeries(uk, freq2);
            TimeSeries t3 = new TimeSeries(uk, freq3);

            ModelOLD m = new ModelOLD(t);
            m.Solve();
            m.PrintShort();


            ModelOLD m1 = new ModelOLD(t1);
            m1.Solve();
            m1.PrintShort();

            ModelOLD m2 = new ModelOLD(t2);
            m2.Solve();
            m2.PrintShort();

            ModelOLD m3 = new ModelOLD(t3);
            m3.Solve();
            m3.PrintShort();

            double[] d = new double[uk.Length];
            for (int i = 0; i < uk.Length; i++)
                d[i] = m.Eval(i);

            try
            {
                using (StreamWriter sw = new StreamWriter(fileo))
                {
                    for (int i = 0; i < uk.Length; i++)
                        sw.WriteLine(uk[i] + "\t" + d[i]);
                }
            }
            catch (Exception e)
            {
                // Let the user know what went wrong.
                Console.WriteLine("The file could not be written:");
                Console.WriteLine(e.Message);
            }

        }
        void a()
        {
            for (int i = 0; i < 15; i += 2)
            {
                int[] f = { 24 * 4 * 0 };

                Console.Write("Trend\t");
                computeMM(ModelType.Trend, f, "c:/data/ukc.txt", "c:/data/a_c", 17520 * (i + 1));
                Console.Write("Implict\t");
                computeMM(ModelType.Implicit, f, "c:/data/ukc.txt", "c:/data/a_c", 17520 * (i + 1));
                Console.Write("Explicit\t");
                computeMM(ModelType.Explicit, f, "c:/data/ukc.txt", "c:/data/a_c", 17520 * (i + 1));
            }
        }

        static void Serlize(int st,ModelTree t, double []uk)
        {
            //Console.WriteLine(t.ToString());
            bool append = st > 0;
            StreamWriter sw = new StreamWriter(dir + "m.txt",append);

            t.ToFile(sw);
            sw.Close();
            using (sw = new StreamWriter(dir + "printc.txt"))
            {
                for (int i = 0; i <uk.Length ; i++)
                    sw.WriteLine(i + "\t" + t.EvalProb(i, 2)+"\t"+t.EvalProb(i, 2000)+"\t"+uk[i] );
            }
            t.Clean();
            XmlSerializer x = new System.Xml.Serialization.XmlSerializer(t.GetType());
            StreamWriter m = new StreamWriter(dir + "a.xml");
            x.Serialize(m, t);

            StreamWriter mm = new StreamWriter(dir + "b.txt");
            t.SerializeAll();
            mm.WriteLine(Global.id);
            for (int i = 0; i < Global.id; i++)
                mm.WriteLine(Global.ht[i]);
            mm.Close();
          
        }
        static void build(int st, int n)
        {
            int[] freq = { 17520, 24 * 4 };
            double[] errors = {1000, 5000, 6000 };
            double[] uk = utils.File.ReadData(dir + file, st, n);
            TimeSeries ts = new TimeSeries(uk, freq);
            ModelTree t = new ModelTree(ts, errors);
            t.BuildTree();
            t.Set();
            Serlize(st, t,uk);
        }
        static void Main(string[] args)
        {
            //GenData.Generate();
            build(0, 2500000);
            //build(2500000, 2500000);*/
        }
    }
}
