using System;
using System.Drawing;
using System.Linq;

namespace SimpleLed
{
    /// <summary>
    /// Represents a single device capable of consuming or producing RGB LEDs
    /// </summary>
    public class ControlDevice
    {
        /// <summary>
        /// Name of device
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Device type. This can be free text but recommended to be consumed from the DeviceTypes static to ensure compatibility with hosts.
        /// </summary>
        public string DeviceType { get; set; }
        /// <summary>
        /// The driver that this device belongs to.
        /// </summary>
        public ISimpleLed Driver { get; set; }
        /// <summary>
        /// Array of LedUnits provided by this device
        /// </summary>
        public LedUnit[] LEDs { get; set; }
        /// <summary>
        /// Amount of LEDs to shift by upon mapping to correct orientation differences.
        /// </summary>
        public int LedShift { get; set; } = 0;
        /// <summary>
        /// Reverse order of LEDs in mapping to correct orientation differences.
        /// </summary>
        public bool Reverse { get; set; } = false;
        /// <summary>
        /// true if device can change its number of LEDs
        /// </summary>
        [Obsolete]
        public bool SupportsLEDCountOverride { get; set; } = false;
        /// <summary>
        /// 256x192 Bitmap of device
        /// </summary>
        public Bitmap ProductImage { get; set; } = null;
        /// <summary>
        /// Map another devices LEDs to this device (with automatic scaling)
        /// </summary>
        /// <param name="otherDevice">Device to copy LEDs from</param>
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

        /// <summary>
        /// Get a UE id for a specific LED
        /// </summary>
        /// <param name="unit">LED to get ID for</param>
        /// <returns></returns>
        public string GetLedUID(LedUnit unit) => Name + "_" + unit.LEDName;

        /// <summary>
        /// Maps LED position based upon device settings.
        /// </summary>
        /// <param name="index">Input LED position</param>
        /// <returns>Mapped LED position</returns>
        public int MapIndex(int index)
        {
            int result = (index + LedShift) % LEDs.Length;
            if (Reverse) result = (LEDs.Length - 1) - result;

            return result;
        }

        /// <summary>
        /// Get a LED by name
        /// </summary>
        /// <param name="name">name to search for</param>
        /// <returns>Found LedUnit or null if not found</returns>
        public LedUnit GetLEDByName(string name)
        {
            return LEDs.FirstOrDefault(x => x.LEDName == name);
        }

        /// <summary>
        /// Model for a single LED in a device
        /// </summary>
        public class LedUnit
        {
            /// <summary>
            /// Name of LED
            /// </summary>
            public string LEDName { get; set; }
            /// <summary>
            /// Current colour of LED
            /// </summary>
            public LEDColor Color { get; set; } = new LEDColor(0, 0, 0);
            /// <summary>
            /// LED Specific data (can be extended)
            /// </summary>
            public LEDData Data { get; set; }

            public override string ToString()
            {
                return $"{LEDName} : {Color}";
            }

        }

        /// <summary>
        /// Base class for LED specific data
        /// </summary>
        public class LEDData
        {
            /// <summary>
            /// LED index
            /// </summary>
            public int LEDNumber { get; set; }
        }

        /// <summary>
        /// Push current LEDs to the device/SDK.
        /// </summary>
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

        /// <summary>
        /// Pull current LEDs from device/SDK into local LEDs
        /// </summary>
        public void Pull()
        {
            Driver.Pull(this);
        }
    }


}
