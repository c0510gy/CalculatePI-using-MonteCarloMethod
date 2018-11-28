using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GetPI_MonteCarloAlgorithm
{
    public partial class Form1 : Form
    {
        GetPI MC;
        int cnt = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void StartMC()
        {
            while (true)
            {
                cnt++;
                MC.Next();
                pictureBox1.Image = MC.Visual;
                label1.Text = cnt + "th PI = " + MC.pi;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Thread tpa = new Thread(StartMC);
            tpa.Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
            MC = new GetPI(1);

            if (!System.IO.Directory.Exists("./imgs"))
            {
                System.IO.Directory.CreateDirectory("./imgs");
            }
        }
    }

    class GetPI
    {
        public int N = 0;
        public int r; //원 반지름
        public int P_in = 0; //원 내부 점 개수
        public int P_out = 0; //원 외부 점 개수
        public double pi;
        public Bitmap Visual;
        public int acc = 1000;

        DrawGraph dg;

        public GetPI(int t_r)
        {
            r = t_r;
            Visual = new Bitmap(r * acc + 50 + 50 + 2200, r * acc + 250 + 50);

            Graphics g = Graphics.FromImage(Visual);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
           
            g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
            g.FillRectangle(Brushes.White, 0, 0, r * acc + 50 + 50 + 2200, r * acc + 250 + 50);
            SolidBrush sb = new SolidBrush(Color.Black);
            g.FillEllipse(sb, -r * acc + 50, -r * acc + 50, r * acc * 2, r * acc * 2);

            g.FillRectangle(Brushes.White, 0, 0, 50, r * acc + 50);
            g.FillRectangle(Brushes.White, 0, 0, r * acc + 50, 50);

            for (int y = 0; 100 * y <= r * acc; y++)
            {
                g.DrawLine(new Pen(Color.Black, 1), 0 + 50, 100 * y + 50, r * acc + 50, 100 * y + 50);
                
                Graphics g_ = Graphics.FromImage(Visual);
                g_.SmoothingMode = SmoothingMode.AntiAlias;
                g_.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g_.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g_.DrawString(((float)y / (float)10).ToString(), new Font("맑은 고딕", 25), Brushes.Black, 0, 100 * y);
            }
            for (int x = 0; 100 * x <= r * acc; x++)
            {
                g.DrawLine(new Pen(Color.Black, 1), 100 * x + 50, 0 + 50, 100 * x + 50, r * acc + 50);

                Graphics g_ = Graphics.FromImage(Visual);
                g_.SmoothingMode = SmoothingMode.AntiAlias;
                g_.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g_.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g_.DrawString(((float)x / (float)10).ToString(), new Font("맑은 고딕", 25), Brushes.Black, 100 * x + (x == 10 ? 25 : 0), 0);
            }

            dg = new DrawGraph(ref Visual, 1000, 4, r * acc + 50 + 150, 50, 2000, 1000, "랜덤 점 개수", "계산된 값");
            dg.AddGoal(ref Visual, 3.14159265359, "π");
        }

        Random rnd = new Random(DateTime.Now.Millisecond);
        public void Next()
        {
            Graphics g = Graphics.FromImage(Visual);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            double x = rnd.NextDouble() * (double)r;
            double y = rnd.NextDouble() * (double)r;
            
            if(x*x + y*y <= r * r)
            {
                //Visual.SetPixel((int)(x * acc) + 50, (int)(y * acc) + 50, Color.Red);
                g.FillEllipse(new SolidBrush(Color.Red), (int)(x * acc) + 50 - 2, (int)(y * acc) + 50 - 2, 4, 4);
                P_in++;
            }
            else
            {
                //Visual.SetPixel((int)(x * acc) + 50, (int)(y * acc) + 50, Color.Blue);
                g.FillEllipse(new SolidBrush(Color.Blue), (int)(x * acc) + 50 - 2, (int)(y * acc) + 50 - 2, 4, 4);
                P_out++;
            }
            
            pi = (double)P_in / (double)(P_in + P_out) * 4;
            
            g.FillRectangle(Brushes.White, 0, r * acc + 50, r * acc, 250);
            g.DrawString("랜덤 점 개수 : " + (P_in + P_out) + "개\n1/4 원 내부 랜덤 점 개수 : " + P_in + "개\n1/4 원 외부 랜덤 점 개수 : " + P_out + "개\n" + "계산된 π=" + pi.ToString(), new Font("맑은 고딕", 30), Brushes.Black, 50, r * acc + 50 + 25);

            dg.addValue(ref Visual, pi);

            Visual.Save("./imgs/" + (P_in + P_out) + ".png", System.Drawing.Imaging.ImageFormat.Png);
        }
    }

    //그래프 그리는 class
    class DrawGraph
    {
        public int MaxX;
        public int MaxY;
        public int GWidth;
        public int GHeight;
        public int LX, LY;

        double preY = 0, preX = 0; //이전 X,Y값
        double goal; //목표값
        bool goal_exist = false;
        string goal_str; //목표치 설명

        string Xaxis_str;
        string Yaxis_str;

        public DrawGraph(ref Bitmap img, int max_x, int max_y, int x_, int y_, int gWidth, int gHeight, string Xaxis, string Yaxis)
        {
            MaxX = max_x;
            MaxY = max_y;
            GWidth = gWidth;
            GHeight = gHeight;
            LX = x_;
            LY = y_;

            Xaxis_str = Xaxis;
            Yaxis_str = Yaxis;

            Graphics g = Graphics.FromImage(img);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            g.DrawLine(new Pen(Color.Black, 5), LX, LY + GHeight, LX + GWidth, LY + GHeight); //X축
            g.DrawLine(new Pen(Color.Black, 5), LX, LY + GHeight, LX, LY); //Y축

            StringFormat stringFormat = new StringFormat();
            stringFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            g.DrawString(Yaxis_str, new Font("맑은 고딕", 25), Brushes.Black, LX - 50, LY + GHeight / 2 - Yaxis.Length * 50 / 2, stringFormat);
            g.DrawString(Xaxis_str, new Font("맑은 고딕", 25), Brushes.Black, LX + GWidth / 2 - Xaxis.Length * 50 / 2, LY + GHeight + 50);

            for (int x = 0; 100 * x <= GWidth; x++)
            {
                g.DrawLine(new Pen(Color.Black, 1), LX + 100 * x, LY + GHeight, LX + 100 * x, LY);
                
                g.DrawString((((float)x * 100 / (float)GWidth) * (float)MaxX).ToString(), new Font("맑은 고딕", 25), Brushes.Black, LX + 100 * x, LY + GHeight);
            }
            for (int y = 0; 100 * y <= GHeight; y++)
            {
                g.DrawLine(new Pen(Color.Black, 1), LX, LY + GHeight - 100 * y, LX + GWidth, LY + GHeight - 100 * y);
                
                g.DrawString((((float)y * 100 / (float)GHeight) * (float)MaxY).ToString(), new Font("맑은 고딕", 25), Brushes.Black, LX, LY + GHeight - 100 * y);
            }
        }

        //목표값 추가
        public void AddGoal(ref Bitmap img, double y, string goal_string)
        {
            goal_exist = true;
            goal = y;
            goal_str = goal_string;

            Graphics g = Graphics.FromImage(img);
            g.DrawLine(new Pen(Color.Blue, 5), LX, LY + (GHeight - (float)(GHeight / MaxY * y)), LX + GWidth, LY + (GHeight - (float)(GHeight / MaxY * y)));
            
            g.DrawString(goal_string, new Font("맑은 고딕", 25), Brushes.Black, LX + GWidth - goal_string.Length * 50, LY + (GHeight - (float)(GHeight / MaxY * y)));
        }
        
        //값 추가
        public void addValue(ref Bitmap img, double y)
        {
            Graphics g = Graphics.FromImage(img);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            Pen p = new Pen(Color.Red, 5);
            p.StartCap = LineCap.Round;
            p.EndCap = LineCap.Round;
            g.DrawLine(p, LX + (float)(GWidth / MaxX * (preX % MaxX)), LY + (GHeight - (float)(GHeight / MaxY * preY)), LX + (float)(GWidth / MaxX * ((preX + 1) % MaxX)), LY + (GHeight - (float)(GHeight / MaxY * y)));
            preX++; preY = y;
            if (preX % MaxX == 0) ClearToNext(ref img);
        }

        public void ClearToNext(ref Bitmap img)
        {
            Graphics g = Graphics.FromImage(img);

            g.FillRectangle(Brushes.White, LX - 50, LY - 50, GWidth + 150, GHeight + 150);
            
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            g.DrawLine(new Pen(Color.Black, 5), LX, LY + GHeight, LX + GWidth, LY + GHeight); //X축
            g.DrawLine(new Pen(Color.Black, 5), LX, LY + GHeight, LX, LY); //Y축

            StringFormat stringFormat = new StringFormat();
            stringFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            g.DrawString(Yaxis_str, new Font("맑은 고딕", 25), Brushes.Black, LX - 50, LY + GHeight / 2 - Yaxis_str.Length * 50 / 2, stringFormat);
            g.DrawString(Xaxis_str, new Font("맑은 고딕", 25), Brushes.Black, LX + GWidth / 2 - Xaxis_str.Length * 50 / 2, LY + GHeight + 50);

            for (int x = 0; 100 * x <= GWidth; x++)
            {
                g.DrawLine(new Pen(Color.Black, 1), LX + 100 * x, LY + GHeight, LX + 100 * x, LY);
                
                g.DrawString((((float)x * 100 / (float)GWidth) * (float)MaxX + preX).ToString(), new Font("맑은 고딕", 25), Brushes.Black, LX + 100 * x, LY + GHeight);
            }
            for (int y = 1; 100 * y <= GHeight; y++)
            {
                g.DrawLine(new Pen(Color.Black, 1), LX, LY + GHeight - 100 * y, LX + GWidth, LY + GHeight - 100 * y);
                
                g.DrawString((((float)y * 100 / (float)GHeight) * (float)MaxY).ToString(), new Font("맑은 고딕", 25), Brushes.Black, LX, LY + GHeight - 100 * y);
            }

            if (goal_exist) AddGoal(ref img, goal, goal_str);
        }
    }
}
