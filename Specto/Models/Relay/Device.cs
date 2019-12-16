using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.ComponentModel; 
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace Specto.Relay
{
    public abstract class Device : INotifyPropertyChanged, IDisposable
    {   
        public static List<Device> Devices { get; private set; } = new List<Device>();  
        /// <summary>
        /// This event is called on UI thread.
        /// </summary>
        public static event System.Action<List<Device>> DevicesChanged;
        public static bool DetectingDevices;
        public static async Task DetectDevicesAsync()
        {
            if (DetectingDevices)
                return;

            FreeDevices(); 
            DetectingDevices = true;
            var serialTask = SerialDevice.GetDevicesAsync();
            var udpTask = UDPDevice.GetDevicesAsync();
            DetectingDevices = false;
        }
         
        public static void FreeDevices()
        {
            for(int i = Devices.Count - 1; i >= 0; i--)
                Devices[i].Dispose();

            Devices.Clear();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool isActive;

        public virtual string Name { get; protected set; }
        public virtual bool IsSerial => false;
        public virtual bool IsWiFi => false; 
        public string SerialNumber { get; protected set; }
        public ConnectionType ConnectionType { get; protected set; } 
        public string ConnectionInfo { get; protected set; }

        public ICommand FetchNetworksCommand => new RelayCommand(() =>
        { 
            if(this is UDPDevice udp) 
                udp.FetchNetworks(); 
        });
        public ICommand SendColorRequest => new RelayCommand<bool>(OnSendColorRequest);
        private void OnSendColorRequest(bool value) => IsActive = !value;

        public virtual bool IsActive
        {
            get => isActive;
            set
            {
                isActive = value;
                if (!isActive)
                    SendColor(Colors.Black);
                RaisePropertyChanged("IsActive");
            }
        }

        public virtual void Dispose()
        {
            Devices.Remove(this);
            NotifyDevicesChanged();
        }

        public void Send(VisualizationData data, bool force = false)
        {
            if (!IsActive && !force)
                return;

            SendColor(data.Color); 
        }

        public abstract void SendColor(Color color);

        protected void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                var eventArgs = new PropertyChangedEventArgs(propertyName);
                handler(this, eventArgs);
            }
        }

        protected static void NotifyDevicesChanged()
        {
            App.Current.Dispatcher.Invoke(() => DevicesChanged?.Invoke(Devices));
        } 
    }
}
