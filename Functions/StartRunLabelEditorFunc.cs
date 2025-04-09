using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.IO.Packaging;
using System.Windows.Controls;

namespace Functions_for_Dynamics_Operations
{
    internal class StartRunLabelEditorFunc
    {
        private readonly AsyncPackage AsyncPackage;
        public LabelEditorControl LabelEditor;
        public ToolWindowPane Window;

        public StartRunLabelEditorFunc(AsyncPackage package)
        {
            AsyncPackage = package;
        }

        /// <summary>
        /// Centralize the label start and or check if started
        /// </summary>
        /// <returns>label editor control</returns>
        public bool StartRunLabelEditor(bool create)
        {
            OptionPageCustom options = (OptionPageCustom)AsyncPackage.GetDialogPage(typeof(OptionPageCustom));
            if (options != null) 
            {
                AzureTranslate.Url = options.TranslateUrl;
                AzureTranslate.Secret = options.TranslateKey;
                AzureTranslate.Region = options.TranslateRegion;

                Constants.Constants.AlwaysNewLabel = options.AlwaysNewLabel;
            }

            LabelFilePath lfp = VStudioUtils.GetActiveAXProjectLabelFilePath();

            if (lfp.Model != null && lfp.FilePath != "" && lfp.FilePath != null)
            {
                if (!create)
                {
                    create = true;
                }
                // Create an instance per model being used
                Window = AsyncPackage.FindToolWindow(typeof(LabelEditor), lfp.Id, create);
                if ((null != Window) && (null != Window.Frame))
                {
                    Window.Caption = "365 Label Editor - " + lfp.Model;

                    LabelEditor = (LabelEditorControl)Window.Content;
                    if (LabelEditor != null)
                    {
                        if (LabelEditor.Primed == false)
                        {   // Set the current directory of project
                            LabelEditor.LabelDirectory = lfp.FilePath;
                            LabelEditor.Model = lfp.Model;

                            LabelEditor.InitForm();
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        public void StopEmptyLabelEditor()
        {
            DTE2 dte = AsyncPackage.GetGlobalService(typeof(SDTE)) as DTE2;

            ToolWindows toolWindows = dte.ToolWindows;
            LabelEditor userControl = (LabelEditor)toolWindows.GetToolWindow("LabelEditor");

        }

        public void StopLabelEditor()
        {
            foreach (LabelFilePath lfp in VStudioUtils.GetAXProjectsModelsInSolutionLFP())
            {
                if (lfp.Model != null && lfp.FilePath != "" && lfp.FilePath != null)
                {   // find each instance per model
                    StopControl(lfp.Id);
                }
            }
        }

        internal void StopControl(int id)
        {
            ToolWindowPane window = AsyncPackage.FindToolWindow(typeof(LabelEditor), id, false);
            if (null != window && null != window.Frame)
            {
                LabelEditorControl labelEditor = (LabelEditorControl)window.Content;
                if (labelEditor.LabelDirectory != null)
                {   // Scrub the forms
                    labelEditor.ScrubForm();
                    var frame = (IVsWindowFrame)window.Frame;
                    frame.CloseFrame((uint)__FRAMECLOSE.FRAMECLOSE_NoSave);
                }
            }
        }
    }
}
