using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSGeo.OGR;
using NoiseAnalysis.SpatialTools;

namespace NoiseAnalysis.Algoriam
{
    class Line
    {

/*!
 * 功能 过线外一点的发现与已知线交点
 * 版本号 1.0
 * 作者 樊晓剑
 * 创建时间  2016年7月5日
 * 参数 
 * 修改时间
 */

        public static Geometry getPoint(Geometry line,Geometry point)
        {
            int count =line.GetPointCount()-1;
            double k = (line.GetY(count) - line.GetY(0)) / (line.GetX(count) - line.GetX(0));
            double b = line.GetY(0) - k * line.GetX(0);
            double k1 = -1 / k;
            double b1 = point.GetY(0) - k1 * point.GetX(0);

            double x = (b1 - b) / (k - k1);
            double y = k * x + b;

            return GeometryCreate.createPoint(x,y) ;
        }












    }
}
