using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.OleDb;

namespace NoiseAnalysis.DataBase
{
    class Access
    {
        // 变量声明处
        public OleDbConnection Conn;
        public string ConnString = "Provider=Microsoft.Jet.OLEDB.4.0 ;Data Source=" + Environment.CurrentDirectory + "\\data\\MODEL.mdb";

        public OleDbConnection getConn()
        {
            Conn = new OleDbConnection(ConnString);
            Conn.Open(); 
            return (Conn);
        }
     public void Close()   
        {   
            Conn.Close();   
        }  

    }
}
