using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using HardwareHelperLib;

namespace SimpleLed
{
    public static class DisableHardware
    {
        const uint DIF_PROPERTYCHANGE = 0x12;
        const uint DICS_ENABLE = 1;
        const uint DICS_DISABLE = 2;  // disable device
        const uint DICS_FLAG_GLOBAL = 1; // not profile-specific
        const uint DIGCF_ALLCLASSES = 4;
        const uint DIGCF_PRESENT = 2;
        const uint ERROR_NO_MORE_ITEMS = 259;
        const uint ERROR_ELEMENT_NOT_FOUND = 1168;

        static DEVPROPKEY DEVPKEY_Device_DeviceDesc;
        static DEVPROPKEY DEVPKEY_Device_HardwareIds;

        [StructLayout(LayoutKind.Sequential)]
        struct SP_CLASSINSTALL_HEADER
        {
            public UInt32 cbSize;
            public UInt32 InstallFunction;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct SP_PROPCHANGE_PARAMS
        {
            public SP_CLASSINSTALL_HEADER ClassInstallHeader;
            public UInt32 StateChange;
            public UInt32 Scope;
            public UInt32 HwProfile;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SP_DEVINFO_DATA
        {
            public UInt32 cbSize;
            public Guid classGuid;
            public UInt32 devInst;
            public UInt32 reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DEVPROPKEY
        {
            public Guid fmtid;
            public UInt32 pid;
        }

        [DllImport("setupapi.dll", SetLastError = true)]
        static extern IntPtr SetupDiGetClassDevsW(
            [In] ref Guid ClassGuid,
            [MarshalAs(UnmanagedType.LPWStr)]
    string Enumerator,
            IntPtr parent,
            UInt32 flags);

        [DllImport("setupapi.dll", SetLastError = true)]
        static extern bool SetupDiDestroyDeviceInfoList(IntPtr handle);

        [DllImport("setupapi.dll", SetLastError = true)]
        static extern bool SetupDiEnumDeviceInfo(IntPtr deviceInfoSet,
            UInt32 memberIndex,
            [Out] out SP_DEVINFO_DATA deviceInfoData);

        [DllImport("setupapi.dll", SetLastError = true)]
        static extern bool SetupDiSetClassInstallParams(
            IntPtr deviceInfoSet,
            [In] ref SP_DEVINFO_DATA deviceInfoData,
            [In] ref SP_PROPCHANGE_PARAMS classInstallParams,
            UInt32 ClassInstallParamsSize);

        [DllImport("setupapi.dll", SetLastError = true)]
        static extern bool SetupDiChangeState(
            IntPtr deviceInfoSet,
            [In] ref SP_DEVINFO_DATA deviceInfoData);

        [DllImport("setupapi.dll", SetLastError = true)]
        static extern bool SetupDiGetDevicePropertyW(
                IntPtr deviceInfoSet,
                [In] ref SP_DEVINFO_DATA DeviceInfoData,
                [In] ref DEVPROPKEY propertyKey,
                [Out] out UInt32 propertyType,
                IntPtr propertyBuffer,
                UInt32 propertyBufferSize,
                out UInt32 requiredSize,
                UInt32 flags);

        static DisableHardware()
        {
            DisableHardware.DEVPKEY_Device_DeviceDesc = new DEVPROPKEY();
            DEVPKEY_Device_DeviceDesc.fmtid = new Guid(
                    0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67,
                    0xd1, 0x46, 0xa8, 0x50, 0xe0);
            DEVPKEY_Device_DeviceDesc.pid = 2;

            DEVPKEY_Device_HardwareIds = new DEVPROPKEY();
            DEVPKEY_Device_HardwareIds.fmtid = new Guid(
                0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67,
                0xd1, 0x46, 0xa8, 0x50, 0xe0);
            DEVPKEY_Device_HardwareIds.pid = 3;
        }




        public static string GetName(Func<string, bool> filter)
        {
            IntPtr info = IntPtr.Zero;
            Guid NullGuid = Guid.Empty;

            info = SetupDiGetClassDevsW(
                ref NullGuid,
                null,
                IntPtr.Zero,
                DIGCF_ALLCLASSES);
            CheckError("SetupDiGetClassDevs");

            SP_DEVINFO_DATA devdata = new SP_DEVINFO_DATA();
            devdata.cbSize = (UInt32)Marshal.SizeOf(devdata);

            // Get first device matching device criterion.
            string devicepath;
            List<IntPtr> infos = new List<IntPtr>();
            for (uint i = 0; ; i++)
            {
                SetupDiEnumDeviceInfo(info, i, out devdata);
                // if no items match filter, throw
                if (Marshal.GetLastWin32Error() == ERROR_NO_MORE_ITEMS)
                    break;
                CheckError("SetupDiEnumDeviceInfo");

                var DEVPKEY_Device_HardwareIdsor = new DisableHardware.DEVPROPKEY();
                DEVPKEY_Device_HardwareIdsor.fmtid = new Guid((uint)0x540b947e, (ushort)0x8b40, (ushort)0x45bc, 0xa8, 0xa2, 0x6a, 0x0b, 0x89, 0x4c, 0xbd, 0xa2);
                DEVPKEY_Device_HardwareIdsor.pid = 4;

                devicepath = GetStringPropertyForDevice(info, devdata, DEVPKEY_Device_HardwareIds);

                if (devicepath != null && filter(devicepath))
                {
                    string prettyName = GetStringPropertyForDevice(info, devdata, DEVPKEY_Device_HardwareIdsor);
                    if (!string.IsNullOrWhiteSpace(prettyName))
                    {
                        if (!devicepath.Contains("&MI_"))
                        {
                            return prettyName;
                        }
                    }
                }

            }

            return GetName(info);
        }

        public static string GetDevicePath(Func<string, bool> filter, Guid hwId)
        {
            IntPtr info = IntPtr.Zero;
            Guid NullGuid = Guid.Empty;

            info = SetupDiGetClassDevsW(
                ref NullGuid,
                null,
                IntPtr.Zero,
                DIGCF_ALLCLASSES);
            CheckError("SetupDiGetClassDevs");

            SP_DEVINFO_DATA devdata = new SP_DEVINFO_DATA();
            devdata.cbSize = (UInt32)Marshal.SizeOf(devdata);

            // Get first device matching device criterion.
            string devicepath;
            List<IntPtr> infos = new List<IntPtr>();
            for (uint i = 0; ; i++)
            {
                SetupDiEnumDeviceInfo(info, i, out devdata);
                // if no items match filter, throw
                if (Marshal.GetLastWin32Error() == ERROR_NO_MORE_ITEMS)
                    break;
                CheckError("SetupDiEnumDeviceInfo");

                DEVPKEY_Device_HardwareIds.fmtid = hwId;

                devicepath = GetStringPropertyForDevice(info, devdata, DEVPKEY_Device_HardwareIds);

                if (devicepath != null && filter(devicepath))
                {
                    return devicepath;
                }

            }

            List<string> names = new List<string>();

            foreach (var i in infos)
            {
                names.Add(GetName(i));
            }

            return GetName(info);
        }

        public static List<string> GetNames(Func<string, bool> filter, bool disable = true)
        {
            List<string> prettyNames = new List<string>();
            var DEVPKEY_Device_HardwareIdsor = new DisableHardware.DEVPROPKEY();
            DEVPKEY_Device_HardwareIdsor.fmtid = new Guid((uint)0x540b947e, (ushort)0x8b40, (ushort)0x45bc, 0xa8, 0xa2, 0x6a, 0x0b, 0x89, 0x4c, 0xbd, 0xa2);
            DEVPKEY_Device_HardwareIdsor.pid = 4;

            IntPtr info = IntPtr.Zero;
            Guid NullGuid = Guid.Empty;

            info = SetupDiGetClassDevsW(
                ref NullGuid,
                null,
                IntPtr.Zero,
                DIGCF_ALLCLASSES);
            CheckError("SetupDiGetClassDevs");

            SP_DEVINFO_DATA devdata = new SP_DEVINFO_DATA();
            devdata.cbSize = (UInt32)Marshal.SizeOf(devdata);

            // Get first device matching device criterion.
            string devicepath;
            List<IntPtr> infos = new List<IntPtr>();
            for (uint i = 0; ; i++)
            {
                SetupDiEnumDeviceInfo(info, i, out devdata);
                // if no items match filter, throw
                if (Marshal.GetLastWin32Error() == ERROR_NO_MORE_ITEMS)
                    break;
                CheckError("SetupDiEnumDeviceInfo");

                devicepath = GetStringPropertyForDevice(info, devdata, DEVPKEY_Device_HardwareIds);
                string prettyName = GetStringPropertyForDevice(info, devdata, DEVPKEY_Device_HardwareIdsor);

                if (devicepath != null && filter(devicepath))
                {
                    infos.Add(info);

                    if (!string.IsNullOrWhiteSpace(prettyName))
                    {
                        prettyNames.Add(prettyName);
                    }
                }

            }

            List<string> names = new List<string>();

            foreach (var i in infos)
            {
                names.Add(GetName(i));
            }

            return names;
        }

        public static string GetName(IntPtr hDevInfo)
        {
            try
            {
                Native.SP_DEVINFO_DATA DeviceInfoData;
                DeviceInfoData = new Native.SP_DEVINFO_DATA();
                DeviceInfoData.cbSize = 28;
                //is devices exist for class
                DeviceInfoData.devInst = 0;
                DeviceInfoData.classGuid = System.Guid.Empty;
                DeviceInfoData.reserved = 0;
                UInt32 i;
                StringBuilder DeviceName = new StringBuilder("");
                DeviceName.Capacity = Native.MAX_DEV_LEN;
                for (i = 0; Native.SetupDiEnumDeviceInfo(hDevInfo, i, DeviceInfoData); i++)
                {
                    Debug.WriteLine(hDevInfo + " - " + i + "- " + DeviceInfoData.classGuid);
                    //Declare vars
                    int skip = 0;
                    while (!Native.SetupDiGetDeviceRegistryProperty(hDevInfo,
                        DeviceInfoData,
                        Native.SPDRP_DEVICEDESC,
                        0,
                        DeviceName,
                        Native.MAX_DEV_LEN,
                        IntPtr.Zero))
                    {
                        //                        Debug.WriteLine("Skip");
                        skip++;

                        if (skip > 1024)
                        {
                            Debug.WriteLine("skipping");
                            break;
                        }
                    }

                    if (skip < 1024)
                    {
                        Debug.WriteLine(skip+ " Got Name " +DeviceName.ToString());
                        return (DeviceName.ToString());
                    }
                }
            }
            catch
            {
            }

            return null;
        }
        private static void CheckError(string message, int lasterror = -1)
        {

            int code = lasterror == -1 ? Marshal.GetLastWin32Error() : lasterror;
            if (code != 0)
                throw new ApplicationException(
                    String.Format("Error disabling hardware device (Code {0}): {1}",
                        code, message));
        }

        public static string GetStringPropertyForDevice(IntPtr info, SP_DEVINFO_DATA devdata, DEVPROPKEY key)
        {
            uint proptype, outsize;
            IntPtr buffer = IntPtr.Zero;
            try
            {
                uint buflen = 512;
                buffer = Marshal.AllocHGlobal((int)buflen);
                SetupDiGetDevicePropertyW(
                    info,
                    ref devdata,
                    ref key,
                    out proptype,
                    buffer,
                    buflen,
                    out outsize,
                    0);
                byte[] lbuffer = new byte[outsize];
                Marshal.Copy(buffer, lbuffer, 0, (int)outsize);
                int errcode = Marshal.GetLastWin32Error();
                if (errcode == ERROR_ELEMENT_NOT_FOUND) return null;
                CheckError("SetupDiGetDeviceProperty", errcode);
                return Encoding.Unicode.GetString(lbuffer);
            }
            finally
            {
                if (buffer != IntPtr.Zero)
                    Marshal.FreeHGlobal(buffer);
            }
        }
    }
}
