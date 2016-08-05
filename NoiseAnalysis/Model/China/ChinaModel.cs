using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSGeo.GDAL;
using OSGeo.OGR;
using OSGeo.OSR;
using NoiseAnalysis.Model.China;
using NoiseAnalysis.Model.SourcePartition;
using NoiseAnalysis.Algoriam.Spatial;

namespace NoiseAnalysis.Model.China
{
    //需要离散的点线面声源进行离散并计算源强，合成一个声源图层
    //输入数据应该是字典，key为地址，value是声源类型



    /// <summary>
    /// 中国模型计算
    /// </summary>
    class ChinaModel
    {

        private Dictionary<String, String> m_sourcePath;
        private String m_barrierPath;
        private float m_gridSize;
        private float m_receiveHeight;
        private Geometry m_computationalGrid;
        private String m_resultPath;
        private float m_splitLength;
        private String m_timeType;
        private int m_frequency;
        private float m_range;





        /// <summary>
        /// 初始化计算参数
        /// </summary>
        /// <param name="sourcePath">声源路径</param>
        /// <param name="barrierPath">障碍物路径</param>
        /// <param name="gridSize">网格大小</param>
        /// <param name="receiveHeight">接收点高度</param>
        /// <param name="computationalGrid">计算区域</param>
        /// <param name="resultPath">转换后输出路径</param>
        /// <param name="splitLength">分割长度</param>
        /// <param name="timeType">时段</param>
        /// <param name="frequency">频率</param>
        /// <param name="range">影响范围</param>

        public ChinaModel(Dictionary<String, String> sourcePath, String barrierPath, float gridSize, float receiveHeight,
            Geometry computationalGrid, String resultPath, float splitLength, String timeType, int frequency, float range)
        {
            m_sourcePath = sourcePath;
            m_barrierPath = barrierPath;
            m_gridSize = gridSize;
            m_receiveHeight = receiveHeight;
            m_computationalGrid = computationalGrid;
            m_resultPath = resultPath;
            m_splitLength = splitLength;
            m_timeType = timeType;
            m_frequency = frequency;
            m_range = range;
        }







        /// <summary>
        /// 线声源离散
        /// </summary>
        /// <param name="sourcePath">声源路径</param>
        /// <param name="resultPath">离散后路径</param>
        /// <param name="splitLength">分割长度</param>
        /// <param name="timeType">时段</param>
        /// <param name="frequency">频率</param>
        /// <returns></returns>
        private Layer getSource()
        {

            String sourceResultPath = m_resultPath + "\\SourceTemp.shp";//输出路径

            // 写入文件
            OSGeo.OGR.Driver resultDriver = Ogr.GetDriverByName("ESRI Shapefile");

            // 创建数据源  

            DataSource resultDataSource;
            if (Ogr.Open(sourceResultPath, 0) != null)
            {
                resultDataSource = Ogr.Open(sourceResultPath, 1);
                resultDataSource.DeleteLayer(0);
            }
            else
            {
                resultDataSource = resultDriver.CreateDataSource(sourceResultPath, null);
            }

            //投影信息
            OSGeo.OSR.SpatialReference projection = new OSGeo.OSR.SpatialReference("");
            projection.ImportFromEPSG(3395);

            //构建声源点图层
            Layer resultLayer = resultDataSource.CreateLayer("POINT", projection, wkbGeometryType.wkbPoint, null);
            //构建属性
            resultLayer.CreateField(new FieldDefn("HEIGHT_G", FieldType.OFTReal), 1);//高度
            resultLayer.CreateField(new FieldDefn("PWLs", FieldType.OFTReal), 1);//单一频率声功率级
            resultLayer.CreateField(new FieldDefn("FREQUENCY", FieldType.OFTReal), 1);//频率

            ISource sourceBean = null;
            DataSource sourceDataSource;
            foreach (KeyValuePair<String, String> item in m_sourcePath)
            {
                switch (item.Key)
                {
                    case "road":
                        sourceBean = new RoadSourrce();
                        break;
                    case "rainway":
                        sourceBean = new RainwaySource();
                        break;

                    case "subway":
                        sourceBean = new RoadSourrce();
                        break;
                }
                //源文件
                sourceDataSource = Ogr.Open(item.Value, 0);
                resultLayer = sourceBean.getSource(sourceDataSource.GetLayerByIndex(0), resultLayer, m_splitLength, m_timeType, m_frequency);
            }
            return resultLayer;
        }



        public void mapCompute()
        {
            //声源离散
            Layer sourceLayer = getSource();
            PathSearch pathbean = new PathSearch();
            //接收点离散
            Queue<Geometry> receiveList = PolygonPartition.staticPartition(m_gridSize, m_computationalGrid, m_receiveHeight);


            //障碍物优化
            DataSource barrierSource = Ogr.Open(m_barrierPath, 0);
            Layer barrierLayer = barrierSource.GetLayerByIndex(0);

            Geometry receivePoint = null;
            Geometry sourcePoint = null;
            Feature sourceFeature = null;
            Geometry[] pathList = null;
            List<float[]> sourceLineList = new List<float[]>();
            //路径计算，循环接收点
            while (receiveList.Count > 0)
            {
                //声源点和接收点
                receivePoint = receiveList.Dequeue();
                sourceLayer.SetSpatialFilter(receivePoint.Buffer(m_range, 30));
                //循环声源点
                while ((sourceFeature = sourceLayer.GetNextFeature()) != null)
                {
                    sourcePoint = sourceFeature.GetGeometryRef();
                    //获取路径
                    pathList = pathbean.getPath(barrierLayer, sourcePoint, receivePoint, m_range);



                }





            }


            //衰减计算










        }





















    }
}
