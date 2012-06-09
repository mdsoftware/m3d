/***************************************************************************

This code is written by Denis Mitrofanov

This code is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or
FITNESS FOR A PARTICULAR PURPOSE.

***************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace m3d
{
    /// <summary>
    /// 3D point where internal measurement is in 1/1000 mm. So range is +/-20m.
    /// All is in 1/1000mm, doubles are in mm.
    /// </summary>
    public struct Point3D
    {
        private int x;
        private int y;
        private int z;

        public Point3D(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Point3D(double x, double y, double z)
            : this(Point3D.MmToInt(x), Point3D.MmToInt(y), Point3D.MmToInt(z))
        {
        }

        public Point3D(float x, float y, float z)
            : this(Point3D.MmToInt(x), Point3D.MmToInt(y), Point3D.MmToInt(z))
        {
        }

        public static Point3D Empty()
        {
            return new Point3D(0, 0, 0);
        }

        public static int MmToInt(double mm)
        {
            return (int)Math.Round(mm * 1000f, 0);
        }

        public static double IntToMm(int i)
        {
            return (double)((double)i / 1000f);
        }

        public int X
        {
            get { return this.x; }
            set { this.x = value; }
        }

        public int Y
        {
            get { return this.y; }
            set { this.y = value; }
        }

        public int Z
        {
            get { return this.z; }
            set { this.z = value; }
        }

        public int[] Vector
        {
            get
            {
                int[] v = new int[3];
                v[0] = this.x;
                v[1] = this.y;
                v[2] = this.z;
                return v;
            }
            set
            {
                if (value.Length != 3)
                    return;
                this.x = value[0];
                this.y = value[1];
                this.z = value[2];
            }
        }

        public void ApplyViewer(Point3D viewer)
        {
            double x = Point3D.IntToMm(this.x);
            double y = Point3D.IntToMm(this.y);
            double z = Point3D.IntToMm(this.z);

            double ex = Point3D.IntToMm(viewer.x);
            double ey = Point3D.IntToMm(viewer.y);
            double ez = Point3D.IntToMm(viewer.z);
            x = (x - ex) * (ez / z);
            y = (y - ey) * (ez / z);

            this.x = Point3D.MmToInt(x);
            this.y = Point3D.MmToInt(y);
            this.z = Point3D.MmToInt(z);
        }

        public void Project(RotateCoeffs rotate, Point3D camera)
        {
            double x = Point3D.IntToMm(this.x);
            double y = Point3D.IntToMm(this.y);
            double z = Point3D.IntToMm(this.z);

            x -= Point3D.IntToMm(camera.x);
            y -= Point3D.IntToMm(camera.y);
            z -= Point3D.IntToMm(camera.z);

            if ((rotate.CosX != 1f) && (rotate.SinX != 0f))
            {
                double y0 = y;
                y = (y0 * rotate.CosX) + (z * -rotate.SinX);
                z = (y0 * rotate.SinX) + (z * rotate.CosX);
            }

            if ((rotate.CosY != 1f) && (rotate.SinY != 0f))
            {
                double x0 = x;
                x = (x0 * rotate.CosY) + (z * rotate.SinY);
                z = (x0 * -rotate.SinY) + (z * rotate.CosY);
            }

            if ((rotate.CosZ != 1f) && (rotate.SinZ != 0f))
            {
                double x0 = x;
                x = (x0 * rotate.CosZ) + (y * -rotate.SinZ);
                y = (x0 * rotate.SinZ) + (y * rotate.CosZ);
            }

            this.x = Point3D.MmToInt(x);
            this.y = Point3D.MmToInt(y);
            this.z = Point3D.MmToInt(z);
        }

        public override string ToString()
        {
            return String.Format("X:{0}({1}) Y:{2}({3}) Z:{4}({ColorSpace.Gap})",
                Point3D.IntToMm(this.x).ToString("##############0.0#"), this.x,
                Point3D.IntToMm(this.y).ToString("##############0.0#"), this.y,
                Point3D.IntToMm(this.z).ToString("##############0.0#"), this.z);
        }
    }

    public struct Limits2D
    {
        private int minX;
        private int maxX;
        private int minY;
        private int maxY;

        public static Limits2D Empty()
        {
            Limits2D l = new Limits2D();
            l.minX = Int32.MaxValue;
            l.minY = Int32.MaxValue;
            l.maxX = Int32.MinValue;
            l.maxY = Int32.MinValue;
            return l;
        }

        public void Update(int x, int y)
        {
            if (x > this.maxX)
                this.maxX = x;
            if (x < this.minX)
                this.minX = x;
            if (y > this.maxY)
                this.maxY = y;
            if (y < this.minY)
                this.minY = y;
        }

        public int MinX
        {
            get { return this.minX; }
        }

        public int MaxX
        {
            get { return this.maxX; }
        }

        public int MinY
        {
            get { return this.minY; }
        }

        public int MaxY
        {
            get { return this.maxY; }
        }
    }

    public struct RotateCoeffs
    {
        private double sinX;
        private double cosX;
        private double sinY;
        private double cosY;
        private double sinZ;
        private double cosZ;

        public static RotateCoeffs Empty()
        {
            RotateCoeffs c = new RotateCoeffs();
            c.sinX = 0f;
            c.cosX = 1f;
            c.sinY = 0f;
            c.cosY = 1f;
            c.sinZ = 0f;
            c.cosZ = 1f;
            return c;
        }

        public double SinX
        {
            get { return this.sinX; }
        }

        public double CosX
        {
            get { return this.cosX; }
        }

        public double SinY
        {
            get { return this.sinY; }
        }

        public double CosY
        {
            get { return this.cosY; }
        }

        public double SinZ
        {
            get { return this.sinZ; }
        }

        public double CosZ
        {
            get { return this.cosZ; }
        }

        /// <summary>
        /// Assign using x, y, and z in 1/10 of degrees.
        /// Values must have a range (-3600 to 3600).
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void Assign(int x, int y, int z)
        {
            this.sinX = 0f;
            this.cosX = 1f;
            this.sinY = 0f;
            this.cosY = 1f;
            this.sinZ = 0f;
            this.cosZ = 1f;
            if ((x != 0) && (x > -3600) && (x < 3600))
            {
                double a = ((double)x * Math.PI) / 1800f;
                this.sinX = Math.Sin(a);
                this.cosX = Math.Cos(a);
            }
            if ((y != 0) && (y > -3600) && (y < 3600))
            {
                double a = ((double)y * Math.PI) / 1800f;
                this.sinY = Math.Sin(a);
                this.cosY = Math.Cos(a);
            }
            if ((z != 0) && (z > -3600) && (z < 3600))
            {
                double a = ((double)z * Math.PI) / 1800f;
                this.sinZ = Math.Sin(a);
                this.cosZ = Math.Cos(a);
            }
        }

    }
}