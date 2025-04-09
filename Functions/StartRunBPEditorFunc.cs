using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Functions_for_Dynamics_Operations
{
    internal class StartRunBPEditorFunc
    {
        private readonly AsyncPackage AsyncPackage;
        public BPEditorControl BpEditor;
        public ToolWindowPane Window;

        public StartRunBPEditorFunc(AsyncPackage package)
        {
            AsyncPackage = package;
        }

        public bool StartRunBPEditor(bool create)
        {
            ModelInfo modelInfo = VStudioUtils.GetActiveAXProjectModelInfo();

            if (modelInfo != null && modelInfo.Name != "")
            {
                if (!create)
                {
                    create = true;
                }

                // Create an instance per model being used
                Window = AsyncPackage.FindToolWindow(typeof(BPEditor), modelInfo.Id, create);
                if ((null != Window) && (null != Window.Frame))
                {
                    Window.Caption = "365 BP Editor - " + modelInfo.Name;

                    BpEditor = (BPEditorControl)Window.Content;
                    if (BpEditor != null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void StopBPEditor()
        {
            foreach (ModelInfo mi in VStudioUtils.GetAXProjectsModelsInfoInSolution())
            {
                if (mi != null && mi.Name != "")
                {   // find each instance per model
                    ToolWindowPane window = AsyncPackage.FindToolWindow(typeof(BPEditor), mi.Id, false);
                    if (null != window && null != window.Frame)
                    {
                        BPEditorControl bpEditor = (BPEditorControl)window.Content;
                        bpEditor?.ScrubForm();

                        var frame = (IVsWindowFrame)window.Frame;

                        frame.CloseFrame((uint)__FRAMECLOSE.FRAMECLOSE_NoSave);
                    }
                }
            }
        }
    }
}
