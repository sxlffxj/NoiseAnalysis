﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeoAPI.Geometries;
using OSGeo.GDAL;
using OSGeo.OGR;
using OSGeo.OSR;
using NoiseAnalysis.SpatialTools;
namespace NoiseAnalysis.Test
{
    class ProjectionToolsTest
    {
        string fromPath = "F:\\arcgis\\test\\buildingw.shp";
        string toPath = "E:\\test\\workspace\\temp\\projection.shp";
        ProjectionTools bean = new ProjectionTools();
        public const String PROJECTION = "PROJCS[\"WGS_1984_World_Mercator\","
+ "GEOGCS[\"GCS_WGS_1984\""
+ ",DATUM[\"D_WGS_1984\",SPHEROID[\"WGS_1984\",6378137.0,298.257223563]],"
+ "PRIMEM[\"Greenwich\",0.0],UNIT[\"Degree\",0.0174532925199433]],"
+ "PROJECTION[\"Mercator\"],PARAMETER[\"False_Easting\",0.0],"
+ "PARAMETER[\"False_Northing\",0.0],PARAMETER[\"Central_Meridian\",0.0]"
+ ",PARAMETER[\"Standard_Parallel_1\",0.0],UNIT[\"Meter\",1.0],AUTHORITY[\"EPSG\",3395]]";
        
        public void testProjectionConvert()
        {

            Gdal.AllRegister();
            Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "YES");
            Gdal.SetConfigOption("SHAPE_ENCODING", "");
            Ogr.RegisterAll();
            Gdal.SetConfigOption("GDAL_DATA", "E://lib//gdal//gdal1.9//data");
            //读取文件
            DataSource ds = Ogr.Open(fromPath, 0);
            Layer oLayer = ds.GetLayerByIndex(0);
            // 写入文件

            OSGeo.OGR.Driver oDriver = Ogr.GetDriverByName("ESRI Shapefile");



            OSGeo.OSR.SpatialReference projection = new OSGeo.OSR.SpatialReference(PROJECTION);
            // 创建数据源  
            DataSource oDS = oDriver.CreateDataSource(toPath, null);
            Layer toLayer = oDS.CreateLayer("POINT", projection, oLayer.GetGeomType(), null);

       


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

                    feature.SetGeometry(bean.projectionConvert(lines, projection));
                    feature.SetField(0, 4.0);
                    feature.SetField(1, ran.Next(40, 120));
                    toLayer.CreateFeature(feature);
           
            }

            oDS.SyncToDisk();
        }















    }
}