using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using m3d;

namespace Demo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.DrawChart();
            this.timer.Enabled = true;
        }

        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.DrawText();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            this.view.Rotate(0.5f, 0.5f, 0.5f);
        }

        private void chartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.DrawChart();
        }

        private void DrawChart()
        {
            Space3D space = new Space3D();

            space.SetColor(100, Color.LightGray);

            float h = 100f;

            Form1.DrawScale(space, h, DrawAxis.X);
            Form1.DrawScale(space, h, DrawAxis.Y);
            Form1.DrawScale(space, h, DrawAxis.Z);

            int steps = 40;

            Color[] gradient = ColorUtils.CreateGradient(Color.Red, Color.Green, steps, ColorGradientFlags.Empty, 0.5f);
            for (int i = 0; i < gradient.Length; i++)
                space.SetColor(i, gradient[i]);

            
            float step = h / (float)steps;

            float x0 = 0f;
            for (int i = 0; i < steps; i++)
            {
                float y0 = 0f;
                float x1 = x0 + step;
                for (int j = 0; j < steps; j++)
                {
                    float y1 = y0 + step;
                    Form1.Plane(space, x0, y0, x1, y1, h, gradient.Length - 1);
                    y0 = y1;
                }
                x0 = x1;
            }


            this.view.Space = space;
        }

        private static void Plane(Space3D faces, float x1, float y1, float x2, float y2, float h, int maxGradient)
        {
            Point3D p0 = new Point3D(x1, y1, Form1.Func(x1, y1));
            Point3D p1 = new Point3D(x1, y2, Form1.Func(x1, y2));
            Point3D p2 = new Point3D(x2, y2, Form1.Func(x2, y2));
            Point3D p3 = new Point3D(x2, y1, Form1.Func(x2, y1));

            float z = (float)(Point3D.IntToMm(p0.Z + p1.Z + p2.Z + p3.Z) / 4f);

            int c = 0;
            if (z < 0f)
            {
                c = 0;
            }
            else if (z > h)
            {
                c = maxGradient;
            }
            else
            {
                c = (int)(z / h * (float)maxGradient);
            }

            faces.Rectangle(c, p0, p1, p2, p3);
         
        }

        private static float Func(float x, float y)
        {
            double t = 0.065f;
            /*
            return 50f + 40f * (float)(Math.Sin(t * (double)x) * Math.Cos(t * (double)y));
             */
            double xx = x * t;
            double yy = y * t;
            return 40f + (float)((xx * xx) * Math.Sin(xx) + (yy * yy) * Math.Cos(yy));
        }

        private static void DrawScale(Space3D faces, float scaleSize, DrawAxis axis)
        {
            faces.Line(100, 0, 0, 0, scaleSize, axis);
            faces.Line(100, 0, scaleSize, scaleSize, scaleSize, axis);
            faces.Line(100, scaleSize, scaleSize, scaleSize, 0, axis);
            faces.Line(100, scaleSize, 0, 0, 0, axis);

            int steps = 10;

            float step = scaleSize / (float)steps;

            float x = step;

            for (int i = 1; i <= 9; i++)
            {
                faces.Line(100, x, 0, x, scaleSize, axis);
                faces.Line(100, 0, x, scaleSize, x, axis);
                x += step;
            }
        }

        private static int ColorToTag(Color color)
        {
            return 0x7f000000 | (color.ToArgb() & 0xffffff);
        }

        private void DrawText()
        {
            Space3D space = new Space3D();

            // Prepare text points and lines
            Text3D text = Text3D.FromString("This is a test string...", new Font("Times New Roman", 24), 10f, FontStyle.Regular, 50);
            
            // Set color for tag 1, if color is need to be specified directly use 0x7f000000 | rgb color (0xrrggbb)
            space.SetColor(0, Color.Black);
            space.Text(0, text, Text3DLocation.XY, Text3DFlip.None, 10f, 10f, 10f);

            text = Text3D.FromString("This is another test string...", new Font("Arial", 24), 10f, FontStyle.Bold, 50);
            space.Text(Form1.ColorToTag(Color.Orange), text, Text3DLocation.XZ, Text3DFlip.None, 10f, 10f, 10f);


            space.SetColor(1, Color.SandyBrown);
            space.SetColor(2, Color.RoyalBlue);
            space.SetColor(3, Color.SeaGreen);
            for (int i = 0; i < 5; i++)
            {
                text = Text3D.FromString(String.Format("String #{0}", i + 1), new Font("Courier New", 24), 10f, FontStyle.Italic, 50);
                space.Text(i % 4, text, Text3DLocation.YZ, Text3DFlip.None, 10f, 10f * (float)i, 10f);
            }


            this.view.Space = space;
        }
    }
}
