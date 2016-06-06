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
        string path = "E:\\WorkSpace\\3DWorks\\NoiseAnalysis\\NoiseAnalysis\\bin\\x86\\Release";
        public void geometryTest()
        {
            Gdal.AllRegister();
            Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "YES");
            Gdal.SetConfigOption("SHAPE_ENCODING", "");
            Ogr.RegisterAll();

         
            Geometry line=new Geometry(wkbGeometryType.wkbLinearRing);

            line.AddPoint(0,0,0);
            line.AddPoint(0, 1,0);
            line.AddPoint(1, 1, 0);
            line.AddPoint(1, 0, 0);
            line.AddPoint(0, 0, 0);



            String a = "";
            line.ExportToWkt(out a);
            Console.WriteLine(a);

            Console.WriteLine(line.GetGeometryCount());

  
            for (int i = 0; i < line.GetGeometryCount(); i++)
            {
                line.GetGeometryRef(i).ExportToWkt(out a);

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


           // Geometry ploy = ploy.GetGeometryRef(0);

           // ploy.ExportToWkt(out a);
           // Console.WriteLine(a);
           // line.ExportToWkt(out a);
           // Console.WriteLine(a);
            Geometry lines = ploy.Intersection(line);


            lines.ExportToWkt(out a);
           // Console.WriteLine(a);


          //  Console.WriteLine(ploy.Intersect(line));

           
            for (int i = 0; i < ploy.GetPointCount();i++ )
            {
                 double[] dou=new double[3];
                ploy.GetPoint(i,dou);
                Geometry po = new Geometry(wkbGeometryType.wkbPoint);
                po.AddPoint(dou[0], dou[1], dou[2]);
                po.ExportToWkt(out a);
                Console.WriteLine(a);
            }

          //  Console.WriteLine(lines.GetPointCount());

           // Console.WriteLine(ploy.GetPointCount());
           // lines.ExportToWkt(out a);
          //  Console.WriteLine(a);

        }




        public void angleTest()
        {

            Gdal.AllRegister();
            Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "YES");
            Gdal.SetConfigOption("SHAPE_ENCODING", "");
            Ogr.RegisterAll();

          
            string fromPath = path + "\\temp\\union.shp";

            string toPath = path + "\\temp\\unionhell.shp";


            DataSource bs = Ogr.Open(fromPath, 0);
            Layer bLayer = bs.GetLayerByIndex(0);


            OSGeo.OGR.Driver oDriver = Ogr.GetDriverByName("ESRI Shapefile");



            // 创建数据源  



            DataSource oDS;
            if (Ogr.Open(toPath, 0) != null)
            {
                oDS = Ogr.Open(toPath, 1);
                oDS.DeleteLayer(0);
            }
            else
            {
                oDS = oDriver.CreateDataSource(toPath, null);
            }

            Layer toLayer = oDS.CreateLayer("direct", bLayer.GetSpatialRef(), wkbGeometryType.wkbPolygon, null);



            FeatureDefn oDefn = toLayer.GetLayerDefn();
            Feature fea = null;
            while ((fea = bLayer.GetNextFeature()) != null)
            {
                Feature feature = new Feature(oDefn);
               // Geometry geo = new Geometry(wkbGeometryType.wkbPolygon);
               // geo.AddGeometry(fea.GetGeometryRef().ConvexHull());



                feature.SetGeometry(fea.GetGeometryRef().ConvexHull());

                toLayer.CreateFeature(feature);


            }



            oDS.SyncToDisk();

        }

        public void dxftest()
        {

            string frompath = "E:\\test\\fjz.dwg";

            string toPath =path + "\\temp\\cov.shp";
            Gdal.AllRegister();
            Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "YES");
            Gdal.SetConfigOption("SHAPE_ENCODING", "");
            Ogr.RegisterAll();

            DataSource bs = Ogr.Open(frompath, 0);


            int iLayerCount = bs.GetLayerCount();


            Layer layer = bs.GetLayerByIndex(0);
            String a="";
            Feature f = null;
            for (int i = 0; i < iLayerCount;i++ )
            {

                Console.WriteLine(iLayerCount);
                Console.WriteLine(bs.GetLayerByIndex(i).GetName());


            }

       

            OSGeo.OGR.Driver oDriver = Ogr.GetDriverByName("ESRI Shapefile");
            DataSource oDS;
            if (Ogr.Open(toPath, 0) != null)
            {
                oDS = Ogr.Open(toPath, 1);
                oDS.DeleteLayer(0);
            }
            else
            {
                oDS = oDriver.CreateDataSource(toPath, null);
            }

            Layer toLayer = oDS.CreateLayer("direct", null, wkbGeometryType.wkbLineString, null);


            FeatureDefn oDefn = toLayer.GetLayerDefn();
            FieldDefn oFieldName = new FieldDefn("area", FieldType.OFTReal);
            toLayer.CreateField(oFieldName, 1);  
            try
            {

                while ((f = layer.GetNextFeature()) != null)
                {
                    Geometry g = f.GetGeometryRef();
                    if (g.GetGeometryType() == wkbGeometryType.wkbLineString)
                    {
                       // Geometry lr=new Geometry(wkbGeometryType.wkbLinearRing);
                       // for (int i = 0; i < g.GetPointCount();i++ )
                       // {
                       //     lr.AddPoint(g.GetX(i),g.GetY(i),g.GetZ(i));
                       // }

                       // lr.CloseRings();


                       // Geometry pr = new Geometry(wkbGeometryType.wkbPolygon);
                      //  pr.AddGeometry(lr);
                      //  if(pr.Area()>=10){
                        Feature feature = new Feature(oDefn);
                      //  feature.SetField("area", pr.Area());
                        feature.SetGeometry(g);
                    toLayer.CreateFeature(feature);
                   // }
                    }
                 

                }
            }
            catch (Exception e)
            {




            }
             
            oDS.SyncToDisk();
          

        }















    }
}
