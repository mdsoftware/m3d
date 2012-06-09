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
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;

namespace m3d
{
    /// <summary>
    /// Simpe bitmap chart canvas.
    /// </summary>
    public class Canvas
    {

        private Bitmap bmp;
        private Graphics gfx;

        /// <summary>
        /// Constructor.
        /// </summary>
        public Canvas()
        {
            this.bmp = null;
            this.gfx = null;
        }

        /// <summary>
        /// Return specified string bound rectangle.
        /// </summary>
        public Size MeasureText(Font font, string s)
        {
            if (this.gfx == null)
                return new Size(0, 0);
            SizeF size = this.gfx.MeasureString(s, font);
            return new Size((int)size.Width, (int)size.Height);
        }

        /// <summary>
        /// Draw horisontal text from specified text.
        /// </summary>
        public void DrawTextH(Font font, Brush brush, string s, int x, int y)
        {
            StringFormat format = new StringFormat(StringFormatFlags.NoClip);
            gfx.DrawString(s, font, brush, x, y, format);
        }

        /// <summary>
        /// Draw horisontal text from specified text.
        /// </summary>
        public void DrawTextH(Font font, Color color, string s, int x, int y)
        {
            StringFormat format = new StringFormat(StringFormatFlags.NoClip);
            SolidBrush brush = new SolidBrush(color);
            gfx.DrawString(s, font, brush, x, y, format);
            brush = null;
        }

        /// <summary>
        /// Draw vertical text from specified text.
        /// </summary>
        public void DrawTextV(Font font, Brush brush, string s, int x, int y)
        {
            StringFormat format = new StringFormat(StringFormatFlags.NoClip | StringFormatFlags.DirectionVertical);
            gfx.DrawString(s, font, brush, x, y, format);
        }

        /// <summary>
        /// Draw vertical text from specified text.
        /// </summary>
        public void DrawTextV(Font font, Color color, string s, int x, int y)
        {
            StringFormat format = new StringFormat(StringFormatFlags.NoClip | StringFormatFlags.DirectionVertical);
            SolidBrush brush = new SolidBrush(color);
            gfx.DrawString(s, font, brush, x, y, format);
            brush = null;
        }

        /// <summary>
        /// Draw line using specified pen.
        /// </summary>
        public void DrawLine(Pen pen, int x1, int y1, int x2, int y2)
        {
            if (this.gfx == null)
                return;
            this.gfx.DrawLine(pen, x1, y1, x2, y2);
        }

        /// <summary>
        /// Draw bar filled with specified brush.
        /// </summary>
        public void DrawBar(Brush brush, int x, int y, int width, int height)
        {
            if (this.gfx == null)
                return;
            this.gfx.FillRectangle(brush, x, y, width, height);
        }

        public void FillPolygon(Brush brush, Point[] points)
        {
            if (this.gfx == null)
                return;
            this.gfx.FillPolygon(brush, points);
        }

        public void CircleMark(Brush brush, Pen pen, int x, int y, int size)
        {
            if (this.gfx == null)
                return;
            this.gfx.FillEllipse(brush, x - size, y - size, size << 1, size << 1);
            if (pen != null)
                this.gfx.DrawEllipse(pen, x - size, y - size, size << 1, size << 1);
        }

        public void TriangleMark(Brush brush, Pen pen, int x, int y, int size)
        {
            if (this.gfx == null)
                return;

            Point[] p = new Point[3];

            int f = size >> 1;
            p[0] = new Point(x - f, y - f);
            p[1] = new Point(x + f, y - f);
            p[2] = new Point(x, y + f);

            this.gfx.FillPolygon(brush, p);

            this.gfx.DrawLine(pen, p[0], p[1]);
            this.gfx.DrawLine(pen, p[1], p[2]);
            this.gfx.DrawLine(pen, p[2], p[0]);
        }

        public void RectangleMark(Brush brush, Pen pen, int x, int y, int size)
        {
            if (this.gfx == null)
                return;

            Point[] p = new Point[4];

            int f = size >> 1;
            p[0] = new Point(x - f, y - f);
            p[1] = new Point(x - f, y + f);
            p[2] = new Point(x + f, y + f);
            p[3] = new Point(x + f, y - f);

            this.gfx.FillPolygon(brush, p);

            this.gfx.DrawLine(pen, p[0], p[1]);
            this.gfx.DrawLine(pen, p[1], p[2]);
            this.gfx.DrawLine(pen, p[2], p[3]);
            this.gfx.DrawLine(pen, p[3], p[0]);
        }

        public void PointMark(Brush brush, int x, int y)
        {
            if (this.gfx == null)
                return;

            this.gfx.FillEllipse(brush, x - 2, y - 2, 4, 4);
        }

        /// <summary>
        /// Draw bar filled with specified color.
        /// </summary>
        public void DrawBar(Color color, int x, int y, int width, int height)
        {
            if (this.gfx == null)
                return;
            SolidBrush brush = new SolidBrush(color);
            this.gfx.FillRectangle(brush, x, y, width, height);
            brush = null;
        }

        /// <summary>
        /// Width.
        /// </summary>
        public int Width
        {
            get { return (this.bmp == null) ? -1 : this.bmp.Width; }
        }

        /// <summary>
        /// Height.
        /// </summary>
        public int Height
        {
            get { return (this.bmp == null) ? -1 : this.bmp.Height; }
        }

        /// <summary>
        /// Opens new canvas.
        /// </summary>
        public void Open(int width, int height)
        {
            this.Close();

            this.bmp = new Bitmap(width, height);
            this.gfx = Graphics.FromImage(bmp);

            this.gfx.PageUnit = GraphicsUnit.Pixel;
            this.gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            this.gfx.TextRenderingHint = TextRenderingHint.AntiAlias;
        }

        /// <summary>
        /// Close all of the things.
        /// </summary>
        public void Close()
        {
            try
            {
                if (this.gfx != null)
                {
                    this.gfx.Dispose();
                    this.gfx = null;
                }
                if (this.bmp != null)
                {
                    this.bmp.Dispose();
                    this.bmp = null;
                }
            }
            catch
            {
            }
            this.gfx = null;
            this.bmp = null;
        }

        /// <summary>
        /// Save image as GIF.
        /// </summary>
        public void SaveGif(string fileName)
        {
            if (this.bmp == null)
                return;
            if (File.Exists(fileName))
                File.Delete(fileName);
            this.bmp.Save(fileName, ImageFormat.Gif);
        }

        /// <summary>
        /// Save image as JPG.
        /// </summary>
        public void SaveJpeg(string fileName, long quality)
        {
            if (this.bmp == null)
                return;
            if (File.Exists(fileName))
                File.Delete(fileName);
            ImageCodecInfo codecInfo = this.GetEncoder("image/jpeg");
            if (codecInfo == null)
            {
                this.bmp.Save(fileName, ImageFormat.Jpeg);
            }
            else
            {
                System.Drawing.Imaging.Encoder encoder = System.Drawing.Imaging.Encoder.Quality;
                System.Drawing.Imaging.EncoderParameters paramList = new EncoderParameters(1);
                paramList.Param[0] = new System.Drawing.Imaging.EncoderParameter(encoder, quality);
                ImageFormat fmt = ImageFormat.Jpeg;
                this.bmp.Save(fileName, codecInfo, paramList);
            }
        }

        public void SaveJpeg(string fileName)
        {
            this.SaveJpeg(fileName, 100);
        }

        public void DrawOn(Point location, Graphics destination)
        {
            if (this.bmp == null)
                return;
            destination.DrawImage(this.bmp, location);
        }

        public void DrawOn(Graphics destination)
        {
            this.DrawOn(new Point(0, 0), destination);
        }

        /// <summary>
        /// Returns specified encoder.
        /// </summary>
        private ImageCodecInfo GetEncoder(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }
    }
}
