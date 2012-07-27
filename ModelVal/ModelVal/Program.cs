using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelVal
{
    class Program
    {
        static void Main(string[] args)
        {
            Model.error_level = 2;
            Model.LoadModules();
            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine(i+ "\t"+Model.GetValue(0, i));
            }
        }
    }
}
