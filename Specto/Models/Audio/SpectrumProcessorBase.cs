using System;
using System.Collections.Generic; 
using System.Threading.Tasks; 
using CSCore;
using CSCore.SoundIn;
using CSCore.Codecs.WAV;
using CSCore.DSP;
using CSCore.Streams;

namespace Specto.Audio
{
    class SpectrumProcessorBase
    { 
        public int numBars = 30;
        public int minFreq = 5;
        public int maxFreq = 4500;
        public int barSpacing = 0;
        public bool logScale = true;
        public bool isAverage = false;
        public float highScaleAverage = 2.0f;
        public float highScaleNotAverage = 3.0f;

        private FftProvider fftProvider;
        private WasapiCapture capture;
        private SingleBlockNotificationStream notificationSource;
        private IWaveSource finalSource;
        private FftSize fftSize;
        private float[] fftBuffer; 

        private List<byte> _processedSpectrum = new List<byte>();
        private List<byte> rawSpectrum = new List<byte>();
        private bool initialized;
        private ILogger logger;

        public SpectrumProcessorBase(ILogger logger)
        {
            this.logger = logger;
            InitAsync();
        }
         
        private async Task InitAsync()
        { 
            logger?.Log("Starting WASAPI capture."); 
            await Task.Run(StartCapture); 
            logger?.Log("Audio capture started."); 
        }

        void StartCapture()
        {
            capture = new WasapiLoopbackCapture();
            capture.Initialize();
            IWaveSource source = new SoundInSource(capture);

            fftSize = FftSize.Fft2048;
            fftBuffer = new float[2048];
            fftBuffer = new float[(int)fftSize];
            fftProvider = new FftProvider(1, fftSize);

            notificationSource = new SingleBlockNotificationStream(source.ToSampleSource());
            notificationSource.SingleBlockRead += SingleBlockRead;
            finalSource = notificationSource.ToWaveSource();

            capture.DataAvailable += CaptureDataAvailable;
            capture.Start();

            initialized = true;
        }

        private void CaptureDataAvailable(object sender, DataAvailableEventArgs e) 
            => finalSource.Read(e.Data, e.Offset, e.ByteCount);

        private void SingleBlockRead(object sender, SingleBlockReadEventArgs e) 
            => fftProvider.Add(e.Left, e.Right);

        public void Free()
        {
            if (initialized)
            {
                capture.Stop();
                capture.Dispose();
            }
        }

        /// <summary>
        /// Sampling audio device and updating processor internal data. Returns 'at the momement' spectrum data.
        /// </summary> 
        public List<byte> Sample(Settings settings)
        {
            if (!initialized)
                return null; 

            fftProvider.GetFftData(fftBuffer);
            rawSpectrum.Clear(); 

            // Get frequencies distribution from FFT data.
            int b0 = 0;
            for (int x = 0; x < settings.SamplingResolution; x++)
            {
                float peak = 0; 
                int b1 = (int)Math.Pow(2, (x * 10.0 / (settings.SamplingResolution)));
                if (b1 > 1023) b1 = 1023;
                if (b1 <= b0) b1 = b0 + 1;
                for (; b0 < b1; b0++)
                    if (peak < fftBuffer[1 + b0]) peak = fftBuffer[1 + b0];

                int y = (int)(Math.Sqrt(peak) * 3 * 255);
                if (y > 255) y = 255;
                if (y < 0) y = 0;

                rawSpectrum.Add((byte)y);
            }
            return rawSpectrum;
        }

        /// <summary>
        /// Instructs processor to smooth stored spectrum data. Returns 'smooth' spectrum data.
        /// </summary> 
        public List<byte> GetSmoothSpectrum(Settings settings)
        {
            byte a_co = (byte)(byte.MaxValue * settings.AmplitudeCutoff);
            byte a_ts = (byte)(byte.MaxValue * settings.AmplitudeThreshold);
            double sm = settings.SpectrumSmoothing;

            for (int x = 0; x < settings.SamplingResolution && x < rawSpectrum.Count; x++)
            {
                if (rawSpectrum[x] > a_co)
                    rawSpectrum[x] = a_co;

                if (_processedSpectrum.Count <= x)
                    _processedSpectrum.Add(rawSpectrum[x]);

                if (rawSpectrum[x] > a_ts)
                    _processedSpectrum[x] = (byte)(_processedSpectrum[x] * sm + (1 - sm) * rawSpectrum[x]);
                else _processedSpectrum[x] = (byte)(_processedSpectrum[x] * sm);
            }

            return _processedSpectrum;
        }

    }
}
