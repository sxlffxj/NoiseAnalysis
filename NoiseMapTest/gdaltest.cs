using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSGeo.GDAL;
using OSGeo.OGR;

namespace NoiseMapTest
{
    class gdaltest
    {
        public void geometryTest()
        {
            Gdal.AllRegister();
            Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "YES");
            Gdal.SetConfigOption("SHAPE_ENCODING", "");
            Ogr.RegisterAll();

            Geometry lines = new Geometry(wkbGeometryType.wkbMultiLineString);
            Geometry line=new Geometry(wkbGeometryType.wkbLineString);

            line.AddPoint(0,0,0);
            line.AddPoint(1, 1,0);
            lines.AddGeometry(line);

            line = new Geometry(wkbGeometryType.wkbLineString);

            line.AddPoint(2, 3, 0);
            line.AddPoint(4, 5, 0);
            lines.AddGeometry(line);


            Console.WriteLine(lines.GetGeometryCount());

            String a = "";

            for (int i = 0; i < lines.GetGeometryCount();i++ )
            {
                lines.GetGeometryRef(i).ExportToWkt(out a);

                Console.WriteLine(a);

            }

        }

        public void crossTest()
        {
            Gdal.AllRegister();
            Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "YES");
            Gdal.SetConfigOption("SHAPE_ENCODING", "");
            Ogr.RegisterAll();

            String a = "";
            Geometry line = new Geometry(wkbGeometryType.wkbLineString);

            line.AddPoint(0, 0, 0);
            line.AddPoint(3, 3, 0);

            Geometry plo = new Geometry(wkbGeometryType.wkbLinearRing);

            plo.AddPoint(1,1,0);
            plo.AddPoint(1, 2, 0);
            plo.AddPoint(2, 2, 0);
            plo.AddPoint(2, 1, 0);
            plo.AddPoint(1, 1, 0);



            Geometry ploy = new Geometry(wkbGeometryType.wkbPolygon);
            ploy.AddGeometry(plo);


            Geometry plo2 = ploy.GetGeometryRef(0);

            plo2.ExportToWkt(out a);
            Console.WriteLine(a);


            line.ExportToWkt(out a);
            Console.WriteLine(a);




           // Geometry lines = plo2.Intersection(line);
            Console.WriteLine(plo2.Intersect(line));

          //  Console.WriteLine(lines.GetPointCount());

           // Console.WriteLine(ploy.GetPointCount());
           // lines.ExportToWkt(out a);
          //  Console.WriteLine(a);

        }






















    }
}
