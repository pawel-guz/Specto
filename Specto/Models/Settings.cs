using System; 
using System.ComponentModel; 
using System.Runtime.CompilerServices; 

namespace Specto
{ 
    public enum Theme { Bright, Dark }
    public class Settings : INotifyPropertyChanged
    {
        // Nonbindable properties.
        public int RefreshRate { get; private set; }
        public int SamplingResolution { get; private set; }
        public double BassRange { get; private set; }
        public Theme Theme { get; set; }

        #region Bindable properties
        private double themeOpacity;
        public double ThemeOpacity
        {
            get { return this.themeOpacity; }
            set
            {
                this.themeOpacity = value;
                NotifyPropertyChanged();
            }
        }

        private bool showSettings;
        public bool ShowSettings
        {
            get { return this.showSettings; }
            set
            {
                this.showSettings = value;
                NotifyPropertyChanged();
            }
        }

        public int barsNumber;
        public int BarsNumber
        {
            get { return this.barsNumber; }
            set
            {
                this.barsNumber = value;
                NotifyPropertyChanged();
            }
        }

        private double samplingRate;
        public double SamplingRate
        {
            get { return this.samplingRate; }
            set
            {
                this.samplingRate = Math.Round(value, 1);
                NotifyPropertyChanged();
            }
        }

        public double spectrumSmoothing;
        public double SpectrumSmoothing
        {
            get { return this.spectrumSmoothing; }
            set
            {
                this.spectrumSmoothing = value;
                NotifyPropertyChanged();
            }
        }

        private double amplitudeThreshold;
        public double AmplitudeThreshold
        {
            get { return this.amplitudeThreshold; }
            set
            {
                this.amplitudeThreshold = value;
                NotifyPropertyChanged();
            }
        }

        private bool adaptiveThreshold;
        public bool AdaptiveThreshold
        {
            get { return this.adaptiveThreshold; }
            set
            {
                this.adaptiveThreshold = value;
                if (AdaptiveThreshold == false)
                    AmplitudeThreshold = 0f;

                NotifyPropertyChanged();
            }
        }
        private double amplitudeCutoff;
        public double AmplitudeCutoff
        {
            get { return this.amplitudeCutoff; }
            set
            {
                this.amplitudeCutoff = value;
                NotifyPropertyChanged();
            }
        }

        private bool bassPulsationMode;
        public bool BassPulsationMode
        {
            get { return this.bassPulsationMode; }
            set
            {
                this.bassPulsationMode = value;
                NotifyPropertyChanged();
            }
        }

        private double bassInfluence;
        public double BassInfluence
        {
            get { return this.bassInfluence; }
            set
            {
                this.bassInfluence = value;
                NotifyPropertyChanged();
            }
        }

        private double colorVariability;
        public double ColorVariability
        {
            get { return this.colorVariability; }
            set
            {
                this.colorVariability = value;
                NotifyPropertyChanged();
            }
        }

        private double brightnessModifier;
        public double BrightnessModifier
        {
            get { return this.brightnessModifier; }
            set
            {
                this.brightnessModifier = value;
                NotifyPropertyChanged();
            }
        }

        private double saturationModifier;
        public double SaturationModifier
        {
            get { return this.saturationModifier; }
            set
            {
                this.saturationModifier = value;
                NotifyPropertyChanged();
            }
        }

        private double hueShift;
        public double HueShift
        {
            get { return this.hueShift; }
            set
            {
                this.hueShift = value;
                NotifyPropertyChanged();
            }
        }
        #endregion

        public Settings()
        {
            Reset();
        }

        public void Reset()
        {
            RefreshRate = 25;
            Theme = Theme.Dark;
            ThemeOpacity = 0.85;

            SamplingResolution = 64;
            SamplingRate = 25.0;
            SpectrumSmoothing = 0.7;
            AmplitudeThreshold = 0.0;
            AdaptiveThreshold = true;
            AmplitudeCutoff = 1.0;

            BassRange = 0.2;
            BassPulsationMode = true;
            BassInfluence = 0.7;
            ColorVariability = 0.4;
            BrightnessModifier = 0.0;
            SaturationModifier = 0.25;
            HueShift = 0.0;

            BarsNumber = 20;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }
        #endregion
    }
}
