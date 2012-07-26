using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Drawing2D;
using Npgsql;
namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
       static public int[] uks;
       static public float[] m1;
       static public float[] m2;
        public Form1()
        {
            InitializeComponent();
        }

        float getx(float x)
        {
            return x / 500 * 527;
        }

        float gety(float y)
        {
            float r=((y-miny)/(maxy-miny))*300*0.9f;
            return 300 - r;
        }
        float gete(float y)
        {
            float r = ((y - mine) / (maxe - mine)) * (121) * 0.9f;
            return 121 - r;
        }

        float miny;
        float maxy;
        float mine;
        float maxe;
       
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
          
            System.Drawing.Graphics graphicsObj;
           System.Drawing.Graphics graphicsObj2;
            readdata();
            graphicsObj = panel3.CreateGraphics();//tabPage3.CreateGraphics();
            graphicsObj2 = panel4.CreateGraphics();//tabPage3.CreateGraphics();
            miny = 100000;
            maxy = 0;
            mine=10000;
            maxe = 0;
            for (int i = 0; i < 500; i++)
            {
               if (uks[i] > maxy) maxy = uks[i];
               if (uks[i]  < miny) miny = uks[i];
              float err = m1[i] - uks[i];
               if (err > maxe) maxe = err;
               if (err < mine) mine = err;
               err = m2[i] - uks[i];
               if (err > maxe) maxe = err;
               if (err < mine) mine = err;
               
            }

            Pen p = new Pen(System.Drawing.Color.Black, 2);

           Pen p1 = new Pen(System.Drawing.Color.Black, 1);
           Pen p2 = new Pen(System.Drawing.Color.Red, 0.5f);
           Pen p3 = new Pen(System.Drawing.Color.Blue,0.5f);
           p2.DashStyle = DashStyle.Dot;
           p3.DashStyle = DashStyle.Dash;
            for (int i = 0; i < 500; i++)
            {
                graphicsObj.DrawLine(p1, getx(i), gety(uks[i]), getx(i + 1), gety(uks[i + 1]));
                graphicsObj.DrawLine(p2, getx(i), gety(m1[i]), getx(i + 1), gety(m1[i + 1]));
                graphicsObj.DrawLine(p3, getx(i), gety(m2[i]), getx(i + 1), gety(m2[i + 1]));
            }

            for (int i = 0; i < 500; i++)
            {
                graphicsObj2.DrawLine(p2, getx(i), gete(m1[i] - uks[i]), getx(i + 1), gete(m1[i+1] - uks[i + 1]));
                graphicsObj2.DrawLine(p3, getx(i), gete(m2[i]-uks[i]), getx(i + 1), gete(m2[i+1]-uks[i+1]));
            }
            //graphicsObj.DrawLine(p, getx(0) - 2, gety(miny) - 2, getx(500) - 2, gety(miny) - 2);
              String drawString = "Models" ;
              Font drawFont = new Font("Arial", 16);
              SolidBrush drawBrush = new SolidBrush(Color.Black);
              PointF drawPoint = new PointF(0F, 10.0F);
              e.Graphics.DrawString(drawString, drawFont, drawBrush, drawPoint);
               drawString = "Errors";
              drawFont = new Font("Arial", 16);
              drawBrush = new SolidBrush(Color.Black);
              drawPoint = new PointF(0F, 300+10.0F);
              e.Graphics.DrawString(drawString, drawFont, drawBrush, drawPoint);
        }
        void readdata()
        {
            try
            {
                uks = new int[672];
                m1 = new float[672];
                m2 = new float[672];
                int i = 0;
                using (StreamReader sr = new StreamReader("c:/data/uk.s"))
                {
                    String line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        uks[i++] = int.Parse(line);
                    }
                }
                i = 0;
                using (StreamReader sr = new StreamReader("c:/data/m1"))
                {
                    String line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        m1[i++] = float.Parse(line);
                    }
                }
                 i = 0;
                using (StreamReader sr = new StreamReader("c:/data/m2"))
                {
                    String line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        m2[i++] = float.Parse(line);
                    }
                }
            }
            catch (Exception e)
            {
                // Let the user know what went wrong.
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
           ArrayList data = GetData.RunQuery(textBox1.Text);
           //  .DataSource = itemStates;
            string s="<html><body><table>";
            int i = -20;
            StreamWriter sw = new StreamWriter("C:/VLDBDemo_win/data/output.html");
            StreamWriter csw = new StreamWriter("C:/VLDBDemo_win/data/curve.html");
            sw.WriteLine(s);
            Point p = new Point();
            int lc = 0;
                
            foreach(ItemState row in data )
            {
                string row_str="<tr><td>";
                int count = 0;
                foreach (string item in row.data)
                {
                    lc++;
                    if (item != "")
                    {
                        row_str = row_str + item.ToString() + "<td/><td>";
                        if (lc < 3) continue;
                        if (count == 0) p.X = (int)Double.Parse(item);
                        else p.Y = (int)Double.Parse(item)-26000;                                 
                        count++;
                    }
                   
                    
                }
                p.Y = p.Y / 2;
                csw.WriteLine("ctx.lineTo(" + p.X*10 + "," + p.Y + ");");
                row_str=row_str+"</tr>";
                i--;
                if (i == 0) break;
                sw.WriteLine(row_str);

            }
            sw.WriteLine( "</table></body></html>");
            sw.Close();
            csw.Close();
            webBrowser1.Url = new Uri("C:/VLDBDemo_win/data/output.html");
            
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }

        
    }
}
