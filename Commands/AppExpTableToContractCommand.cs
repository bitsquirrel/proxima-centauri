using System;
using System.ComponentModel.Design;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace Functions_for_Dynamics_Operations
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class AppExpTableToContractCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 285;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("cd805c4b-712d-4b40-bcdc-f42885e12eef");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppExpTableToCtrctCmd"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private AppExpTableToContractCommand(AsyncPackage package, OleMenuCommandService commandService)
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
        public static AppExpTableToContractCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in AppExpTableToCtrctCmd's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new AppExpTableToContractCommand(package, commandService);
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
            // DEPRECATED : MICROSOFT HAS NOW ADDED THIS IN THE STANDARD
            /*
            try
            {
                EnvDTE80.DTE2 dte = (EnvDTE80.DTE2)Package.GetGlobalService(typeof(SDTE));
                // Pickup the toolwindow of the application explorer
                Window window = dte.Windows.Item(1);

                if (window.Object is Microsoft.Dynamics.Framework.Tools.ApplicationExplorer.ApplicationExplorerControlAutomation applicationExplorer)
                {
                    if (VStudioUtils.GetActiveAXProjectModelName() != "")
                    {
                        ObjectTypes appExplNodeSelect = new ObjectTypes(applicationExplorer.SelectedNodeName);

                        if (appExplNodeSelect.NodeName != "")
                        {
                            switch (appExplNodeSelect.NodeType)
                            {
                                case NodeType.Table:
                                    if (VStudioUtils.GetDesignMetaModelService().GetTable(appExplNodeSelect.NodeName) != null)
                                        new TableFunc().CreateClassFromTable(appExplNodeSelect.NodeName);
                                    break;
                            }
                        }
                    }
                }
            }
            catch (ExceptionVsix ex)
            {
                ex.Log();
            }
            */
        }
    }
}
