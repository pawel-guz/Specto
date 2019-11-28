using System;
using System.Collections.Generic;
using System.Collections.ObjectModel; 
using System.Net; 
using System.Threading.Tasks; 

namespace Specto.Relay
{
    class UDPDevice : IVisualizationDevice
    {
        #region Static      
        public static List<UDPDevice> Devices { get; private set; } = new List<UDPDevice>();
        private static UDPMessenger BroadcastMessanger = new UDPMessenger(1234);
        private static ILogger Logger { get; set; }

        public static event EventHandler OnDevicesChanged;

        public static void DetectDevices(ILogger logger)
        {
            FreeDevices();
            Logger = logger;

            DataReceivedEventArgs replyArgs;
            Reply reply;

            BroadcastMessanger.SendCommand(new Command.GetInfo());
            replyArgs = BroadcastMessanger.Listen(50);
            reply = Reply.ParseFrom(replyArgs.Data);

            if (reply is Reply.DeviceInfo)
            {
                var deviceInfo = (Reply.DeviceInfo)reply;
                var device = new UDPDevice(replyArgs.RemoteEndPoint.Address.ToString(), deviceInfo.SerialNumber.ToString());

                device.DedicatedMessanger.SendCommand(new Command.FetchNetworks());
                device.DedicatedMessanger.OnDataReceived += (s, e) =>
                {
                    var async_reply = Reply.ParseFrom(e.Data);
                    if (async_reply is Reply.NetworkData)
                    {
                        var data = (Reply.NetworkData)async_reply;
                        var wifiData = new Network.WiFiData(data.SSID, data.IsProtected);

                        App.Current.Dispatcher.Invoke((Action)delegate 
                        {
                            device.AvailableNetworks.Add(wifiData);
                        });
                    }
                };
                device.DedicatedMessanger.ListenAsync(2000);
                Devices.Add(device);
            }

        }
        public static void FreeDevices()
        {
            foreach (var d in Devices)
                d.Dispose();

            Devices.Clear();
        }
        #endregion

        private IPAddress Address { get; set; }
        private UDPMessenger DedicatedMessanger { get; set; }

        public string Name => "UDP";
        public bool IsActive { get; set; }
        public string SerialNumber { get; private set; }

        public ObservableCollection<Network.WiFiData> AvailableNetworks { get; private set; }
        public bool IsSerial { get { return false; } }
        public bool IsWiFi { get { return true; } }


        private UDPDevice(string ipAddress, string serialNumber)
        {
            Address = IPAddress.Parse(ipAddress);
            SerialNumber = serialNumber;
            DedicatedMessanger = new UDPMessenger(1234, Address);
            AvailableNetworks = new ObservableCollection<Network.WiFiData>();
            IsActive = true;
        }

        public void Dispose()
        {
            DedicatedMessanger.Dispose();
        }

        public void Send(VisualizationData data)
        {
            if (!IsActive)
                return;

            string color = data.Color.R.ToString("X2") + data.Color.G.ToString("X2") + data.Color.B.ToString("X2");
            DedicatedMessanger.SendCommand(new Command.SetColor(color, false));
        }
        
        public async Task ConnectWithNetwork(string ssid, string key)
        {
            bool connected = await Task.Run(() => SetNetwork(ssid, key));
            Logger.Log(connected ? $"Device {SerialNumber}: connected with {ssid} network." :
                                   $"Device {SerialNumber}: unable to connect with {ssid}");
            await Task.Delay(500);
            Logger.Log("");
            OnDevicesChanged?.Invoke(this, EventArgs.Empty);
        }

        private bool SetNetwork(string ssid, string key)
        {
            DedicatedMessanger.SendCommand(new Command.SetWiFi(ssid, key));
            var reply = Reply.ParseFrom(DedicatedMessanger.Listen(500).Data);
            var fb = reply as Reply.SetWiFiFeedback;
            return fb?.Connected == true;
        }
    }
}
