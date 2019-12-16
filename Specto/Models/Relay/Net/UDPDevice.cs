using GalaSoft.MvvmLight.Command;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace Specto.Relay
{
    public class UDPDevice : Device
    {
        #region Static       
        private static UDPMessenger BroadcastMessanger = new UDPMessenger(1234);

        public static async Task GetDevicesAsync()
        { 
            BroadcastMessanger.SendCommand(new Command.GetInfo());
            DataReceivedEventHandler detector = (s, e) =>
            {
                Reply reply = Reply.ParseFrom(e.Data);

                if (reply is Reply.DeviceInfo)
                {
                    var deviceInfo = (Reply.DeviceInfo)reply;
                    string serialNumber = deviceInfo.SerialNumber.ToString();
                    string name = deviceInfo.Name;
                    name = (name.Length) > 0 ? name : serialNumber; 
                    bool inNetwork = (deviceInfo.NetworkName.Length > 0);

                    lock (Devices)
                    {
                        if (Devices.SingleOrDefault(d => d.Name == name) != null)
                            return;

                        var device = new UDPDevice(e.RemoteEndPoint.Address.ToString());
                        device.SerialNumber = serialNumber;
                        device.Name = name.Length > 0 ? name : serialNumber;
                        device.ConnectionInfo = inNetwork ? $"via {deviceInfo.NetworkName} network" : "via wifi direct";
                        device.ConnectionType = inNetwork ? ConnectionType.LAN : ConnectionType.DirectWiFI;
                        Devices.Add(device);
                        NotifyDevicesChanged();
                    }
                }
            };

            BroadcastMessanger.OnDataReceived += detector;
            await BroadcastMessanger.EventListenAsync(300);  
            BroadcastMessanger.OnDataReceived += detector;
            
        }
        #endregion

        float fetchNetworksProgress;
        bool isFetching;
         
        public override bool IsWiFi => true;

        private IPAddress Address { get; set; }
        private UDPMessenger DedicatedMessanger { get; set; }
        public ObservableCollection<Network.WiFiData> AvailableNetworks { get; private set; } 
         
        public float FetchNetworksProgress 
        {
            get => fetchNetworksProgress;
            set
            {
                fetchNetworksProgress = value;
                RaisePropertyChanged("FetchNetworksProgress");
            } 
        }

        public bool IsFetching
        {
            get => isFetching;
            set
            {
                isFetching = value;
                RaisePropertyChanged("IsFetching");
            } 
        } 

        private UDPDevice(string ipAddress)
        {
            Address = IPAddress.Parse(ipAddress);
            DedicatedMessanger = new UDPMessenger(1234, Address);
            AvailableNetworks = new ObservableCollection<Network.WiFiData>();
            IsActive = true; 
            DedicatedMessanger.OnDataReceived += (s, e) => ProcessDataReceived(s, e); 
        }

        public override void Dispose()
        {
            base.Dispose();
            DedicatedMessanger.Dispose();
        }

        public override void SendColor(Color color)
        { 
            string hexColor = color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
            DedicatedMessanger.SendCommand(new Command.SetColor(hexColor, false));
        }


        public void SetName(string name)
        {
            DedicatedMessanger.SendCommand(new Command.SetName(name));
            Dispose();
            Device.DetectDevicesAsync();
        }

        public void ConnectWithNetwork(string ssid, string key)
        {
            DedicatedMessanger.SendCommand(new Command.SetWiFi(ssid, key));
            Dispose();
        }

        public void FetchNetworks()
        {
            if (isFetching)
                return;

            AvailableNetworks.Clear();
            IsFetching = true;
            FetchNetworksProgress = 0;
            DedicatedMessanger.SendCommand(new Command.FetchNetworks()); 
            Task.Run(() => DedicatedMessanger.EventListenAsync(3000));

            BackgroundWorker listenWorker = new BackgroundWorker();
            listenWorker.DoWork += (s, e) =>
            {
                int t = 0;
                int interval = 40;
                int total = 3000;
                while (t < total)
                {
                    Thread.Sleep(interval);
                    listenWorker.ReportProgress((int)(100f * t / (float)total)); 
                    t += interval;
                } 
            };
            listenWorker.RunWorkerCompleted += (s, e) => IsFetching = false;
            listenWorker.ProgressChanged += (s, e) => FetchNetworksProgress = e.ProgressPercentage / 100f;
            listenWorker.WorkerReportsProgress = true;
            listenWorker.RunWorkerAsync();
        }

        private void ProcessDataReceived(object sender, DataReceivedEventArgs e)
        {  
            var async_reply = Reply.ParseFrom(e.Data);
            if (async_reply is Reply.NetworkData)
            {
                var data = (Reply.NetworkData)async_reply;
                var wifiData = new Network.WiFiData(data.SSID, data.IsProtected);
                App.Current.Dispatcher.Invoke(() => this.AvailableNetworks.Add(wifiData));
            }
        }
    }
}
