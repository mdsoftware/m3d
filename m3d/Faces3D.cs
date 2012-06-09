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
    /// 3D face
    /// </summary>
    public class Face3D
    {
        protected Point3D[] points;
        private int tag;
        private int shadow;

        public Face3D(int size)
        {
            this.tag = 0;
            this.shadow = 0;
            this.points = new Point3D[size];
        }

        public Face3D Copy()
        {
            Face3D f = new Face3D(this.points.Length);
            for (int i = 0; i < f.points.Length; i++)
                f.points[i] = this.points[i];
            f.tag = this.tag;
            f.shadow = this.shadow;
            return f;
        }

        public Point3D[] Points
        {
            get { return this.points; }
        }

        public int Depth
        {
            get
            {
                if (this.points == null)
                    return 0;
                int x = 0;
                for (int i = 0; i < this.points.Length; i++)
                    x += this.points[i].Z;
                return (int)Math.Round((double)x / (double)this.points.Length, 0);
            }
        }

        public Face3D(List<Point3D> points)
            : this(points.Count)
        {
            for (int i = 0; i < this.points.Length; i++)
                this.points[i] = points[i];
        }

        public Face3D(Point3D[] points)
            : this(points.Length)
        {
            this.points = new Point3D[points.Length];
            for (int i = 0; i < this.points.Length; i++)
                this.points[i] = points[i];
        }

        public void Project(RotateCoeffs rotate, Point3D camera)
        {
            if (this.points == null)
                return;
            for (int i = 0; i < this.points.Length; i++)
                this.points[i].Project(rotate, camera);
        }

        public void ApplyViewer(Point3D viewer)
        {
            if (this.points == null)
                return;
            for (int i = 0; i < this.points.Length; i++)
                this.points[i].ApplyViewer(viewer);
        }

        public int Tag
        {
            get { return this.tag; }
            set { this.tag = value; }
        }

        public int Shadow
        {
            get { return this.shadow; }
            set { this.shadow = value; }
        }

        public int Count
        {
            get { return this.points.Length; }
        }
    }

    /// <summary>
    /// 3D triangle
    /// </summary>
    public sealed class Triangle3D : Face3D
    {
        public Triangle3D(Point3D a, Point3D b, Point3D c)
            : base(3)
        {
            this.points[0] = a;
            this.points[1] = b;
            this.points[2] = c;
        }
    }

    /// <summary>
    /// 3D rectangle
    /// </summary>
    public sealed class Rectangle3D : Face3D
    {
        public Rectangle3D(Point3D a, Point3D b, Point3D c, Point3D d)
            : base(4)
        {
            this.points[0] = a;
            this.points[1] = b;
            this.points[2] = c;
            this.points[3] = d;
        }
    }

    /// <summary>
    /// 3D line
    /// </summary>
    public sealed class Line3D : Face3D
    {
        public Line3D(Point3D a, Point3D b)
            : base(2)
        {
            this.points[0] = a;
            this.points[1] = b;
        }
    }


}