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
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace m3d
{
    public enum Text3DLocation
    {
        XY = 0,
        XZ,
        YZ
    }

    [Flags]
    public enum Text3DFlip
    {
        None = 0,
        X = 1,
        Y = 2
    }

    public sealed class Text3D
    {
        private List<Text3DPoint> points;

        public Text3D()
        {
            this.points = new List<Text3DPoint>();
        }

        public int Count
        {
            get { return this.points.Count; }
        }

        public List<Text3DPoint> Points
        {
            get { return this.points; }
        }

        public static Text3D FromString(string s, Font font, double height, FontStyle style, int flatFactor)
        {
            GraphicsPath path = new GraphicsPath();
            int h = Point3D.MmToInt(height);
            path.AddString(s, font.FontFamily, (int)style, h, new RectangleF(0, 0, Single.MaxValue, Single.MaxValue), StringFormat.GenericDefault);
            if (flatFactor <= 0)
                flatFactor = 100;
            path.Flatten(null, (Single)h / (Single)flatFactor);
            PathData pd = path.PathData;

            Text3D t = new Text3D();

            int px = 0;
            int py = 0;
            int px0 = 0;
            int py0 = 0;
            for (int i = 0; i < pd.Points.Length; i++)
            {
                int px1 = (int)pd.Points[i].X;
                int py1 = (int)pd.Points[i].Y;

                switch ((byte)((int)pd.Types[i] & 0x07))
                {
                    case (byte)PathPointType.Line:
                        t.points.Add(new Text3DPoint(Text3DPointType.LineTo, px1, py1));
                        break;

                    case (byte)PathPointType.Start:
                        px0 = px1;
                        py0 = py1;
                        t.points.Add(new Text3DPoint(Text3DPointType.StartPoint, px1, py1));
                        break;
                }
                if (((int)pd.Types[i] & (int)PathPointType.CloseSubpath) != 0)
                    t.points.Add(new Text3DPoint(Text3DPointType.LineTo, px0, py0));

                px = px1;
                py = py1;
            }

            return t;
        }

    }

    public struct Text3DPoint
    {
        public Text3DPointType Type;
        public int X; // in 1/1000 mm
        public int Y; // in 1/1000 mm

        public Text3DPoint(int x, int y)
        {
            this.Type = Text3DPointType.StartPoint;
            this.X = x;
            this.Y = y;
        }

        public Text3DPoint(Text3DPointType type, int x, int y)
            : this(x, y)
        {
            this.Type = type;
            this.X = x;
            this.Y = y;
        }

        public Text3DPoint(double x, double y)
        {
            this.Type = Text3DPointType.StartPoint;
            this.X = Point3D.MmToInt(x);
            this.Y = Point3D.MmToInt(y);
        }

        public override string ToString()
        {
            return String.Format("{0} X:{1} Y:{1}", this.Type,
                Point3D.IntToMm(this.X).ToString("#########0.0##"),
                Point3D.IntToMm(this.Y).ToString("#########0.0##"));
        }
    }

    public enum Text3DPointType : byte
    {
        StartPoint = 0,
        LineTo
    }
}