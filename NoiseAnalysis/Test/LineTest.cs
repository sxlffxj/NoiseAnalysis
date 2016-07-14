using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NoiseAnalysis.Algoriam;
using OSGeo.OGR;
using OSGeo.GDAL;

namespace NoiseAnalysis.Test
{
    class LineTest
    {
        string fromPath = Environment.CurrentDirectory + "\\DataSource\\road.shp";
        string toPath = Environment.CurrentDirectory + "\\temp\\centre.shp";

        public void testStaticPartition()
        {

            Gdal.AllRegister();
            Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "YES");
            Gdal.SetConfigOption("SHAPE_ENCODING", "");
            Ogr.RegisterAll();

            //读取文件
            DataSource ds = Ogr.Open(fromPath, 0);
            Layer oLayer = ds.GetLayerByIndex(0);
            // 写入文件

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


            Layer toLayer = oDS.CreateLayer("POINT", oLayer.GetSpatialRef(), wkbGeometryType.wkbPoint, null);
            Random ran = new Random();
            Feature oFeature = null;
            Geometry lines = null;
            FeatureDefn oDefn = oLayer.GetLayerDefn();

            FieldDefn oFieldID = new FieldDefn("HEIGHT_G", FieldType.OFTReal);
            toLayer.CreateField(oFieldID, 1);

            FieldDefn oFieldName = new FieldDefn("PWLs", FieldType.OFTReal);
            toLayer.CreateField(oFieldName, 1);

            while ((oFeature = oLayer.GetNextFeature()) != null)
            {
                //read current feature

                lines = oFeature.GetGeometryRef();
                Feature feature = new Feature(oDefn);
                feature.SetGeometry(Line.getPoint(lines, lines.Centroid()));
                feature.SetField(0, 4.0);
                feature.SetField(1, ran.Next(40, 120));
                toLayer.CreateFeature(feature);

            }

            oDS.SyncToDisk();
        }



    }
}
