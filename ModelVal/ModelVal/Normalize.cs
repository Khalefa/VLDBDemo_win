using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelVal
{
    // normalize an array of double
    class Utils
    {
        public static double[] normalize(double[] a, ref double scale, ref double shift)
        {
            //first get the minimim and the maximum
            double min = double.MaxValue;
            double max = double.MinValue;
            double[] r = new double[a.Length];
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] > max) max = a[i];
                if (a[i] < min) min = a[i];
            }
            scale = max - min;
            shift = min;
            if( scale > 0) 
            for (int i = 0; i < a.Length; i++)
            {
                r[i] = (a[i] - shift )/ scale;  
            } else
                for (int i = 0; i < a.Length; i++)
                {
                    r[i] = (a[i] - shift);
                }
            return r;
        }


        public static double[] denormalize(double[] a, double shift, double scale)
        {
            double[] r = new double[a.Length];
            for (int i = 0; i < a.Length; i++)
            {
                r[i] = (a[i] * scale) + shift;
            }
            
            return r;
        }
        // use a simople function for simlarity
        // a[]-b[] 
        // good simliar a=b -->0 
        // only considering array with the same lenght               
        // can return either sum prod or sum+prod or sum*prod (check and compare)
        public static double sim(double[] a, double[] b)
        {
            System.Diagnostics.Debug.Assert(a.Length==b.Length);
            double sum = 0;
            double prod=1;
            for (int i = 0; i < a.Length; i++)
            {
                sum += Math.Abs(b[i] - a[i]);
                prod *= Math.Abs(b[i] - a[i]);
            }
            return sum+prod;
        }

       

}
}
