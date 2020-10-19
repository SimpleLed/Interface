using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleLed
{
    public class Events
    {
        public class DeviceChangeEventArgs
        {
            public ControlDevice ControlDevice { get; private set; }

            public DeviceChangeEventArgs(ControlDevice controlDevice)
            {
                ControlDevice = controlDevice;
            }
        }

        public delegate void DeviceChangeEventHandler(object sender, DeviceChangeEventArgs e);
    }
}
