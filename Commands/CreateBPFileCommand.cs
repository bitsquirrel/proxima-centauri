using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using Task = System.Threading.Tasks.Task;

namespace Functions_for_Dynamics_Operations
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class CreateBPFileCommand : BaseCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 171;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("f16df604-a4eb-458f-a357-d2ec3e1f92a7");

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static CreateBPFileCommand Instance { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateBPFileCommand"/> class.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private CreateBPFileCommand(AsyncPackage package, OleMenuCommandService commandService)
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
                (pkg, cmdService) => new CreateBPFileCommand(pkg, cmdService));
        }

        /// <summary>
        /// Executes the command logic.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        protected override void ExecuteCommand(object sender, EventArgs e)
        {
            new BestPracticeFunc(VStudioUtils.GetActiveAXProjectModelInfo());
        }

        /// <summary>
        /// Gets the error message to log when command execution fails.
        /// </summary>
        /// <returns>Error message string.</returns>
        protected override string GetErrorMessage()
        {
            return "Unable to create Best Practice file";
        }
    }
}
