using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeoAPI.Geometries;
using OSGeo.OGR;
using NoiseAnalysis.Algorithm;
using NoiseAnalysis.SpatialTools;


namespace NoiseAnalysis.SourcePartition
{
    /*!
     * 功能 线声源分割
     * 版本号 1.0
     * 作者 樊晓剑
     * 创建时间  2016年4月27日
     * 修改时间
     */
    class LineSourcePartition
    {
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
                 double[] before=new double[3];
                 double[] after = new double[3];
                 Coordinate conBefore;
                 Coordinate conAfter;
                for (int i = 0; i < line.GetPointCount() - 1; i++)
                {
                    line.GetPoint(i,before);
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
    }
}
