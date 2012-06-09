using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using m3d;

namespace Demo
{
    public partial class View3D : UserControl
    {

        private Space3D space;

        private float angleX;
        private float angleY;
        private float angleZ;

        public View3D()
        {
            InitializeComponent();
            this.space = null;
            this.angleX = this.angleY = this.angleZ = 0f;
            this.DoubleBuffered = true;
        }

        public Space3D Space
        {
            set
            {
                this.space = value;
                this.Invalidate();
            }
        }

        public void Rotate(float x, float y, float z)
        {
            this.angleX = View3D.AddAngle(angleX, x);
            this.angleY = View3D.AddAngle(angleY, y);
            this.angleZ = View3D.AddAngle(angleZ, z);
            this.Invalidate();
        }

        private static float AddAngle(float angle, float add)
        {
            angle += add;
            while (angle < 0f) angle += 360f;
            while (angle > 360f) angle -= 360f;
            return angle;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.FillRectangle(Brushes.White, this.ClientRectangle);
            if (space == null)
                return;
            Canvas canvas = this.space.Draw(this.ClientRectangle.Width - 4, this.ClientRectangle.Height - 4,
                this.angleX, this.angleY,this.angleZ, Point3D.Empty());
            if (canvas == null)
                return;
            canvas.DrawOn(new Point(2, 2), e.Graphics);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.Invalidate();
        }
    }
}
