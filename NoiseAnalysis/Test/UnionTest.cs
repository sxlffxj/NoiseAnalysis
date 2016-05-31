using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSGeo.GDAL;
using OSGeo.OGR;
using OSGeo.OSR;
namespace NoiseAnalysis.Test
{
    class UnionTest
    {

        string bPath = Environment.CurrentDirectory + "\\DataSource\\building.shp";
        string toPath = Environment.CurrentDirectory + "\\temp\\union.shp";


        public struct geounion
        {
            public int key;
            public double high;
            public Geometry geo;
        }

        public void directTest()
        {
            Gdal.AllRegister();
            Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "YES");
            Gdal.SetConfigOption("SHAPE_ENCODING", "");
            Ogr.RegisterAll();

            //读取文件

            DataSource bs = Ogr.Open(bPath, 1);
            Layer bLayer = bs.GetLayerByIndex(0);

            FieldDefn oFieldName = new FieldDefn("idUnion", FieldType.OFTReal);
            if (bLayer.GetLayerDefn().GetFieldIndex("idUnion") < 0)
            {
                bLayer.CreateField(oFieldName, 0);
            }
            Feature bfeature = null;
            while ((bfeature = bLayer.GetNextFeature()) != null)
            {
                bfeature.SetField("idUnion", 0);
                bLayer.SetFeature(bfeature);
            }
            bLayer.ResetReading();

            // List<Geometry> geos = new List<Geometry>();
            Dictionary<double, geounion> geos = new Dictionary<double, geounion>();
            geounion geo;

            int count = bLayer.GetFeatureCount(0);

            for (int i = 0; i < count; i++)
            {
                if (bLayer.GetFeature(i).GetFieldAsInteger("idUnion") != 1)
                {
                    geo.geo = bLayer.GetFeature(i).GetGeometryRef();
                    geo.high = 0;
                    geo.key = bLayer.GetFeature(i).GetFID();
                   
                    try
                    {
                        bLayer.SetSpatialFilter(geo.geo.Buffer(0.5, 30));
                        geo = getTouch(bLayer, geo);
                        if (geos.ContainsKey(geo.key))
                        {
                            geos.Remove(geo.key);
                        }
                        geos.Add(geo.key, geo);

                        // bLayer.ResetReading();

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }

                    Console.WriteLine(i + ":" + count + "      " + DateTime.Now + ":" + DateTime.Now.Millisecond);

                }
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
                // 创建数据源  
                Layer toLayer = oDS.CreateLayer("direct", bLayer.GetSpatialRef(), wkbGeometryType.wkbPolygon, null);
                FieldDefn oFieldID = new FieldDefn("HEIGHT_G", FieldType.OFTReal);
                toLayer.CreateField(oFieldID, 1);

                FeatureDefn oDefn = toLayer.GetLayerDefn();
                foreach (double key in geos.Keys)
                {
                    //read current feature
                    Feature feature = new Feature(oDefn);
                    feature.SetField(0, geos[key].high);
                    feature.SetGeometry(geos[key].geo);
                    toLayer.CreateFeature(feature);
                }
                oDS.SyncToDisk();
           
        }

        public geounion getTouch(Layer bLayer, geounion geo)
        {
            try
            {
                Feature bfeature = null;
                while ((bfeature = bLayer.GetNextFeature()) != null)
               // for (int i = 0; i < bLayer.GetFeatureCount(0);i++ )
                {
                    if (bfeature.GetFieldAsInteger("idUnion") != 1)
                    {
                       // bfeature = bLayer.GetFeature(i);
                        // while (isIngeo(bLayer, geo.geo))
                        //  {
                        geo.geo = geo.geo.Union(bfeature.GetGeometryRef());
                        geo.key = bfeature.GetFID();
                        if (geo.high < bfeature.GetFieldAsDouble("B_HI"))
                        {
                            geo.high = bfeature.GetFieldAsDouble("B_HI");

                        }
                        bfeature.SetField("idUnion", 1);
                        bLayer.SetFeature(bfeature);
                        bLayer.SetSpatialFilter(geo.geo.Buffer(0.5, 30));
                        Console.WriteLine(bLayer.GetFeatureCount(0));
                    }                 
                    // }
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return geo;
        }
        public bool isIngeo(Layer layer, Geometry geo)
        {
            bool isIn = false;
            layer.ResetReading();
            Feature bfeature = null;
            while ((bfeature = layer.GetNextFeature())!=null)
            {
                if (bfeature.GetFieldAsInteger("idUnion") == 0)
                {
                   isIn = true;
                   break;
                }
            }
            return isIn;
        }
    }
}
