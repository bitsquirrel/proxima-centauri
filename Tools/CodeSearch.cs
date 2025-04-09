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
    [Guid("28984386-13bc-47e8-9701-025f4b115285")]
    public class CodeSearch : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeSearch"/> class.
        /// </summary>
        public CodeSearch() : base(null)
        {
            this.Caption = "365 Code Search";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            this.Content = new CodeSearchControl();
        }
    }
}
