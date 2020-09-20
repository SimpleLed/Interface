using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleLed.RawInput;

namespace SimpleLed
{
    public static class KeyboardHelper
    {
        public static void AddKeyboardWatcher(int vid, int pid, InputTrigger.DeviceSpecificEventHandler handler)
        {
            InternalSolids.RawInput.AddWatcher(vid,pid,handler);
        }
    }
}
