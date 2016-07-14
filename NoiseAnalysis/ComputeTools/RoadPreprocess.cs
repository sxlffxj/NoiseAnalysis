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
using NoiseAnalysis.SourcePartition;
using NoiseAnalysis.Algorithm;

namespace NoiseAnalysis.ComputeTools
{
    class RoadPreprocess
    {

        public enum Times { DAY, EVENING, NIGHT };
        public enum Type { LARGE, MIDDLE, SMALL };

        public enum RoadType { CEMENT, BITUMIOUS };

        /*!
         * 功能 计算路面声压级
         * 参数 接收点，声源线，影响距离
         * 返回值 路面声压级
         * 版本号 1.0
         * 作者 樊晓剑
         * 创建时间  2016年7月12日
         * 修改时间
         */
        public void getRoadSource(String fromPath,String toPath, float splitLength)
        {

            LineSourcePartition bean = new LineSourcePartition();
            toPath = toPath + "\\RoadSourceTemp.shp";
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

            //构建声源点图层
            Layer toLayer = oDS.CreateLayer("POINT", oLayer.GetSpatialRef(), wkbGeometryType.wkbPoint, null);
            //构建属性
            toLayer.CreateField(new FieldDefn("HEIGHT_G", FieldType.OFTReal), 1);//高度
            toLayer.CreateField(new FieldDefn("PWLs", FieldType.OFTReal), 1);//单一频率声功率级
            FeatureDefn oDefn = oLayer.GetLayerDefn();

            Feature oFeature = null;
            Geometry roadPoint = null;
            while ((oFeature = oLayer.GetNextFeature()) != null)
            {
                //read current feature
                double height = oFeature.GetFieldAsDouble("HEIGHT_G");
                double LW = getRoadLW(oFeature, Times.DAY);

                Queue<Geometry> LineSource = bean.staticPartition(10, oFeature.GetGeometryRef());

                while (LineSource.Count > 0)
                {
                  roadPoint = LineSource.Dequeue();

                    Feature feature = new Feature(oDefn);
                    feature.SetGeometry(GeometryCompute.getCentre(roadPoint));
                    feature.SetField(0, height);
                    feature.SetField(1, 10*Math.Log10(Math.Pow(10, LW / 10) * roadPoint.Length()));
                    toLayer.CreateFeature(feature);
 
  
                }
            }











        }

        /*!
         * 功能 路长影响计算
         * 参数 roadLine 声源线
         *      receiveePoint 接收点
         * 返回值 路长影响值
         * 版本号 1.0
         * 作者 樊晓剑
         * 创建时间  2016年7月12日
         * 修改时间
         */
        public double roadLengthAffect(Geometry roadLine, Geometry receivePoint)
        {



            //公路弯曲或有限长路段引起的交通噪声修正量∆L_1的计算
            double angle = AngleUtility.AngleBetween(new Coordinate(roadLine.GetX(0), roadLine.GetY(0)),
                new Coordinate(receivePoint.GetX(0), receivePoint.GetY(0)),
                new Coordinate(roadLine.GetX(roadLine.GetPointCount() - 1), roadLine.GetY(roadLine.GetPointCount() - 1)));
            double L_1 = 10 * Math.Log10(angle / Math.PI);


            return L_1;

        }



        /*!
         * 内容 计算车速
         * 版本号 1.0
         * 作者 樊晓剑
         * 创建时间  2016年6月30日
         * 参数 
         * 修改时间
         */
        public double getRoadLW(Feature sFeature, Times time)
        {

            String timeType = "";
            switch (time)
            {
                case Times.DAY:
                    timeType = "D";
                    break;

                case Times.EVENING:
                    timeType = "E";
                    break;
                case Times.NIGHT:
                    timeType = "N";
                    break;
            }

            //小车车流量与车速
            double speeds = sFeature.GetFieldAsDouble("SPEED_S" + timeType);
            double flows = sFeature.GetFieldAsDouble("FLOW_S" + timeType);
            //中车车流量与车速
            double speedm = sFeature.GetFieldAsDouble("SPEED_M" + timeType);
            double flowm = sFeature.GetFieldAsDouble("FLOW_M" + timeType);
            //大车车流量与车速
            double speedl = sFeature.GetFieldAsDouble("SPEED_L" + timeType);
            double flowl = sFeature.GetFieldAsDouble("FLOW_L" + timeType);

            //计算大中小车单位长度道路声功率级

            double lol = getLOLM(speedl, sFeature.GetFieldAsDouble("ROAD_G"), Type.LARGE, flowl);
            double lom = getLOLM(speedm, sFeature.GetFieldAsDouble("ROAD_G"), Type.MIDDLE, flowm);


            RoadType roadType = RoadType.CEMENT;
            switch (sFeature.GetFieldAsString("ROAD_T"))
            {
                case "":
                    roadType = RoadType.CEMENT;
                    break;

                case "1":
                    roadType = RoadType.BITUMIOUS;
                    break;

            }


            double los = getLOS(speeds, roadType, flows);


            //离散后各段源强计算

            return getLW(lol, lom, los);

        }



        /*!
         * 功能 计算大中型车单位长度道路声功率级
         * 参数 velocity 速度 
         * gradient 坡度
         * type 车辆类型
         * flow 车流量
         * 返回值 单单位长度道路声功率级
         * 版本号 1.0
         * 作者 樊晓剑
         * 创建时间  2016年7月12日
         * 修改时间
         */

        public double getLOLM(double velocity, double gradient, Type type, double flow)
        {
            //坡度修正
            double l = 0;
            double lolm = 0;
            if (gradient > 3 && gradient <= 5)
            {
                l = 1;

            }
            else if (gradient > 5 && gradient <= 7)
            {
                l = 5;
            }
            else if (gradient > 7)
            {
                l = 3;
            }

            switch (type)
            {
                case Type.LARGE:
                    lolm = 22 + 36.32 * Math.Log10(velocity) + l;
                    break;

                case Type.MIDDLE:
                    lolm = 8.8 + 40.48 * Math.Log10(velocity) + l;
                    break;
            }
            return lolm + 10 * Math.Log10(flow / velocity) - 4.1;
        }





        /*!
        * 功能 计算小型车单位长度道路声功率级
        * 参数 velocity 速度 
        * gradient 坡度
        * roadtype 路面类型
        * flow 车流量
        * 返回值 单位长度道路声功率级
        * 版本号 1.0
        * 作者 樊晓剑
        * 创建时间  2016年7月12日
        * 修改时间
        */
        public double getLOS(double velocity, RoadType roadtype, double flow)
        {
            //路面修正
            double l = 0;

            double los = 0;
            if (roadtype == RoadType.CEMENT)
            {
                if (velocity <= 35)
                {
                    l = 1;

                }
                else if (velocity > 35 && velocity <= 45)
                {
                    l = 1.5;
                }
                else if (velocity > 45)
                {
                    l = 2;
                }
            }
            los = 12.6 + 34.73 * Math.Log10(velocity) + l;

            return los + 10 * Math.Log10(flow / velocity) - 4.1;
        }

        public double getLW(double lol, double lom, double los)
        {
            double LW = 10 * Math.Log10(Math.Pow(10, 0.1 * lol) + Math.Pow(10, 0.1 * lom) + Math.Pow(10, 0.1 * los));

            return LW;

        }


    }
}
