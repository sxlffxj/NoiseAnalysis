using NoiseAnalysis.ComputeTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoiseAnalysis.Test
{
    class RainwayPreprocesstest
    {
        RainwayPreprocess bean = new RainwayPreprocess();
        public void getSpurceTest() { 
        double a = bean.getSpurce("160", "B-NONBALLASTED-TRACK");

        Console.WriteLine(a);




    }








    }
}
