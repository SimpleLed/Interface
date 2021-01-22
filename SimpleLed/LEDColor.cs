using System;

namespace SimpleLed
{
    public static class LEDColors
    {
        public static LEDColor Black = new LEDColor(0,0,0);
        public static LEDColor White = new LEDColor(255,255,255);

        public static LEDColor Red = new LEDColor(255, 0, 0);
        public static LEDColor Green = new LEDColor(0, 255, 0);
        public static LEDColor Blue = new LEDColor(0, 0, 255);
    }

    /// <summary>
    /// SimpleLed specific Color system
    /// </summary>
    public class LEDColor
    {
        /// <summary>
        /// amount of Red (0-255)
        /// </summary>
        public int Red { get; set; }
        /// <summary>
        /// amount of Green (0-255)
        /// </summary>
        public int Green { get; set; }
        /// <summary>
        /// amount of Blue (0-255)
        /// </summary>
        public int Blue { get; set; }

        public string AsString() => Red + "," + Green + "," + Blue;

        /// <summary>
        /// LEDColor from R,G,B
        /// </summary>
        /// <param name="r">Red (0->255)</param>
        /// <param name="g">Green (0->255)</param>
        /// <param name="b">Blue (0->255)</param>
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

        /// <summary>
        /// LEDColor from System.Drawing.Color
        /// </summary>
        /// <param name="color">System.Drawing.Color</param>
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

        /// <summary>
        /// LEDColor from Hue, Saturation, Value
        /// </summary>
        /// <param name="hue">Hue (0->360)</param>
        /// <param name="saturation">Saturation (0->100)</param>
        /// <param name="value">Value (-100->100)</param>
        /// <returns></returns>
        public static LEDColor FromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            switch (hi)
            {
                case 0:
                    return new LEDColor(v, t, p);
                case 1:
                    return new LEDColor(q, v, p);
                case 2:
                    return new LEDColor(p, v, t);
                case 3:
                    return new LEDColor(p, q, v);
                case 4:
                    return new LEDColor(t, p, v);
                default:
                    return new LEDColor(v, p, q);
            }
        }
    }
}
