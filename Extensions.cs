using System;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace SimpleLed
{
    public static class Extensions
    {
        public static LEDColor ColorFromHSV(double hue, double saturation, double value)
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

        public static Bitmap GetEmbeddedImage(this Assembly myAssembly, string path)
        {
            using (Stream myStream = myAssembly.GetManifestResourceStream(path))
            {
                return (Bitmap)Image.FromStream(myStream);
            }

        }


    }
}
