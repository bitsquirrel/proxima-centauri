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
    internal sealed class LookupCommand : BaseCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 4515;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("89d99d81-7b70-40ce-8965-5c095a127236");

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static LookupCommand Instance { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LookupCommand"/> class.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private LookupCommand(AsyncPackage package, OleMenuCommandService commandService)
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
                (pkg, cmdService) => new LookupCommand(pkg, cmdService));
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
            textSelection.Insert(DynaxUtils.LookupCode(), 1);

            dte.ActiveDocument.Save();
        }

        /// <summary>
        /// Gets the error message to log when command execution fails.
        /// </summary>
        /// <returns>Error message string.</returns>
        protected override string GetErrorMessage()
        {
            return "Unable to create lookup";
        }
    }
}
