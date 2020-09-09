using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SimpleLed
{
    public class SLSManager
    {
        private string configPath;
        public List<ISimpleLed> Drivers = new List<ISimpleLed>();

        public SLSManager(string cfgPath)
        {
            configPath = cfgPath;
        }

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

        public void Init()
        {
            foreach (var simpleLedDriver in Drivers)
            {
                simpleLedDriver.Configure(null);
            }
        }

        public void LoadConfigs()
        {
            foreach (ISimpleLedWithConfig simpleLedDriver in Drivers.OfType<ISimpleLedWithConfig>())
            {
                LoadConfig(simpleLedDriver);
            }
        }

        public void SaveConfigs()
        {
            foreach (ISimpleLedWithConfig simpleLedDriver in Drivers.OfType<ISimpleLedWithConfig>())
            {
                SaveConfig(simpleLedDriver);
            }
        }

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
