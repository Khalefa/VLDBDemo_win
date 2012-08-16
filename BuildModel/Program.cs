using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using utils;
using System.IO;
using System.Xml.Serialization;


namespace ModelGen
{
   public class Program
    {
        static void Main(string[] args)
        {
           /* double[] uk = utils.File.ReadData("c:/data/uk", 0, 100);
            int[] freq = { 192, 24 * 4 };
            TimeSeries ts = new TimeSeries(uk, freq);
            Model m = new Model(ts);
            */
            //GenData.GenRandom();
           /* dir="c:/VLDBDemo_win/data/r/";
            file = "random.txt";*/
            //BuildNiceTree();
          // buildRandom(0, 1000*1000);
           GenTree.build(0, 1000*1000);            
        }
       
    }
}
