using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NoiseAnalysis.DataBase;
using System.Data.OleDb;

namespace NoiseAnalysis.ComputeTools
{
    class RainwayPreprocess
    {

        public double getSpurce(String speed, String type)
        {
            double source = 0;
            String sql = "select v" + speed + " from TRAINSOURCE where TRAINTYPE ='" + type + "'";

            Access access = new Access();
            OleDbConnection connection=  access.getConn();


            OleDbCommand command = new OleDbCommand(sql, connection);
            OleDbDataReader reader = command.ExecuteReader(); //执行command并得到相应的DataReader
            while (reader.Read())
            {
                source =Convert.ToDouble( reader["v"+speed]);

            }
            reader.Close();
            access.Close();


            return source;

        }

















    }
}
