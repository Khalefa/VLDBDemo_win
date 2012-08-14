using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenerateEx
{
    class Program
    {
        //paramters
        //	error
        //      scal
        // history future
        // pinterval
        //exact query																																																																																																																																																																																																																		
        static void Main(string[] args)
        {

            ex2();
        }
        static void ex1()
        {  //ex1 scalability
            int size = 1000 * 100;
            double error = 1000;
            int layers = 0;
            int interval = 1;
            for (int i = 100 * 1000; i < 2 * 1000 * 1000 + 100 * 1000; i += 100 * 1000)
            {
                size = i;
                Console.WriteLine("select a, b from mb[1,{0}] layers={1} pinterval={2};", size, layers, interval);
                Console.WriteLine("select a, b from uk3 where a <{0};", size);
            }
            size = 1000 * 1000;
            for (int i = 1; i < 6; i++)
            {
                layers = i;
                Console.WriteLine("select a, b from mb[1,{0}] layers={1} pinterval={2};", size, layers, interval);
            }
            Console.WriteLine("select a, b from uk3 where a <{0};", size);
            error = 1;
            //interval
            layers = 0;
            int[] intervals = { 1, 2, 4, 8, 16, 32, 48 };
            foreach (int v in intervals)
            {
                interval = v;
                Console.WriteLine("select a, b from mb[1,{0}] layers={1} pinterval={2} qcache=0 func=\'avg\';", size, layers, interval);
                Console.WriteLine("select a/{0}, avg(b) from uk3 where a<{1} group by a/{0};", interval, size);
            }
            for (int i = 48 * 2; i < 1600 * 1000; i = i * 2)
            {
                interval = i;
                Console.WriteLine("select a, b from mb[1,{0}] layers={1} pinterval={2} qcache=0 func=\'avg\';", size, layers, interval);
                Console.WriteLine("select a/{0}, avg(b) from uk3 where a<{1} group by a/{0};", interval, size);
            }

        }
        static void ex2()
        {
            //ex1 scalability
            int size = 1000 * 100;
            double error = 1000;
            int layers = 0;
            int interval = 100;
            for (int i = 100 * 1000; i <= 1 * 1000 * 1000; i += 100 * 1000)
            {
                size = i;
                Console.WriteLine("select a, b from mb[1,{0}] layers={1} pinterval={2};", size / interval, layers, interval);
                Console.WriteLine("select a, b from r where a <{0};", size);
            }
            size = 1000 * 1000;
            for (int i = 0; i < 6; i++)
            {
                layers = i;
                Console.WriteLine("select a, b from mb[1,{0}] layers={1} pinterval={2};", size / interval, layers, interval);
            }
            Console.WriteLine("select a, b from r where a <{0};", size);
            error = 1;
            //interval
            layers = 0;
            int[] intervals = { 1, 2, 10, 20, 50 };
            int[] Layers = { 0, 1, 2 };
            foreach (int layer_id in Layers)
            {
                layers = layer_id;
                foreach (int v in intervals)
                {
                    interval = v;
                    Console.WriteLine("select a, b from mb[1,{0}] layers={1} pinterval={2}  func=\'avg\';", size / interval, layers, interval);
                    Console.WriteLine("select a/{0}, avg(b) from r where a<{1} group by a/{0};", interval, size);
                }
                for (int i = 50 * 2; i <= 1000 * 1000; i = i * 10)
                {
                    interval = i;
                    Console.WriteLine("select a, b from mb[1,{0}] layers={1} pinterval={2}  func=\'avg\';", size / interval, layers, interval);
                    Console.WriteLine("select a/{0}, avg(b) from r where a<{1} group by a/{0};", interval, size);
                }
            }

            // with forcaset
            foreach (int layer_id in Layers)
            {
                layers = layer_id;
                foreach (int v in intervals)
                {
                    interval = v;
                    Console.WriteLine("SELECT a, b from mb[1,{0}] layers={1} pinterval={2}  func=\'avg\';", (size / interval)+10, layers, interval);
                    Console.WriteLine("select a/{0} ua, avg(b) ub from r where a<{1} group by a/{0} forecast ub on ua number 10 ;", interval, size);
                }
                for (int i = 50 * 2; i <= 1000 * 1000; i = i * 10)
                {
                    interval = i;
                    Console.WriteLine("select a, b from mb[1,{0}] layers={1} pinterval={2}  func=\'avg\';", size / interval, layers, interval);
                    Console.WriteLine("select a/{0} ua, avg(b) ub from r where a<{1} group by a/{0} forecast ub on ua number 10 ;", interval, size);
                }
            }

            foreach (int layer_id in Layers)
            {
                layers = layer_id;
                foreach (int v in intervals)
                {
                    interval = v;
                    Console.WriteLine("(select a, b from mb[1,{0}] layers={1} pinterval={2}  func=\'avg\' )forecast b on a number 10;", size / interval, layers, interval);
                    Console.WriteLine("select a/{0} ua, avg(b) ub from r where a<{1} group by a/{0} forecast ub on ua number 10 ;", interval, size);
                }
                for (int i = 50 * 2; i <= 1000 * 1000; i = i * 10)
                {
                    interval = i;
                    Console.WriteLine("select a, b from mb[1,{0}] layers={1} pinterval={2}  func=\'avg\';", size / interval, layers, interval);
                    Console.WriteLine("select a/{0} ua, avg(b) ub from r where a<{1} group by a/{0} forecast ub on ua number 10 ;", interval, size);
                }
            }

        }

    }
}
