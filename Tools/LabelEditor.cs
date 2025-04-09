using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;

namespace Functions_for_Dynamics_Operations
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("c06ebc5f-2082-42fd-904e-fb555bd0820d")]
    public class LabelEditor : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LabelEditor"/> class.
        /// </summary>
        public LabelEditor() : base(null)
        {
            Microsoft.Dynamics.AX.Metadata.MetaModel.ModelInfo modelInfo = VStudioUtils.GetActiveAXProjectModelInfo();
            if (modelInfo != null)
            {
                string modelName = modelInfo.Module;

                if (modelName != null && modelName != "")
                {   // Label editor per model in the solution
                    Caption = "365 Label Editor - " + modelInfo.DisplayName;
                    Content = new LabelEditorControl(modelName);
                }
                else
                {
                    Caption = "NO PROJECT";
                    Content = new LabelEditorControl("");
                }
            }
            else
            {
                Caption = "NO PROJECT";
                Content = new LabelEditorControl("");
            }
        }
    }
}
