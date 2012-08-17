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
using System.Windows.Forms.DataVisualization.Charting;
using M;
using ModelGen;

namespace VLDBDemo
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
            float r = ((y - miny) / (maxy - miny)) * 300 * 0.9f;
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

            /* System.Drawing.Graphics graphicsObj;
             System.Drawing.Graphics graphicsObj2;
             readdata();
             graphicsObj = panel3.CreateGraphics();//tabPage3.CreateGraphics();
             graphicsObj2 = panel4.CreateGraphics();//tabPage3.CreateGraphics();
             miny = 100000;
             maxy = 0;
             mine = 10000;
             maxe = 0;
             for (int i = 0; i < 500; i++)
             {
                 if (uks[i] > maxy) maxy = uks[i];
                 if (uks[i] < miny) miny = uks[i];
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
             Pen p3 = new Pen(System.Drawing.Color.Blue, 0.5f);
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
                 graphicsObj2.DrawLine(p2, getx(i), gete(m1[i] - uks[i]), getx(i + 1), gete(m1[i + 1] - uks[i + 1]));
                 graphicsObj2.DrawLine(p3, getx(i), gete(m2[i] - uks[i]), getx(i + 1), gete(m2[i + 1] - uks[i + 1]));
             }
             //graphicsObj.DrawLine(p, getx(0) - 2, gety(miny) - 2, getx(500) - 2, gety(miny) - 2);
             String drawString = "Models";
             Font drawFont = new Font("Arial", 16);
             SolidBrush drawBrush = new SolidBrush(Color.Black);
             PointF drawPoint = new PointF(0F, 10.0F);
             e.Graphics.DrawString(drawString, drawFont, drawBrush, drawPoint);
             drawString = "Errors";
             drawFont = new Font("Arial", 16);
             drawBrush = new SolidBrush(Color.Black);
             drawPoint = new PointF(0F, 300 + 10.0F);
             e.Graphics.DrawString(drawString, drawFont, drawBrush, drawPoint);
         */
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
        static int x;
        private void button1_Click(object sender, EventArgs e)
        {
            ArrayList data = GetData.RunQuery(textBox1.Text);
            string s = "<html><body><table>";
            int i = -20;
            StreamWriter sw = new StreamWriter("C:/VLDBDemo_win/data/output.html");
            sw.WriteLine(s);

            int lc = 0;
            ArrayList points = new ArrayList();
            foreach (ItemState row in data)
            {
                string row_str = "<tr><td>";
                Point p = new Point();
                int count = 0;
                int n = row.data.Length;
                string[] rd;
                rd = row.data;
                if ((n == 1) && (row.data[0].Contains('\t')))
                    rd = row.data[0].Split('\t');

                foreach (string item in rd)
                {
                    lc++;
                    if (item != "")
                    {
                        row_str = row_str + item.ToString() + "<td/><td>";
                        if (lc < 3) continue;
                        if (count == 0) p.X = (int)Double.Parse(item);
                        else p.Y = (int)Double.Parse(item);
                        count++;
                    }
                }
                if (lc > 3)
                    if (!(p.X == 0 && p.Y == 0))
                        points.Add(p);

                row_str = row_str + "</tr>";
                i--;
                if (i == 0) break;
                sw.WriteLine(row_str);

            }
            sw.WriteLine("</table></body></html>");
            sw.Close();
            webBrowser1.Url = new Uri("C:/VLDBDemo_win/data/output.html");
            System.Windows.Forms.DataVisualization.Charting.Series series = new System.Windows.Forms.DataVisualization.Charting.Series();
            series.Name = x.ToString();

            chart1.Series.Add(series);
            foreach (Point p in points)
            {
                chart1.Series[x.ToString()].Points.AddXY(p.X, p.Y);
                //chart1.Series["Series2"].Points.AddY(random.Next(5, 75));
            }

            // Set series chart type
            chart1.Series[x.ToString()].ChartType = SeriesChartType.Line;
            //chart1.Series["Series2"].ChartType = SeriesChartType.Spline;

            // Set point labels
            chart1.Series[x.ToString()].IsValueShownAsLabel = false;
            //chart1.Series["Series2"].IsValueShownAsLabel = true;
            chart1.ChartAreas["Default"].CursorX.IsUserEnabled = true;
            chart1.ChartAreas["Default"].CursorX.IsUserSelectionEnabled = true;
            chart1.ChartAreas["Default"].AxisX.ScaleView.Zoomable = true;
            chart1.ChartAreas["Default"].AxisX.ScrollBar.IsPositionedInside = true;
            chart1.ChartAreas["Default"].CursorY.IsUserEnabled = true;
            chart1.ChartAreas["Default"].CursorY.IsUserSelectionEnabled = true;
            chart1.ChartAreas["Default"].AxisY.ScaleView.Zoomable = true;
            chart1.ChartAreas["Default"].AxisY.ScrollBar.IsPositionedInside = true;


            x++;

        }


        private void button2_Click(object sender, EventArgs e)
        {
          M.Model.LoadModules(Global.ukdir+"b.txt");
            treeView1.Nodes.Add(M.Model.buildTree(0));
            /*    Model m=Model.models[0];
                treeView1.Nodes.Add(m.ToString());
           
                TreeNode tn = (TreeNode)treeView1.Nodes[0];
                for(int i=0;i<m.nc;i++){
                 Model cm=Model.models[m.children[i]];
                 tn.Nodes.Add(cm.ToString());
                 }*/
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (ipTextbox.Text.Length > 0)
                Global.ip = ipTextbox.Text;
            if (fileText.Text.Length > 0)
                Global.ukfile = fileText.Text;
            if (dirtextBox.Text.Length > 0)
                Global.ukdir = dirtextBox.Text;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            textBox1.Text = "select id, val from uk limit 100;";
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            textBox1.Text = "select id, val from m_uk[0,99] pinterval=1 layers=1;";
        }

        double[] GetErrorLevels(string str)
        {
            str=str.Trim();
            if (str == "") return null;
            string[] errs = str.Split(',');
            if (errs.Length == 0) return null; 
            double[] errors = new double[errs.Length];
            for (int i = 0; i < errs.Length; i++)
            {
                errors[i] = double.Parse(errs[i]);
            }
            return errors;
        }
        int[] GetFreq(string str)
        {
            string[] freqs = str.Split(',');
            if (str == "") return null;
            if (freqs.Length == 0) return null;
            int[] freq = new int[freqs.Length];
            for (int i = 0; i < freq.Length; i++)
            {
                freq[i] = int.Parse(freqs[i]);
            }
            return freq;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string errors_str = errorTextBox.Text;

            double[] errors = GetErrorLevels(errors_str);
            int[] freqs = GetFreq(freqtextBox.Text);
            int n=1000*1000;
            if(nTextBox.Text!="")
            n = int.Parse(nTextBox.Text);
            if(dirtextBox.Text.Length>0)
            ModelGen.GenTree.dir  = dirtextBox.Text;
            if (fileText.Text.Length > 0)
            ModelGen.GenTree.file = fileText.Text;
            if (errors!=null ) ModelGen.GenTree.errors = errors;
            if (freqs !=null) ModelGen.GenTree.freq = freqs;
            ModelGen.GenTree.build (0, n);
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            textBox1.Text = "select id, val from m_uk pinterval=25000000 layers=0 func='max';";
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            textBox1.Text = "select max(val) from uk;";
        }

        private void linkLabel6_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            textBox1.Text = "select * from m_uk  [162040,162250] pinterval=1 layers=0 ;";

                       
        }
        bool loaded_chart2 = false;
        private void chart2_Click(object sender, EventArgs e)
        {
            if(loaded_chart2==false)
            Loaddata();
            loaded_chart2 = true;
        }

        void Loaddata(){
            System.Windows.Forms.DataVisualization.Charting.Series series = new System.Windows.Forms.DataVisualization.Charting.Series();
            series.Name = "uk";
            
            chart2.Series.Add(series);
            double[] vals = utils.File.ReadData(Global.ukdir + "uk.txt" , 0, 1000000);
            int i=1;
            double min=0;
            double max=0;
            foreach (double p in vals)
            {
                if (p > max) max = p;
                if (p < min) min = p;
                chart2.Series["uk"].Points.AddXY(i++, p);
                //chart1.Series["Series2"].Points.AddY(random.Next(5, 75));
            }
            Global.uk_range = new Range(min, max);
            // Set series chart type
            chart2.Series["uk"].ChartType = SeriesChartType.Line;
            //chart1.Series["Series2"].ChartType = SeriesChartType.Spline;

            // Set point labels
            chart2.Series["uk"].IsValueShownAsLabel = false;
            //chart1.Series["Series2"].IsValueShownAsLabel = true;
            chart2.ChartAreas["Default"].CursorX.IsUserEnabled = true;
            chart2.ChartAreas["Default"].CursorX.IsUserSelectionEnabled = true;
            chart2.ChartAreas["Default"].AxisX.ScaleView.Zoomable = true;
            chart2.ChartAreas["Default"].AxisX.ScrollBar.IsPositionedInside = true;
            chart2.ChartAreas["Default"].CursorY.IsUserEnabled = true;
            chart2.ChartAreas["Default"].CursorY.IsUserSelectionEnabled = true;
            chart2.ChartAreas["Default"].AxisY.ScaleView.Zoomable = false;
            chart2.ChartAreas["Default"].AxisY.ScrollBar.IsPositionedInside = true;

        }

        private void chart1_Click(object sender, EventArgs e)
        {
                      ;
        }

        private void linkLabel8_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
                VerticalLineAnnotation l = new VerticalLineAnnotation();
	            l.AxisX = chart1.ChartAreas["Default"].AxisX;
                ArrayList data = GetData.RunQuery("show grp_len;");
                ItemState row = (ItemState) data[1];
                string[] rd;
                rd = row.data;
                    int interval = int.Parse( row.data[0]);

                    l.AnchorX = 162144 / interval;// int.Parse(data[1]);
	            l.IsInfinitive = true;
                l.ClipToChartArea = chart1.ChartAreas["Default"].Name;
	            l.LineColor = Color.Blue;
                chart1.Annotations.Add(l);
        }

        private void linkLabel9_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            chart1.Series.Clear();
            x = 0;
        }

        private void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            textBox1.Text = "select * from  uk forecast val on id number 10;";

        }

        private void linkLabel7_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            
            textBox1.Text = "select * from  m_uk [16214,16216] pinterval=10 layers=0 func='avg'";
        }

    }
}
