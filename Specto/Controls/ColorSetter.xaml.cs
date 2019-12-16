using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Specto.Relay;
using Specto.Tools;

namespace Specto
{
    public partial class ColorSetter : UserControl
    {
        public ColorSetter()
        {
            InitializeComponent();
           
        }

        private void ColorChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => UpdateColor();
        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {  
 
        }

        private void UpdateColor(bool force = true)
        {
            var device = (Device)this.DataContext;
            if (device != null)
            {
                var color = ColorManipulation.ColorFromHSV(Hue.Value, Saturation.Value, Value.Value).ToWMColor();
                ColorPreview.Fill = new SolidColorBrush(color);
                device.Send(new VisualizationData(null, color), force);
            }
        }

    }
}
