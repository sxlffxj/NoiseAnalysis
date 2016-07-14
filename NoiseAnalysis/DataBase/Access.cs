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

        public OleDbConnection getConn()
        {
            string connstr = "Provider=Microsoft.Jet.OLEDB.4.0 ;Data Source=F:\\web\\notesbook\\class\\leavenotes.mdb";
            OleDbConnection tempconn = new OleDbConnection(connstr);
            
            return (tempconn);
        }


    }
}
