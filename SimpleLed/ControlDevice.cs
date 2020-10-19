﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using SimpleLed.RawInput;

namespace SimpleLed
{
    public class InteractiveControlDevice : ControlDevice
    {
        public void HandleInput(KeyPressEvent e)
        {
            TriggerAllMapped(new ControlDevice.TriggerEventArgs
            {
                FloatX = (e.XPosition / (float)GridWidth),
                FloatY = (e.YPosition / (float)GridHeight),
                X = e.XPosition,
                Y = e.YPosition
            });
        }
    }

    /// <summary>
    /// Represents a single device capable of consuming or producing RGB LEDs
    /// </summary>
    public class ControlDevice
    {
        private class MappedToEventArgs
        {
            public ControlDevice DestinationDevice { get; set; }
            public ControlDevice SourceDevice { get; set; }
        }

        public delegate void DestTriggeredEventHandler(object sender, TriggerEventArgs args);

        /// <summary>
        /// Event that is triggered on a Dest device firing an interactivity
        /// </summary>
        public event DestTriggeredEventHandler DestTriggeredEvent;

        /// <summary>
        /// Fire an interactivity event on all mapped devices
        /// </summary>
        /// <param name="e"></param>
        public void TriggerAllMapped(TriggerEventArgs e)
        {
            foreach (MappedListItem mappedListItem in mappedDevices)
            {
                try
                {
                    mappedListItem.Device.DestTriggeredEvent?.Invoke(null, e);
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// If this device supports 2D mode and this flag is set, Push will push from the Grid rather than the LEDs array
        /// </summary>
        public bool In2DMode { get; set; }
        /// <summary>
        /// Signifies if this device can handle 2d led arrays
        /// </summary>
        public bool Has2DSupport { get; set; }
        /// <summary>
        /// if this device has 2d support, this signifies the width of the 2d grid
        /// </summary>
        public int GridWidth { get; set; }
        /// <summary>
        /// if this device has 2d support, this signifies the height of the 2d grid
        /// </summary>
        public int GridHeight { get; set; }
        /// <summary>
        /// Name of device
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Rather than use the name of the driver, use this as the title
        /// </summary>
        public string TitleOverride { get; set; }

        /// <summary>
        /// Name of device the device is connected to.
        /// Used for drivers that support multiple devices, ie Lightning node pros
        /// </summary>
        public string ConnectedTo { get; set; }
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

        private readonly List<MappedListItem> mappedDevices = new List<MappedListItem>();

        private class MappedListItem
        {
            public ControlDevice Device { get; set; }
            public DateTime LastMapped { get; set; }
        }

        private void Dv_MappedToEvent(object sender, MappedToEventArgs args)
        {
            //try catch the hell out of this, dont want any issues for something thats nonessential.
            try
            {
                if (mappedDevices.All(x => x.Device != args.SourceDevice))
                {
                    mappedDevices.Add(new MappedListItem
                    {
                        Device = args.SourceDevice,
                        LastMapped = DateTime.Now
                    });
                }
                else
                {
                    mappedDevices.First(x => x.Device == args.SourceDevice).LastMapped = DateTime.Now;
                }

                try
                {
                    var removeList = mappedDevices.Where(x => (DateTime.Now - x.LastMapped).TotalSeconds > 1);

                    foreach (var mappedListItem in removeList)
                    {
                        mappedDevices.Remove(mappedListItem);
                    }
                }
                catch
                {
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Map another devices LEDs to this device (with automatic scaling)
        /// </summary>
        /// <param name="otherDevice">Device to copy LEDs from</param>
        public void MapLEDs(ControlDevice otherDevice)
        {
            Dv_MappedToEvent(this, new MappedToEventArgs
            {
                DestinationDevice = this,
                SourceDevice = otherDevice
            });

            if (this.Has2DSupport && otherDevice.Has2DSupport)
            {
                In2DMode = true;
                MapLEDs2Dto2D(otherDevice);
            }
            else
            {
                Bitmap bm = new Bitmap(otherDevice.LEDs.Length, 1);

                for (var i = 0; i < otherDevice.LEDs.Length; i++)
                {
                    bm.SetPixel(i, 0,
                        Color.FromArgb(otherDevice.LEDs[i].Color.Red, otherDevice.LEDs[i].Color.Green,
                            otherDevice.LEDs[i].Color.Blue));
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
        }

        /// <summary>
        /// model describing an interactivity event.
        /// </summary>
        public class TriggerEventArgs
        {
            public int X { get; set; }
            public int Y { get; set; }
            public float FloatX { get; set; }
            public float FloatY { get; set; }

            public int LedNumber { get; set; }
            public float RatioPosition { get; set; }
        }

        private void MapLEDs2Dto2D(ControlDevice otherDevice)
        {

            Bitmap bm = new Bitmap(otherDevice.GridWidth, otherDevice.GridHeight);

            for (int y = 0; y < otherDevice.GridHeight; y++)
            {
                for (int x = 0; x < otherDevice.GridWidth; x++)
                {
                    var px = otherDevice.GetGridLED(x, y);
                    if (px != null)
                    {
                        bm.SetPixel(x, y, Color.FromArgb(px.Red, px.Green, px.Blue));
                    }
                }
            }

            Bitmap bm2 = new Bitmap(bm, new Size(GridWidth, GridHeight));
            
            for (int y = 0; y < GridHeight; y++)
            {
                for (int x = 0; x < GridWidth; x++)
                {
                    Color cl = bm2.GetPixel(x, y);

                    SetGridLED(x, y, new LEDColor(cl.R, cl.G, cl.B));
                }
            }
        }

        /// <summary>
        /// If device supports 2d grids, this allows gets LEDs via X/Y coords
        /// </summary>
        public LEDColor GetGridLED(int x, int y)
        {
            return LEDs.FirstOrDefault(p => p.Data is PositionalLEDData pd && pd.X == x && pd.Y == y)?.Color;
        }

        /// <summary>
        /// If device supports 2d grids, this allows sets LEDs via X/Y coords
        /// </summary>
        public void SetGridLED(int x, int y, LEDColor color)
        {
            var led = LEDs.FirstOrDefault(p => p.Data is PositionalLEDData pd && pd.X == x && pd.Y == y);
            if (led != null)
            {
                led.Color = color;
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
        /// Led data with position awareness
        /// </summary>
        public class PositionalLEDData : LEDData
        {
            public int X { get; set; }
            public int Y { get; set; }
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
