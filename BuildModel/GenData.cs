using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ModelGen
{
    class GenData
    {
      public  static  void Generate() {
            //first read ukc
            int[] freq = { 17520};
            double[] uk = utils.File.ReadData("c:/data/ukc.txt");
            TimeSeries ts = new TimeSeries(uk, freq);
            Model m = new Model(ts,false);
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
                e[i] = m.errors[i % m.trend.Length]*(r.NextDouble()/2+0.5)*0.001;
            }
            StreamWriter sw = new StreamWriter("c:/data/n/uk3.txt");
            for (int i = 0; i < m.trend.Length * 30; i++)
            {
                double tt=t[i] + s[i] + e[i];
                   sw.WriteLine((int)tt);
            }
             sw.Close();
      
        }
    }
}
