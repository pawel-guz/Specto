using System.Windows;
using System.Windows.Controls;

namespace Specto
{ 
    public partial class SettingSliderWithIcon : UserControl
    {
        #region IconSizeProperty
        public int IconSize
        {
            get { return (int)GetValue(IconSizeProperty); }
            set { SetValue(IconSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IconSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconSizeProperty =
            DependencyProperty.Register("IconSize", typeof(int), typeof(SettingSliderWithIcon));
        #endregion
        #region IconProperty
        public Image Icon
        {
            get { return (Image)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Icon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(Image), typeof(SettingSliderWithIcon));
        #endregion
        #region SliderPositionProperty
        public Dock SliderPosition
        {
            get { return (Dock)GetValue(SliderPositionProperty); }
            set { SetValue(SliderPositionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SliderPosition.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SliderPositionProperty =
            DependencyProperty.Register("SliderPosition", typeof(Dock), typeof(SettingSliderWithIcon), 
                new FrameworkPropertyMetadata(default(Dock), FrameworkPropertyMetadataOptions.AffectsRender));

        //private static readonly void SliderPositionPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        //{
        //    if (source is SettingSliderWithIcon)
        //        ((SettingSliderWithIcon)source)
        //}
        #endregion
        #region IconOpacityProperty
        public double IconOpacity
        {
            get { return (double)GetValue(IconOpacityProperty); }
            set { SetValue(IconOpacityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IconOpacity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconOpacityProperty =
            DependencyProperty.Register("IconOpacity", typeof(double), typeof(SettingSliderWithIcon), new PropertyMetadata(1.0));
        #endregion
        #region ValueProperty
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(SettingSliderWithIcon), new PropertyMetadata(0.0));
        #endregion
        #region MinMinValueProperty
        public double MinValue
        {
            get { return (double)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, MinValue); }
        }

        // Using a DependencyProperty as the backing store for MinValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register("MinValue", typeof(double), typeof(SettingSliderWithIcon), new PropertyMetadata(0.0, MinMaxValuePropertyChanged));
        #endregion
        #region MaxValueProperty
        public double MaxValue
        {
            get { return (double)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, MaxValue); }
        }

        // Using a DependencyProperty as the backing store for MaxValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register("MaxValue", typeof(double), typeof(SettingSliderWithIcon), new PropertyMetadata(0.0, MinMaxValuePropertyChanged));
        #endregion
        #region ShowProperty
        public bool Show
        {
            get { return (bool)GetValue(ShowProperty); }
            set { SetValue(ShowProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Show.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowProperty =
            DependencyProperty.Register("Show", typeof(bool), typeof(SettingSliderWithIcon), new PropertyMetadata(true));
        #endregion
        #region SliderOrientation
        public Orientation SliderOrientation
        {
            get { return (Orientation)GetValue(SliderOrientationProperty); }
            set { SetValue(SliderOrientationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SliderOrientation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SliderOrientationProperty =
            DependencyProperty.Register("SliderOrientation", typeof(Orientation), typeof(SettingSliderWithIcon), new PropertyMetadata(Orientation.Horizontal));
        #endregion

        private static void MinMaxValuePropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (source is SettingSliderWithIcon)
                ((SettingSliderWithIcon)source).Set();
        }

        public SettingSliderWithIcon()
        {
            InitializeComponent();
        }

        void Set()
        {
            var sc = (MaxValue - MinValue) / 25;
            var lc = (MaxValue - MinValue) / 10;

            Slider.SmallChange = sc > 0 ? sc : 0.1; 
            Slider.LargeChange = lc > 0 ? lc : 1 ;
            Slider.TickFrequency = lc;
        }
    }
}
