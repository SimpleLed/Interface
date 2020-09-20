using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleLed.RawInput;

namespace SimpleLed
{
    public class InputTrigger
    {
        public delegate void DeviceSpecificEventHandler(KeyPressEvent e);

        public event DeviceSpecificEventHandler KeyPressed;

        public int Vendor { get; set; }
        public int Product { get; set; }

        public void Fire(KeyPressEvent arg)
        {
            KeyPressed?.Invoke(arg);
        }
    }

    ///// <summary>
    ///// Class with details of Keyboard interactions
    ///// </summary>
    //public class RawInputEventArg : EventArgs
    //{
    //    public RawInputEventArg(KeyPressEvent arg)
    //    {
    //        KeyPressEvent = arg;
    //    }

    //    public KeyPressEvent KeyPressEvent { get; private set; }
    //}
}
