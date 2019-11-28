using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Specto.Relay
{
    class SerialDevice : IVisualizationDevice
    {
        #region Static   
        public static List<SerialDevice> Devices { get; private set; } = new List<SerialDevice>();

        private static readonly byte DataSignal = 255;
        private static readonly byte[] Handshake = new byte[] { DataSignal, DataSignal };

        public static void DetectDevices()
        {
            FreeDevices();

            foreach (var portName in SerialPort.GetPortNames())
            {
                var device = new SerialDevice(new SerialPort(portName));

                var deviceCompatible = false;
                try
                {
                    device.Connect();
                    if (device.Port.IsOpen)
                    {
                        device.Port.Write(Handshake, 0, Handshake.Length);
                        Thread.Sleep(50);

                        byte response = 0;
                        while (device.Port.BytesToRead > 0)
                        {
                            response = (byte)device.Port.ReadByte();
                            if (response == DataSignal)
                                deviceCompatible = true;
                        }
                    }
                }
                finally
                {
                    if (deviceCompatible)
                        Devices.Add(device);
                    else device.Dispose();
                }
            } 
        }

        public static void FreeDevices()
        {
            foreach (var d in Devices)
                d.Dispose();

            Devices.Clear();
        }
        #endregion

        private bool _isActive;

        private SerialPort Port { get; set; }
        private bool IsReady { get; set; }        

        public string Name { get; private set; }
        public string SerialNumber { get; private set; }
        public ConnectionType ConnectionType { get; private set; }
        public string ConnectionInfo { get; private set; } 
        public bool IsSerial { get { return true; } }
        public bool IsWiFi { get { return false; } } 
        public ObservableCollection<Network.WiFiData> AvailableNetworks { get { return null; } }

        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if(value)
                {
                    _isActive = true;
                    Connect();
                }
                else
                {
                    Disconnect();
                    _isActive = false;
                }
            }
        }

        private SerialDevice(SerialPort port)
        {
            Name = port.PortName;
            SerialNumber = "-";
            ConnectionType = ConnectionType.Serial;
            ConnectionInfo = Enum.GetName(typeof(ConnectionType), ConnectionType) + ": " + port.PortName;

            Port = port;
            Port.BaudRate = 19200;
            Port.ReadTimeout = 500;
            Port.WriteTimeout = 500;

            IsActive = true;
        }

        void Connect()
        {
            if (!Port.IsOpen)
            {
                try
                {
                    Port.Open();
                    IsReady = true;
                }
                catch
                {
                    IsReady = false;
                }
            }
        }

        void Disconnect()
        {
            if (Port.IsOpen)
            {
                Send(new VisualizationData(null, Colors.Black));
                try
                {
                    Port.Close();
                    IsReady = false;
                }
                catch
                {
                    IsReady = true;
                }
            }
        }

        public void Send(VisualizationData data)
        {
            if (!IsActive)
                return;

            if (!IsReady)
                Connect();

            if (IsReady)
            {
                var color = data.Color;
                byte r, g, b;
                r = color.R == DataSignal ? (byte)254 : color.R;
                g = color.G == DataSignal ? (byte)254 : color.G;
                b = color.B == DataSignal ? (byte)254 : color.B;

                try
                {
                    Byte[] toSend = { byte.MaxValue, r, g, b };
                    Port.Write(toSend, 0, 4);
                }
                catch
                {
                    IsReady = false;
                }
            }
        }

        public void Dispose()
        {
            Disconnect();
            Port.Dispose();
        }
        
        public Task ConnectWithNetwork(string ssid, string key)
        {
            // Send network configuration to device.
            return null;
        }
    }
}
