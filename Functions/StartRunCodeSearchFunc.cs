using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Functions_for_Dynamics_Operations.Utilities;

namespace Functions_for_Dynamics_Operations
{
    internal class StartRunCodeSearchFunc : ToolWindowsFunc
    {
        internal StartRunCodeSearchFunc(AsyncPackage asyncPackage) : base(asyncPackage)
        {

        }

        internal bool StartRunCodeSearch()
        {
            CodeViewUtils.DoNotLaunchOtherTools = true;
            // Create an instance per model being used
            Window = AsyncPackage.FindToolWindow(typeof(CodeSearch), 0, true);
            CodeViewUtils.DoNotLaunchOtherTools = false;

            if ((null != Window) && (null != Window.Frame))
            {
                OptionPageCustom options = (OptionPageCustom)AsyncPackage.GetDialogPage(typeof(OptionPageCustom));
                if (options != null && Window.Content is CodeSearchControl codeSearch)
                {
                    if (options.UseVsTheme)
                        codeSearch.ApplyVsTheme();
                    else
                        codeSearch.ApplyColors(options.LabelForeColor, options.LabelBackColor, options.GridForeColor, options.GridBackColor);
                }
                return true;
            }

            return false;
        }

        internal void StopCodeSearch()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            // find each instance per model
            ToolWindowPane window = AsyncPackage.FindToolWindow(typeof(CodeSearch), 0, false);
            if (null != window && null != window.Frame)
            {
                var frame = (IVsWindowFrame)window.Frame;

                frame.CloseFrame((uint)__FRAMECLOSE.FRAMECLOSE_NoSave);
            }
        }
    }
}
