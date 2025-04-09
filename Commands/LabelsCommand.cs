using EnvDTE80;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.Dynamics.Framework.Tools.ProjectSystem;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using Task = System.Threading.Tasks.Task;

namespace Functions_for_Dynamics_Operations
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class LabelsCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 1112;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("3a130553-9af1-45d5-ab49-55c2028cd892");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="LabelsCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private LabelsCommand(AsyncPackage package, OleMenuCommandService commandService)
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
        public static LabelsCommand Instance
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
            // Switch to the main thread - the call to AddCommand in TableLabelsCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new LabelsCommand(package, commandService);
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
                StartRunLabelEditorFunc startRunLabel = new StartRunLabelEditorFunc(package);

                if (startRunLabel.StartRunLabelEditor(true) && startRunLabel.LabelEditor.LabelFileCollectionSelected != null)
                {
                    DTE2 dte = Package.GetGlobalService(typeof(SDTE)) as DTE2;

                    OAVSProjectFileItem oAVSProjectFileItem = (OAVSProjectFileItem)dte.SelectedItems.Item(1).ProjectItem;

                    if (oAVSProjectFileItem.Object != null)
                    {
                        VSProjectFileNode vSProjectFileNode = (VSProjectFileNode)oAVSProjectFileItem.Object;
                        Microsoft.Dynamics.Framework.Tools.MetaModel.Core.IDesignMetaModelService designMetaModel = VStudioUtils.GetDesignMetaModelService();

                        ModelInfo modelInfo = VStudioUtils.GetActiveAXProjectModelInfo();

                        if (vSProjectFileNode.MetadataReference.MetadataType.BaseType.Name == "AxEdt")
                        {
                            AxEdt edt = null;

                            switch (vSProjectFileNode.MetadataReference.MetadataType.Name)
                            {
                                case nameof(AxEdt):
                                    edt = designMetaModel.GetExtendedDataType(vSProjectFileNode.FileName);
                                    break;
                                case nameof(AxEdtContainer):
                                    edt = (AxEdtContainer)designMetaModel.GetExtendedDataType(vSProjectFileNode.FileName);
                                    break;
                                case nameof(AxEdtDate):
                                    edt = (AxEdtDate)designMetaModel.GetExtendedDataType(vSProjectFileNode.FileName);
                                    break;
                                case nameof(AxEdtGuid):
                                    edt = (AxEdtGuid)designMetaModel.GetExtendedDataType(vSProjectFileNode.FileName);
                                    break;
                                case nameof(AxEdtInt):
                                    edt = (AxEdtInt)designMetaModel.GetExtendedDataType(vSProjectFileNode.FileName);
                                    break;
                                case nameof(AxEdtInt64):
                                    edt = (AxEdtInt64)designMetaModel.GetExtendedDataType(vSProjectFileNode.FileName);
                                    break;
                                case nameof(AxEdtReal):
                                    edt = (AxEdtReal)designMetaModel.GetExtendedDataType(vSProjectFileNode.FileName);
                                    break;
                                case nameof(AxEdtString):
                                    edt = (AxEdtString)designMetaModel.GetExtendedDataType(vSProjectFileNode.FileName);
                                    break;
                                case nameof(AxEdtTime):
                                    edt = (AxEdtTime)designMetaModel.GetExtendedDataType(vSProjectFileNode.FileName);
                                    break;
                                case nameof(AxEdtUtcDateTime):
                                    edt = (AxEdtUtcDateTime)designMetaModel.GetExtendedDataType(vSProjectFileNode.FileName);
                                    break;
                                case nameof(AxEdtEnum):
                                    edt = (AxEdtEnum)designMetaModel.GetExtendedDataType(vSProjectFileNode.FileName);
                                    break;
                            }
                            // Set the label and help properties
                            EdtFunc.GenLabelsForEdt(startRunLabel.LabelEditor, edt, modelInfo.Name);

                            designMetaModel.CurrentMetadataProvider.Edts.Update(edt, new ModelSaveInfo(modelInfo));
                        }
                        else
                        {
                            switch (vSProjectFileNode.MetadataReference.MetadataType.Name)
                            {
                                #region Our Model

                                case nameof(AxTable):
                                    AxTable axTable = designMetaModel.GetTable(vSProjectFileNode.FileName);

                                    TableFunc.GenLabelsForTable(startRunLabel.LabelEditor, axTable);

                                    designMetaModel.CurrentMetadataProvider.Tables.Update(axTable, new ModelSaveInfo(modelInfo));
                                    break;

                                case nameof(AxForm):
                                    AxForm axForm = designMetaModel.GetForm(vSProjectFileNode.FileName);

                                    FormFunc.GenLabelsForForm(startRunLabel.LabelEditor, axForm, modelInfo.Name);

                                    designMetaModel.CurrentMetadataProvider.Forms.Update(axForm, new ModelSaveInfo(modelInfo));
                                    break;

                                case nameof(AxEnum):
                                    AxEnum axEnum = designMetaModel.GetEnum(vSProjectFileNode.FileName);

                                    EdtFunc.GenLabelsForEnum(startRunLabel.LabelEditor, axEnum, modelInfo.Name);

                                    designMetaModel.CurrentMetadataProvider.Enums.Update(axEnum, new ModelSaveInfo(modelInfo));
                                    break;

                                case nameof(AxView):
                                    AxView axView = designMetaModel.GetView(vSProjectFileNode.FileName);

                                    ViewFunc.GenLabelsForView(startRunLabel.LabelEditor, axView);

                                    designMetaModel.CurrentMetadataProvider.Views.Update(axView, new ModelSaveInfo(modelInfo));
                                    break;

                                case nameof(AxMap):
                                    AxMap axMap = designMetaModel.GetMap(vSProjectFileNode.FileName);

                                    MapFunc.GenLabelsForMap(startRunLabel.LabelEditor, axMap);

                                    designMetaModel.CurrentMetadataProvider.Maps.Update(axMap, new ModelSaveInfo(modelInfo));
                                    break;

                                case nameof(AxDataEntityView):
                                    AxDataEntityView axDataEntityView = designMetaModel.GetDataEntityView(vSProjectFileNode.FileName);

                                    DataFunc.GenLabelsForDataEntity(startRunLabel.LabelEditor, axDataEntityView, modelInfo.Name);

                                    designMetaModel.CurrentMetadataProvider.DataEntityViews.Update(axDataEntityView, new ModelSaveInfo(modelInfo));
                                    break;

                                case nameof(AxMenuItemAction):
                                    AxMenuItemAction axMenuItemAction = designMetaModel.GetMenuItemAction(vSProjectFileNode.FileName);

                                    MenuFunc.GenLabelsForActionMenuItem(startRunLabel.LabelEditor, axMenuItemAction);

                                    designMetaModel.CurrentMetadataProvider.MenuItemActions.Update(axMenuItemAction, new ModelSaveInfo(modelInfo));
                                    break;

                                case nameof(AxMenuItemDisplay):
                                    AxMenuItemDisplay axMenuItemDisplay = designMetaModel.GetMenuItemDisplay(vSProjectFileNode.FileName);

                                    MenuFunc.GenLabelsForDisplayMenuItem(startRunLabel.LabelEditor, axMenuItemDisplay);

                                    designMetaModel.CurrentMetadataProvider.MenuItemDisplays.Update(axMenuItemDisplay, new ModelSaveInfo(modelInfo));
                                    break;

                                case nameof(AxMenuItemOutput):
                                    AxMenuItemOutput axMenuItemOutput = designMetaModel.GetMenuItemOutput(vSProjectFileNode.FileName);

                                    MenuFunc.GenLabelsForOutputMenuItem(startRunLabel.LabelEditor, axMenuItemOutput);

                                    designMetaModel.CurrentMetadataProvider.MenuItemOutputs.Update(axMenuItemOutput, new ModelSaveInfo(modelInfo));
                                    break;

                                case nameof(AxMenu):
                                    AxMenu axMenu = designMetaModel.GetMenu(vSProjectFileNode.FileName);

                                    MenuFunc.GenLabelsForMenu(startRunLabel.LabelEditor, axMenu);

                                    designMetaModel.CurrentMetadataProvider.Menus.Update(axMenu, new ModelSaveInfo(modelInfo));
                                    break;

                                case nameof(AxTile):
                                    AxTile axTile = designMetaModel.GetTile(vSProjectFileNode.FileName);

                                    TileFunc.GenLabelsForTile(startRunLabel.LabelEditor, axTile);

                                    designMetaModel.CurrentMetadataProvider.Tiles.Update(axTile, new ModelSaveInfo(modelInfo));
                                    break;

                                case nameof(AxSecurityPrivilege):
                                    AxSecurityPrivilege axSecurityPrivilege = designMetaModel.GetSecurityPrivilege(vSProjectFileNode.FileName);

                                    SecurityFunc.GenLabelsForPrivilege(startRunLabel.LabelEditor, axSecurityPrivilege);

                                    designMetaModel.CurrentMetadataProvider.SecurityPrivileges.Update(axSecurityPrivilege, new ModelSaveInfo(modelInfo));
                                    break;

                                case nameof(AxSecurityDuty):
                                    AxSecurityDuty axSecurityDuty = designMetaModel.GetSecurityDuty(vSProjectFileNode.FileName);

                                    SecurityFunc.GenLabelsForDuty(startRunLabel.LabelEditor, axSecurityDuty);

                                    designMetaModel.CurrentMetadataProvider.SecurityDuties.Update(axSecurityDuty, new ModelSaveInfo(modelInfo));
                                    break;

                                case nameof(AxSecurityRole):
                                    AxSecurityRole axSecurityRole = designMetaModel.GetSecurityRole(vSProjectFileNode.FileName);

                                    SecurityFunc.GenLabelsForRole(startRunLabel.LabelEditor, axSecurityRole);

                                    designMetaModel.CurrentMetadataProvider.SecurityRoles.Update(axSecurityRole, new ModelSaveInfo(modelInfo));
                                    break;

                                case nameof(AxSecurityPolicy):
                                    AxSecurityPolicy axSecurityPolicy = designMetaModel.GetSecurityPolicy(vSProjectFileNode.FileName);

                                    SecurityFunc.GenLabelsForPolicy(startRunLabel.LabelEditor, axSecurityPolicy);

                                    designMetaModel.CurrentMetadataProvider.SecurityPolicies.Update(axSecurityPolicy, new ModelSaveInfo(modelInfo));
                                    break;

                                #endregion

                                #region Extension

                                case nameof(AxEdtExtension):
                                    AxEdtExtension axEdtExt = designMetaModel.CurrentMetadataProvider.EdtExtensions.Read(vSProjectFileNode.Name);

                                    EdtFunc.GenLabelsForEdtExt(startRunLabel.LabelEditor, axEdtExt, modelInfo.Name);

                                    designMetaModel.CurrentMetadataProvider.EdtExtensions.Update(axEdtExt, new ModelSaveInfo(modelInfo));
                                    break;

                                case nameof(AxTableExtension):
                                    AxTableExtension axTableExtension = designMetaModel.GetTableExtension(vSProjectFileNode.FileName);

                                    TableFunc.GenLabelsForTableExt(startRunLabel.LabelEditor, axTableExtension, modelInfo.Name);

                                    designMetaModel.CurrentMetadataProvider.TableExtensions.Update(axTableExtension, new ModelSaveInfo(modelInfo));
                                    break;

                                case nameof(AxFormExtension):
                                    AxFormExtension axFormExtension = designMetaModel.GetFormExtension(vSProjectFileNode.FileName);

                                    FormFunc.GenLabelsForFormExt(startRunLabel.LabelEditor, axFormExtension, modelInfo.Name);

                                    designMetaModel.CurrentMetadataProvider.FormExtensions.Update(axFormExtension, new ModelSaveInfo(modelInfo));
                                    break;

                                case nameof(AxEnumExtension):
                                    AxEnumExtension axEnumExtension = designMetaModel.GetEnumExtension(vSProjectFileNode.FileName);

                                    EdtFunc.GenLabelsForEnumExt(startRunLabel.LabelEditor, axEnumExtension, modelInfo.Name);

                                    designMetaModel.CurrentMetadataProvider.EnumExtensions.Update(axEnumExtension, new ModelSaveInfo(modelInfo));
                                    break;

                                case nameof(AxViewExtension):
                                    AxViewExtension axViewExtension = designMetaModel.GetViewExtension(vSProjectFileNode.FileName);

                                    ViewFunc.GenLabelsForViewExt(startRunLabel.LabelEditor, axViewExtension, modelInfo.Name);

                                    designMetaModel.CurrentMetadataProvider.ViewExtensions.Update(axViewExtension, new ModelSaveInfo(modelInfo));
                                    break;

                                case nameof(AxDataEntityViewExtension):
                                    AxDataEntityViewExtension axDataEntityViewExtension = designMetaModel.GetDataEntityViewExtension(vSProjectFileNode.FileName);

                                    DataFunc.GenLabelsForDataEntityExt(startRunLabel.LabelEditor, axDataEntityViewExtension, modelInfo.Name);

                                    designMetaModel.CurrentMetadataProvider.DataEntityViewExtensions.Update(axDataEntityViewExtension, new ModelSaveInfo(modelInfo));
                                    break;

                                case nameof(AxMenuItemActionExtension):
                                    AxMenuItemActionExtension axMenuItemActionExtension = designMetaModel.CurrentMetadataProvider.MenuItemActionExtensions.Read(vSProjectFileNode.Name);

                                    MenuFunc.GenLabelsForActionMenuItemExt(startRunLabel.LabelEditor, axMenuItemActionExtension, modelInfo.Name);

                                    designMetaModel.CurrentMetadataProvider.MenuItemActionExtensions.Update(axMenuItemActionExtension, new ModelSaveInfo(modelInfo));
                                    break;

                                case nameof(AxMenuItemDisplayExtension):
                                    AxMenuItemDisplayExtension axMenuItemDisplayExtension = designMetaModel.CurrentMetadataProvider.MenuItemDisplayExtensions.Read(vSProjectFileNode.Name);

                                    MenuFunc.GenLabelsForDisplayMenuItemExt(startRunLabel.LabelEditor, axMenuItemDisplayExtension, modelInfo.Name);

                                    designMetaModel.CurrentMetadataProvider.MenuItemDisplayExtensions.Update(axMenuItemDisplayExtension, new ModelSaveInfo(modelInfo));
                                    break;

                                case nameof(AxMenuItemOutputExtension):
                                    AxMenuItemOutputExtension axMenuItemOutputExtension = designMetaModel.CurrentMetadataProvider.MenuItemOutputExtensions.Read(vSProjectFileNode.Name);

                                    MenuFunc.GenLabelsForOutputMenuItemExt(startRunLabel.LabelEditor, axMenuItemOutputExtension, modelInfo.Name);

                                    designMetaModel.CurrentMetadataProvider.MenuItemOutputExtensions.Update(axMenuItemOutputExtension, new ModelSaveInfo(modelInfo));
                                    break;

                                case nameof(AxMenuExtension):
                                    AxMenuExtension axMenuExtension = designMetaModel.GetMenuExtension(vSProjectFileNode.FileName);

                                    MenuFunc.GenLabelsForMenuExt(startRunLabel.LabelEditor, axMenuExtension);

                                    designMetaModel.CurrentMetadataProvider.MenuExtensions.Update(axMenuExtension, new ModelSaveInfo(modelInfo));
                                    break;

                                    #endregion
                            }
                        }
                    }
                }
            }
            catch (ExceptionVsix ex)
            {
                ex.Log();
            }
        }
    }
}
