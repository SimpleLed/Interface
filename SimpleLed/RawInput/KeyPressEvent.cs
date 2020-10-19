using System;

namespace SimpleLed.RawInput
{
    /// <summary>
    /// Event triggered when a watched keyboard has an interactions
    /// </summary>
    public class KeyPressEvent
    {
        /// <summary>
        /// Device Path, i.e.  i.e. \\?\HID#VID_045E&PID_00DD&MI_00#8&1eb402&0&0000#{884b96c3-56ef-11d1-bc8c-00a0c91405dd}
        /// </summary>
        public string DevicePath;       // i.e. \\?\HID#VID_045E&PID_00DD&MI_00#8&1eb402&0&0000#{884b96c3-56ef-11d1-bc8c-00a0c91405dd}
        /// <summary>
        /// Device Type - KEYBOARD or HID
        /// </summary>
        public string DeviceType;       // KEYBOARD or HID

        /// <summary>
        /// Handle to the device that sent the input
        /// </summary>
        public IntPtr DeviceHandle;     // Handle to the device that send the input
        /// <summary>
        /// Name of device, i.e. Microsoft USB Comfort Curve Keyboard 2000 (Mouse and Keyboard Center)
        /// </summary>
        public string Name;             // i.e. Microsoft USB Comfort Curve Keyboard 2000 (Mouse and Keyboard Center)
        private string _source;         // Keyboard_XX
        /// <summary>
        /// VirtualKey
        /// </summary>
        public int VKey;                // Virtual Key. Corrected for L/R keys(i.e. LSHIFT/RSHIFT) and Zoom
        /// <summary>
        /// Virtual Key Name
        /// </summary>
        public string VKeyName;         // Virtual Key Name. Corrected for L/R keys(i.e. LSHIFT/RSHIFT) and Zoom
        /// <summary>
        /// WM_KEYDOWN or WM_KEYUP  
        /// </summary>
        public uint Message;            // WM_KEYDOWN or WM_KEYUP        
        /// <summary>
        /// MAKE or BREAK
        /// </summary>
        public string KeyPressState;    // MAKE or BREAK

        public int XPosition;
        public int YPosition;
        /// <summary>
        /// Vendor id as integer
        /// </summary>
        public int Vendor;
        /// <summary>
        /// product Id as integer
        /// </summary>
        public int Product;

        internal string Source
        {
            get { return _source; }
            set { _source = string.Format("Keyboard_{0}", value.PadLeft(2, '0')); }
        }

        public string MSKeyName { get; internal set; }

        public int ScanCode;

        public override string ToString()
        {
            return string.Format("Device\n DevicePath: {0}\n DeviceType: {1}\n DeviceHandle: {2}\n Name: {3}\n", DevicePath, DeviceType, DeviceHandle.ToInt64().ToString("X"), Name);
        }
    }
}
                                         

