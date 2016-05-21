using NoiseAnalysis.SourcePartition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeoAPI.Geometries;
using OSGeo.GDAL;
using OSGeo.OGR;
using OSGeo.OSR;
using System.Collections;
using NoiseAnalysis.SpatialTools;



namespace NoiseAnalysis.Test
{
    class LineSourcePartitionTest
    {
        LineSourcePartition bean = new LineSourcePartition();

        string fromPath = "F:\\arcgis\\test\\road.shp";
        string toPath = "E:\\test\\workspace\\temp\\staticPartition.shp";


 

        // string toPath = "E:\\test\\workspace\\temp\\staticPartitionpX.shp";

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
            DataSource oDS = oDriver.CreateDataSource(toPath,null);
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
                Queue<Geometry> LineSource = bean.staticPartition(10, lines);
                while (LineSource.Count != 0)
                {
                    Feature feature = new Feature(oDefn);
                    Geometry line = LineSource.Dequeue();
                    feature.SetGeometry(GeometryCompute.getCentre(line));
                    feature.SetField(0, 4.0);
                    feature.SetField(1, ran.Next(40, 120));
                    toLayer.CreateFeature(feature);
                }
            }
         
            oDS.SyncToDisk();
        }
    }
}
