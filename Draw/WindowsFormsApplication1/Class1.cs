using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;
namespace WindowsFormsApplication1
{
    public class ItemState
    {
        public string id;
        //public int b;
    }
    public class GetData
    {
        static NpgsqlConnection conn = null;

        static void connect()
        {
            conn = new NpgsqlConnection("Server=192.168.1.34;Port=5432;User Id=khalefa;Database=t;");
            conn.Open();
        }
       
        static public ArrayList RunQuery(string query)
        {
            Npgsql.NpgsqlCommand c = new NpgsqlCommand();
            if (conn == null) connect();
            c.Connection = conn;
            c.CommandText = "select count(*) from uk3 ;";
            c.CommandText = "select a, b from mb  error=2  pinterval=1 order by a";
            c.CommandText = query;
            //c.CommandText = "select a, sum(b) from mb group by a error=2  pinterval=1 order by a";
            //c.CommandText = "select a/96, sum(b) from uk3 group by a/96  order by a/96";
            NpgsqlDataReader d = c.ExecuteReader();
            ArrayList ar = new ArrayList();
            
            //return d;
            while (d.Read())
            {
                //for(int i=0;i<d.Depth;i++) 
                //  Console.WriteLine(d[1].ToString() );
                ItemState m = new ItemState();
              //  m.a = (int)d[0]; m.b =(int) d[1];
                ar.Add(m);
            }
            return ar;
            //conn.Close();
        }
    }
}
