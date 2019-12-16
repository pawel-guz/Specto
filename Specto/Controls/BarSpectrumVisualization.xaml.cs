using System;
using System.Collections.Generic; 
using System.Windows;
using System.Windows.Controls; 
using System.Windows.Input;
using System.Windows.Media; 

namespace Specto
{
    public partial class BarSpectrumVizualization : UserControl, IVisualizationDataReceiver
    {
        #region MainColorProperty
        public static readonly DependencyProperty MainColorProperty =
        DependencyProperty.Register("MainColor", 
            typeof(Color), 
            typeof(BarSpectrumVizualization), 
            new FrameworkPropertyMetadata(default(Color), 
                FrameworkPropertyMetadataOptions.AffectsRender, MainColorPropertyChanged));

        private static void MainColorPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (source is BarSpectrumVizualization)
                ((BarSpectrumVizualization)source).Rearange();
        }

        public Color MainColor
        {
            get { return (Color)this.GetValue(MainColorProperty); }
            set { this.SetValue(MainColorProperty, value); }
        }
        #endregion
        #region BarsNumberProperty
        public static readonly DependencyProperty BarsNumberProperty =
        DependencyProperty.Register("BarsNumber", 
            typeof(int), 
            typeof(BarSpectrumVizualization),
            new FrameworkPropertyMetadata(8, FrameworkPropertyMetadataOptions.AffectsRender, BarsNumberPropertyChanged));

        private static void BarsNumberPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (source is BarSpectrumVizualization)
            {
                ((BarSpectrumVizualization)source).Rearange();
            }
        }

        public int BarsNumber
        {
            get { return (int)this.GetValue(BarsNumberProperty); }
            set { this.SetValue(BarsNumberProperty, value); }
        }
        #endregion
        #region MaxBarsProperty
        public static readonly DependencyProperty MaxBarsProperty =
        DependencyProperty.Register("MaxBars",
            typeof(int),
            typeof(BarSpectrumVizualization),
            new FrameworkPropertyMetadata(16, FrameworkPropertyMetadataOptions.AffectsRender, MaxBarsPropertyChanged));

        private static void MaxBarsPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (source is BarSpectrumVizualization)
                ((BarSpectrumVizualization)source).Rearange();
        }

        public int MaxBars
        {
            get { return (int)this.GetValue(MaxBarsProperty); }
            set { this.SetValue(MaxBarsProperty, value); }
        }
        #endregion
        #region MinBarsProperty
        public static readonly DependencyProperty MinBarsProperty =
        DependencyProperty.Register("MinBars",
            typeof(int),
            typeof(BarSpectrumVizualization),
            new FrameworkPropertyMetadata(16, FrameworkPropertyMetadataOptions.AffectsRender, MinBarsPropertyChanged));

        private static void MinBarsPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (source is BarSpectrumVizualization)
                ((BarSpectrumVizualization)source).Rearange();
        }

        public int MinBars
        {
            get { return (int)this.GetValue(MinBarsProperty); }
            set { this.SetValue(MinBarsProperty, value); }
        }
        #endregion
        #region BarMargin
        public static DependencyProperty BarMarginProperty = DependencyProperty.Register("BarMargin",
            typeof(Thickness),
            typeof(BarSpectrumVizualization),
            new FrameworkPropertyMetadata(default(Thickness), FrameworkPropertyMetadataOptions.AffectsArrange, BarMarginPropertyChanged));

        private static void BarMarginPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (source is BarSpectrumVizualization)
                ((BarSpectrumVizualization)source).Rearange();
        }

        public Thickness BarMargin
        {
            get { return (Thickness)this.GetValue(BarMarginProperty); }
            set { this.SetValue(BarMarginProperty, value); }
        }
        #endregion

        List<ProgressBar> bars = new List<ProgressBar>();

        public BarSpectrumVizualization()
        {
            InitializeComponent();
            Rearange();
        }
        
        void Rearange()
        {
            Grid.Children.Clear();
            Grid.ColumnDefinitions.Clear();
            bars.Clear();

            Brush colorBrush = new SolidColorBrush(MainColor);
            Brush bgBrush = new SolidColorBrush(Colors.Transparent);
            
            for (int i = 0; i < BarsNumber; i++)
            {
                Grid.ColumnDefinitions.Add(new ColumnDefinition());

                var pb = new ProgressBar()
                {
                    Orientation = Orientation.Vertical,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Margin = BarMargin,

                    Minimum = 0,
                    Maximum = 255,
                    Value = 1,

                    Foreground = colorBrush,
                    Background = bgBrush,
                    BorderThickness = new Thickness(0),
                };

                bars.Add(pb);
                Grid.Children.Add(pb);
                Grid.SetColumn(pb, Grid.ColumnDefinitions.Count - 1);
            }
        }

        public void Send(VisualizationData data, bool force = false)
        {
            var brush = new SolidColorBrush(data.Color);

            float step = data.Spectrum.Count / bars.Count;
            for (int i = 0; i < bars.Count; i++)
            {
                int si = (int)Math.Round((float)i / bars.Count * data.Spectrum.Count);
                if (si >= data.Spectrum.Count)
                    continue;
            
                var d = data.Spectrum[si];
                bars[i].Value = d > 2 ? d : 2;
                bars[i].Foreground = brush;
            }
        }


        #region Events
        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0 && BarsNumber < MaxBars)
                BarsNumber += 1;
            else if (e.Delta < 0 && BarsNumber > MinBars && BarsNumber > 0)
                BarsNumber -= 1;
        }
        #endregion
    }
}
