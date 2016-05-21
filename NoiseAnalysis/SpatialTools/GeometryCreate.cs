using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeoAPI.Geometries;
using OSGeo.OGR;

namespace NoiseAnalysis.SpatialTools
{
   static class GeometryCreate
    {
        /**
         * 根据坐标构建折线
         * @date 2016年3月1日
         * @author sxlffxj
         * @return 构造的折线
         */
        public static Geometry createLineString3D(Coordinate p0, Coordinate p1)
        {
            Geometry geometry = new Geometry(wkbGeometryType.wkbLineString);
            geometry.AddPoint(p0.X,p0.Y,p0.Z);
            geometry.AddPoint(p1.X, p1.Y,p1.Z);
            return geometry;
        }

        /**
         * 根据坐标构建折线
         * @date 2016年3月1日
         * @author sxlffxj
         * @return 构造的折线
         */
        public static Geometry createLineString3D(double x0, double y0, double z0, double x1, double y1, double z1)
        {
            Geometry geometry = new Geometry(wkbGeometryType.wkbLineString);
            geometry.AddPoint(x0, y0, z0);
            geometry.AddPoint(x1, y1, z1);
            return geometry;
        }
        /**
  * 根据坐标构建折线
  * @date 2016年3月1日
  * @author sxlffxj
  * @return 构造的折线
  */
        public static Geometry createLineString(double x0, double y0,  double x1, double y1)
        {
            Geometry geometry = new Geometry(wkbGeometryType.wkbLineString);
            geometry.AddPoint_2D(x0, y0);
            geometry.AddPoint_2D(x1, y1);
            return geometry;
        }
     /**
     * 根据坐标值构建点
     * 
     * @date 2015年12月21日
     * @author sxlffxj
     * @param x
     *            x坐标
     * @param y
     *            y坐标
     * @return 点状数据
     */
        public static Geometry createPoint(double x, double y)
        {
            Geometry geometry = new Geometry(wkbGeometryType.wkbPoint);
            geometry.AddPoint_2D(x, y);
            return geometry;
        }

        public static Geometry createPoint3D(double x, double y, double z)
        {
            Geometry geometry = new Geometry(wkbGeometryType.wkbPoint);
            geometry.AddPoint(x, y, z);
            return geometry;
        }

    }
}
