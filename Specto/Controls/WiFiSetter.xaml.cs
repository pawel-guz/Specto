using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Specto.Relay;

namespace Specto
{ 
    public partial class WiFiSetter : UserControl
    {
        public WiFiSetter()
        {
            InitializeComponent();
        }

        private void Set_Click(object sender, RoutedEventArgs e)
        {
            var device = (UDPDevice)this.DataContext;
            if (device != null)
            {
                // Get data from SSID combobox.
                var wifi = (Network.WiFiData)SSID.SelectedItem;
                string pass = Pass.Password;
                if (wifi != null) 
                    device.ConnectWithNetwork(wifi.SSID, Pass.Password); 
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        { 
            var device = (UDPDevice)this.DataContext;
            device.FetchNetworks();
        }
    }
}
