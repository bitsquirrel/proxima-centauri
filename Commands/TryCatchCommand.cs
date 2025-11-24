using System;
using System.ComponentModel.Design;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace Functions_for_Dynamics_Operations
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class TryCatchCommand : BaseCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 102;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("b4197cf2-8e29-4a12-946b-3879be914007");

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static TryCatchCommand Instance { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TryCatchCommand"/> class.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private TryCatchCommand(AsyncPackage package, OleMenuCommandService commandService)
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
                (pkg, cmdService) => new TryCatchCommand(pkg, cmdService));
        }

        /// <summary>
        /// Executes the command logic.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        protected override void ExecuteCommand(object sender, EventArgs e)
        {
            EnvDTE.DTE dte = Package.GetGlobalService(typeof(SDTE)) as EnvDTE.DTE;

            TextSelection textSelection = (TextSelection)dte.ActiveDocument.Selection;
            textSelection.Insert(DynaxUtils.TryCatchClrCode(textSelection.CurrentColumn), 1);

            dte.ActiveDocument.Save();
        }

        /// <summary>
        /// Gets the error message to log when command execution fails.
        /// </summary>
        /// <returns>Error message string.</returns>
        protected override string GetErrorMessage()
        {
            return "Failed to create try catch code";
        }
    }
}
