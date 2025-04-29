using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Functions_for_Dynamics_Operations
{
    internal class StartRunCodeSearchFunc : ToolWindowsFunc
    {
        internal StartRunCodeSearchFunc(AsyncPackage asyncPackage) : base(asyncPackage)
        {

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
