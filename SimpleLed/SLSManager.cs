using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using RawInput_dll;

namespace SimpleLed
{
    /// <summary>
    /// Manager for SimpleLed drivers
    /// </summary>
    public class SLSManager
    {
        private readonly RawInput rawInput;
        private readonly string configPath;

        public List<ISimpleLed> Drivers = new List<ISimpleLed>();

        /// <summary>
        /// Initialise a new instance of the SLSManager.
        /// </summary>
        /// <param name="cfgPath">Path where configs are stored by the drivers</param>
        public SLSManager(string cfgPath)
        {
            DummyForm dummy = null;
            IntPtr handle = IntPtr.Zero;

            Task.Run(() =>
            {
                dummy = new DummyForm();

                handle = dummy.Handle;

                dummy.Hide();
                Application.Run(dummy);

            });

            while (handle == IntPtr.Zero)
            {
                Thread.Sleep(33);
            }


            //Thread.Sleep(1000);

            configPath = cfgPath;

            rawInput = new RawInput(handle);
            rawInput.AddMessageFilter(); // Adding a message filter will cause keypresses to be handled

        }

        /// <summary>
        /// Gets a List of all the devices provided by the loaded SimpleLed Drivers
        /// </summary>
        /// <returns>List of ControlDevice</returns>
        public List<ControlDevice> GetDevices()
        {
            List<ControlDevice> controlDevices = new List<ControlDevice>();
            foreach (var simpleLedDriver in Drivers)
            {
                try
                {
                    var devices = simpleLedDriver.GetDevices();
                    if (devices != null)
                    {
                        controlDevices.AddRange(devices);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }

            return controlDevices;
        }

        /// <summary>
        /// Runs the initial non-constructor setup for all loaded SimpleLed drivers.
        /// </summary>
        public void Init()
        {
            foreach (var simpleLedDriver in Drivers)
            {

                if ((simpleLedDriver as ISimpleLedWithKeyboardHook) != null)
                {
                    ((ISimpleLedWithKeyboardHook)simpleLedDriver).RawInput = rawInput;
                }


                if ((simpleLedDriver as ISimpleLedWithKeyboardHookAndConfig) != null)
                {
                    ((ISimpleLedWithKeyboardHookAndConfig)simpleLedDriver).RawInput = rawInput;
                }

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
                LoadConfig(simpleLedDriver);
            }
        }

        /// <summary>
        /// Saves config for all loaded SimpleLed drivers
        /// </summary>
        public void SaveConfigs()
        {
            foreach (ISimpleLedWithConfig simpleLedDriver in Drivers.OfType<ISimpleLedWithConfig>())
            {
                SaveConfig(simpleLedDriver);
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
        /// <param name="simpleLed">device to save config for</param>
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
