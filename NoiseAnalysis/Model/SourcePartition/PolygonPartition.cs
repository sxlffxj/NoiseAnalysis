using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSGeo.OGR;
using NoiseAnalysis.Algoriam.Spatial;


namespace NoiseAnalysis.Model.SourcePartition
{
    /*!
     * 功能 分割接收点区域，生成接收点图层
     * 版本号 1.0
     * 作者 樊晓剑
     * 创建时间  2016年4月25日
     * 修改时间
     */
    public class PolygonPartition : ISourcePartition
    {
        /*!
         * 功能 根据距离分割计算区域
         * 参数 
         * 返回值
         * 版本号 1.0
         * 作者 樊晓剑
         * 创建时间  2016年4月27日
         * 修改时间
         */
        public Queue<Geometry> staticPartition(double distance, Geometry receiveArea, double height)
        {
            Queue<Geometry> receivePointSource = new Queue<Geometry>();
        
            //获取外接矩形，设置起始点
            Envelope bound = new Envelope();
            receiveArea.GetEnvelope(bound);

            double pointX = bound.MinX + distance / 2;
            //         double pointY = a.MinY + distance / 2;
            // int i = 0;
            while (pointX < bound.MaxX)
            {
                double pointY = bound.MinY + distance / 2;
                while (pointY < bound.MaxY)
                {
                    //新建一个接收点
                    Geometry receivePoint = GeometryCreate.createPoint3D(pointX, pointY, height);
                    //Geometry receivePoint = GeometryCreate.createPoint(pointX, pointY);

                    //判断接收点是否在区域内

                    if (receiveArea.Intersects(receivePoint))
                    {
                        //如果在，新建一个要素
                        receivePointSource.Enqueue(receivePoint);
                    }
                    pointY += distance;
                }
                pointX += distance;
            }
            return receivePointSource;

        }
    }
}
