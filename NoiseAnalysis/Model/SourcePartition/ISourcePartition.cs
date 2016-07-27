using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSGeo.OGR;

namespace NoiseAnalysis.Model.SourcePartition
{
    /*!
     * 功能 线声源分割接口
     * 版本号 1.0
     * 作者 樊晓剑
     * 创建时间  2016年4月27日
     * 修改时间
     */
    interface ISourcePartition
    {
        /*!
         * 功能 根据距离网格大小分割接收点
         * 参数 
         * 返回值
         * 版本号 1.0
         * 作者 樊晓剑
         * 创建时间  2016年4月27日
         * 修改时间
         */
        Queue<Geometry> staticPartition(double distance, Geometry receiveArea, double height);

    }
}
