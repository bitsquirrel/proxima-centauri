﻿using EnvDTE;
using EnvDTE80;
using Functions_for_Dynamics_Operations.Functions;
using Microsoft.Dynamics.AX.Metadata.Core.MetaModel;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.Dynamics.Framework.Tools.ProjectSystem;
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
    internal sealed class CreateMenuItemCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 3331;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("3a130553-9af1-45d5-ab49-55c2028cd892");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateMenuItemCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private CreateMenuItemCommand(AsyncPackage package, OleMenuCommandService commandService)
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
        public static CreateMenuItemCommand Instance
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
            // Switch to the main thread - the call to AddCommand in CreateMenuItemCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new CreateMenuItemCommand(package, commandService);
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

                OAVSProjectFileItem oAVSProjectFileItem = (OAVSProjectFileItem)dte.SelectedItems.Item(1).ProjectItem;

                if (oAVSProjectFileItem.Object != null)
                {
                    VSProjectFileNode vSProjectFileNode = (VSProjectFileNode)oAVSProjectFileItem.Object;
                    Microsoft.Dynamics.Framework.Tools.MetaModel.Core.IDesignMetaModelService designMetaModel = VStudioUtils.GetDesignMetaModelService();

                    ModelInfo modelInfo = VStudioUtils.GetActiveAXProjectModelInfo();

                    switch (vSProjectFileNode.MetadataReference.MetadataType.Name)
                    {
                        case nameof(AxForm):
                            new PrivilegeFunc().CheckCreateDisplayMenuItemAndPrivileges(designMetaModel, vSProjectFileNode, EntryPointType.MenuItemDisplay, designMetaModel.GetForm(vSProjectFileNode.FileName).Design.Caption);
                            break;
                        case nameof(AxClass):
                            new PrivilegeFunc().CheckCreateActionMenuItemAndPrivileges(designMetaModel, vSProjectFileNode, EntryPointType.MenuItemAction, $"Menu item for class {vSProjectFileNode.Name}");
                            break;
                        default:
                            // For other types, we do not create menu items
                            VStudioUtils.LogToGenOutput($"Menu item creation for {vSProjectFileNode.MetadataReference.MetadataType.Name}, is not supported");
                            break;
                    }
                }
            }
            catch (ExceptionVsix ex)
            {
                ex.Log("Unable to create menu item");
            }
        }


    }
}
