using System;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Functions_for_Dynamics_Operations.Utilities;

namespace Functions_for_Dynamics_Operations
{
    internal class StartRunLabelSearchFunc : ToolWindowsFunc
    {
        internal StartRunLabelSearchFunc(AsyncPackage asyncPackage) : base(asyncPackage) { }

        internal bool StartRunLabelSearch()
        {
            CodeViewUtils.DoNotLaunchOtherTools = true;
            Window = AsyncPackage.FindToolWindow(typeof(LabelSearch), 0, true);
            CodeViewUtils.DoNotLaunchOtherTools = false;

            if ((null != Window) && (null != Window.Frame))
            {
                return true;
            }

            return false;
        }

        internal void StopLabelSearch()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            // Close all instances of the LabelSearchControl tool window for each model in the solution
            ToolWindowPane window = AsyncPackage.FindToolWindow(typeof(LabelSearch), 0, false);
            if (null != window && null != window.Frame)
            {
                var frame = (IVsWindowFrame)window.Frame;
                frame.CloseFrame((uint)__FRAMECLOSE.FRAMECLOSE_NoSave);
            }
        }
    }
}
