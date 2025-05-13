using Functions_for_Dynamics_Operations.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Functions_for_Dynamics_Operations.Functions
{
    public class RuntimeHost
    {
        public static bool IsCloudHosted()
        {
            // Pick up the default config (default is the config under the users documents) to check if we are cloud hosted
            var defaultConfig = Microsoft.Dynamics.Framework.Tools.Configuration.ConfigurationHelper.InstalledConfigurationEntries.FirstOrDefault(a => a.Item1 == "DefaultConfig");
            // The XML fails conversion dynamically - so load it direct to XML
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(File.ReadAllText(defaultConfig.Item2));
            // Pick up the host type node to determine if we are cloud hosted
            XmlNode specificNode = xmlDocument.GetElementsByTagName("RuntimeHostType")[0];
            // This is a cloud hosted UDE environment
            return specificNode.InnerXml == "CloudRuntime";
        }

        internal static string GetOneBoxConfigFileName()
        {
            // Pick up the default config (default is the config under the users documents) to check if we are cloud hosted
            var defaultConfig = Microsoft.Dynamics.Framework.Tools.Configuration.ConfigurationHelper.InstalledConfigurationEntries.FirstOrDefault(a => a.Item1 == "DefaultConfig");
            // The XML fails conversion dynamically - so load it direct to XML
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(File.ReadAllText(defaultConfig.Item2));
            // This is a cloud hosted UDE environment
            return xmlDocument.DocumentElement.Name;
        }

        public static (string modelStoreFolder, string frameworkDirectory) GetModelStoreAndFrameworkDirectories()
        {
            // Now pickup the current metadata config that is active
            var currentMetadata = Microsoft.Dynamics.Framework.Tools.Configuration.ConfigurationHelper.InstalledConfigurationEntries.FirstOrDefault(a => a.Item1 == "CurrentMetadataConfig");
            // This is the Json file containing the setup of the current active config used to developed
            CurrentMetadataConfig currentConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<CurrentMetadataConfig>(File.ReadAllText(currentMetadata.Item2));

            return (currentConfig.ModelStoreFolder, currentConfig.FrameworkDirectory);
        }

        public static string GetCloudHostedCurrentConfig()
        {
            // Now pickup the current metadata config that is active
            var currentMetadata = Microsoft.Dynamics.Framework.Tools.Configuration.ConfigurationHelper.InstalledConfigurationEntries.FirstOrDefault(a => a.Item1 == "CurrentMetadataConfig");
            // This is the Json file containing the setup of the current active config used to developed
            return File.ReadAllText(currentMetadata.Item2);
        }

        public static void SaveCloudHostedCurrentConfig(string jsonConfig)
        {
            // Now pickup the current metadata config that is active
            var currentMetadata = Microsoft.Dynamics.Framework.Tools.Configuration.ConfigurationHelper.InstalledConfigurationEntries.FirstOrDefault(a => a.Item1 == "CurrentMetadataConfig");
            // Overwrite the current config with the new values
            File.WriteAllText(currentMetadata.Item2, jsonConfig);
        }
    }
}
