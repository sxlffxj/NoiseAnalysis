using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeoAPI.Geometries;
using OSGeo.GDAL;
using OSGeo.OGR;
using OSGeo.OSR;
using System.Collections;
using NoiseAnalysis.Algoriam.Spatial;
using NoiseAnalysis.Algorithm;
using NoiseAnalysis.Model.SourcePartition;

namespace NoiseAnalysis.Model.China
{
    /*!
     * 功能 道路声源模型
     * 版本号 1.0
     * 作者 樊晓剑
     * 创建时间  2016年7月27日
     * 修改时间
     */
    internal class RoadSourrce:ISource
    {
        /*!
         * 功能 计算路面声压级并离散
         * 参数 sourcePath源文件路径
         *      resultPath 结果文件路径
         *      splitLength 分割长度
         *      timeType 计算时段
         * 返回值 路面声压级
         * 版本号 1.0
         * 作者 樊晓剑
         * 创建时间  2016年7月12日
         * 修改时间
         */
        public Layer getSource(Layer sourceLayer, Layer resultLayer, float splitLength, String timeType, int frequency)
        {
            LineSourcePartition bean = new LineSourcePartition();
            FeatureDefn resultDefn = sourceLayer.GetLayerDefn();//字段名

            Feature resultFeature = null;//循环获取要素
            Geometry roadPoint = null;//离散节点
            double height = 0;//高度
            double LW = 0;//声功率级
            while ((resultFeature = sourceLayer.GetNextFeature()) != null)
            {
                //根据所选要素获取相关属性
                height = resultFeature.GetFieldAsDouble("HEIGHT_G");
                LW = getRoadLW(resultFeature, timeType);//计算道路声功率级

                Queue<Geometry> LineSource = bean.staticPartition(10, resultFeature.GetGeometryRef());//公路声源离散

                //离散结果生成新的图层
                while (LineSource.Count > 0)
                {
                    roadPoint = LineSource.Dequeue();
                    Feature feature = new Feature(resultDefn);
                    feature.SetGeometry(roadPoint.Centroid());
                    feature.SetField("HEIGHT_G", height);
                    feature.SetField("PWLs", SourceStrength.getLineSource(LW, roadPoint.Length()));//离散后声功率级
                    feature.SetField("FREQUENCY", frequency);//频率

                    resultLayer.CreateFeature(feature);
                }
            }
            
            return resultLayer;

        }

        #region 内部方法

        /*!
         * 内容 计算车速
         * 版本号 1.0
         * 作者 樊晓剑
         * 创建时间  2016年6月30日
         * 参数 
         * 修改时间
         */
        private double getRoadLW(Feature sFeature, String timeType)
        {
            switch (timeType)
            {
                case "day":
                    timeType = "D";
                    break;

                case "evening":
                    timeType = "E";
                    break;
                case "night":
                    timeType = "N";
                    break;
                default:
                    timeType = "D";
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

            //坡度修正
            float gradientCorrect = getGradientCorrect(sFeature.GetFieldAsDouble("ROAD_G"));
            //路面修正
            float roadtypeCorrect = getRoadTypeCorrect(speeds, sFeature.GetFieldAsString("ROAD_T"));


            //计算大中小车单位长度道路声功率级
            double lol = getLOL(speedl, flowl) + gradientCorrect;
            double lom = getLOM(speedm, flowm) + gradientCorrect;
            double los = getLOS(speeds, flows) + roadtypeCorrect;


            //离散后各段源强计算

            return getLW(lol, lom, los);

        }

        /*!
         * 功能 坡度修正
         * 参数 gradient 坡度
         * 返回值 坡度修正量
         * 版本号 1.0
         * 作者 樊晓剑
         * 创建时间  2016年7月27日
         * 修改时间
         */
        private float getGradientCorrect(double gradient)
        {
            float correctedValue = 0;

            if (gradient > 3 && gradient <= 5)
            {
                correctedValue = 1;

            }
            else if (gradient > 5 && gradient <= 7)
            {
                correctedValue = 3;
            }
            else if (gradient > 7)
            {
                correctedValue = 5;
            }
            return correctedValue;

        }

        /*!
         * 功能 路面修正
         * 参数 gradient 坡度
         * 返回值 坡度修正量
         * 版本号 1.0
         * 作者 樊晓剑
         * 创建时间  2016年7月27日
         * 修改时间
         */
        private float getRoadTypeCorrect(double velocity, String roadType)
        {
            float correctedValue = 0;

            if (roadType == "cement")
            {
                if (velocity <= 35)
                {
                    correctedValue = 1;

                }
                else if (velocity > 35 && velocity <= 45)
                {
                    correctedValue = 1.5f;
                }
                else if (velocity > 45)
                {
                    correctedValue = 2;
                }
            }
            return correctedValue;

        }

        /*!
         * 功能 计算大型车单位长度道路声功率级
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

        private double getLOL(double velocity, double flow)
        {
            //坡度修正
            double lol = 22 + 36.32 * Math.Log10(velocity);
            return lol + 10 * Math.Log10(flow / velocity) - 4.41;
        }

        /*!
         * 功能 计算中型车单位长度道路声功率级
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

        private double getLOM(double velocity, double flow)
        {
            //坡度修正

            double lom = 8.8 + 40.48 * Math.Log10(velocity);

            return lom + 10 * Math.Log10(flow / velocity) - 4.41;
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
        private double getLOS(double velocity, double flow)
        {
            double los = 12.6 + 34.73 * Math.Log10(velocity);

            return los + 10 * Math.Log10(flow / velocity) - 4.41;
        }

        /*!
         * 功能 单位长度道路声功率级
         * 参数 lol大车单车辐射噪声
         *      lom中车单车辐射噪声
         *      los小车单车辐射噪声
         * 返回值
         * 版本号 1.0
         * 作者 樊晓剑
         * 创建时间  2016年7月27日
         * 修改时间
         */
        private double getLW(double lol, double lom, double los)
        {
            double LW = 10 * Math.Log10(Math.Pow(10, 0.1 * lol) + Math.Pow(10, 0.1 * lom) + Math.Pow(10, 0.1 * los));
            return LW;
        }

        #endregion
        #region 未使用代码

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
        private double roadLengthAffect(Geometry roadLine, Geometry receivePoint)
        {
            //公路弯曲或有限长路段引起的交通噪声修正量∆L_1的计算
            double angle = AngleUtility.AngleBetween(new Coordinate(roadLine.GetX(0), roadLine.GetY(0)),
                new Coordinate(receivePoint.GetX(0), receivePoint.GetY(0)),
                new Coordinate(roadLine.GetX(roadLine.GetPointCount() - 1), roadLine.GetY(roadLine.GetPointCount() - 1)));
            double L_1 = 10 * Math.Log10(angle / Math.PI);


            return L_1;

        }
        #endregion


    }
}
