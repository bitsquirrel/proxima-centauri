using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Functions_for_Dynamics_Operations.Functions;
using Microsoft.Dynamics.AX.Metadata.Core.MetaModel;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.Dynamics.Framework.Tools.ProjectSupport;
using Microsoft.Dynamics.Framework.Tools.ProjectSupport.Automation;
using Microsoft.Dynamics.Framework.Tools.ProjectSystem;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace Functions_for_Dynamics_Operations
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class GenLabelsProjectCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 4333;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("18e89167-6f4e-4a92-8640-856c1837a099");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenLabelsProjectCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private GenLabelsProjectCommand(AsyncPackage package, OleMenuCommandService commandService)
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
        public static GenLabelsProjectCommand Instance
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
            // Switch to the main thread - the call to AddCommand in GenLabelsProjectCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new GenLabelsProjectCommand(package, commandService);
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
                DTE2 dte = Package.GetGlobalService(typeof(SDTE)) as DTE2;

                Microsoft.Dynamics.Framework.Tools.ProjectSystem.OAVSProject oAVSProject = (Microsoft.Dynamics.Framework.Tools.ProjectSystem.OAVSProject)dte.SelectedItems.Item(1).Project;
                if (oAVSProject == null)
                {
                    return;
                }

                GenLabelsForProjectItems genLabelsForProjectItems = new GenLabelsForProjectItems(new StartRunLabelEditorFunc(package));

                VSProjectNode projectNode = (VSProjectNode)oAVSProject.Object;

                foreach (var itemInProject in oAVSProject.ProjectItems)
                {
                    if (itemInProject is Microsoft.Dynamics.Framework.Tools.ProjectSupport.Automation.OAFolderItem folder)
                    {
                        foreach (var itemInFolder in folder.ProjectItems)
                        {
                            if (itemInFolder is OAVSProjectFileItem item)
                            {
                                genLabelsForProjectItems.GenerateLabels(item);
                            }
                        }
                    }
                    else if (itemInProject is OAVSProjectFileItem oAVSProjectFileItem)
                    {
                        genLabelsForProjectItems.GenerateLabels(oAVSProjectFileItem);
                    }
                }
            }
            catch (ExceptionVsix ex)
            {
                ex.Log("Unable to create labels for object in the project");
            }
        }
    }
}
