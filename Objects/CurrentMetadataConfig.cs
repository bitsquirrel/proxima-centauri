using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Functions_for_Dynamics_Operations.Objects
{
    public class CurrentMetadataConfig
    {
        public string CrossReferencesDatabaseName { get; set; }
        public string CrossReferencesDbServerName { get; set; }
        public string ModelStoreFolder { get; set; }
        public object ModuleExclusionList { get; set; }
        public string DefaultModelForNewProjects { get; set; }
        public string DebugSourceFolder { get; set; }
        public string FrameworkDirectory { get; set; }
        public string AdminIdentityProvider { get; set; }
        public string AosWebsiteName { get; set; }
        public string ApplicationHostConfigFile { get; set; }
        public string AudienceUri { get; set; }
        public string BusinessDatabaseName { get; set; }
        public string BusinessDatabaseUserName { get; set; }
        public string BusinessDatabasePassword { get; set; }
        public string AzureAppID { get; set; }
        public object AzureCR_Key { get; set; }
        public object ContainerMemory { get; set; }
        public object RunBatchWithinAOS { get; set; }
        public string CloudInstanceURL { get; set; }
        public string DatabaseServer { get; set; }
        public string DefaultCompany { get; set; }
        public bool EnableOfflineAuthentication { get; set; }
        public string OfflineAuthenticationAdminEmail { get; set; }
        public string PartitionKey { get; set; }
        public int RuntimeHostType { get; set; }
        public string Description { get; set; }
        public string WebRoleDeploymentFolder { get; set; }
        public List<string> ReferencePackagesPaths { get; set; }
        public string RuntimePackagesDirectory { get; set; }
    }
}
