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
        public ThemeWatcher.WindowsTheme GetTheme()
        {
            return InternalSolids.WindowsTheme;
        }

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

            configPath = cfgPath;
            currentUSBDevices = SLSManager.GetUSBDevices(true);
            InternalSolids.RawInput = new RawInput.RawInput(handle);
            InternalSolids.RawInput.OnDeviceChange = OnDeviceChange;
            InternalSolids.RawInput.AddMessageFilter(); // Adding a message filter will cause keypresses to be handled

        }
        private List<CustomDeviceSpecification> userTemplates = new List<CustomDeviceSpecification>();

        public void AddUserTemplate(CustomDeviceSpecification cds)
        {
            userTemplates.Add(cds);
        }

        public List<CustomDeviceSpecification> GetCustomDeviceSpecifications()
        {
            List<CustomDeviceSpecification> result = new List<CustomDeviceSpecification>();

            result.Add(new GenericLEDStrip());
            result.Add(new EightByEightMatrix());
            result.Add(new GenericAIO());
            result.Add(new GenericBulb());
            result.Add(new GenericCooler());
            result.Add(new GenericFan());
            result.Add(new GenericGPU());
            result.Add(new GenericHeadSet());
            result.Add(new GenericKeyboard());
            result.Add(new GenericKeypad());
            result.Add(new GenericLEDStrip());
            result.Add(new GenericMemory());
            result.Add(new GenericMotherboard());
            result.Add(new GenericMouse());
            result.Add(new GenericMousePad());
            result.Add(new GenericOther());
            result.Add(new GenericPSU());
            result.Add(new GenericSpeakers());
            result.Add(new GenericWaterBlock());
            foreach (ISimpleLed simpleLed in Drivers)
            {
                result.AddRange(simpleLed?.GetProperties()?.GetCustomDeviceSpecifications?.Invoke() ?? new List<CustomDeviceSpecification>());
            }

            result.AddRange(userTemplates);
            return result;
        }

        public List<Type> GetMappers(ISimpleLed driver)
        {
            List<Type> result = new List<Type>();
            result.Add(typeof(LMatrix));
            result.Add(typeof(SMatrix));

            //TODO fix STA stuff
            //var props = driver.GetProperties();
            //if (props?.GetMappers != null)
            //{
            //    result.AddRange(props.GetMappers());
            //}

            return result;
        }

        public static Dictionary<string, Type> GetMapperLookup(ISimpleLed driver)
        {
            List<Type> result = new List<Type>();
            result.Add(typeof(LMatrix));
            result.Add(typeof(SMatrix));

            var props = driver.GetProperties();
            if (props?.GetMappers != null)
            {
                result.AddRange(props.GetMappers());
            }

            Dictionary<string,Type> r = new Dictionary<string, Type>();
            foreach (Type type in result)
            {
                Mapper t = (Mapper) Activator.CreateInstance(type);
                r.Add(t.GetName(), type);
            }

            return r;
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
                    List<USBDevice> interestedAdds = props.SupportedDevices.Where(x => newlyConnectedDevices.Any(p => x.VID == p.VID && (x.HID == null || x.HID.Value == p.HID))).ToList();

                    foreach (USBDevice interestedAdd in interestedAdds)
                    {
                        try
                        {
                            simpleLed.InterestedUSBChange(interestedAdd.VID, interestedAdd.HID.Value, true);
                        }
                        catch
                        {
                        }
                    }
                    
                    List<USBDevice> interestedRemoves = props.SupportedDevices.Where(x => newlyDisconnectedDevices.Any(p => x.VID == p.VID && (x.HID == null || x.HID.Value == p.HID))).ToList();

                    foreach (USBDevice interestedAdd in interestedRemoves)
                    {
                        try
                        {
                            simpleLed.InterestedUSBChange(interestedAdd.VID, interestedAdd.HID.Value, false);
                        }
                        catch
                        {
                        }
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
            
            List<USBDevice> result = devices.ToList();

            foreach (USBDevice hidDeviceInfo in result)
            {
                var manu = usbDevices.Where(x => x.VID == hidDeviceInfo.VID).ToList();

                if (hidDeviceInfo.HID.HasValue)
                {
                    var test = manu.FirstOrDefault(x => x.HID == hidDeviceInfo.HID.Value);

                    hidDeviceInfo.IsConnected = usbDevices.Any(x => x.HID == hidDeviceInfo.HID && x.VID == hidDeviceInfo.VID);
                    continue;
                }
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
                            (string)device.GetPropertyValue("DeviceID"),
                            (string)device.GetPropertyValue("PNPDeviceID"),
                            (string)device.GetPropertyValue("Description"))
                    { Name = (string)device.GetPropertyValue("Name") });

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
                                if (!string.IsNullOrWhiteSpace(dvc.PID))
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

        public void Init()
        {
            foreach (ISimpleLed simpleLedDriver in Drivers)
            {
                simpleLedDriver.Configure(null);
            }
        }

        private ColorProfile colorProfile;

        public ColorProfile ColorProfile
        {
            get => colorProfile;
            set
            {
                colorProfile = value;
                foreach (ISimpleLed simpleLedDriver in Drivers)
                {
                    var props = simpleLedDriver.GetProperties();
                    props.SetColorProfileAction?.Invoke(value);
                }
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
            string path = configPath + "\\" + simpleLed.GetProperties().ProductId + "_config.json";
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
            string path = configPath + "\\" + simpleLed.GetProperties().ProductId + "_config.json";
            try
            {
                File.WriteAllText(path, json);
            }
            catch
            {
            }
        }

    }
}
