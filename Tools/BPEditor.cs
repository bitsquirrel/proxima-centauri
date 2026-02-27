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
    [Guid("7b0429fd-99d2-4471-9dd9-9e868422a9b4")]
    public class BPEditor : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BPEditor"/> class.
        /// The caption is set later by <see cref="StartRunBPEditorFunc"/>.
        /// </summary>
        public BPEditor() : base(null)
        {
            Caption = "365 BP Editor";
            Content = new BPEditorControl();
        }
    }
}
