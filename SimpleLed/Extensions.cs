using System;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Reflection;
using SimpleLed.RawInput;

namespace SimpleLed
{
    /// <summary>
    /// Extensions to provide helper classes
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Lerp between two colours
        /// </summary>
        /// <param name="color1">first input color</param>
        /// <param name="color2">second input color</param>
        /// <param name="perc">lerp amount</param>
        /// <returns></returns>
        public static LEDColor LerpTo(this LEDColor color1, LEDColor color2, float perc)=>Vector3.Lerp(color1.ToVector3(), color2.ToVector3(), perc).ToLEDColor();

        /// <summary>
        /// helper method to translate a LED color to a Vector3
        /// </summary>
        /// <param name="c">input color</param>
        /// <returns></returns>
        public static Vector3 ToVector3(this LEDColor c)=>new Vector3(c.Red,c.Green,c.Blue);

        /// <summary>
        /// Helper method to translate a vector3 to a LED color
        /// </summary>
        /// <param name="v">input vector3</param>
        /// <returns></returns>
        public static LEDColor ToLEDColor(this Vector3 v)=>new LEDColor((int)v.X, (int)v.Y,(int)v.Z);

        /// <summary>
        /// Get a LED colour from a H/S/V value
        /// </summary>
        /// <param name="hue">Hue</param>
        /// <param name="saturation">Saturation</param>
        /// <param name="value">Value</param>
        /// <returns>LEDColor</returns>
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

        /// <summary>
        /// Simplifies getting embedded resource PNGs for use as the device image
        /// </summary>
        /// <param name="myAssembly"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Bitmap GetEmbeddedImage(this Assembly myAssembly, string path)
        {
            using (Stream myStream = myAssembly.GetManifestResourceStream(path))
            {
                if (myStream != null)
                {
                    return (Bitmap) Image.FromStream(myStream);
                }
                else
                {
                    return new Bitmap(1,1);
                }
            }

        }

        internal static RawInput.RawInput RawInput;
    }
}
