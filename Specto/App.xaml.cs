using System;
using System.Windows;

namespace Specto
{
    public partial class App : Application
    { 
        Uri darkThemeUri = new Uri("ResourceDictionaries/Themes/DarkTheme.xaml", UriKind.Relative);
        Uri brightThemeUri = new Uri("ResourceDictionaries/Themes/BrightTheme.xaml", UriKind.Relative);

        public ResourceDictionary ThemeDictionary => Resources.MergedDictionaries[0]; 

        public Theme SwitchTheme()
        {
            Theme current;

            var source = ThemeDictionary.MergedDictionaries[0].Source;
            if (source == darkThemeUri)
            {
                source = brightThemeUri;
                current = Theme.Bright;
            }
            else
            { 
                source = darkThemeUri;
                current = Theme.Dark;
            } 

            ThemeDictionary.MergedDictionaries.Clear();
            ThemeDictionary.MergedDictionaries.Add(new ResourceDictionary() { Source = source });

            return current;
        } 
    }
} 