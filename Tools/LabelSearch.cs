using Microsoft.Dynamics.AX.Metadata.Extensions.Reports;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.IO;
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
    [Guid("e89a9d21-636c-4330-aa45-aa8ab2089415")]
    public class LabelSearch : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LabelSearch"/> class.
        /// </summary>
        public LabelSearch() : base(null)
        {
            Caption = "365 Label Search";

            Content = new LabelSearchControl(new LabelSearchUtils().SearchForLabels());
        }
    }
}
