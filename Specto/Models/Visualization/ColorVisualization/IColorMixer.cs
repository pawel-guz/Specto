using System.Collections.Generic;
using System.Drawing; 

namespace Specto.ColorVizualization
{
    public interface IColorMixer
    {
        Color GetColor(Settings settings, List<byte> spectrum);
    }
}
