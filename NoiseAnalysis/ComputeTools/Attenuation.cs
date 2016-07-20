using NoiseAnalysis.DataBase;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;

namespace NoiseAnalysis.ComputeTools
{
    /*!
     * 功能 衰减计算
     * 版本号 1.0
     * 作者 樊晓剑
     * 创建时间  2016年7月14日
     * 修改时间
     */
    class Attenuation
    {

        /*!
         * 内容 
         * 版本号 1.0
         * 作者 樊晓剑
         * 创建时间  2016年7月14日
         * 参数 distance 声源点到接收点距离
         * 修改时间
         */
        public double geometryAttenuation(double distance)
        {
            return 20 * Math.Log10(distance) + 11;
        }

        /*!
         * 功能 空气效应衰减
         * 参数 
         * 返回值
         * 版本号 1.0
         * 作者 樊晓剑
         * 创建时间  2016年7月14日
         * 修改时间
         */
        public double airAttenuation(double temperature, double humidity, double frequency)
        {

            double source = 0;
            String sql = "select f" + frequency + " from 1/3ATM where T ='" + temperature + "',and HM=" + humidity+"'";

            Access access = new Access();
            OleDbConnection connection = access.getConn();


            OleDbCommand command = new OleDbCommand(sql, connection);
            OleDbDataReader reader = command.ExecuteReader(); //执行command并得到相应的DataReader
            while (reader.Read())
            {
                source = Convert.ToDouble(reader["v" + frequency]);

            }
            reader.Close();
            access.Close();


            return source;


        }



        #region 地面效应系列

        /*!
 * 功能 地面效应衰减简化模型,忽略地形
 * 参数 hs 声源点的高度
 *      hr接收点高度
 *      distance 距离  
 * 返回值
 * 版本号 1.0
 * 作者 樊晓剑
 * 创建时间  2016年7月14日
 * 修改时间
 */
        public double getGroundAttenuation(double hs, double hr, double distance)
        {
            double hm = (hs + hr) * distance / 2 / Math.Sqrt((hr - hs) * (hr - hs) - distance * distance);


            double agr = 4.8 - (2 * hm / distance) * (17 + 300 / distance);

            if (agr >= 0)
            {
                return agr;
            }
            else
            {
                return 0;
            }
        }


        /*!
         * 功能 地面效应衰减
         * 参数 hs 声源点的高度
         *      hr接收点高度
         *      frequency 频率
         *      distance 声源点和接收点距离
         * 返回值
         * 版本号 1.0
         * 作者 樊晓剑
         * 创建时间  2016年7月14日
         * 修改时间
         */
        public double getGroundAttenuation(double hs, double hr, double frequency, double distance)
        {
            double gm=getGroundParam();
            double sm=getGroundParam();
            double rm=getGroundParam();

            double q=getDistanceParam(hs,hr,distance);

            double Am=getAm(q,gm,frequency);

            double As = getFrequency(sm, frequency, hs, distance);

            double Ar = getFrequency(rm, frequency, hr, distance);

            return As - Ar - Am;

        }
    public double getFrequency(double g, double frequency, double h, double distance)
        {
            double normalFrequency = -1.5;


            if (frequency >= 100 && frequency<=160)
            {
                normalFrequency = normalFrequency + g * getHType(h, "a", distance);
            }
            else if (frequency >= 200 && frequency <= 315)
            {
                normalFrequency = normalFrequency + g * getHType(h, "b", distance);
            }
            else if (frequency >= 400 && frequency <= 630)
            {
                normalFrequency = normalFrequency + g * getHType(h, "c", distance);

            }
            else if (frequency >= 800 && frequency <= 1250)
            {
                normalFrequency = normalFrequency + g * getHType(h, "d", distance);

            }
            else if (frequency > 1600)
            {

                normalFrequency = normalFrequency + (1 - g);
            }


            return normalFrequency;

        }





        public double getHType(double h, String type, double d)
        {
            double e = Math.E;
            switch (type)
            {
                case "a":
                    return 1.5 + 3 * Math.Pow(e, -0.12 * (h - 5) * (h - 5))
                        * (1 - Math.Pow(e, -d / 50))
                        + 5.7 * Math.Pow(e, -0.09 * h * h)
                        * (1 - Math.Pow(e, -2.8 * Math.Pow(10, -6) * d * d));
                case "b":
                    return 1.5 + 8.6 * Math.Pow(e, -0.09 * h * h) * (1 - Math.Pow(e, -d / 50));
                case "c":
                    return 1.5 + 14 * Math.Pow(e, -0.45 * h * h) * (1 - Math.Pow(e, -d / 50));
                case "d":
                    return 1.5 + 5 * Math.Pow(e, -0.9 * h * h) * (1 - Math.Pow(e, -d / 50));
                default:
                    return -1;
            }

        }


        /*!
         * 内容 地面因子计算
         * 版本号 1.0
         * 作者 樊晓剑
         * 创建时间  2016年7月14日
         * 参数 
         * 修改时间
         */
        public double getGroundParam()
        {

            //计划将地面因子和影响长度存为字典或其他键值对，其中没有影响的区域按照0计算，现阶段不做




            return 0;







        }
   
        
        
        
        /*!
         * 功能 计算中间区域衰减
         * 参数 q 距离因子
         *      g 地面因子
         *      frequency 频率
         * 返回值
         * 版本号 1.0
         * 作者 樊晓剑
         * 创建时间  2016年7月14日
         * 修改时间
         */
        public double getAm(double q, double g, double frequency)
        {
            if (frequency < 100)
            {

                return 3 * q;

            }
            else
            {
                return 3 * q * (1 - g);

            }
        }

        public double getDistanceParam(double hs, double hr, double dp)
        {

            if (dp <= 30 * (hr + hs))
            {

                return 0;

            }
            else
            {
                return 1 - 30 * (hr + hs) / dp;
            }
        }

        #endregion






    }
}
