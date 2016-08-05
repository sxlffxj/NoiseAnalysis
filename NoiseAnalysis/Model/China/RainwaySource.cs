using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NoiseAnalysis.DataBase;
using System.Data.OleDb;
using OSGeo.GDAL;
using OSGeo.OGR;
using OSGeo.OSR;
using NoiseAnalysis.Model.SourcePartition;

namespace NoiseAnalysis.Model.China
{
    /// <summary>
    /// 铁路源强
    /// </summary>
    class RainwaySource : ISource
    {
        /// <summary>
        /// 铁路声源计算
        /// </summary>
        /// <param name="sourceLayer">声源图层</param>
        /// <param name="resultLayer">导出结果图层</param>
        /// <param name="splitLength">分割长度</param>
        /// <param name="timeType">时段</param>
        /// <returns>离散后声源图层</returns>
        public Layer getSource(Layer sourceLayer, Layer resultLayer, float splitLength, String timeType)
        {
            LineSourcePartition bean = new LineSourcePartition();
            FeatureDefn resultDefn = sourceLayer.GetLayerDefn();//字段名

            Feature resultFeature = null;//循环获取要素
            Geometry rainwayPoint = null;//离散节点
            double LW = 0;
            double height = 0;
            double frequency = 0;
            while ((resultFeature = sourceLayer.GetNextFeature()) != null)
            {
                //根据所选要素获取相关属性
                  //待修改
                LW = getSpurce(resultFeature.GetFieldAsDouble("HEIGHT_G"), resultFeature.GetFieldAsString("HEIGHT_G"));//计算道路声功率级
                height = resultFeature.GetFieldAsDouble("HEIGHT_G");
                frequency = resultFeature.GetFieldAsDouble("HEIGHT_G");

                Queue<Geometry> LineSource = bean.staticPartition(10, resultFeature.GetGeometryRef());//公路声源离散

                //离散结果生成新的图层
                while (LineSource.Count > 0)
                {
                    rainwayPoint = LineSource.Dequeue();
                    Feature feature = new Feature(resultDefn);
                    feature.SetGeometry(rainwayPoint.Centroid());
                    feature.SetField("HEIGHT_G", height);
                    feature.SetField("PWLs", SourceStrength.getLineSource(LW, rainwayPoint.Length()));//离散后声功率级
                    feature.SetField("FREQUENCY", frequency);//频率

                    resultLayer.CreateFeature(feature);
                }
            }

            return resultLayer;


        }

        /// <summary>
        /// 在数据库中查询源强
        /// </summary>
        /// <param name="speed">速度</param>
        /// <param name="type">车辆类型</param>
        /// <returns>铁路源强</returns>
        public double getSpurce(double speed, String type)
        {
            double source = 0;
            String sql = "select v" + speed + " from TRAINSOURCE where TRAINTYPE ='" + type + "'";

            NoiseAnalysis.DataBase.Access access = new NoiseAnalysis.DataBase.Access();
            OleDbConnection connection=  access.getConn();
            OleDbCommand command = new OleDbCommand(sql, connection);
            OleDbDataReader reader = command.ExecuteReader(); //执行command并得到相应的DataReader
            while (reader.Read())
            {
                source =Convert.ToDouble( reader["v"+speed]);
            }
            reader.Close();
            access.Close();
            return source;
        }
    }
}
