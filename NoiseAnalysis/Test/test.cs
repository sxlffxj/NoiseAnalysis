using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using NoiseAnalysis.Test;



namespace NoiseAnalysis
{
    class test
    {
       
        public static void Main()
        {
            string fromPath = "F:\\arcgis\\test\\buildingw.shp";
            DateTime a = DateTime.Now;
            Console.WriteLine(DateTime.Now);
           // LineSourcePartitionTest bean = new LineSourcePartitionTest();
           // bean.testStaticPartition();
 

            //PolygonPartitionTest bean = new PolygonPartitionTest();
           // bean.testPolygonPartition();

           // PathSearchTest bean = new PathSearchTest();

           // LineTest bean = new LineTest();
           // bean.testStaticPartition();
           // bean.directTest();

            RainwayPreprocesstest bean = new RainwayPreprocesstest();

            bean.getSpurceTest();


          //  ProjectionToolsTest bean = new ProjectionToolsTest();
          //  bean.testProjectionConvert();

           // Class1 bean = new Class1();
           // bean.readLayerTest(fromPath);
          // UnionTest bean = new UnionTest();
          //  bean.directTest();



            Console.WriteLine(a);
            Console.WriteLine(DateTime.Now);
            Console.WriteLine("ok");
            Console.ReadKey();
        }

    }
}
