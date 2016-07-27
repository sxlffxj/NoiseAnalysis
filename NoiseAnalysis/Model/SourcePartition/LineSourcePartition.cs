using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeoAPI.Geometries;
using OSGeo.OGR;
using NoiseAnalysis.Algorithm;
using NoiseAnalysis.Algoriam.Spatial;


namespace NoiseAnalysis.Model.SourcePartition
{
    /*!
     * 功能 线声源分割
     * 版本号 1.0
     * 作者 樊晓剑
     * 创建时间  2016年4月27日
     * 修改时间
     */
    class LineSourcePartition:ISourcePartition
    {








        #region 静态分割
        /*!
         * 功能 普通线声源分割
         * 参数 splitLength分割因子 line 需要分割的生源线
         * 返回值 分割后的声源线集合
         * 版本号 1.0
         * 作者 樊晓剑
         * 创建时间  2016年4月27日
         * 修改时间
         */

        public Queue<Geometry> staticPartition(double splitLength, Geometry line)
        {

            //构建返回值集合
            Queue<Geometry> lineSource = new Queue<Geometry>();
            //如果声源线长度小于分割长度，返回其本身
            if (line.Length() <= splitLength)
            {
                lineSource.Enqueue(line);
            }
            else
            {
                double[] before = new double[3];
                double[] after = new double[3];
                Coordinate conBefore;
                Coordinate conAfter;
                Geometry sourceline;
                for (int i = 0; i < line.GetPointCount() - 1; i++)
                {
                    line.GetPoint(i, before);
                    line.GetPoint(i + 1, after);

                    conBefore = new Coordinate(before[0], before[1]);
                    conAfter = new Coordinate(after[0], after[1]);
                    while (conAfter.Distance(conBefore) > splitLength)
                    {
                        // double angle = AngleUtility.AngleBetween(pointCoordinates[i + 1], pointCoordinate, new Coordinate(pointCoordinates[i + 1].X, pointCoordinate.Y));// 计算两点与水平线夹角
                        double angle = AngleUtility.Angle(conBefore, conAfter);// 计算两点与水平线夹角
                        double xplus = conBefore.X + splitLength * Math.Cos(angle);// 计算分割点坐标
                        double yplus = conBefore.Y + splitLength * Math.Sin(angle);
                        lineSource.Enqueue(GeometryCreate.createLineString3D(conBefore.X, conBefore.Y, conBefore.Z, xplus, yplus, 0));// 添加分割线段
                        // 将分割点设为新的起点
                        conBefore.X = xplus;
                        conBefore.Y = yplus;
                        // break;
                    }
                    lineSource.Enqueue(GeometryCreate.createLineString3D(conBefore.X, conBefore.Y, conBefore.Z, conAfter.X, conAfter.Y, conAfter.Z));

                }
            }
            return lineSource;
        }
        #endregion

        # region 动态分割尝试
        public Queue<Geometry> dynamicPartition2(Geometry roadLine, Geometry receivePoint, double param)
        {
            Queue<Geometry> roadFeature = new Queue<Geometry>();

            Geometry centroid = roadLine.Centroid();

            double c = roadLine.GetX(0);
            double d = roadLine.GetY(0);
            int count = roadLine.GetPointCount();
            Geometry directLine = GeometryCreate.createLineString(c, d, roadLine.GetX(count - 1), roadLine.GetY(count - 1));

            //简化声源线参数
            double k = (roadLine.GetX(count - 1) - c) / (roadLine.GetY(count - 1) - d);
            double f = d + k * c;

            double centdistance = receivePoint.Distance(roadLine.Centroid());







            while (!centroid.Buffer(centdistance, 30).Contains(roadLine))
            {






            }










            return roadFeature;
        }

        /*!
         * 功能 线声源动态分割
         * 版本号 1.0
         * 作者 樊晓剑
         * 创建时间  2016年6月28日
         * 参数 
         * 修改时间
         */

        public Queue<Geometry> dynamicPartition(Geometry roadLine, Geometry receivePoint, double param)
        {

            Queue<Geometry> roadFeature = new Queue<Geometry>();
            //接收点坐标信息
            double a = receivePoint.GetX(0);
            double b = receivePoint.GetY(0);
            //道路起始点坐标信息
            double c = roadLine.GetX(0);
            double d = roadLine.GetY(0);

            //取道路的外接矩形
            Geometry roadrect = new Geometry(wkbGeometryType.wkbPolygon);

            roadrect.AddGeometry(roadLine.Boundary());



            int count = roadLine.GetPointCount();
            //简化声源线
            Geometry directLine = GeometryCreate.createLineString(c, d, roadLine.GetX(count - 1), roadLine.GetY(count - 1));

            //简化声源线参数
            double k = (roadLine.GetX(count - 1) - c) / (roadLine.GetY(count - 1) - d);
            double f = d - k * c;

            Geometry point = GeometryCreate.createPoint(roadLine.GetX(0), roadLine.GetY(0));

            Geometry sourcePoint = getSourcePoint(k, f, a, b, c, d, param);

            while (roadrect.Intersects(sourcePoint))
            {







            }






            return roadFeature;

        }

        /*!
         * 功能 计算分割点
         * 版本号 1.0
         * 作者 樊晓剑
         * 创建时间  2016年7月5日
         * 参数 k,b：简化后道路线段参数，a,b：接收点参数 c,d：分割点参数 p:分割因子
         * 修改时间
         */
        public Geometry getSourcePoint(double k, double f, double a, double b, double c, double d, double p)
        {
            p = 1 / p;

            double u = (2 * p * p * k * f - 2 * p * p * c + 0 / 5 * c + a - 0.5 * k * f + 2 * d * k + b * k) / (k * k + 1) / (p * p - 0.25);

            double v = p * p * c * c + p * p * f * f - 2 * d * f * p * p + d * d - 0.25 * c * c - a * c - a * a - 0.25 * f * f + 2 * d * f - d * d - b * f + b * d + b * b;

            double x = Math.Sqrt(Math.Abs(u * u - v)) - u;
            double y = k * x + f;

            return GeometryCreate.createPoint(x, y);


        }



        #endregion



    }
}
