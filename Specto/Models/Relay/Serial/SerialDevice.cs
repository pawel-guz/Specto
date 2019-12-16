using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace Specto.Relay
{
    class SerialDevice : Device
    {
        #region Static   
        private static readonly byte DataSignal = 255;
        private static readonly byte[] Handshake = new byte[] { DataSignal, DataSignal };

        public static async Task GetDevicesAsync()
        {
            foreach (var portName in SerialPort.GetPortNames())
            {
                var device = new SerialDevice(new SerialPort(portName));
                bool  deviceCompatible = false;

                try
                {
                    device.Connect();
                    if (device.Port.IsOpen)
                    {
                        device.Port.Write(Handshake, 0, Handshake.Length);
                        await Task.Delay(50);

                        byte response = 0;
                        while (device.Port.BytesToRead > 0)
                        {
                            response = (byte)device.Port.ReadByte(); 
                            deviceCompatible = (response == DataSignal);
                        }
                    }
                }

                finally
                {
                    if (deviceCompatible)
                    {
                        lock(Devices)
                        { 
                            Device.Devices.Add(device);
                            NotifyDevicesChanged();
                        }
                    }
                    else device.Dispose();
                }
            } 
        }
        #endregion

        private SerialPort Port { get; set; }
        private bool IsReady { get; set; }

        public override string Name => "Serial";
        public override bool IsSerial => true;

        public ICommand SendColorRequest => new RelayCommand<bool>(OnSendColorRequest);
        private void OnSendColorRequest(bool value) => IsActive = !value;

        public override bool IsActive
        {
            get { return isActive; }
            set
            {
                isActive = value;
                SetConnection(isActive);
                RaisePropertyChanged("IsActive");
            }
        }

        private SerialDevice(SerialPort port)
        { 
            Name = port.PortName;
            SerialNumber = "-";
            ConnectionType = ConnectionType.Serial;
            ConnectionInfo = $"via {port.PortName} serial port" ;

            Port = port;
            Port.BaudRate = 19200;
            Port.ReadTimeout = 500;
            Port.WriteTimeout = 500;

            IsActive = true;
        }

        void SetConnection(bool value)
        {
            if (value)
                Connect();
            else Disconnect();
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
                try
                { 
                    SendColor(Colors.Black);
                    Port.Close();
                    IsReady = false;
                }
                catch
                {
                    IsReady = true;
                }
            }
        }

        public override void SendColor(Color color)
        {  
            if (!IsReady)
                Connect();

            if (IsReady)
            { 
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

        public override void Dispose()
        {
            base.Dispose();
            Disconnect();
            Port.Dispose();
        } 
    }
}
