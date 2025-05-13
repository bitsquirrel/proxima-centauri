using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using Task = System.Threading.Tasks.Task;

namespace Functions_for_Dynamics_Operations
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class SearchLabelInCode
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 85678;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("c02b9a5c-7778-4cbb-994c-5117746d2d8a");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchLabelInCode"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private SearchLabelInCode(AsyncPackage package, OleMenuCommandService commandService)
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
        public static SearchLabelInCode Instance
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
            // Switch to the main thread - the call to AddCommand in SearchLabelInCode's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new SearchLabelInCode(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                DTE dte = Package.GetGlobalService(typeof(SDTE)) as DTE;

                TextSelection textSelection = (TextSelection)dte.ActiveDocument.Selection;

                StartRunLabelEditorFunc startRunLabel = new StartRunLabelEditorFunc(package);
                // For now we will not allow the user to select the label file, we will use the default one
                if (startRunLabel.StartRunLabelEditor(false))
                {
                    if (!startRunLabel.LabelEditor.LabelFileCollectionSelected.IsNull())
                    {
                        if (LabelUtils.CheckTextAlreadyExists(startRunLabel.LabelEditor.LabelFileCollectionSelected, startRunLabel.LabelEditor.LabelFileSelected, textSelection.Text, out string labelidExisting))
                        {
                            if (labelidExisting != "")
                            {
                                textSelection.Text = $"@{startRunLabel.LabelEditor.LabelFileCollectionSelected.Id}:{labelidExisting}";

                                dte.ActiveDocument.Save();
                            }
                        }
                        else
                            VStudioUtils.LogToOutput("No existing label found for selected text");
                    }
                    else
                        VStudioUtils.LogToOutput("Label editor not fully setup, select label file and language");
                }
                else
                    VStudioUtils.LogToOutput("Label editor not initialized");
            }
            catch (ExceptionVsix ex)
            {
                ex.Log("Unable to search for label in code");
            }
        }
    }
}
