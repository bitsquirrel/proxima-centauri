using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Functions_for_Dynamics_Operations
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class BPEditorCommand : BaseCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 4992;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("0d83bcff-55f6-4b2c-9583-5b3df6a1104d");

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static BPEditorCommand Instance { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BPEditorCommand"/> class.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private BPEditorCommand(AsyncPackage package, OleMenuCommandService commandService)
            : base(package, commandService, CommandSet, CommandId)
        {
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            Instance = await InitializeCommandAsync(package, CommandSet, CommandId,
                (pkg, cmdService) => new BPEditorCommand(pkg, cmdService));
        }

        /// <summary>
        /// Executes the command logic - shows the tool window when the menu item is clicked.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        protected override void ExecuteCommand(object sender, EventArgs e)
        {
            StartRunBPEditorFunc startRun = new StartRunBPEditorFunc(package);
            // When called from here, it will create a new instance of the editor
            if (startRun.StartRunBPEditor(true))
            {
                IVsWindowFrame windowFrame = (IVsWindowFrame)startRun.Window.Frame;
                ErrorHandler.ThrowOnFailure(windowFrame.Show());
            }
        }

        /// <summary>
        /// Gets the error message to log when command execution fails.
        /// </summary>
        /// <returns>Error message string.</returns>
        protected override string GetErrorMessage()
        {
            return "Unable to open Best Practice Editor";
        }
    }
}
