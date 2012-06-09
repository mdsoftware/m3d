/***************************************************************************

This code is written by Denis Mitrofanov

This code is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or
FITNESS FOR A PARTICULAR PURPOSE.

***************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace m3d
{

    public sealed class Space3D
    {
        public const int ColorSignSize = 12;
        private SortedDictionary<int, Color> palette;
        private List<Face3D> faces;

        public Space3D()
        {
            this.palette = new SortedDictionary<int, Color>();
            this.faces = new List<Face3D>();
        }

        public void SortDepth()
        {
            this.faces.Sort(new PointZComparer());
        }

        public int Count
        {
            get { return this.faces.Count; }
        }

        public Face3D this[int i]
        {
            get { return this.faces[i]; }
        }

        public void Add(Face3D face)
        {
            this.faces.Add(face);
        }

        private Color GetColor(int tag)
        {
            Color c;
            if (this.palette.TryGetValue(tag, out c)) return c;
            return Color.Black;
        }

        public void SetColor(int tag, Color color)
        {
            if (this.palette.ContainsKey(tag))
            {
                this.palette[tag] = color;
            }
            else
            {
                this.palette.Add(tag, color);
            }
        }

        public Canvas Draw(int width, int height, float angleX, float angleY, float angleZ, Point3D camera)
        {
            if (this.faces.Count == 0)
                return null;

            Space3D faces = this.Copy();

            Canvas c = new Canvas();
            c.Open(width, height);
            c.DrawBar(Brushes.White, 0, 0, c.Width, c.Height);

            faces.Project((int)(angleX * 10f), (int)(angleY * 10f), (int)(angleZ * 10f), camera);
            faces.SortDepth();

            Limits2D lim = faces.Limits;

            double d = (double)(c.Width - 10) / (double)(lim.MaxX - lim.MinX);
            double scale = (double)(c.Height - 10) / (double)(lim.MaxY - lim.MinY);
            if (scale > d) scale = d;

            List<TopMarks> top = new List<TopMarks>();

            for (int i = 0; i < faces.Count; i++)
            {
                Face3D f = faces[i];
                Color clr = Color.Black;
                if ((f.Tag & 0xff000000) == 0x7f000000)
                {
                    clr = Color.FromArgb((int)(f.Tag >> 16) & 0xff,
                        (int)(f.Tag >> 8) & 0xff,
                        (int)f.Tag & 0xff);
                }
                else
                {
                    clr = this.GetColor(f.Tag);
                }
                int x;
                int y;
                if (f.Count > 1)
                {
                    if (f.Shadow != 0)
                        clr = ColorUtils.DarkColor(clr, f.Shadow);

                    x = (int)((double)(f.Points[0].X - lim.MinX) * scale);
                    y = (int)((double)(f.Points[0].Y - lim.MinY) * scale);
                    Point[] plst = null;
                    if (f.Count > 2)
                    {
                        plst = new Point[f.Count];
                        plst[0] = new Point(x, y);
                    }

                    for (int j = 1; j < f.Count; j++)
                    {
                        int x0 = (int)((double)(f.Points[j].X - lim.MinX) * scale);
                        int y0 = (int)((double)(f.Points[j].Y - lim.MinY) * scale);

                        if (f.Count == 2)
                        {
                            c.DrawLine(new Pen(clr), x, y, x0, y0);
                            break;
                        }
                        else
                        {
                            plst[j] = new Point(x0, y0);
                        }
                    }

                    if (plst != null)
                    {
                        c.FillPolygon(new SolidBrush(clr), plst);
                    }
                }
                else
                {
                    x = (int)((double)(f.Points[0].X - lim.MinX) * scale);
                    y = (int)((double)(f.Points[0].Y - lim.MinY) * scale);
                    Pen pen = null;
                    if (((clr.R + clr.G + clr.B) / 3) > 0x7f)
                    {
                        pen = Pens.Black;
                    }
                    else
                    {
                        pen = Pens.White;
                    }

                    switch (f.Shadow)
                    {
                        case -1:
                            c.TriangleMark(new SolidBrush(clr), pen, x, y, Space3D.ColorSignSize);
                            break;

                        case -2:
                            c.RectangleMark(new SolidBrush(clr), pen, x, y, Space3D.ColorSignSize);
                            break;

                        case -3:
                            c.PointMark(new SolidBrush(clr), x, y);
                            break;

                        case -101:
                            top.Add(new TopMarks(clr, x, y, -1));
                            break;

                        case -102:
                            top.Add(new TopMarks(clr, x, y, -2));
                            break;

                        case -103:
                            top.Add(new TopMarks(clr, x, y, -3));
                            break;

                        default:
                            c.CircleMark(new SolidBrush(clr), pen, x, y, Space3D.ColorSignSize >> 1);
                            break;
                    }

                }
            }

            for (int i = 0; i < top.Count; i++)
            {
                TopMarks t = top[i];

                Color clr = t.Color;

                Pen pen = null;
                if (((clr.R + clr.G + clr.B) / 3) > 0x7f)
                {
                    pen = Pens.Black;
                }
                else
                {
                    pen = Pens.White;
                }
                int x = t.X;
                int y = t.Y;

                switch (t.Shadow)
                {
                    case -1:
                        c.TriangleMark(new SolidBrush(clr), pen, x, y, Space3D.ColorSignSize);
                        break;

                    case -2:
                        c.RectangleMark(new SolidBrush(clr), pen, x, y, Space3D.ColorSignSize);
                        break;

                    case -3:
                        c.PointMark(new SolidBrush(clr), x, y);
                        break;
                }
            }

            return c;
        }

        /// <summary>
        /// Text 3D
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="text"></param>
        /// <param name="location"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="coordinate"></param>
        public void Text(int tag, Text3D text, Text3DLocation location, Text3DFlip flip, float x, float y, float coordinate)
        {
            this.Text(tag, text, location, flip, Point3D.MmToInt(x), Point3D.MmToInt(y), Point3D.MmToInt(coordinate));
        }

        /// <summary>
        /// Text 3D
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="text"></param>
        public void Text(int tag, Text3D text, Text3DLocation location, Text3DFlip flip, int x, int y, int coordinate)
        {
            int px0 = 0;
            int py0 = 0;
            for (int i = 0; i < text.Count; i++)
            {
                Text3DPoint tp = text.Points[i];

                int px1 = x;
                int py1 = y;
                if ((flip & Text3DFlip.X) == 0)
                {
                    px1 += tp.X;
                }
                else
                {
                    px1 -= tp.X;
                }
                if ((flip & Text3DFlip.Y) == 0)
                {
                    py1 += tp.Y;
                }
                else
                {
                    py1 -= tp.Y;
                }

                if (tp.Type == Text3DPointType.LineTo)
                {
                    switch (location)
                    {
                        case Text3DLocation.XY:
                            this.Line(tag, px0, py0, coordinate, px1, py1, coordinate);
                            break;

                        case Text3DLocation.XZ:
                            this.Line(tag, px0, coordinate, py0, px1, coordinate, py1);
                            break;

                        case Text3DLocation.YZ:
                            this.Line(tag, coordinate, px0, py0, coordinate, px1, py1);
                            break;
                    }
                }
                px0 = px1;
                py0 = py1;
            }
        }

        /// <summary>
        /// All measurement in mm and in 1/10 degree. Degrees are in range 0 .. 3600.
        /// </summary>
        /// <returns></returns>
        public int Sector(int tag, double centerX, double centerY, double innerR, double outerR, int startAngle, int endAngle, int stepAngle, double gap, double depth)
        {
            if ((startAngle < 0) || (startAngle > 3600))
                return -1;
            if ((endAngle < 0) || (endAngle > 3600))
                return -1;
            if (innerR >= outerR)
                return -1;

            int count = 0;

            double x0;
            double y0;
            double x1;
            double y1;

            int angle = startAngle;
            Space3D.GetSectorPoint(centerX, centerY, innerR, angle, gap, out x0, out y0);
            Space3D.GetSectorPoint(centerX, centerY, outerR, angle, gap, out x1, out y1);

            Face3D f;
            if (depth > 0)
            {
                f = new Rectangle3D(
                    new Point3D(x0, y0, 0f),
                    new Point3D(x1, y1, 0f),
                    new Point3D(x1, y1, depth),
                    new Point3D(x0, y0, depth));
                f.Tag = tag;
                f.Shadow = 20;
                this.Add(f);
                ++count;
            }

            while (true)
            {
                if (angle >= endAngle)
                    break;
                angle += stepAngle;
                if (angle > endAngle)
                    angle = endAngle;

                double x2;
                double y2;
                double x3;
                double y3;

                double g = 0;
                if (angle >= endAngle)
                    g = -gap;
                Space3D.GetSectorPoint(centerX, centerY, innerR, angle, g, out x2, out y2);
                Space3D.GetSectorPoint(centerX, centerY, outerR, angle, g, out x3, out y3);

                f = new Rectangle3D(
                    new Point3D(x0, y0, 0f),
                    new Point3D(x1, y1, 0f),
                    new Point3D(x3, y3, 0f),
                    new Point3D(x2, y2, 0f));
                f.Tag = tag;
                this.Add(f);
                ++count;

                if (depth > 0)
                {
                    f = new Rectangle3D(
                        new Point3D(x0, y0, 0f),
                        new Point3D(x2, y2, 0f),
                        new Point3D(x2, y2, depth),
                        new Point3D(x0, y0, depth));
                    f.Tag = tag;
                    f.Shadow = this.GetShadow(angle, 30);
                    this.Add(f);
                    ++count;

                    f = new Rectangle3D(
                        new Point3D(x1, y1, 0f),
                        new Point3D(x3, y3, 0f),
                        new Point3D(x3, y3, depth),
                        new Point3D(x1, y1, depth));
                    f.Tag = tag;
                    f.Shadow = this.GetShadow(angle, 30);
                    this.Add(f);
                    ++count;
                }

                if (angle < endAngle)
                {
                    Space3D.GetSectorPoint(centerX, centerY, innerR, angle - 10, 0f, out x0, out y0);
                    Space3D.GetSectorPoint(centerX, centerY, outerR, angle - 10, 0f, out x1, out y1);
                }
                else
                {
                    x0 = x2;
                    y0 = y2;
                    x1 = x3;
                    y1 = y3;
                }
            }

            if (depth > 0)
            {
                f = new Rectangle3D(
                    new Point3D(x0, y0, 0f),
                    new Point3D(x1, y1, 0f),
                    new Point3D(x1, y1, depth),
                    new Point3D(x0, y0, depth));
                f.Tag = tag;
                f.Shadow = 20;
                this.Add(f);
                ++count;
            }

            return count;
        }

        private int GetShadow(int angle, int v)
        {
            return (int)Math.Round((double)v * Math.Sin((double)angle * Math.PI / 1800f));
        }

        private static void GetSectorPoint(double cx, double cy, double r, int angle, double gap, out double x, out double y)
        {
            double a = (double)angle * Math.PI / 1800f;
            if (gap != 0)
            {
                a += gap / (2f * Math.PI * r);
            }
            x = cx + (r * Math.Sin(a));
            y = cy + (r * Math.Cos(a));
        }

        public void Project(int x, int y, int z, Point3D camera)
        {
            RotateCoeffs c = new RotateCoeffs();
            c.Assign(x, y, z);
            for (int i = 0; i < this.Count; i++)
                this[i].Project(c, camera);
        }

        public void ApplyViwer(Point3D viewer)
        {
            for (int i = 0; i < this.Count; i++)
                this[i].ApplyViewer(viewer);
        }

        public Limits2D Limits
        {
            get
            {
                Limits2D lim = Limits2D.Empty();
                for (int i = 0; i < this.Count; i++)
                {
                    Face3D f = this[i];
                    for (int j = 0; j < f.Count; j++)
                        lim.Update(f.Points[j].X, f.Points[j].Y);
                }
                return lim;
            }
        }

        public Face3D Point(int tag, Point3D p)
        {
            Face3D f = new Face3D(1);
            f.Points[0] = p;
            f.Tag = tag;
            this.Add(f);
            return f;
        }

        public Face3D Point(int tag, float x, float y, float z)
        {
            Face3D f = new Face3D(1);
            f.Points[0] = new Point3D(x, y, z);
            f.Tag = tag;
            this.Add(f);
            return f;
        }

        public Face3D Line(int tag, Point3D a, Point3D b)
        {
            Face3D f = new Line3D(a, b);
            f.Tag = tag;
            this.Add(f);
            return f;
        }

        public Face3D Line(int tag, int x0, int y0, int z0, int x1, int y1, int z1)
        {
            Face3D f = new Line3D(new Point3D(x0, y0, z0), new Point3D(x1, y1, z1));
            f.Tag = tag;
            this.Add(f);
            return f;
        }

        public Face3D Line(int tag, float x0, float y0, float x1, float y1, DrawAxis axis)
        {
            return this.Line(tag, x0, y0, x1, y1, 0f, axis);
        }

        public Face3D Line(int tag, float x0, float y0, float x1, float y1, float z, DrawAxis axis)
        {
            switch (axis)
            {
                case DrawAxis.Z:
                    return this.Line(tag, x0, y0, z, x1, y1, z);

                case DrawAxis.Y:
                    return this.Line(tag, x0, z, y0, x1, z, y1);

                case DrawAxis.X:
                    return this.Line(tag, z, x0, y0, z, x1, y1);
            }
            return null;
        }

        public Face3D Line(int tag, float x0, float y0, float z0, float x1, float y1, float z1)
        {
            Face3D f = new Line3D(new Point3D(x0, y0, z0), new Point3D(x1, y1, z1));
            f.Tag = tag;
            this.Add(f);
            return f;
        }

        public Face3D Rectangle(int tag, Point3D a, Point3D b, Point3D c, Point3D d)
        {
            Face3D f = new Rectangle3D(a, b, c, d);
            f.Tag = tag;
            this.Add(f);
            return f;
        }

        public Face3D Triangle(int tag, Point3D a, Point3D b, Point3D c)
        {
            Face3D f = new Triangle3D(a, b, c);
            f.Tag = tag;
            this.Add(f);
            return f;
        }

        public Space3D Copy()
        {
            Space3D f = new Space3D();
            for (int i = 0; i < this.Count; i++)
                f.Add(this[i].Copy());
            return f;
        }

        struct TopMarks
        {
            public Color Color;
            public int X;
            public int Y;
            public int Shadow;

            public TopMarks(Color color, int x, int y, int shadow)
            {
                this.Color = color;
                this.X = x;
                this.Y = y;
                this.Shadow = shadow;
            }
        }
    }

    sealed class PointZComparer : IComparer<Face3D>
    {
        public PointZComparer()
        {
        }

        public int Compare(Face3D x, Face3D y)
        {
            return -x.Depth.CompareTo(y.Depth);
        }
    }

    public enum DrawAxis
    {
        X = 0,
        Y,
        Z
    }
}

