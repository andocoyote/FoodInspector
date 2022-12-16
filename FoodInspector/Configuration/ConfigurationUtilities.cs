using System.Collections.Specialized;
using System.Configuration;

namespace FoodInspector.Configuration
{
    public class ConfigurationUtilities
    {
        /// <summary>
        /// Returns the NameValueCollection of app settings for the app service
        /// </summary>
        /// <returns>NameValueCollection: app settings for the app service or null</returns>
        public static NameValueCollection GetAllAppSettings()
        {
            NameValueCollection appSettings = null;

            try
            {
                appSettings = ConfigurationManager.AppSettings;
            }
            catch (ConfigurationErrorsException)
            {
                appSettings = null;
            }

            return appSettings;
        }

        /// <summary>
        /// Returns the value of app setting for the app service
        /// </summary>
        /// <returns>string: the value of the app setting or null</returns>
        public static string GetAppSetting(string key)
        {
            string appSetting = null;
            try
            {
                appSetting = ConfigurationManager.AppSettings[key];
            }
            catch (ConfigurationErrorsException)
            {
                appSetting = null;
            }

            return appSetting;
        }

        /// <summary>
        /// Returns the value of connection string for the app service
        /// </summary>
        /// <returns>string: the value of the connection string or null</returns>
        public static string GetConnectionString(string key)
        {
            string connectionString = null;
            try
            {
                connectionString = ConfigurationManager.ConnectionStrings[key]?.ConnectionString;
            }
            catch (ConfigurationErrorsException)
            {
                connectionString = null;
            }

            return connectionString;
        }

        public static void AddUpdateAppSettings(string key, string value)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;

                if (settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;
                }

                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error writing app settings");
            }
        }

        public static void AddUpdateConnectionStrings(string key, string value, string provider)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var connectionStrings = configFile.ConnectionStrings.ConnectionStrings;
                ConnectionStringSettings connectionStringsSettings = new ConnectionStringSettings();
                connectionStringsSettings.Name = key;
                connectionStringsSettings.ConnectionString = value;
                connectionStringsSettings.ProviderName= provider;

                if (connectionStrings[key] == null)
                {
                    connectionStrings.Add(connectionStringsSettings);
                }
                else
                {
                    connectionStrings[key].ConnectionString = value;
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.ConnectionStrings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error writing connection string settings");
            }
        }
    }
}
