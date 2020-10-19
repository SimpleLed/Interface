using System;
using System.Diagnostics;
using System.Globalization;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace SimpleLed.RawInput
{
    public sealed class RawKeyboard
	{
        public void AddWatcher(int vid, int pid, InputTrigger.DeviceSpecificEventHandler handler)
        {
            if (DeviceSpecificWatchers.Any(x => x.Product != pid && x.Vendor != vid))
            {
                DeviceSpecificWatchers.Remove(DeviceSpecificWatchers.First(x => x.Product != pid && x.Vendor != vid));
            }

            var lcs = new InputTrigger
			{
                    Product = pid,
                    Vendor = vid
                };

                lcs.KeyPressed += handler;

                DeviceSpecificWatchers.Add(lcs);
            
        }
		
		public List<InputTrigger> DeviceSpecificWatchers = new List<InputTrigger>();

        private readonly Dictionary<IntPtr, KeyPressEvent> _deviceList = new Dictionary<IntPtr, KeyPressEvent>();
		public delegate void DeviceEventHandler(KeyPressEvent keyPressEventArgs);
		public event DeviceEventHandler KeyPressed;
		readonly object _padLock = new object();
		public int NumberOfKeyboards { get; private set; }
		static InputData _rawBuffer;

        public RawKeyboard(IntPtr hwnd, bool captureOnlyInForeground)
        {
            var rid = new RawInputDevice[1];

            rid[0].UsagePage = HidUsagePage.GENERIC;
            rid[0].Usage = HidUsage.Keyboard;
            rid[0].Flags = RawInputDeviceFlags.INPUTSINK | RawInputDeviceFlags.DEVNOTIFY;
            rid[0].Target = hwnd;

            if (!Win32.RegisterRawInputDevices(rid, (uint)rid.Length, (uint)Marshal.SizeOf(rid[0])))
            {
                throw new ApplicationException("Failed to register raw input device(s).");
            }

            ScanMap = new List<XYScan>();
            var pp = xyscanMap.Split(',');
            for (int i = 0; i < pp.Length; i = i + 3)
            {
                ScanMap.Add(new XYScan
                {
                    X = int.Parse(pp[i]),
                    Y = int.Parse(pp[i + 1]),
                    ScanCode = int.Parse(pp[i + 2])
                });
            }
		}

        public class XYScan
        {
			public int X { get; set; }
			public int Y { get; set; }
			public int ScanCode { get; set; }

        }
        public RawKeyboard()
        {
            var rid = new RawInputDevice[1];

            rid[0].UsagePage = HidUsagePage.GENERIC;
            rid[0].Usage = HidUsage.Keyboard;
            rid[0].Flags = (false ? RawInputDeviceFlags.NONE : RawInputDeviceFlags.INPUTSINK) | RawInputDeviceFlags.DEVNOTIFY;
            //rid[0].Target = hwnd;

            if (!Win32.RegisterRawInputDevices(rid, (uint)rid.Length, (uint)Marshal.SizeOf(rid[0])))
            {
                throw new ApplicationException("Failed to register raw input device(s).");
            }

            ScanMap = new List<XYScan>();
			var pp = xyscanMap.Split(',');
            for (int i = 0; i < pp.Length; i = i + 3)
            {
				ScanMap.Add(new XYScan
                {
					X=int.Parse(pp[i]),
					Y=int.Parse(pp[i+1]),
					ScanCode = int.Parse(pp[i+2])
                });
            }
        }

		public List<XYScan> ScanMap = new List<XYScan>();

		public void EnumerateDevices()
		{
			lock (_padLock)
			{
				_deviceList.Clear();

				var keyboardNumber = 0;

				var globalDevice = new KeyPressEvent
				{
					DevicePath = "Global Keyboard",
					DeviceHandle = IntPtr.Zero,
					DeviceType = Win32.GetDeviceType(DeviceType.RimTypekeyboard),
					Name = "Fake Keyboard. Some keys (ZOOM, MUTE, VOLUMEUP, VOLUMEDOWN) are sent to rawinput with a handle of zero.",
					Source = keyboardNumber++.ToString(CultureInfo.InvariantCulture)
				};

				_deviceList.Add(globalDevice.DeviceHandle, globalDevice);
				
				var numberOfDevices = 0;
				uint deviceCount = 0;
				var dwSize = (Marshal.SizeOf(typeof(Rawinputdevicelist)));

				if (Win32.GetRawInputDeviceList(IntPtr.Zero, ref deviceCount, (uint)dwSize) == 0)
				{
					var pRawInputDeviceList = Marshal.AllocHGlobal((int)(dwSize * deviceCount));
					Win32.GetRawInputDeviceList(pRawInputDeviceList, ref deviceCount, (uint)dwSize);

					for (var i = 0; i < deviceCount; i++)
					{
						uint pcbSize = 0;

						// On Window 8 64bit when compiling against .Net > 3.5 using .ToInt32 you will generate an arithmetic overflow. Leave as it is for 32bit/64bit applications
						var rid = (Rawinputdevicelist)Marshal.PtrToStructure(new IntPtr((pRawInputDeviceList.ToInt64() + (dwSize * i))), typeof(Rawinputdevicelist));

						Win32.GetRawInputDeviceInfo(rid.hDevice, RawInputDeviceInfo.RIDI_DEVICENAME, IntPtr.Zero, ref pcbSize);

						if (pcbSize <= 0) continue;

						var pData = Marshal.AllocHGlobal((int)pcbSize);
						Win32.GetRawInputDeviceInfo(rid.hDevice, RawInputDeviceInfo.RIDI_DEVICENAME, pData, ref pcbSize);
						var deviceName = Marshal.PtrToStringAnsi(pData);

                        if (rid.dwType == DeviceType.RimTypekeyboard || rid.dwType == DeviceType.RimTypeHid)
						{
							var deviceDesc = Win32.GetDeviceDescription(deviceName);

							var dInfo = new KeyPressEvent
							{
								DevicePath = Marshal.PtrToStringAnsi(pData),
								DeviceHandle = rid.hDevice,
								DeviceType = Win32.GetDeviceType(rid.dwType),
								Name = deviceDesc,
								Source = keyboardNumber++.ToString(CultureInfo.InvariantCulture)
							};
						   
							if (!_deviceList.ContainsKey(rid.hDevice))
							{
								numberOfDevices++;
								_deviceList.Add(rid.hDevice, dInfo);
							}
						}

						Marshal.FreeHGlobal(pData);
					}

					Marshal.FreeHGlobal(pRawInputDeviceList);

					NumberOfKeyboards = numberOfDevices;
					Debug.WriteLine("EnumerateDevices() found {0} Keyboard(s)", NumberOfKeyboards);
					return;
				}
			}
			
			throw new Win32Exception(Marshal.GetLastWin32Error());
		}

        public static string xyscanMap =
            "0,0,1,0,1,41,0,2,15,0,3,58,0,4,42,0,5,29,15,0,88,13,1,13,12,0,67,9,1,10,11,2,24,11,3,38,11,4,51,14,5,56,15,3,28,16,5,107,2,0,59,1,1,2,2,2,16,2,3,30,1,4,86,1,5,91,16,0,55,13,0,68,10,1,11,12,2,25,12,3,39,12,4,52,14,3,28,17,5,72,3,0,60,2,1,3,3,2,17,3,3,31,2,4,44,2,5,56,17,0,70,14,1,14,14,0,87,12,1,12,13,2,26,13,3,41,13,4,53,18,5,77,4,0,61,3,1,4,4,2,18,4,3,32,3,4,45,18,0,29,16,2,83,19,2,71,19,1,69,21,3,77,5,0,62,4,1,5,5,2,19,5,3,33,4,4,46,7,5,57,16,1,82,17,2,79,20,2,72,20,1,53,19,4,79,7,0,63,5,1,6,7,2,20,7,3,34,5,4,47,17,1,71,18,2,81,21,2,73,21,1,55,20,4,80,8,0,64,6,1,7,8,2,21,8,3,35,7,4,48,18,1,73,14,4,54,22,1,74,21,4,81,9,0,65,7,1,8,9,2,22,9,3,36,9,4,49,12,5,56,14,2,27,15,5,29,19,3,75,22,2,78,19,5,82,10,0,88,8,1,9,10,2,23,10,3,37,10,4,50,13,5,92,15,2,43,17,4,72,20,3,76,21,5,83";


		public void ProcessRawInput(IntPtr hdevice)
		{
			//Debug.WriteLine(_rawBuffer.data.keyboard.ToString());
			//Debug.WriteLine(_rawBuffer.data.hid.ToString());
			//Debug.WriteLine(_rawBuffer.header.ToString());

			if (_deviceList.Count == 0) return;

			var dwSize = 0;
			Win32.GetRawInputData(hdevice, DataCommand.RID_INPUT, IntPtr.Zero, ref dwSize, Marshal.SizeOf(typeof(Rawinputheader)));

			if (dwSize != Win32.GetRawInputData(hdevice, DataCommand.RID_INPUT, out _rawBuffer, ref dwSize, Marshal.SizeOf(typeof (Rawinputheader))))
			{
				Debug.WriteLine("Error getting the rawinput buffer");
				return;
			}

            var sc = _rawBuffer.data.keyboard.Makecode;
			int virtualKey = _rawBuffer.data.keyboard.VKey;
			int makeCode = _rawBuffer.data.keyboard.Makecode;
			int flags = _rawBuffer.data.keyboard.Flags;

			if (virtualKey == Win32.KEYBOARD_OVERRUN_MAKE_CODE) return; 
			
			var isE0BitSet = ((flags & Win32.RI_KEY_E0) != 0);

			KeyPressEvent keyPressEvent;

			//Debug.WriteLine(KeyMapper.GetMicrosoftKeyName(virtualKey)+" == "+sc);
			if (_deviceList.ContainsKey(_rawBuffer.header.hDevice))
			{
				lock (_padLock)
				{
					keyPressEvent = _deviceList[_rawBuffer.header.hDevice];
				}
			}
			else
			{
				Debug.WriteLine("Handle: {0} was not in the device list.", _rawBuffer.header.hDevice);
				return;
			}

			var isBreakBitSet = ((flags & Win32.RI_KEY_BREAK) != 0);
			
			keyPressEvent.KeyPressState = isBreakBitSet ? "BREAK" : "MAKE"; 
			keyPressEvent.Message = _rawBuffer.data.keyboard.Message;
			keyPressEvent.VKeyName = KeyMapper.GetKeyName(VirtualKeyCorrection(virtualKey, isE0BitSet, makeCode)).ToUpper();
			keyPressEvent.MSKeyName = KeyMapper.GetMicrosoftKeyName(virtualKey);
            keyPressEvent.ScanCode = sc;
			keyPressEvent.VKey = virtualKey;

            var ps = ScanMap.FirstOrDefault(x => x.ScanCode == sc);
            if (ps != null)
            {
                keyPressEvent.XPosition = ps.X;
                keyPressEvent.YPosition = ps.Y;
            }

            if (keyPressEvent.DevicePath.Contains("VID_") && keyPressEvent.DevicePath.Contains("PID_"))
            {
                var vid = "0x" +keyPressEvent.DevicePath.Substring(keyPressEvent.DevicePath.IndexOf("VID_") + 4, 4);
                var pid = "0x" + keyPressEvent.DevicePath.Substring(keyPressEvent.DevicePath.IndexOf("PID_") + 4, 4);

                keyPressEvent.Vendor = Convert.ToInt32( vid, 16);
                keyPressEvent.Product = Convert.ToInt32( pid,16);
            }

            var specific = DeviceSpecificWatchers.FirstOrDefault(x =>
                x.Product == keyPressEvent.Product && x.Vendor == keyPressEvent.Vendor);

            specific?.Fire(keyPressEvent);

            KeyPressed?.Invoke(keyPressEvent);
        }

		private static int VirtualKeyCorrection(int virtualKey, bool isE0BitSet, int makeCode)
		{
			var correctedVKey = virtualKey;

			if (_rawBuffer.header.hDevice == IntPtr.Zero)
			{
				// When hDevice is 0 and the vkey is VK_CONTROL indicates the ZOOM key
				if (_rawBuffer.data.keyboard.VKey == Win32.VK_CONTROL)
				{
					correctedVKey = Win32.VK_ZOOM;
				}
			}
			else
			{
				switch (virtualKey)
				{
					// Right-hand CTRL and ALT have their e0 bit set 
					case Win32.VK_CONTROL:
						correctedVKey = isE0BitSet ? Win32.VK_RCONTROL : Win32.VK_LCONTROL;
						break;
					case Win32.VK_MENU:
						correctedVKey = isE0BitSet ? Win32.VK_RMENU : Win32.VK_LMENU;
						break;
					case Win32.VK_SHIFT:
						correctedVKey = makeCode == Win32.SC_SHIFT_R ? Win32.VK_RSHIFT : Win32.VK_LSHIFT;
						break;
					default:
						correctedVKey = virtualKey;
						break;
				}
			}

			return correctedVKey;
		}
    }
}
