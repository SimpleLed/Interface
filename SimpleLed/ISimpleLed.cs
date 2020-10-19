using System;
using System.Collections.Generic;
using System.Windows.Controls;
using SimpleLed.RawInput;

namespace SimpleLed
{
    /// <summary>
    /// Main interface for simple led.
    /// </summary>
    public interface ISimpleLed : IDisposable
    {
        event Events.DeviceChangeEventHandler DeviceAdded;
        event Events.DeviceChangeEventHandler DeviceRemoved;


        /// <summary>
        /// Event that is fired when the number of control devices this driver is offering changes
        /// </summary>
        //event EventHandler DeviceRescanRequired;

        /// <summary>
        /// Initial config/setup post constructor
        /// </summary>
        /// <param name="driverDetails">specific details this plugin needs to startup - param probably being obsoleted soon, just pass null</param>
        void Configure(DriverDetails driverDetails);
        ///// <summary>
        ///// Fetch all devices found by device
        ///// </summary>
        ///// <returns>List of devices provided</returns>
        //List<ControlDevice> GetDevices();
        /// <summary>
        /// Push local LEDs to device/SDK
        /// </summary>
        /// <param name="controlDevice">Device to push</param>
        void Push(ControlDevice controlDevice);
        /// <summary>
        /// Pull LEDs from device/SDK to local LEDs
        /// </summary>
        /// <param name="controlDevice">Device to pull from</param>
        void Pull(ControlDevice controlDevice);
        /// <summary>
        /// Get Properties from this driver
        /// </summary>
        /// <returns></returns>
        DriverProperties GetProperties();
        /// <summary>
        /// Fetch config from driver
        /// </summary>
        /// <typeparam name="T">SLSConfig data or inherited type</typeparam>
        /// <returns>SLSConfig data or inherited type</returns>
        T GetConfig<T>() where T : SLSConfigData;
        /// <summary>
        /// Push config to driver
        /// </summary>
        /// <typeparam name="T">SLSConfig data or inherited type</typeparam>
        /// <param name="config">SLSConfig data or inherited type</param>
        void PutConfig<T>(T config) where T : SLSConfigData;
        /// <summary>
        /// Fetch name of driver
        /// </summary>
        /// <returns>pretty driver name as string</returns>
        string Name();

        void InterestedUSBChange(int VID, int PID);

    }

    /// <summary>
    /// Extended version of interface for drivers that support custom UI config
    /// </summary>
    public interface ISimpleLedWithConfig : ISimpleLed
    {
        /// <summary>
        /// Get custom UI UserControl. Should encompass driver specifics and device specifics. With the latter, a ControlDevice is passed which is assumed to be the specific device being configured.
        /// </summary>
        /// <param name="controlDevice">Device to config</param>
        /// <returns>UserControl containing the custom UI</returns>
        UserControl GetCustomConfig(ControlDevice controlDevice);
        /// <summary>
        /// Ascertains if current config is "dirty" and needs to be saved.
        /// </summary>
        /// <returns>true if save is required.</returns>
        bool GetIsDirty();
        /// <summary>
        /// Overwrites if the current config should be considered dirty or not.
        /// </summary>
        /// <param name="val"></param>
        void SetIsDirty(bool val);
    }

    /// <summary>
    /// Driver properties
    /// </summary>
    public class DriverProperties
    {
        /// <summary>
        /// Can this device Pull LEDs from its Device/SDK?
        /// </summary>
        public bool SupportsPull { get; set; }
        /// <summary>
        /// Does this device support pushing to the Device/SDK
        /// </summary>
        public bool SupportsPush { get; set; }
        /// <summary>
        /// Is this device a "source" - does it generate its own LEDs?
        /// </summary>
        public bool IsSource { get; set; }
        /// <summary>
        /// Does this device support custom configs
        /// </summary>
        public bool SupportsCustomConfig { get; set; }
        /// <summary>
        /// Driver specific UUID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Name of the creator of this driver.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Current Version Number
        /// </summary>
        public ReleaseNumber CurrentVersion { get; set; }
        /// <summary>
        /// Text about this driver
        /// </summary>
        public string Blurb { get; set; }
        /// <summary>
        /// Link to public GitHub (or alternative) project page.
        /// </summary>
        public string GitHubLink { get; set; }
        /// <summary>
        /// Link to driver's homepage
        /// </summary>
        public string HomePage { get; set; }
        /// <summary>
        /// Is this a publicly released driver ( considered beta otherwise )
        /// </summary>
        public bool IsPublicRelease { get; set; }

        public List<USBDevice> SupportedDevices { get; set; }

    }


    /// <summary>
    /// Describes a USB device
    /// </summary>
    public class USBDevice
    {
        public int VID { get; set; }
        public int? HID { get; set; }

        public string ManufacturerName { get; set; }
        public string DeviceName { get; set; }
        public string DeviceType { get; set; }
        public bool IsConnected { get; set; }
        public string DevicePrettyName { get; set; }
    }

    /// <summary>
    ///  details to pass the plugin on init
    /// </summary>
    public class DriverDetails
    {
        /// <summary>
        /// path the plugin was loaded from
        /// </summary>
        public string HomeFolder { get; set; }
    }
}
