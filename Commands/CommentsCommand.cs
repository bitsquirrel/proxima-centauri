using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using Task = System.Threading.Tasks.Task;

namespace Functions_for_Dynamics_Operations
{
    /// <summary>
    /// Create comments on objects
    /// </summary>
    internal sealed class CommentsCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 110;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("34c200eb-bc9e-45af-9e9f-a9cb1a4e7668");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentsCmd"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private CommentsCommand(AsyncPackage package, OleMenuCommandService commandService)
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
        public static CommentsCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IAsyncServiceProvider ServiceProvider
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
            // Switch to the main thread - the call to AddCommand in CommentsCmd's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new CommentsCommand(package, commandService);
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
                DTE dte = (DTE)Package.GetGlobalService(typeof(SDTE));

                if (dte.ActiveDocument.Saved)
                {
                    if (dte.ActiveDocument.Name != "")
                    {
                        if (dte.ActiveDocument.Name.Substring(0, 8) == "AxClass_")
                            ClassFunc.ApplyComments(dte.ActiveDocument.Name.Replace("AxClass_", "").Replace(".xpp", ""));
                        else if (dte.ActiveDocument.Name.Substring(0, 8) == "AxTable_")
                            TableFunc.ApplyComments(dte.ActiveDocument.Name.Replace("AxTable_", "").Replace(".xpp", ""));
                        else if (dte.ActiveDocument.Name.Substring(0, 7) == "AxForm_")
                            FormFunc.ApplyComments(dte.ActiveDocument.Name.Replace("AxForm_", "").Replace(".xpp", ""));
                        else if (dte.ActiveDocument.Name.Substring(0, 7) == "AxView_")
                            ViewFunc.ApplyComments(dte.ActiveDocument.Name.Replace("AxView_", "").Replace(".xpp", ""));
                        else if (dte.ActiveDocument.Name.Substring(0, 17) == "AxDataEntityView_")
                            DataFunc.ApplyComments(dte.ActiveDocument.Name.Replace("AxDataEntityView_", "").Replace(".xpp", ""));
                        else if (dte.ActiveDocument.Name.Substring(0, 8) == "AxQuery_")
                            QueryFunc.ApplyComments(dte.ActiveDocument.Name.Replace("AxQuery_", "").Replace(".xpp", ""));
                        else if (dte.ActiveDocument.Name.Substring(0, 6) == "AxMap_")
                            MapFunc.ApplyComments(dte.ActiveDocument.Name.Replace("AxMap_", "").Replace(".xpp", ""));
                        // Xpp file location
                        string file = dte.ActiveDocument.FullName;
                        // Reload the window
                        dte.ActiveDocument.Close();
                        dte.ItemOperations.OpenFile(file);
                    }
                }
                else
                    VStudioUtils.LogToOutput("Save before using this function");
            }
            catch (ExceptionVsix ex)
            {
                ex.Log("Unable to apply comments");
            }
        }
    }
}
