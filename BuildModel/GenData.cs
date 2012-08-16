using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ModelGen
{
    class GenData
    {
      public  static  void GenUK() {
            //first read ukc
            int[] freq = { 17520};
            double[] uk = utils.File.ReadData("c:/data/ukc.txt");
            TimeSeries ts = new TimeSeries(uk, freq);
            Model m = new Model(ts);
            double []errors = new double[m.trend.Length];
            for (int i = 0; i < m.trend.Length; i++)
            {
                errors[i] = ts.data[i] - m.Eval(i);
            }
            double[] t = new double[m.trend.Length * 100];
            double[] s = new double[m.trend.Length * 100];
           double[] e = new double[m.trend.Length * 100];
          Random r = new Random();
            for (int i = 0; i < m.trend.Length ; i++)
            {
                for(int j=0;j<100;j++)
                t[j*m.trend.Length+  i] = m.trend[i];
            }
            for (int i = 0; i < m.trend.Length*100; i++)
            {
                double mult = 1;
                if ((i < m.trend.Length * 10) && (i > 0)) mult = 1.1;
                if ((i < m.trend.Length * 20) && (i > m.trend.Length * 10)) mult = 1.1;
                if ((i < m.trend.Length * 30) && (i > m.trend.Length * 20)) mult = 1.0;
                if ((i < m.trend.Length * 40) && (i > m.trend.Length * 30)) mult = 1.4;
                if ((i < m.trend.Length * 50) && (i > m.trend.Length * 40)) mult = 1.2;
                if ((i < m.trend.Length * 60) && (i > m.trend.Length * 50)) mult = 1.3;
                if ((i < m.trend.Length * 70) && (i > m.trend.Length * 60)) mult = 1.5;
                if ((i < m.trend.Length * 80) && (i > m.trend.Length * 70)) mult = 1.2;
                if ((i < m.trend.Length * 90) && (i > m.trend.Length * 80)) mult = 1.1;
                if ((i < m.trend.Length * 100) && (i > m.trend.Length * 90)) mult = 1.0;
                s[i] = m.seasonal.Eval(i % freq[0])*mult;
            }
            for (int i = 0; i < m.trend.Length * 100; i++)
            {
                e[i] = errors[i % m.trend.Length]*(r.NextDouble()/2+0.5)*0.001;
            }
            StreamWriter sw = new StreamWriter("c:/data/n/uk3.txt");
            for (int i = 0; i < m.trend.Length * 30; i++)
            {
                double tt=t[i] + s[i] + e[i];
                   sw.WriteLine((int)tt);
            }
             sw.Close();
      
        }
      public static void GenRandom()
      {
          //first read ukc
          int[] freq = { 100 };
          double[] s = new double[100];
          Random r = new Random();
          double x=0;
          for (int i = 0; i < 50; i++)
          {
              x = x + r.Next(10);
              s[i] = x;
              s[99 - i] = x;
          }

          double[] t = new double[10000];
          Random rr = new Random();
          Random rx = new Random();
          x=0;
          for (int i = 0; i < 10000; i++)
          {

              t[i] = rr.Next(1000);
          }

          StreamWriter sw = new StreamWriter("C:/VLDBDemo_win/data/random.txt");
          for (int i = 0; i < 10000; i++)
          {
              for (int j = 0; j < 100; j++)
              {
                  double tt = t[i] + s[j];
                  sw.WriteLine((int)tt);
              }
          }
          sw.Close();

      }
      void GenQuery()
      {
          /*double []d=utils.File.ReadData("C:/VLDBDemo_win/data/r/random.txt", 0, 100);
          StreamWriter sr = new StreamWriter("C:/VLDBDemo_win/data/r/random.sql");
          for (int i = 0; d.Length; i++)
          {
              for (int j = 0; j < 100; j++)
              {
                  double tt = t[i] + s[j];
                  sw.WriteLine((int)tt);
              }
          }
          sw.Close();*/

      }
    }
}
