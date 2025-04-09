using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;

namespace Functions_for_Dynamics_Operations
{
    public class VStudioCache
    {
        public static Settings GetSettings(string model)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            SettingsManager settingsManager = new ShellSettingsManager(Microsoft.VisualStudio.Shell.ServiceProvider.GlobalProvider);
            WritableSettingsStore userSettingsStore = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);

            Settings settings = new Settings
            {
                Model = model
            };

            string userSettingsStoreName = GetCacheId() + settings.Model;

            settings.File = userSettingsStore.PropertyExists(userSettingsStoreName, "FILE") ? userSettingsStore.GetString(userSettingsStoreName, "FILE") : "";
            settings.Label = userSettingsStore.PropertyExists(userSettingsStoreName, "LABEL") ? userSettingsStore.GetString(userSettingsStoreName, "LABEL") : "";
            // settings.Prefix = userSettingsStore.PropertyExists(userSettingsStoreName, "PREFIX") ? userSettingsStore.GetString(userSettingsStoreName, "PREFIX") : "";
            settings.Default = userSettingsStore.PropertyExists(userSettingsStoreName, "DEFAULT") ? userSettingsStore.GetString(userSettingsStoreName, "DEFAULT") : "";
            settings.AutoProp = userSettingsStore.PropertyExists(userSettingsStoreName, "AUTOPROP") ? userSettingsStore.GetBoolean(userSettingsStoreName, "AUTOPROP") : true;
            settings.AutoTrans = userSettingsStore.PropertyExists(userSettingsStoreName, "AUTOTRANS") ? userSettingsStore.GetBoolean(userSettingsStoreName, "AUTOTRANS") : false;
            settings.NoTransLang = userSettingsStore.PropertyExists(userSettingsStoreName, "NOTRANSLANG") ? userSettingsStore.GetString(userSettingsStoreName, "NOTRANSLANG") : "";
            settings.NoDefaultDesc = userSettingsStore.PropertyExists(userSettingsStoreName, "NODEFAULTDESC") ? userSettingsStore.GetBoolean(userSettingsStoreName, "NODEFAULTDESC") : false;

            return settings;
        }

        public static void SaveSettings(Settings settings)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            SettingsManager settingsManager = new ShellSettingsManager(Microsoft.VisualStudio.Shell.ServiceProvider.GlobalProvider);
            WritableSettingsStore userSettingsStore = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);

            string userSettingsStoreName = GetCacheId() + settings.Model;

            if (!userSettingsStore.CollectionExists(userSettingsStoreName))
                userSettingsStore.CreateCollection(userSettingsStoreName);

            userSettingsStore.SetString(userSettingsStoreName, "FILE", settings.File);
            userSettingsStore.SetString(userSettingsStoreName, "LABEL", settings.Label);
            // userSettingsStore.SetString(userSettingsStoreName, "PREFIX", settings.Prefix);
            userSettingsStore.SetString(userSettingsStoreName, "DEFAULT", settings.Default);
            userSettingsStore.SetBoolean(userSettingsStoreName, "AUTOPROP", settings.AutoProp);
            userSettingsStore.SetBoolean(userSettingsStoreName, "AUTOTRANS", settings.AutoTrans);
            userSettingsStore.SetString(userSettingsStoreName, "NOTRANSLANG", settings.NoTransLang);
            userSettingsStore.SetBoolean(userSettingsStoreName, "NODEFAULTDESC", settings.NoDefaultDesc);
        }

        private static string GetCacheId()
        {
            return "D365F&OLABELEDITORPRIME";
        }
    }

    public class Settings
    {
        public string Model { get; set; }
        public string File { get; set; }
        public string Label { get; set; }
        // public string Prefix { get; set; }
        public string Default { get; set; }
        public bool AutoProp { get; set; }
        public bool AutoTrans { get; set; }
        public string NoTransLang { get; set; }
        public bool NoDefaultDesc { get; set; }
    }
}
