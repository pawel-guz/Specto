using System;
using System.Collections.Generic; 
using System.Drawing; 

namespace Specto.ColorVizualization
{
    class Basic : IColorMixer
    { 
        private Color lastColor = Color.Black;
        private List<byte> lastSpectrum = new List<byte>();

        public Color GetColor(Settings settings, List<byte> spectrum)
        {
            if (spectrum == null || spectrum.Count == 0)
                return Color.Black;

            if(lastSpectrum.Count != spectrum.Count)
            {
                lastSpectrum.Clear();
                lastSpectrum.AddRange(new byte[spectrum.Count]);
            }

            byte min = 255, max = 0;
            byte peakR = 0, peakG = 0, peakB = 0;
            float resolution = settings.SamplingResolution;
            float redRange = resolution * 0.25f;
            float blueRange = resolution * 0.5f;
            float greenRange = resolution * 0.75f;

            int bassRange = (int)(settings.BassRange * spectrum.Count);
            int totalBass = 0, maxBassDifference = 0, maxBassDifferenceIndex = 0;
            int indexShift = (int)(settings.HueShift * spectrum.Count);

            for (int i = 0; i < spectrum.Count; i++)
            {
                // Get shifted index.
                int si = i + indexShift;
                if (si >= spectrum.Count)
                    si -= spectrum.Count;

                byte source = spectrum[i];
                min = Math.Min(source, min);
                max = Math.Max(source, max);
                
                if (i < bassRange)
                {
                    totalBass += source; 
                    if (lastSpectrum.Count == spectrum.Count)
                    {
                        int bassDifference = source - lastSpectrum[i];
                        if (Math.Abs(bassDifference) > maxBassDifference)
                        {
                            maxBassDifference = bassDifference;
                            maxBassDifferenceIndex = i;
                        }
                    }
                }
                
                if (si < redRange)
                    peakR = (source > peakR) ? (byte)(0.5 * source) : peakR;
                else if (si < blueRange)
                    peakB = (source > peakB) ? source : peakB;
                else if (si < greenRange)
                    peakG = (source > peakG) ? source : peakG;
                else
                    peakR = (source > peakR) ? source : peakR;

                lastSpectrum[i] = spectrum[i];
            }


            if (settings.AdaptiveThreshold)
                settings.AmplitudeThreshold = 0.6 * ((double)max / 255);
                 
            double bass_avg = (double)totalBass / (bassRange * byte.MaxValue);
            double bass = bass_avg * 2;
              
            // Calculate RGB values based on their peaks.
            var g = (byte)((peakG * settings.ColorVariability) + lastColor.G * (1 - settings.ColorVariability));
            var b = (byte)((peakB * settings.ColorVariability) + lastColor.B * (1 - settings.ColorVariability));
            var r = (byte)((peakR * settings.ColorVariability) + lastColor.R * (1 - settings.ColorVariability));
            Tools.ColorManipulation.ColorToHSV(Color.FromArgb(r, g, b), out double h, out double s, out double v);

            // Modify brightness.
            // If bass pulsation is enabled, it will influence final brightness.
            v = (settings.BassPulsationMode) ? bass : v; 
            v *= (1f + settings.BrightnessModifier);
            v = (v < 0) ? 0 : ((v > 1) ? 1 : v);
            
            // Modify saturation.
            s *= (1f + settings.SaturationModifier);
            s = (settings.SaturationModifier <= -1.0f) ? 0 : s; 
            s = (s < 0) ? 0 : ((s > 1) ? 1 : s);

            var color = Tools.ColorManipulation.ColorFromHSV(h, s, v); 
            lastColor = color;
            return color;
        }
    }

}
