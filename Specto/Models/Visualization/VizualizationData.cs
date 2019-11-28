using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Specto
{
    public class VisualizationData
    {
        public List<byte> Spectrum { get; set; }
        public Color Color { get; set; }

        public VisualizationData(List<byte> spectrum, Color color)
        {
            Spectrum = spectrum;
            Color = color;
        }
    }
}
