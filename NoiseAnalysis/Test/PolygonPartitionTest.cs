using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NoiseAnalysis.SourcePartition;
using System.Collections;
using NoiseAnalysis.SpatialTools;
using OSGeo.GDAL;
using OSGeo.OGR;
using OSGeo.OSR;


namespace NoiseAnalysis.Test
{


    class PolygonPartitionTest
    {
        PolygonPartition bean = new PolygonPartition();

        string fromPath = "F:\\arcgis\\test\\New_Shapefile.shp";
        string toPath = "E:\\test\\workspace\\temp\\receivePoint.shp";
        string bPath = "F:\\arcgis\\test\\building.shp";

        public void testPolygonPartition()
        {
            Gdal.AllRegister();
            Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "YES");
            Gdal.SetConfigOption("SHAPE_ENCODING", "");
            Ogr.RegisterAll();

            //读取文件
            DataSource ds = Ogr.Open(fromPath, 0);
            Layer oLayer = ds.GetLayerByIndex(0);

            DataSource dsb = Ogr.Open(bPath, 0);
            Layer bLayer = dsb.GetLayerByIndex(0);
            // 写入文件

            OSGeo.OGR.Driver oDriver = Ogr.GetDriverByName("ESRI Shapefile");
            // 创建数据源  
            DataSource oDS = oDriver.CreateDataSource(toPath, null);


           Layer toLayer = oDS.CreateLayer("POINT", oLayer.GetSpatialRef(), wkbGeometryType.wkbPoint, null);

           // Layer toLayer = oDS.CreateLayer("POINT", oLayer.GetSpatialRef(), wkbGeometryType.wkbPolygon, null);
            FieldDefn oFieldID = new FieldDefn("HEIGHT_G", FieldType.OFTReal);
            toLayer.CreateField(oFieldID, 1);

            FieldDefn oFieldName = new FieldDefn("PWLs", FieldType.OFTReal);
            toLayer.CreateField(oFieldName, 1);


            polygonPartition(oLayer, toLayer, bLayer);
      
            oDS.SyncToDisk();
        }

        public void polygonPartition(Layer fromLayer, Layer toLayer, Layer buildingLayer)
        {

            Feature aFeature = null;
            FeatureDefn oDefn = toLayer.GetLayerDefn();
            while ((aFeature = fromLayer.GetNextFeature()) != null)
            {

                Geometry receiveExtent = fromLayer.GetFeature(0).GetGeometryRef();

                //read current feature
                Queue<Geometry> LineSource = bean.staticPartition(10, receiveExtent, 4);

                 Feature bFeature = null;
                 Queue<Geometry> buildings = new Queue<Geometry>();
                 while ((bFeature = buildingLayer.GetNextFeature()) != null)
                 {
                     if (bFeature.GetGeometryRef().Intersects(receiveExtent))
                     {
                         buildings.Enqueue(bFeature.GetGeometryRef());           
                     }
                 }

                 Random ran = new Random();
                 Geometry point = null;
                 while (LineSource.Count != 0)
                 {
                     point = LineSource.Dequeue();
                     foreach (Geometry building in buildings)
                     {
                         if (building.Intersects(point))
                         {
                             point = null;
                             break;
                         }
                     }
                     if (point!=null)
                     {
                         Feature feature = new Feature(oDefn);
                        // feature.SetGeometry(point.Buffer(500,30));
                         feature.SetGeometry(point);
                         feature.SetField(0, 4);
                         feature.SetField(1, ran.Next(40, 120));
                         toLayer.CreateFeature(feature);
      
                     }
                 }
            }
        }
    }

}
