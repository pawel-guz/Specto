using System; 
using System.Windows;
using System.Windows.Media;
using Specto.ColorVizualization;
using Specto.Relay;
using Specto.Audio;
using Specto.Tools; 
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace Specto
{
    public partial class MainWindow : Window
    {
        bool FullScreen { get; set; }
        Settings Settings { get; set; }
        AsyncStatusLogger Logger { get; set; }
        ObservableCollection<IVisualizationDevice> ObservableVisualDevices { get; set; }
        IVisualizationDataReceiver VisualizationDrawer { get; set; }
        IColorMixer ColorMixer { get; set; } 
        SpectrumProcessorBase SpectrumProcessorBase { get; set; }

        DispatcherTimer dispatcherTimer;
        int timerTicks;

        public MainWindow()
        {
            Logger = new AsyncStatusLogger(1f);
            InitializeComponent();
            FullScreen = false; 

            // Get configuration file if possible.
            Settings = ConfigManager.Load();
            Settings.ShowSettings = false;
            UpdateSettingsButtons();

            // Bind UI elements.
            DataContext = Settings;
            StatusInfo.DataContext = Logger;

            SpectrumProcessorBase = new SpectrumProcessorBase(Logger);
            ColorMixer = new ColorVizualization.Basic(); 
            VisualizationDrawer = Drawer;
            ObservableVisualDevices = new ObservableCollection<IVisualizationDevice>();
            UDPDevice.OnDevicesChanged += (s, e) => { UpdateDevices(); };
            UpdateDevices();

            // Start timer.
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += UpdateSpectrum;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 1000 / Settings.RefreshRate);
            dispatcherTimer.Start();
        }

        private void UpdateDevices()
        {
            try
            { 
                SerialDevice.FreeDevices();
                UDPDevice.FreeDevices();

                ObservableVisualDevices.Clear();

                SerialDevice.DetectDevices();
                UDPDevice.DetectDevices(Logger);

                foreach (var device in SerialDevice.Devices)
                    ObservableVisualDevices.Add(device);

                foreach (var device in UDPDevice.Devices)
                    ObservableVisualDevices.Add(device);

                Devices.ItemsSource = ObservableVisualDevices;
            }
            catch 
            { 
                Logger?.Log("Unable to detect devices."); 
            }
        }

        private void UpdateSpectrum(object sender, EventArgs e)
        { 
            if (Settings.SamplingRate > 0)
            {
                if (++timerTicks * dispatcherTimer.Interval.Milliseconds >= 1000 / Settings.SamplingRate)
                {
                    SpectrumProcessorBase.Sample(Settings);
                    timerTicks = 0;
                }
            }

            var spectrum = SpectrumProcessorBase.GetSmoothSpectrum(Settings);
            var color = ColorMixer.GetColor(Settings, spectrum).ToWMColor();
            var data = new VisualizationData(spectrum, color);

            // Update devices.
            foreach (var device in ObservableVisualDevices)
                device.Send(data);

            // Update UI.
            VisualizationDrawer.Send(data);
            Background = new SolidColorBrush(color);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            SpectrumProcessorBase.Free();
            SerialDevice.FreeDevices();
            UDPDevice.FreeDevices();
        }

        #region UI events
        private void ChangeThemeButton_Click(object sender, RoutedEventArgs e)
        {
            var app = (App)Application.Current;
            Settings.Theme = app.SwitchTheme();
            FullScreenButton.Content = FindResource(FullScreen ? "NormalScreenIcon" : "FullScreenIcon");
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var state = Settings.ShowSettings;
            if (state == false)
            {
                MainPanel.Visibility = Visibility.Visible;
                DeviceManager.Visibility = Visibility.Collapsed;
                SettingsMargin.Height = new GridLength(0);
            }
            else SettingsMargin.Height = new GridLength(25);
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateDevices();
        }

        private void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.Reset();
            Logger?.Log("Default settings restored.");
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This program is using:\n" +
                " ◦ icons copyrighted by: https://icons8.com \n" +
                " ◦ CSCore - .NET Audio Library under MS-PL license.", "About");
        }

        private void DeviceManagerButton_Click(object sender, RoutedEventArgs e)
        {
            var v = MainPanel.Visibility;
            MainPanel.Visibility = DeviceManager.Visibility;
            DeviceManager.Visibility = v;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string s = ConfigManager.Save(Settings) ? "Settings saved." : "Save failed.";
            Logger?.Log(s);
        }

        private void FullScreenButton_Click(object sender, RoutedEventArgs e)
        {
            FullScreen = !FullScreen;
            if(FullScreen)
            { 
                WindowStyle = WindowStyle.None;
                if (WindowState == WindowState.Maximized)
                    WindowState = WindowState.Normal;

                WindowState = WindowState.Maximized;  
            }
            else
            { 
                WindowState = WindowState.Normal;
                WindowStyle = WindowStyle.SingleBorderWindow; 
            }

            FullScreenButton.Content = FindResource(FullScreen ? "NormalScreenIcon" : "FullScreenIcon");
        }

        private void PulsationButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.BassPulsationMode = !Settings.BassPulsationMode;
            UpdateSettingsButtons();
        }

        private void AutoThreshold_Click(object sender, RoutedEventArgs e)
        {
            Settings.AdaptiveThreshold = !Settings.AdaptiveThreshold;
            UpdateSettingsButtons();
        }
        #endregion

        void UpdateSettingsButtons()
        {
            switch (Settings.BassPulsationMode)
            {
                case false: Pulsation.Opacity = 0.25; break;
                case true:  Pulsation.Opacity = 0.45; break;
            }
            switch (Settings.AdaptiveThreshold)
            {
                case true: AutoThreshold.Opacity = 0.45; break;
                default: AutoThreshold.Opacity = 0.25; break;
            }
        }
    }
}
