using Specto.Relay; 
using System.Windows;
using System.Windows.Controls;  

namespace Specto
{
    /// <summary>
    /// Logika interakcji dla klasy NameSetter.xaml
    /// </summary>
    public partial class NameSetter : UserControl
    {
        public NameSetter()
        {
            InitializeComponent();
        }

        private void SendName_Click(object sender, RoutedEventArgs e)
        {
            var device = (UDPDevice)this.DataContext;
            if(device != null)
            {
                if (Name.Text.Length > 0)
                    device.SetName(Name.Text); 
            }
        }

    }
}
