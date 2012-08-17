using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;
using System.IO;
using System.Drawing;
namespace VLDBDemo
{
    public class ItemState
    {
        public string []data;
        //public int b;
    }
    public class GetData
    {
        static NpgsqlConnection conn = null;

        static void connect()
        {
            try
            {
                conn = new NpgsqlConnection("Server="+Global.ip+";Port=5432;User Id=khalefa;Database=t;");
                conn.Open();
            }
            catch (Exception e) { conn = null; }
        }
        static public ArrayList ReadDataFile(string filename)
        {

            StreamReader sr = new StreamReader(filename);
            ArrayList l = new ArrayList();

            String line;
            while ((line = sr.ReadLine()) != null)
            {
                string[] parts = line.Split(' ');
                ItemState i = new ItemState();
                i.data = parts;
                l.Add(i);
            }

            return l;
        }
        static public ArrayList RunQuery(string query)
        {
          //  return ReadDataFile("C:/VLDBDemo_win/data/m.txt");
            Npgsql.NpgsqlCommand c = new NpgsqlCommand();
            if (conn == null) connect();
            if (conn != null)
            {
                c.Connection = conn;
                c.CommandText = query;
                //c.CommandText = "select a, sum(b) from mb group by a error=2  pinterval=1 order by a";
                //c.CommandText = "select a/96, sum(b) from uk3 group by a/96  order by a/96";
                NpgsqlDataReader d = c.ExecuteReader();
                ArrayList ar = new ArrayList();

                //Create header
                ItemState m = new ItemState();
                m.data = new string[d.FieldCount];
                for (int i = 0; i < d.FieldCount; i++)
                {
                    m.data[i] = d.GetName(i);
                }
                ar.Add(m);
                while (d.Read())
                {
                    //for(int i=0;i<d.Depth;i++) 
                    //  Console.WriteLine(d[1].ToString() );
                    m = new ItemState();
                    m.data = new string[d.FieldCount];
                    for (int i = 0; i < d.FieldCount; i++)
                    {
                        m.data[i] = d[i].ToString();
                    }
                    //  m.a = (int)d[0]; m.b =(int) d[1];
                    ar.Add(m);
                }
                return ar;
                //conn.Close();
            }
            //else return ReadDataFile("C:/VLDBDemo_win/data/ukt.txt");
            return null;
        }

        static public ArrayList Scale(ArrayList l, int x, int y)
        {
            int minY=int.MaxValue;
            int maxY=int.MinValue;
            foreach (Point p in l)
            {
                if (minY > p.Y) minY = p.Y;
                if (maxY < p.Y) maxY = p.Y;
            }
            double ratio = (maxY - minY) / y;
            ArrayList pp = new ArrayList();
            foreach(Point p in l) {
                int yy=y-(int) ((p.Y-minY) / ratio);
                int xx = (int)(p.X *x/ l.Count );
                pp.Add(new Point(xx, yy));
            }
            return pp;
        }
    }
}
