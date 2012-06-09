/***************************************************************************

This code is written by Denis Mitrofanov

This code is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or
FITNESS FOR A PARTICULAR PURPOSE.

***************************************************************************/
using System;
using System.Collections.Generic;
using System.Drawing;

namespace m3d
{
    public static class ColorUtils
    {

        public static Color DarkColor(Color color, int percent)
        {
            HsvColor hls = ColorUtils.RgbToHsv(color);

            float l = (float)hls.V / 1000f;
            l = l - ((l * (float)percent) / 100f);
            if (l < 0f) l = 0f;
            if (l > 1f) l = 1f;
            hls.V = (short)(l * 1000f);

            return ColorUtils.HsvToRgb(hls);
        }

        public static Color[] CreateGradient(Color start, Color end, int steps, ColorGradientFlags flags, double saturation)
        {
            HsvColor c0 = ColorUtils.RgbToHsv(start);
            HsvColor c1 = ColorUtils.RgbToHsv(end);

            int value = 0;
            if ((flags & ColorGradientFlags.MinValue) != 0)
            {
                value = (c0.V < c1.V) ? c0.V : c1.V;
            }
            else if ((flags & ColorGradientFlags.MaxValue) != 0)
            {
                value = (c0.V < c1.V) ? c1.V : c0.V;
            }
            else
            {
                value = (c0.V + c1.V) >> 1;
            }

            int h = c0.H;
            int a = c1.H - h;
            if (a < 0)
                a = a + 36000;
            int step = 0;
            if ((flags & ColorGradientFlags.CounterClockwise) == 0)
            {
                step = (int)((double)(a) / (double)(steps - 1));
            }
            else
            {
                step = -(int)((double)(36000 - a) / (double)(steps - 1));
            }

            int s = c0.S;
            int sstep = 0;
            if ((flags & ColorGradientFlags.ReplaceSaturation) != 0)
            {
                s = (int)(saturation * 1000f);
            }
            else if ((flags & ColorGradientFlags.AlignSaturation) == 0)
            {
                sstep = (int)((double)(c1.S - s) / (double)steps);
            }

            List<Color> l = new List<Color>();

            for (int i = 0; i < steps; i++)
            {
                l.Add(ColorUtils.HsvToRgb(new HsvColor(h, s, value)));
                s += sstep;
                h += step;
                if (h < 0)
                {
                    h = h + 36000;
                }
                else if (h > 36000)
                {
                    h = h - 36000;
                }
            }

            return l.ToArray();
        }

        public static Color HsvToRgb(HsvColor hsv)
        {

            double h = (double)hsv.H / 100f;
            double s = (double)hsv.S / 1000f;
            double v = (double)hsv.V / 1000f;

            double chroma = s * v;
            double hdash = h / 60.0;
            double x = chroma * (1.0 - Math.Abs((hdash % 2.0) - 1.0));

            double r = 0f;
            double g = 0f;
            double b = 0f;
            if (hdash < 1.0)
            {
                r = chroma;
                g = x;
            }
            else if (hdash < 2.0)
            {
                r = x;
                g = chroma;
            }
            else if (hdash < 3.0)
            {
                g = chroma;
                b = x;
            }
            else if (hdash < 4.0)
            {
                g = x;
                b = chroma;
            }
            else if (hdash < 5.0)
            {
                r = x;
                b = chroma;
            }
            else if (hdash < 6.0)
            {
                r = chroma;
                b = x;
            }

            double min = v - chroma;

            r += min;
            g += min;
            b += min;

            return Color.FromArgb(
                    (int)(r * 255f),
                    (int)(g * 255f),
                    (int)(b * 255f)
                  );
        }

        private static double FMin(double x, double y)
        {
            return (x < y) ? x : y;
        }

        private static double FMax(double x, double y)
        {
            return (x > y) ? x : y;
        }

        public static HsvColor RgbToHsv(Color rgb)
        {

            double r = (double)rgb.R / 255f;
            double g = (double)rgb.G / 255f;
            double b = (double)rgb.B / 255f;

            double min = ColorUtils.FMin(ColorUtils.FMin(r, g), b);
            double max = ColorUtils.FMax(ColorUtils.FMax(r, g), b);
            double chroma = max - min;

            double h = 0f;
            double s = 0f;

            //If Chroma is 0, then S is 0 by definition, and H is undefined but 0 by convention.
            if (chroma != 0f)
            {
                if (r == max)
                {
                    h = (g - b) / chroma;

                    if (h < 0.0)
                        h += 6.0;
                }
                else if (g == max)
                {
                    h = ((b - r) / chroma) + 2.0f;
                }
                else
                {
                    h = ((r - g) / chroma) + 4.0f;
                }

                h *= 60.0f;
                s = chroma / max;
            }

            return new HsvColor(h, s, max);
        }
    }

    public struct HsvColor
    {
        public int H; // 0..36000 (1/100 degree)
        public short S; // 0..1000
        public short V; // 0..1000

        public HsvColor(double h, double s, double v)
        {
            this.H = (int)(h * 100f);
            this.S = (short)(s * 1000f);
            this.V = (short)(v * 1000f);
        }

        public HsvColor(int h, int s, int v)
        {
            this.H = h;
            this.S = (short)s;
            this.V = (short)v;
        }
    }

    [Flags]
    public enum ColorGradientFlags : ushort
    {
        Empty = 0x0000,
        MinValue = 0x0001,
        MaxValue = 0x0002,
        CounterClockwise = 0x0004,
        AlignSaturation = 0x0008,
        ReplaceSaturation = 0x0010
    }

}