

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSGeo.OGR;
using OSGeo.OSR;
using GeoAPI.Geometries;
using System.Collections;


namespace NoiseAnalysis.Algoriam.Spatial
{
    /*!
 * 功能 对图层投影的操作
 * 版本号 1.0
 * 作者 樊晓剑
 * 创建时间  2016年4月21日
 * 修改时间
 */
    public static class ProjectionUtility
    {
        /*!
         * 内容 默认投影的EPSG编码，默认WGS84
         * 版本号 1.0
         * 作者 樊晓剑
         * 创建时间  2016年4月21日
         * 修改时间
         */
        public const int EPSG_ID = 3395;

        public const String EPSG_NAME = "WGS_1984_World_Mercator";




        // private readonly TransformMethod _transform;

        /*!
        * 功能 
        * 参数 
        * 返回值
        * 版本号 1.0
        * 作者 樊晓剑
        * 创建时间  2016年4月21日
        * 修改时间
        */
        public Geometry projectionConvert(Geometry toConvert, OSGeo.OSR.SpatialReference projection)
        {
            double[] point = new double[3];
            double[] topoint = new double[3];
            //获取投影,判断投影类型，如果符合要求则返回
            if (toConvert.GetSpatialReference().AutoIdentifyEPSG() != EPSG_ID)
            {


                OSGeo.OSR.CoordinateTransformation transformation = new OSGeo.OSR.CoordinateTransformation(toConvert.GetSpatialReference(), projection);


                for (int i = 0; i < toConvert.GetPointCount(); i++)
                {
                    toConvert.GetPoint(i, point);

                    transformation.TransformPoint(topoint, point[0], point[1], point[2]);

                    Geometry geometry = new Geometry(wkbGeometryType.wkbPolygon);
                    geometry.AddPoint(topoint[0], topoint[1], topoint[2]);
                    return geometry;

                }


                //若不符合要求则进行转换


                //返回转换结果输出路径

            }

            return toConvert;

        }






    }
}
