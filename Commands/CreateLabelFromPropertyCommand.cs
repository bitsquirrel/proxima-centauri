using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using Task = System.Threading.Tasks.Task;

namespace Functions_for_Dynamics_Operations
{
    /// <summary>
    /// Command handler for creating a label from the D365 element designer property window.
    /// This button appears in the context menu of the D365 properties pane.
    /// </summary>
    internal sealed class CreateLabelFromPropertyCommand
    {
        /// <summary>
        /// Command ID matching <c>cmdidCreateLabelFromPropertyCmd</c> in the VSCT file.
        /// </summary>
        public const int CommandId = 121;

        /// <summary>
        /// Command set GUID matching <c>propertiesLabelCmdSet</c> in the VSCT file.
        /// This is the same GUID used by the D365 element designer properties pane.
        /// </summary>
        public static readonly Guid CommandSet = new Guid("A72BD644-1979-4CBC-A620-EA4112198A66");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateLabelFromPropertyCommand"/> class.
        /// Adds our command handler for the menu item (the command must exist in the VSCT file).
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private CreateLabelFromPropertyCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static CreateLabelFromPropertyCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new CreateLabelFromPropertyCommand(package, commandService);
        }

        /// <summary>
        /// Opens the 365 Label Editor when the user clicks "Create label" in the D365 element
        /// designer properties context menu.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                StartRunLabelEditorFunc startRunLabel = new StartRunLabelEditorFunc(package);

                if (startRunLabel.StartRunLabelEditor(true))
                {
                    IVsWindowFrame windowFrame = (IVsWindowFrame)startRunLabel.Window.Frame;

                    Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
                }
            }
            catch (ExceptionVsix ex)
            {
                ex.Log("Unable to open label editor from property window");
            }
        }
    }
}
