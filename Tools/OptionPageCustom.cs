using Microsoft.VisualStudio.Shell;
using System.ComponentModel;
using System.Drawing;

namespace Functions_for_Dynamics_Operations
{
    internal class OptionPageCustom : DialogPage
    {
        [Category("Label Editor Appearance")]
        [DisplayName("Use Visual Studio theme")]
        [Description("When enabled, all editor and search controls automatically follow the current Visual Studio color theme (dark/light/blue). Custom color settings below are ignored.")]
        public bool UseVsTheme { get; set; } = true;

        [Category("Label Editor Appearance")]
        [DisplayName("Label foreground")]
        [Description("Foreground (text) color used for the field labels. Only applied when 'Use Visual Studio theme' is disabled.")]
        public Color LabelForeColor { get; set; } = Color.CadetBlue;

        [Category("Label Editor Appearance")]
        [DisplayName("Editor background")]
        [Description("Background color of the tool windows. Only applied when 'Use Visual Studio theme' is disabled.")]
        public Color LabelBackColor { get; set; } = Color.Transparent;

        [Category("Label Editor Appearance")]
        [DisplayName("Grid foreground")]
        [Description("Text color for the data grids. Only applied when 'Use Visual Studio theme' is disabled.")]
        public Color GridForeColor { get; set; } = Color.FromArgb(220, 220, 220);

        [Category("Label Editor Appearance")]
        [DisplayName("Grid background")]
        [Description("Background color for the data grids. Only applied when 'Use Visual Studio theme' is disabled.")]
        public Color GridBackColor { get; set; } = Color.FromArgb(30, 30, 30);

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
