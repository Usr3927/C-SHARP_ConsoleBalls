using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace ConsoleBalls
{
    class Canvas : Control
    {
        public Canvas() {
            DoubleBuffered = true;
        }
    }   
    class Program :Form
    {
        private Timer timer = new Timer();
        private Canvas canvas = new Canvas();
        private Canvas histogram = new Canvas();
        private double[,] particles = new double[64,4];
        private int[] nV=new int[0];
        private double v0 = 400.0;
        public Program() {
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            this.canvas.Paint += new PaintEventHandler(canvasPaint);
            //this.canvas.Dock = DockStyle.fill;
            this.canvas.Location = new Point(0, 0);
            this.canvas.Size = new Size(512, 512);
            this.canvas.Margin= new Padding(0,0,0,0);
            this.Controls.Add(canvas);

            this.histogram.Paint += new PaintEventHandler(histogramPaint);
            this.histogram.Location = new Point(512, 0);
            this.histogram.Size = new Size(256, 256);
            this.histogram.Margin = new Padding(0, 0, 0, 0);
            this.Controls.Add(histogram);

            timer.Tick += new EventHandler(timerTick);
            timer.Interval = 40;
            timer.Enabled = true;

            Random rnd = new Random(0);
            for(int i=0; i<8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    particles[i * 8 + j, 0] = -128 + 32 * i;
                    particles[i * 8 + j, 1] = -128 + 32 * j;
                    particles[i * 8 + j, 2] = (rnd.NextDouble() - 0.5) * 2.0 * v0 / 40.0;
                    particles[i * 8 + j, 3] = (rnd.NextDouble() - 0.5) * 2.0 * v0 / 40.0;
                }
            }
        }

        private void timerTick(object sender, EventArgs e)
        {
            bool discsOverlap = false;
            do {
                for (int i = 0; i < particles.GetLength(0); i++)
                {
                    for (int j = i + 1; j < particles.GetLength(0); j++)
                    {
                        double normDsp =
                            (particles[i, 0] + particles[i, 2] - particles[j, 0] - particles[j, 2]) * (particles[i, 0] + particles[i, 2] - particles[j, 0] - particles[j, 2]) +
                            (particles[i, 1] + particles[i, 3] - particles[j, 1] - particles[j, 3]) * (particles[i, 1] + particles[i, 3] - particles[j, 1] - particles[j, 3]);
                        double norm = Math.Sqrt(
                            (particles[i, 0] - particles[j, 0]) * (particles[i, 0] - particles[j, 0]) +
                            (particles[i, 1] - particles[j, 1]) * (particles[i, 1] - particles[j, 1]));
                        double[] xy = new double[2] { particles[i, 0] - particles[j, 0], particles[i, 1] - particles[j, 1] };
                        double[] iVel = new double[2] {
                        xy[0]/norm*(particles[j,2]*xy[0]/norm + particles[j,3]*xy[1]/norm) -xy[1]/norm*(particles[i,2]*-xy[1]/norm + particles[i,3]*xy[0]/norm),
                        xy[1]/norm*(particles[j,2]*xy[0]/norm + particles[j,3]*xy[1]/norm) +xy[0]/norm*(particles[i,2]*-xy[1]/norm + particles[i,3]*xy[0]/norm)};
                        double[] jVel = new double[2] {
                        xy[0]/norm*(particles[i,2]*xy[0]/norm + particles[i,3]*xy[1]/norm) -xy[1]/norm*(particles[j,2]*-xy[1]/norm + particles[j,3]*xy[0]/norm),
                        xy[1]/norm*(particles[i,2]*xy[0]/norm + particles[i,3]*xy[1]/norm) +xy[0]/norm*(particles[j,2]*-xy[1]/norm + particles[j,3]*xy[0]/norm)};
                        particles[i, 2] = normDsp < 16 * 16 ? iVel[0] : particles[i, 2];
                        particles[i, 3] = normDsp < 16 * 16 ? iVel[1] : particles[i, 3];
                        particles[j, 2] = normDsp < 16 * 16 ? jVel[0] : particles[j, 2];
                        particles[j, 3] = normDsp < 16 * 16 ? jVel[1] : particles[j, 3];
                    }
                }
                
                for (int i = 0; i < particles.GetLength(0); i++)
                {
                    double dspX = particles[i, 2];
                    double dspY = particles[i, 3];
                    particles[i, 2] = particles[i, 0] + dspX < this.canvas.Width / 2.0 - 8 && particles[i, 0] + dspX > -this.canvas.Width / 2.0 + 8 ?
                        particles[i, 2] : -Math.Sign(particles[i, 0]) * Math.Abs(particles[i, 2]);
                    particles[i, 3] = particles[i, 1] + dspY < this.canvas.Height / 2.0 - 8 && particles[i, 1] + dspY > -this.canvas.Height / 2.0 + 8 ?
                        particles[i, 3] : -Math.Sign(particles[i, 1]) * Math.Abs(particles[i, 3]);

                }

                discsOverlap = false;
                for (int i = 0; i < particles.GetLength(0); i++)
                {
                    for (int j = i + 1; j < particles.GetLength(0); j++)
                    {
                        double normDsp =
                            (particles[i, 0] + particles[i, 2] - particles[j, 0] - particles[j, 2]) * (particles[i, 0] + particles[i, 2] - particles[j, 0] - particles[j, 2]) +
                            (particles[i, 1] + particles[i, 3] - particles[j, 1] - particles[j, 3]) * (particles[i, 1] + particles[i, 3] - particles[j, 1] - particles[j, 3]);
                        discsOverlap = normDsp < 16 * 16 ? true : discsOverlap;
                    }
                }
            } while (discsOverlap);

            int nBins = (int)Math.Floor(0.5 + Math.Sqrt(particles.GetLength(0)));
            nV = new int[nBins];
            double vMax = 2.0 * Math.Sqrt(2) * v0 / 40;
            for (int i = 0; i < particles.GetLength(0); i++)
            {
                double dspX = particles[i, 2];
                double dspY = particles[i, 3];
                particles[i, 0] += particles[i, 0] + dspX < this.canvas.Width / 2.0 - 8 && particles[i, 0] + dspX > -this.canvas.Width / 2.0 + 8 ?
                    dspX : -dspX - particles[i, 0] + (this.canvas.Width / 2.0 - 8.0) * Math.Sign(particles[i, 0] + dspX);
                particles[i, 1] += particles[i, 1] + dspY < this.canvas.Height / 2.0 - 8 && particles[i, 1] + dspY > -this.canvas.Height / 2.0 + 8 ?
                    dspY : -dspY - particles[i, 1] + (this.canvas.Height / 2.0 - 8.0) * Math.Sign(particles[i, 1] + dspY);

                double vParticle = Math.Sqrt(particles[i, 3] * particles[i, 3] + particles[i, 2] * particles[i, 2]);
                if (vParticle < vMax)
                {
                    nV[(int)Math.Floor((double)nBins * (vParticle / vMax))]++;
                }

            }

            this.canvas.Invalidate(new Rectangle(new Point(0, 0), new Size(this.canvas.Width, this.canvas.Height)));
            this.canvas.Update();
            this.histogram.Invalidate(new Rectangle(new Point(0, 0), new Size(this.histogram.Width, this.histogram.Height)));
            this.histogram.Update();
        }

        private void canvasPaint(object sender, PaintEventArgs e)
        {
            SolidBrush blackBrush = new SolidBrush(Color.Black);
            SolidBrush redBrush = new SolidBrush(Color.Red);
            
            Rectangle rect = new Rectangle(0, 0, this.canvas.Width, this.canvas.Height);
            e.Graphics.FillRectangle(blackBrush, rect);
            for (int i = 0; i < particles.GetLength(0); i++)
            {
                Rectangle ellipseRectangle = new Rectangle(
                    (int)Math.Floor(particles[i, 0] + 0.5 + this.canvas.Width / 2 -8),
                    (int)Math.Floor(particles[i, 1] + 0.5 + this.canvas.Height / 2 -8), 16, 16);
                e.Graphics.FillEllipse(redBrush, ellipseRectangle);
            }
        }
        private void histogramPaint(object sender, PaintEventArgs e)
        {
            SolidBrush blackBrush = new SolidBrush(Color.Black);
            SolidBrush redBrush = new SolidBrush(Color.Red);
            Pen blackPen = new Pen(blackBrush);
            Rectangle rect = new Rectangle(0, 0, this.histogram.Width, this.histogram.Height);
            e.Graphics.FillRectangle(blackBrush, rect);
            int width = this.histogram.Width;
            int height = this.histogram.Height;
            int max = 0;
            for (int i = 0; i < nV.GetLength(0); i++)
            {
                max = nV[i] > max ? nV[i] : max;
            }
            if (max != 0)
            {
                for (int i = 0; i < nV.GetLength(0); i++)
                {
                    Rectangle bin = new Rectangle(i * width / nV.GetLength(0), (max-nV[i]) * height / max, width / nV.GetLength(0), height);
                    e.Graphics.FillRectangle(redBrush, bin);
                    e.Graphics.DrawRectangle(blackPen, bin);

                }
            }

        }

        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.Run(new Program());
        }
    }
}
