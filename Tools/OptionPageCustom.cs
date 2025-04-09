using Microsoft.VisualStudio.Shell;
using System.ComponentModel;

namespace Functions_for_Dynamics_Operations
{
    internal class OptionPageCustom : DialogPage
    {
        [Category("Azure Translate")]
        [DisplayName("Url")]
        [Description("Azure translation Url")]
        public string TranslateUrl { get; set; }

        [Category("Azure Translate")]
        [DisplayName("Region")]
        [Description("Azure region")]
        public string TranslateRegion { get; set; }

        [Category("Azure Translate")]
        [DisplayName("Key")]
        [PasswordPropertyText(true)]
        [Description("Key to the transation API in Azure")]
        public string TranslateKey { get; set; }

        [Category("Labels")]
        [DisplayName("Always new")]
        [Description("Do not check if the label already exists, create a new one")]
        public bool AlwaysNewLabel { get; set; }
    }
}
