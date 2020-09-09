using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace SimpleLed
{
    public interface ISimpleLed : IDisposable
    {
        void Configure(DriverDetails driverDetails);
        List<ControlDevice> GetDevices();
        void Push(ControlDevice controlDevice);
        void Pull(ControlDevice controlDevice);
        DriverProperties GetProperties();
        T GetConfig<T>() where T : SLSConfigData;
        void PutConfig<T>(T config) where T : SLSConfigData;



        string Name();
    }

    public interface ISimpleLedWithConfig : ISimpleLed
    {
        UserControl GetCustomConfig(ControlDevice controlDevice);
        bool GetIsDirty();
        void SetIsDirty(bool val);
    }

    public class DriverProperties
    {
        public bool SupportsPull { get; set; }
        public bool SupportsPush { get; set; }
        public bool IsSource { get; set; }
        public bool SupportsCustomConfig { get; set; }
        public Guid Id { get; set; }

        public string Author { get; set; }

        public ReleaseNumber CurrentVersion { get; set; }
        public string Blurb { get; set; }
        public string GitHubLink { get; set; }
        public string HomePage { get; set; }
        public bool IsPublicRelease { get; set; }
    }

    public class DriverDetails
    {

        public string Name { get; set; }
        public virtual string Platform { get; } = "Unknown";
    }

    public class MadLedDriverDetails : DriverDetails
    {
        public override string Platform { get; } = "MadLed";
        public string ComPort { get; set; }
    }
}
