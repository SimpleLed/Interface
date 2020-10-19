using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleLed
{
    internal class USBModels
    {
        internal class USBDeviceInfo
        {
            public USBDeviceInfo(string deviceID, string pnpDeviceID, string description)
            {
                this.DeviceID = deviceID;
                this.PnpDeviceID = pnpDeviceID;
                this.Description = description;
            }
            public string DeviceID { get; private set; }
            public string PnpDeviceID { get; private set; }
            public string Description { get; private set; }
            public string VEN { get; set; }
            public string PID { get; set; }
            public string Root { get; set; }

            public int VID { get; set; }
            public int HID { get; set; }
            public string Name { get; set; }
            public string PrettyName { get; set; }
        }
    }
}
