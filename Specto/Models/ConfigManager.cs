using Newtonsoft.Json; 
using System.IO; 

namespace Specto
{
    public static class ConfigManager
    {
        public static bool Save(Settings settings)
        {
            try
            {
                string output = JsonConvert.SerializeObject(settings);
                File.WriteAllText("config", output);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static  Settings Load()
        {
            Settings settings = new Settings();
            try
            {
                string input = File.ReadAllText("config");
                settings = JsonConvert.DeserializeObject<Settings>(input);
            }
            catch
            {
                settings = new Settings();
            }

            return settings;
        }
    }
}
