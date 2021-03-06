using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Windows;
using HardwareHelperLib;
using Microsoft.Win32;
using Application = System.Windows.Forms.Application;

namespace SimpleLed.RawInput
{
    internal class RawInput : NativeWindow
    {
        internal Action OnDeviceChange;
        internal static RawKeyboard KeyboardDriver;
        readonly IntPtr _devNotifyHandle;
        static readonly Guid DeviceInterfaceHid = new Guid("4D1E55B2-F16F-11CF-88CB-001111000030");
        private PreMessageFilter _filter;

        internal event RawKeyboard.DeviceEventHandler KeyPressed
        {
            add { KeyboardDriver.KeyPressed += value; }
            remove { KeyboardDriver.KeyPressed -= value; }
        }

        internal void AddWatcher(int vid, int pid, InputTrigger.DeviceSpecificEventHandler handler)
        {
            KeyboardDriver.AddWatcher(vid, pid, handler);
        }

        internal int NumberOfKeyboards
        {
            get { return KeyboardDriver.NumberOfKeyboards; }
        }

        public void AddMessageFilter()
        {
            if (null != _filter) return;

            _filter = new PreMessageFilter();
            Application.AddMessageFilter(_filter);
        }

        private void RemoveMessageFilter()
        {
            if (null == _filter) return;

            Application.RemoveMessageFilter(_filter);
        }

        internal RawInput(IntPtr parentHandle)
        {
            AssignHandle(parentHandle);

            KeyboardDriver = new RawKeyboard(parentHandle, false);
            KeyboardDriver.EnumerateDevices();
            _devNotifyHandle = RegisterForDeviceNotifications(parentHandle);
        }


        static IntPtr RegisterForDeviceNotifications(IntPtr parent)
        {
            var usbNotifyHandle = IntPtr.Zero;
            var bdi = new BroadcastDeviceInterface();
            bdi.DbccSize = Marshal.SizeOf(bdi);
            bdi.BroadcastDeviceType = BroadcastDeviceType.DBT_DEVTYP_DEVICEINTERFACE;
            bdi.DbccClassguid = DeviceInterfaceHid;

            var mem = IntPtr.Zero;
            try
            {
                mem = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(BroadcastDeviceInterface)));
                Marshal.StructureToPtr(bdi, mem, false);
                usbNotifyHandle = Win32.RegisterDeviceNotification(parent, mem, DeviceNotification.DEVICE_NOTIFY_WINDOW_HANDLE);
            }
            catch (Exception e)
            {
                Debug.Print("Registration for device notifications Failed. Error: {0}", Marshal.GetLastWin32Error());
                Debug.Print(e.StackTrace);
            }
            finally
            {
                Marshal.FreeHGlobal(mem);
            }

            if (usbNotifyHandle == IntPtr.Zero)
            {
                Debug.Print("Registration for device notifications Failed. Error: {0}", Marshal.GetLastWin32Error());
            }

            return usbNotifyHandle;
        }

        [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
        protected override void WndProc(ref Message message)
        {
            
            switch (message.Msg)
            {
                case Win32.WM_INPUT:
                    {
                        KeyboardDriver.ProcessRawInput(message.LParam);
                    }
                    break;

                case Win32.WM_USB_DEVICECHANGE:
                    {
                        switch (message.WParam.ToInt32())
                        {
                            case Win32.DBT_DEVICEARRIVAL:
                                Debug.WriteLine("USB Device Arrival");
                                OnDeviceChange?.Invoke();
                                break;

                            case Win32.DBT_DEVICEREMOVECOMPLETE:
                                Debug.WriteLine("USB Device Removal");
                                OnDeviceChange?.Invoke();
                                break;
                        }
                        
                        KeyboardDriver.EnumerateDevices();
                    }
                    break;
            }

            base.WndProc(ref message);
        }

        ~RawInput()
        {
            Win32.UnregisterDeviceNotification(_devNotifyHandle);
            RemoveMessageFilter();
        }

    }
}
