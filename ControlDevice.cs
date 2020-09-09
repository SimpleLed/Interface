using System.Drawing;
using System.Linq;

namespace SimpleLed
{
    public class ControlDevice
    {
        public string Name { get; set; }
        public string DeviceType { get; set; }
        public ISimpleLed Driver { get; set; }
        public LedUnit[] LEDs { get; set; }
        public int LedShift { get; set; } = 0;
        public bool Reverse { get; set; } = false;

        public bool SupportsLEDCountOverride { get; set; } = false;
        public Bitmap ProductImage { get; set; } = null;
        public void MapLEDs(ControlDevice otherDevice)
        {

            Bitmap bm = new Bitmap(otherDevice.LEDs.Length, 1);

            for (var i = 0; i < otherDevice.LEDs.Length; i++)
            {
                bm.SetPixel(i, 0, Color.FromArgb(otherDevice.LEDs[i].Color.Red, otherDevice.LEDs[i].Color.Green, otherDevice.LEDs[i].Color.Blue));
            }

            Bitmap bm2 = new Bitmap(bm, new Size(LEDs.Length, 1));

            for (var i = 0; i < LEDs.Length; i++)
            {
                int x = MapIndex(i);
                var cl = bm2.GetPixel(i, 0);
                LEDs[x].Color.Red = cl.R;
                LEDs[x].Color.Green = cl.G;
                LEDs[x].Color.Blue = cl.B;
            }
        }

        public string GetLedUID(LedUnit unit) => Name + "_" + unit.LEDName;

        public int MapIndex(int index)
        {
            int result = (index + LedShift) % LEDs.Length;
            if (Reverse) result = (LEDs.Length - 1) - result;

            return result;
        }

        public LedUnit GetLEDByName(string name)
        {
            return LEDs.FirstOrDefault(x => x.LEDName == name);
        }

        public class LedUnit
        {
            public string LEDName { get; set; }
            public LEDColor Color { get; set; } = new LEDColor(0, 0, 0);
            public LEDData Data { get; set; }

            public override string ToString()
            {
                return $"{LEDName} : {Color}";
            }

        }

        public class LEDData
        {
            public int LEDNumber { get; set; }
        }


        public void Push()
        {
            try
            {
                Driver.Push(this);
            }
            catch
            {
            }
        }

        public void Pull()
        {
            Driver.Pull(this);
        }
    }


}
