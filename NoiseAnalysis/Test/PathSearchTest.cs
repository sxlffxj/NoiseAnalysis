﻿using NoiseAnalysis.ComputeTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSGeo.GDAL;
using OSGeo.OGR;
using OSGeo.OSR;

namespace NoiseAnalysis.Test
{
    class PathSearchTest
    {
        string rPath = "E:\\test\\workspace\\temp\\receivePoint.shp";
        string sPath = "E:\\test\\workspace\\temp\\staticPartition.shp";
        string bPath = "F:\\arcgis\\test\\building.shp";
        string toPath = "E:\\test\\workspace\\temp\\direct.shp";

        PathSearch bean = new PathSearch();


        public void directTest()
        {
            Gdal.AllRegister();
            Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "YES");
            Gdal.SetConfigOption("SHAPE_ENCODING", "");
            Ogr.RegisterAll();

            //读取文件
            DataSource rs = Ogr.Open(rPath, 0);
            Layer rLayer = rs.GetLayerByIndex(0);

            DataSource bs = Ogr.Open(bPath, 0);
            Layer bLayer = bs.GetLayerByIndex(0);


            DataSource ss = Ogr.Open(sPath, 0);
            Layer sLayer = ss.GetLayerByIndex(0);

            Feature rFeature = null;

            int i = 0;
            List<Geometry> geos = new List<Geometry>();
            while ((rFeature = rLayer.GetNextFeature()) != null)
            {
                Geometry point = rFeature.GetGeometryRef();
               // bean.direct(bLayer, sLayer, point);
              //  Console.WriteLine(rFeature.GetFID() + ":" + bean.direct(bLayer, rLayer, point));
              



                geos.AddRange(bean.direct(bLayer, sLayer, point,500));
                Console.WriteLine(rFeature.GetFID() + ":" + rLayer.GetFeatureCount(0) + ":" + DateTime.Now);
             //   i++;
                //    if(i==5){
 //break;
                //    }
               

            }





            OSGeo.OGR.Driver oDriver = Ogr.GetDriverByName("ESRI Shapefile");



            // 创建数据源  
            DataSource oDS = oDriver.CreateDataSource(toPath, null);
            Layer toLayer = oDS.CreateLayer("POINT", sLayer.GetSpatialRef(), wkbGeometryType.wkbLineString, null);

            FeatureDefn oDefn = toLayer.GetLayerDefn();
           foreach(Geometry geom in geos)
            {
                //read current feature


                Feature feature = new Feature(oDefn);

                feature.SetGeometry(geom);

                toLayer.CreateFeature(feature);

            }

            oDS.SyncToDisk();



















        }


































    }
}