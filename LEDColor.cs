using System;

namespace SimpleLed
{
    public class LEDColor
    {
        public int Red { get; set; }
        public int Green { get; set; }
        public int Blue { get; set; }

        public string AsString() => Red + "," + Green + "," + Blue;

        public LEDColor(int r, int g, int b)
        {
            Red = r;
            Green = g;
            Blue = b;

            if (Red < 0) Red = 0;
            if (Green < 0) Green = 0;
            if (Blue < 0) Blue = 0;

            if (Red > 255) Red = 255;
            if (Green > 255) Green = 255;
            if (Blue > 255) Blue = 255;
        }

        public LEDColor(System.Drawing.Color color)
        {
            Red = color.R;
            Green = color.G;
            Blue = color.B;

            if (Red < 0) Red = 0;
            if (Green < 0) Green = 0;
            if (Blue < 0) Blue = 0;

            if (Red > 255) Red = 255;
            if (Green > 255) Green = 255;
            if (Blue > 255) Blue = 255;
        }

        public override string ToString()
        {
            return $"{Red},{Green},{Blue}";
        }

        public static LEDColor FromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return new LEDColor(v, t, p);
            else if (hi == 1)
                return new LEDColor(q, v, p);
            else if (hi == 2)
                return new LEDColor(p, v, t);
            else if (hi == 3)
                return new LEDColor(p, q, v);
            else if (hi == 4)
                return new LEDColor(t, p, v);
            else
                return new LEDColor(v, p, q);
        }



        public static LEDColor FromHSL(int h, int s, int l)
        {
            if (h < 0 || h > 360) throw new Exception("Hue out of range (0-360)");
            if (s < 0 || s > 100) throw new Exception("Saturation out of range (0-100)");
            if (l < 0 || l > 200) throw new Exception("Lightness out of range (0-200, pure = 100)");

            return HlsToRgb((double)h, s / 100d, l / 200d);
        }



        private static RGB GetRGBFromHSLWithChroma(double hue, double s, double l)
        {
            double min, max, h;

            h = hue / 360d;

            max = l < 0.5d ? l * (1 + s) : (l + s) - (l * s);
            min = (l * 2d) - max;

            RGB rgb = new RGB();
            rgb.R = ComponentFromHue(min, max, h + (1d / 3d));
            rgb.G = ComponentFromHue(min, max, h);
            rgb.B = ComponentFromHue(min, max, h - (1d / 3d));
            return rgb;
        }



        private static double ComponentFromHue(double m1, double m2, double h)
        {
            h = (h + 1d) % 1d;
            if ((h * 6d) < 1)
                return m1 + (m2 - m1) * 6d * h;
            else if ((h * 2d) < 1)
                return m2;
            else if ((h * 3d) < 2)
                return m1 + (m2 - m1) * ((2d / 3d) - h) * 6d;
            else
                return m1;
        }

        // Convert an HLS value into an RGB value.
        private static LEDColor HlsToRgb(double h, double s, double l)
        {
            RGB rgb = new RGB();

            if (s == 0d)
                rgb.R = rgb.G = rgb.B = l;
            else
                rgb = GetRGBFromHSLWithChroma(h, s, l);

            return rgb.ToLEDColor();
        }
    }



    public class RGB
    {
        public double R { get; set; }
        public double G { get; set; }
        public double B { get; set; }

        public LEDColor ToLEDColor()
        {
            int r = (int)Math.Round(R * 255d);
            int g = (int)Math.Round(G * 255d);
            int b = (int)Math.Round(B * 255d);
            return new LEDColor(r, g, b);
        }
    }
}
