using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using stllib;
using utils;

namespace ModelGen
{
    public class TimeSeries
    {
        public double []data;       
        public int[] freq;
        public Range r=null;
        public TimeSeries(int n, int[] freq)
        {
            data = new double[n];
            this.freq = freq;
        }
        public TimeSeries(double[] x, int[] freq)
        {
            int n = x.Length;
            data = new double[n];
            for (int i = 0; i < n; i++) data[i] = x[i];
            this.freq = freq;
        }
        public TimeSeries() { }
        
        public int Length
        {
            get
            {
                return data.Length;
            }
        }

        public void Print()
        {
            if(freq==null)
                Console.WriteLine("Length" + data.Length + " Freq  null");
            else
            Console.WriteLine("Length" + data.Length + " Freq " + freq[0]);
        }

        public override string ToString()
        {
            string s="";
            for(int i=0;i<data.Length;i++)
                s=s+data[i]+ " ";
            return s;
        }
        public TimeSeries(ArrayList ar, int []freq)
        {
            data = new double[ar.Count];
            for (int j = 0; j < ar.Count; j++) data[j] = (double)ar[j];
            this.freq = freq;
        }

        public TimeSeries(TimeSeries ts, Range r)
        {
            this.data = new double[(int)r.len];
            this.r = r;
            for (int i = (int)r.s; i <= (int)r.e; i++)
                data[i - (int)r.s] = ts.data[i];
            this.freq = ts.freq;
        }
        public ArrayList Divide(ArrayList ranges)
        {
            ArrayList tss = new ArrayList();
            
            foreach( Range r in ranges) {
                ArrayList tmp=new ArrayList();
                for(int i=(int)r.s;i<=(int)r.e;i++){
                    tmp.Add(data[i]);
                }
                TimeSeries ts=new TimeSeries(tmp, freq);
                    ts.r = r;
                tss.Add(ts);
            }
            return tss;
        }
        public ArrayList Divide(int length)
        {
            ArrayList tss = new ArrayList();
            ArrayList tmp= new ArrayList();
            for (int i = 0; i < Length; i++)
            {
                tmp.Add(data[i]);
                if(tmp.Count== length){
                    TimeSeries ts = new TimeSeries(tmp, freq);
                    ts.r = new Range(i-tmp.Count+1, i );
                    tss.Add(ts);
                    tmp.Clear();
                }                
            }
            if (tmp.Count > 0)
            {
                TimeSeries ts = new TimeSeries(tmp, freq);
                ts.r = new Range(Length - tmp.Count - 1, Length);
                tss.Add(ts);
            }
            return tss;
        }

    }


}
