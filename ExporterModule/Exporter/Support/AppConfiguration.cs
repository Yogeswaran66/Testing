using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace exhelper
{
    public static class AppConfiguration
    {
        public static Boolean SetValueToAppConfig(String key, String value)
        {
            Configuration Config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            String s = ConfigurationManager.AppSettings[key];
            if (!String.IsNullOrEmpty(s))
            {
                Config.AppSettings.Settings[key].Value = value;
            }
            else
            {
                Config.AppSettings.Settings.Add(key,value);
            }

            Config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
            return true;
        }

        public static string GetValueFromAppConfig(String key, String Defval)
        {
            String value;

            Configuration Config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);           

            var appSettings = Config.AppSettings;

            if (ConfigurationManager.AppSettings[key] == null)
                return "";
            value = appSettings.Settings[key].Value;
            if (!String.IsNullOrEmpty(value))
            {
                return value;
            }
            else
            {
                return Defval;
            }
        }           
    }
}
