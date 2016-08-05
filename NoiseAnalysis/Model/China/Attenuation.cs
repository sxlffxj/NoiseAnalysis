using NoiseAnalysis.DataBase;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using OSGeo.GDAL;
using OSGeo.OGR;
using OSGeo.OSR;
using NoiseAnalysis.Algoriam.Spatial;

namespace NoiseAnalysis.Model.China
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

        float m_temperature;
        float m_humidity;
        float m_frequency;


        /// <summary>
        /// 初始化参数
        /// </summary>
        /// <param name="distance">两点距离</param>
        /// <param name="temperature">温度</param>
        /// <param name="humidity">湿度</param>
        /// <param name="frequency">频率</param>
        public Attenuation(float temperature,float humidity,float frequency)
        {

            m_temperature = temperature;
            m_humidity = humidity;
            m_frequency = frequency;
        }


        /// <summary>
        /// 衰减计算
        /// </summary>
        /// <param name="LW">源强</param>
        /// <param name="path">传播路径</param>
        /// <returns>衰减后源强</returns>
        public float[] getAttenuation(float LW, float[] pathList, float hr, float hs,float distance)
        {
            float[] attenuation = new float[5];

            float though=throughAttenuation( LW,  hs, hr,distance);









            //分别计算衰减
            if (pathList[1] == null)
            {
                //有直射情况
                attenuation[0] = though;
                attenuation[1] = 0;
                attenuation[2] = 0;
                attenuation[3] = 0;
                //反射
                attenuation[4] = 2 * though;
            }
            else
            {
                //无直射情况
                attenuation[0] = 0;
                attenuation[1] = 0;
                attenuation[2] = 0;
                attenuation[3] = 0;
                attenuation[4] = 0;

            }

     


            return attenuation;

        }













        /*!
         * 功能 直达衰减
         * 参数 LW 源强
         *      distance 接收点与声源点之间的直线距离
         *      temperature 温度
         *      humidity 湿度
         *      frequency 频率
         *      hs 声源点高度
         *      hr 接收点高度
         * 返回值
         * 版本号 1.0
         * 作者 樊晓剑
         * 创建时间  2016年7月27日
         * 修改时间
         */
        private float throughAttenuation(float LW, float hs, float hr,float distance)
        {

            //几何衰减
            float Adiv = getGeometryAttenuation(distance);

            //地面衰减
            float Agr = getGroundAttenuation(hs, hr, distance);

            //大气衰减
            float Aatm = getAirAttenuation(m_temperature, m_humidity, m_frequency);


            return LW - Adiv - Aatm - Agr-11;


        }

        /// <summary>
        /// 绕射衰减
        /// </summary>
        /// <param name="flankLine">绕射路径</param>
        /// <param name="flankType">绕射类型</param>
        /// <param name="distance">直线距离</param>
        /// <returns></returns>
        private float flankAttenuation(Geometry flankLine,bool flankType,float distance)
        {
          //  D_(z,i)=10lg[3+(C_2⁄λ) C_3 zK_met]

            float flank = 0;//绕射衰减

            //参数
            float C2 = 20;
            float C3 = 0;

            float z = 0;//声程差
            float Kmet = 0;//参数
            float wavelength = 340 / m_frequency;//波长


            double[] point0=new double[3];
            double[] point1=new double[3];
            double[] point2=new double[3];
            double[] point3 =new double[3];
            flankLine.GetPoint(0, point0);
            flankLine.GetPoint(1, point1);
            flankLine.GetPoint(flankLine.GetPointCount() - 2, point2);
            flankLine.GetPoint(flankLine.GetPointCount()-1, point3);

            float dss=(float)Distance3D.getPointDistance(point0,point1);
            float e=(float)Distance3D.getPointDistance(point2,point1);
            float dst=(float)Distance3D.getPointDistance(point3,point2);
            float d = (float)Distance3D.getPointDistance(point3, point0);

            if(flankType){
                C3 = 1;
                z = (float)Math.Sqrt((dss + dst) * (dss + dst) + distance * distance) - d;
            }
            else
            {
                C3=(1+(5*wavelength/e)*(5*wavelength/e))/(1/3+(5*wavelength/e)*(5*wavelength/e));
                z = (float)Math.Sqrt((dss + dst + e) * (dss + dst + e) + distance * distance) - d;

            }
            Kmet = z <= 0 ? 1 : (float)Math.Pow(e, -(1 / 2000) * Math.Sqrt(dss * dst * d / 2 / z));
            flank = 10 * (float)Math.Log10(3 + (C2 / wavelength) * C3 * z * Kmet);
            return flank;

        }




















        /*!
         * 功能 计算几何衰减
         * 参数 distance 声源点与接收点距离
         * 返回值 几何衰减值
         * 作者 樊晓剑
         * 创建时间  2016年7月27日
         * 修改时间
         */
        private float getGeometryAttenuation(float distance)
        {
            return 20 * (float)Math.Log10(distance) + 11;
        }

        /*!
         * 功能 空气效应衰减
         * 参数 temperature 温度
         *      humidity 湿度
         *      frequency 频率
         * 返回值 空气效应衰减值
         * 版本号 1.0
         * 作者 樊晓剑
         * 创建时间  2016年7月14日
         * 修改时间
         */
        private float getAirAttenuation(float temperature, float humidity, float frequency)
        {

            float source = 0;
            String sql = "select f" + frequency + " from 1/3ATM where T ='" + temperature + "',and HM=" + humidity + "'";

            NoiseAnalysis.DataBase.Access access = new NoiseAnalysis.DataBase.Access();
            OleDbConnection connection = access.getConn();


            OleDbCommand command = new OleDbCommand(sql, connection);
            OleDbDataReader reader = command.ExecuteReader(); //执行command并得到相应的DataReader
            while (reader.Read())
            {
                source = (float)(reader["v" + frequency]);

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
        private float getGroundAttenuation(float hs, float hr, float distance)
        {
            double hm = ((hs + hr) * distance / 2 / Math.Sqrt((hr - hs) * (hr - hs) - distance * distance));


            double agr = 4.8 - ((2 * hm / distance) * (17 + 300 / distance));

            if (agr >= 0)
            {
                return (float)agr;
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
        private float getGroundAttenuation(float hs, float hr, float frequency, float distance)
        {
            float gm = getGroundParam();
            float sm = getGroundParam();
            float rm = getGroundParam();

            float q = getDistanceParam(hs, hr, distance);

            float Am = getAm(q, gm, frequency);

            float As = getFrequency(sm, frequency, hs, distance);

            float Ar = getFrequency(rm, frequency, hr, distance);

            return As - Ar - Am;

        }
        private float getFrequency(float g, float frequency, float h, float distance)
        {
            double normalFrequency = -1.5f;


            if (frequency >= 100 && frequency <= 160)
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


            return (float)normalFrequency;

        }





        private double getHType(float h, String type, float d)
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
        private float getGroundParam()
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
        private float getAm(float q, float g, float frequency)
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

        private float getDistanceParam(float hs, float hr, float dp)
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
