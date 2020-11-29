using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SimpleLed.RawInput;
using System.Management;
using HardwareHelperLib;

namespace SimpleLed
{
    /// <summary>
    /// Manager for SimpleLed drivers
    /// </summary>
    public class SLSManager
    {
        public event Events.DeviceChangeEventHandler DeviceAdded;
        public event Events.DeviceChangeEventHandler DeviceRemoved;

        private readonly string configPath;
        private List<USBModels.USBDeviceInfo> currentUSBDevices;
        public ObservableCollection<ISimpleLed> Drivers = new ObservableCollection<ISimpleLed>();

        /// <summary>
        /// Initialise a new instance of the SLSManager.
        /// </summary>
        /// <param name="cfgPath">Path where configs are stored by the drivers</param>
        public SLSManager(string cfgPath)
        {
            Drivers.CollectionChanged += DriversOnCollectionChanged;

            DummyForm dummy = null;
            IntPtr handle = IntPtr.Zero;

            Debug.WriteLine("Starting up keyboard monitor");
            Task.Run(() =>
            {
                dummy = new DummyForm();

                handle = dummy.Handle;

                dummy.Hide();
                Debug.WriteLine("Running kb monitor");
                Application.Run(dummy);
                Debug.WriteLine("kb monitor exit");

            });


            while (handle == IntPtr.Zero)
            {
                Thread.Sleep(33);
            }


            //Thread.Sleep(1000);

            configPath = cfgPath;
            currentUSBDevices = SLSManager.GetUSBDevices(true);
            InternalSolids.RawInput = new RawInput.RawInput(handle);
            InternalSolids.RawInput.OnDeviceChange = OnDeviceChange;
            InternalSolids.RawInput.AddMessageFilter(); // Adding a message filter will cause keypresses to be handled

        }

        private void OnDeviceChange()
        {
            List<USBModels.USBDeviceInfo> newDevices = SLSManager.GetUSBDevices(true);

            List<USBModels.USBDeviceInfo> newlyConnectedDevices = newDevices.Where(x => currentUSBDevices.All(p => p.VID != x.VID && p.HID != x.HID)).ToList();
            List<USBModels.USBDeviceInfo> newlyDisconnectedDevices = currentUSBDevices.Where(x => newDevices.All(p => p.VID != x.VID && p.HID != x.HID)).ToList();

            foreach (ISimpleLed simpleLed in Drivers)
            {
                var props = simpleLed.GetProperties();
                if (props.SupportedDevices != null)
                {
                    var interestedAdds = props.SupportedDevices.Where(x =>
                            newlyConnectedDevices.Any(p => x.VID == p.VID && (x.HID == null || x.HID.Value == p.HID)))
                        .ToList();

                    foreach (USBDevice interestedAdd in interestedAdds)
                    {
                        simpleLed.InterestedUSBChange(interestedAdd.VID, interestedAdd.HID.Value,true);
                    }


                    var interestedRemoves = props.SupportedDevices.Where(x =>
                            newlyDisconnectedDevices.Any(p => x.VID == p.VID && (x.HID == null || x.HID.Value == p.HID)))
                        .ToList();

                    foreach (USBDevice interestedAdd in interestedRemoves)
                    {
                        simpleLed.InterestedUSBChange(interestedAdd.VID, interestedAdd.HID.Value,false);
                    }

                }
            }

            currentUSBDevices = newDevices.ToList();

        }

        public List<USBDevice> GetSupportedDevices()
        {
            List<USBDevice> thisList = new List<USBDevice>();
            foreach (ISimpleLed simpleLed in Drivers)
            {
                var props = simpleLed.GetProperties();
                if (props.SupportedDevices != null && props.SupportedDevices.Count > 0)
                {
                    thisList.AddRange(props.SupportedDevices);
                }
            }

            return GetSupportedDevices(thisList);
        }

        public static List<USBDevice> GetSupportedDevices(List<USBDevice> devices)
        {
            List<USBModels.USBDeviceInfo> usbDevices = GetUSBDevices();
            string json = JsonConvert.SerializeObject(usbDevices);

            // File.WriteAllText("fakeHardware.json",json);
            List<USBDevice> result = devices.ToList();
            foreach (USBDevice hidDeviceInfo in result)
            {
                if (hidDeviceInfo.HID.HasValue)
                {
                    hidDeviceInfo.IsConnected = usbDevices.Any(x => x.HID == hidDeviceInfo.HID && x.VID == hidDeviceInfo.VID);
                    continue;
                }
                //else
                //{
                //    var related = usbDevices.Where(t => !string.IsNullOrWhiteSpace(t.VEN)
                //                                        && Extensions.HexToInt(t.VEN) > 0
                //                                        && Extensions.HexToInt(t.VEN) == hidDeviceInfo.VID).ToList();


                //    List<int> pids = new List<int>();

                //    //Debug.WriteLine("Looking for " + hidDeviceInfo.DeviceType);
                //    foreach (var r in related)
                //    {
                //        if (pids.All(x => x != r.HID))
                //        {
                //            pids.Add(r.HID);
                //        }
                //    }

                //    foreach (var pid in pids)
                //    {
                //        var pidHardware = related.Where(x => x.HID == pid).ToList();
                //        bool keepProcessing = true;

                //        int ct = 0;
                //        foreach (var temp in pidHardware)
                //        {

                //           // Debug.WriteLine(pid + " -- " + ct + " - " + temp.PrettyName.Substring(0, temp.PrettyName.Length - 1) + " / " + temp.DeviceID + " | " + temp.Description);
                //            bool isExpectedHardwareType = DeviceClasses.ContainsKey(temp.Description);

                //            if (isExpectedHardwareType && keepProcessing)
                //            {
                //                //if (DeviceClasses[temp.Description] == hidDeviceInfo.DeviceType)
                //                {
                //                    //Debug.WriteLine(pid + " -- " + ct + " - its this one? " + temp.PrettyName.Substring(0, temp.PrettyName.Length - 1) + " / " + temp.Description + " / " + DeviceClasses[temp.Description] + " / " + temp.DeviceID);
                //                }

                //                //keepProcessing = false;
                //            }

                //            ct++;

                //        }
                //    }



                //    var g = related.ToList().GroupBy(x => x.PID);

                //    bool found = false;
                //    foreach (var glist in g)
                //    {
                //        //Debug.WriteLine(glist.First().HID);
                //        if (!found)
                //        {
                //            List<USBModels.USBDeviceInfo> gdevices = glist.ToList();

                //            var firstFound = gdevices.FirstOrDefault(x => DeviceClasses.ContainsKey(x.Description));
                //            if (firstFound != null)
                //            {
                //                if (hidDeviceInfo.DeviceType == DeviceClasses[firstFound.Description])
                //                {
                //                    found = true;
                //                    hidDeviceInfo.HID = firstFound.HID;
                //                    hidDeviceInfo.DeviceName = g.First().First().Description;
                //                    hidDeviceInfo.DevicePrettyName = g.First().First().PrettyName;

                //                }
                //            }

                //        }
                //    }

                //    hidDeviceInfo.IsConnected = found;

                //}
            }

            return result.Where(x => x.IsConnected).ToList();
        }

        private static Dictionary<string, string> DeviceClasses = new Dictionary<string, string>
        {
            {"HID Keyboard Device", "Keyboard"},
            {"HID-compliant mouse", "Mouse"},
            {"USB Audio Device","Headset"}
        };

        internal static List<USBModels.USBDeviceInfo> GetUSBDevices(bool simpleMode = false)
        {
            List<USBModels.USBDeviceInfo> devices = new List<USBModels.USBDeviceInfo>();

            ManagementObjectCollection collection;
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"Select * From Win32_PnPEntity"))
            {
                collection = searcher.Get();
            }

            foreach (ManagementBaseObject device in collection)
            {
                try
                {
                    USBModels.USBDeviceInfo dvc = (new USBModels.USBDeviceInfo(
                            (string) device.GetPropertyValue("DeviceID"),
                            (string) device.GetPropertyValue("PNPDeviceID"),
                            (string) device.GetPropertyValue("Description"))
                        {Name = (string) device.GetPropertyValue("Name")});

                    var parts = dvc.DeviceID.Split('\\');
                    dvc.Root = parts[0];
                    if (dvc.DeviceID.Contains("VEN_") || dvc.DeviceID.Contains("VID_") && parts.Length > 1)
                    {
                        var things = parts[1].Split('&');

                        if (things.Length > 1)
                        {
                            dvc.VEN = things[0].Split('_').Last();
                            dvc.PID = things[1].Split('_').Last();

                            try
                            {
                                dvc.HID = dvc.PID.HexToInt();
                                dvc.VID = dvc.VEN.HexToInt();

                                if (!simpleMode)
                                {
                                    string prettyName = "";
                                    prettyName = DisableHardware.GetName(n =>
                                        n.ToUpperInvariant().Contains("VID_" + dvc.VEN + "&PID_" + dvc.PID));
                                    dvc.PrettyName = prettyName;
                                }
                            }
                            catch
                            {
                            }
                        }

                        devices.Add(dvc);
                    }
                }
                catch
                {
                }
            }

            collection.Dispose();
            return devices;
        }


        private void DriversOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (object t in e.NewItems)
                {
                    try
                    {
                        if (t is ISimpleLed drv)
                        {
                            drv.DeviceAdded += Drv_DeviceAdded;
                            drv.DeviceRemoved += Drv_DeviceRemoved;
                        }
                    }
                    catch
                    {
                    }
                }
            }

            if (e.OldItems != null)
            {
                foreach (object t in e.OldItems)
                {
                    try
                    {
                        if (t is ISimpleLed drv)
                        {
                            drv.DeviceAdded -= Drv_DeviceAdded;
                            drv.DeviceRemoved -= Drv_DeviceRemoved;
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }

        private void Drv_DeviceAdded(object sender, Events.DeviceChangeEventArgs e)
        {
            DeviceAdded?.Invoke(sender, e);
        }

        private void Drv_DeviceRemoved(object sender, Events.DeviceChangeEventArgs e)
        {
            DeviceRemoved?.Invoke(sender, e);
        }

        private void DeviceRescanRequired(ISimpleLed drv)
        {
            RescanRequired?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Event fired when any managed driver has a change of devices offered
        /// </summary>
        public event EventHandler RescanRequired;

        /// <summary>
        /// Gets a List of all the devices provided by the loaded SimpleLed Drivers
        /// </summary>
        /// <returns>List of ControlDevice</returns>
        //public List<ControlDevice> GetDevices()
        //{
        //    List<ControlDevice> controlDevices = new List<ControlDevice>();
        //    foreach (var simpleLedDriver in Drivers)
        //    {
        //        try
        //        {
        //            var devices = simpleLedDriver.GetDevices();
        //            if (devices != null)
        //            {
        //                controlDevices.AddRange(devices);
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            Debug.WriteLine(e.Message);
        //        }
        //    }

        //    return controlDevices;
        //}

        /// <summary>
        /// Runs the initial non-constructor setup for all loaded SimpleLed drivers.
        /// </summary>
        public void Init()
        {
            foreach (ISimpleLed simpleLedDriver in Drivers)
            {
                simpleLedDriver.Configure(null);
            }
        }

        /// <summary>
        /// Loads config for all loaded SimpleLed drivers
        /// </summary>
        public void LoadConfigs()
        {
            foreach (ISimpleLedWithConfig simpleLedDriver in Drivers.OfType<ISimpleLedWithConfig>())
            {
                try
                {
                    LoadConfig(simpleLedDriver);
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Saves config for all loaded SimpleLed drivers
        /// </summary>
        public void SaveConfigs()
        {
            foreach (ISimpleLedWithConfig simpleLedDriver in Drivers.OfType<ISimpleLedWithConfig>())
            {
                try
                {
                    SaveConfig(simpleLedDriver);
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Load config for a specific device
        /// </summary>
        /// <param name="simpleLed">device to load config for</param>
        public void LoadConfig(ISimpleLedWithConfig simpleLed)
        {
            string path = configPath + "\\" + simpleLed.GetProperties().Id + "_config.json";
            string json = File.ReadAllText(path);
            SLSConfigData data = JsonConvert.DeserializeObject<SLSConfigData>(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                // $type no longer needs to be first
                MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead
            });
            simpleLed.PutConfig(data);
        }

        /// <summary>
        /// Save config for a specific device
        /// </summary>
        /// <param name="simpleLed">device to save config for</param>""
        public void SaveConfig(ISimpleLedWithConfig simpleLed)
        {
            simpleLed.SetIsDirty(false);
            SLSConfigData data = simpleLed.GetConfig<SLSConfigData>();
            string json = JsonConvert.SerializeObject(data, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                // $type no longer needs to be first
                MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead
            });
            string path = configPath + "\\" + simpleLed.GetProperties().Id + "_config.json";
            File.WriteAllText(path, json);
        }

    }
}
