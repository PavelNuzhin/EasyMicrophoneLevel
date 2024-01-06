using System.Configuration;

internal class AppConfiguration
{
    internal static bool Contains(string name)
    {
        return ConfigurationManager.AppSettings[name] is not null;
    }

    internal static string Get(string name)
    {
        return ConfigurationManager.AppSettings[name];
    }

    internal static void Set(string name, string value)
    {
        var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        var settings = configFile.AppSettings.Settings;
        if (settings[name] == null)
        {
            settings.Add(name, value);
        }
        else
        {
            settings[name].Value = value;
        }
        configFile.Save(ConfigurationSaveMode.Modified);
        ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
    }
}