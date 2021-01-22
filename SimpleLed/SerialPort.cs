using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace SimpleLed
{
    public class SerialPortModel
    {
        public string PortName { get; set; }
        public string Name { get; set; }
        public Int16 VID { get; set; }
        public Int16 PID { get; set; }
    }

    public static class MadSerialPort
    {
        public static List<SerialPortModel> GetPortsFromUSBDetails(int vid, int pid)
        {
            string vidString = $"{vid:X4}";
            string pidString = $"{pid:X4}";
            string[] ports = SerialPort.GetPortNames();

            ManagementClass processClass = new ManagementClass("Win32_PnPEntity");

            ManagementObjectCollection Ports = processClass.GetInstances();

            List<SerialPortModel> serialPorts = new List<SerialPortModel>();

            foreach (ManagementBaseObject managementBaseObject in Ports)
            {
                if (managementBaseObject.GetPropertyValue("Name") != null)
                {

                    if (managementBaseObject.GetPropertyValue("Name").ToString().Contains("COM"))
                    {
                        var did = managementBaseObject.GetPropertyValue("DeviceID").ToString();
                        if (did.Contains(vidString) && did.Contains(pidString))
                        {
                            var caption = managementBaseObject["Caption"].ToString();

                            var portList = ports.Where(n => caption.Contains(n)).ToList();

                            if (portList.Any())
                            {
                                serialPorts.Add(new SerialPortModel
                                {
                                    Name = managementBaseObject.GetPropertyValue("Name").ToString(),
                                    PID = (Int16)pid,
                                    VID = (Int16)vid,
                                    PortName = portList.First()
                                });
                            }
                        }
                    }
                }
            }

            return serialPorts;
        }
    }
}
