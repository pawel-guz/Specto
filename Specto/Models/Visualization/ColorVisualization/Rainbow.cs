using System.Collections.Generic;
using System.Drawing; 

namespace Specto.ColorVizualization
{
    class Rainbow : IColorMixer
    {
        private Color lastColor = Color.Black;
        private List<byte> lastSpectrum = new List<byte>();

        public Color GetColor(Settings settings, List<byte> spectrum)
        {
            if (lastSpectrum.Count != spectrum.Count)
            {
                lastSpectrum.Clear();
                lastSpectrum.AddRange(new byte[spectrum.Count]);
            }

            int r = 0, g = 0, b = 0;
            float range = 0.7f;
            float rangeShift = 0.2f;

            for (int i = 0; i < spectrum.Count; i++)
            {
                float pos = i * settings.SamplingResolution;
                var val = spectrum[i];

                if (pos > rangeShift && pos < rangeShift + 0.25 * range)
                    r += val;
                else if (pos < rangeShift + 0.5 * range)
                    g += val;
                else if (pos < rangeShift + 0.75 * range)
                    b += val;

                lastSpectrum[i] = spectrum[i];
            }

            double total = r + g + b;
            total = (total == 0) ? 1 : total; 
             
            Tools.ColorManipulation.ColorToHSV(Color.FromArgb(
                (byte)(r / total * 255), 
                (byte)(g / total * 255), 
                (byte)(b / total * 255)), 
                out double h, out double s, out double v);

            // Modify saturation.
            s += settings.SaturationModifier;
            s = (s < 0) ? 0 : ((s > 1) ? 1 : s);
            var color = Tools.ColorManipulation.ColorFromHSV(h, s, v);

            lastColor = color;
            return color;
        }
    }
}
