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
























    }
}
