﻿using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Functions_for_Dynamics_Operations
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class FindRecIdCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 4142;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("89d99d81-7b70-40ce-8965-5c095a127236");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="FindRecIdCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private FindRecIdCommand(AsyncPackage package, OleMenuCommandService commandService)
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
        public static FindRecIdCommand Instance
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
            // Switch to the main thread - the call to AddCommand in FindRecIdCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new FindRecIdCommand(package, commandService);
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

                DynType dynType = DynaxUtils.GetObjectTypeAndName(dte.ActiveDocument.Name);

                Microsoft.Dynamics.AX.Metadata.MetaModel.AxTable table = VStudioUtils.GetDesignMetaModelService().CurrentMetaModelService.GetTable(dynType.Name);
                if (!(table is null))
                {   // Only works on tables
                    DynTableFieldEdt tablefieldedt = DynaxUtils.GetAXTableFieldAndEdtRecId(table);

                    if (tablefieldedt != null)
                        textSelection.Insert(DynaxUtils.FindCode(tablefieldedt, "findRecId"), 1);

                    dte.ActiveDocument.Save();
                }
                else
                {
                    VStudioUtils.LogToOutput("Function not run, Please ensure that the Label Editor was opened");
                }
            }
            catch (ExceptionVsix ex)
            {
                ex.Log("Unable to create the find method using the RecId");
            }
        }
    }
}
