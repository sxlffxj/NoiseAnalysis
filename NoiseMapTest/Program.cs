using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO; 


namespace NoiseMapTest
{
    class Program
    {
        static void Main(string[] args)
        {
            gdaltest g = new gdaltest();
            g.dxftest();

            Console.WriteLine(DateTime.Now);
            Console.WriteLine("ok");
            Console.ReadKey();

        }
    }
}
