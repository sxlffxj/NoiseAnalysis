using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSGeo.GDAL;
using OSGeo.OGR;
using OSGeo.OSR;

namespace NoiseAnalysis.Algoriam.Spatial
{

    /// <summary>
    /// 三维空间距离计算类
    /// </summary>
    public class Distance3D
    {

        /// <summary>
        /// 计算两点间三维空间距离
        /// </summary>
        /// <param name="point1">第一个点</param>
        /// <param name="point2">第二个点</param>
        /// <returns>两点距离</returns>
        public static double getPointDistance(Geometry point1,Geometry point2)
        {
            return Math.Sqrt(Math.Pow(point1.Distance(point2), 2) + Math.Pow(point2.GetZ(0)-point1.GetZ(0), 2));
        }

        /// <summary>
        /// 计算两点间三维空间距离
        /// </summary>
        /// <param name="point1">第一个点</param>
        /// <param name="point2">第二个点</param>
        /// <returns>两点距离</returns>
        public static float getPointDistance(double[] point1, double[] point2)
        {
            int Dcount=0;
            Dcount = (point1.Count() >= point2.Count()) ? point1.Count() :point2.Count();

            switch (Dcount)
            {
                case 1:
                    return 0;
                case 2:
                    return Math.Sqrt(Math.Pow((point1[0] - point2[0]), 2) + Math.Pow((point1[1] - point2[1]), 2));
                case 3:
                    return Math.Sqrt(Math.Pow((point1[0] - point2[0]), 2) + Math.Pow((point1[1] - point2[1]), 2) + Math.Pow((point1[2] - point2[2]), 2));
                default:
                    return 0;
            }
        }







        /// <summary>
        /// 计算直线三维长度
        /// </summary>
        /// <param name="line">需要计算的直线</param>
        /// <returns>直线长度</returns>
        public static double getLineLength(Geometry line)
        {
            int count =line.GetPointCount();
            double lineLength = 0;
            for(int i=0;i<count-1;i++){
                lineLength += Math.Sqrt(Math.Pow(line.GetZ(i + 1) - line.GetZ(i), 2) 
                    + Math.Pow(line.GetX(i + 1) - line.GetX(i), 2) + Math.Pow(line.GetY(i + 1) - line.GetY(i), 2));
            }
            return lineLength;
        }
    }
}
