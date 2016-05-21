using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoiseAnalysis.ComputeTools
{
    class SourceStrength
    {
        /*!
         * 功能 线声源源强离散计算
         * 参数 Lpower线声源源强，lenght离散后长度
         * 返回值 离散后源强
         * 版本号 1.0
         * 作者 樊晓剑
         * 创建时间  2016年5月10日
         * 修改时间
         */
        public double getLineSource(double Lpower,double lenght)
        {
            return Math.Log10(Math.Pow(10, Lpower / 10) * lenght) * 10;

        }

        /*!
         * 功能 面声源源强离散计算
         * 参数 Lpower线声源源强，lenght离散后边长
         * 返回值 离散后源强
         * 版本号 1.0
         * 作者 樊晓剑
         * 创建时间  2016年5月10日
         * 修改时间
         */
        public double getPolygonSource(double Lpower, double lenght)
        {

            return Math.Log10(Math.Pow(10, Lpower / 10) * lenght*lenght) * 10;

        }










    }
}
