using Microsoft.VisualStudio.Shell;
using System.ComponentModel;

namespace Functions_for_Dynamics_Operations
{
    internal class OptionPageCustom : DialogPage
    {
        [Category("DeepL Translate")]
        [DisplayName("API Key")]
        [PasswordPropertyText(true)]
        [Description("DeepL API authentication key (get free key at https://www.deepl.com/pro-api)")]
        public string DeepLApiKey { get; set; }

        [Category("DeepL Translate")]
        [DisplayName("API Url")]
        [Description("DeepL API endpoint URL (use https://api-free.deepl.com/v2/translate for free tier, https://api.deepl.com/v2/translate for pro)")]
        public string DeepLUrl { get; set; } = "https://api-free.deepl.com/v2/translate";

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
