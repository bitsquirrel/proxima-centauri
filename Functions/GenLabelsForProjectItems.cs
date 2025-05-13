using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.Dynamics.Framework.Tools.ProjectSupport;
using Microsoft.Dynamics.Framework.Tools.ProjectSystem;

namespace Functions_for_Dynamics_Operations.Functions
{
    internal class GenLabelsForProjectItems
    {
        internal StartRunLabelEditorFunc StartRunLabel;
        internal bool LabelEditorStarted = false;

        public GenLabelsForProjectItems(StartRunLabelEditorFunc startRunLabel) 
        { 
            StartRunLabel = startRunLabel;

            if (startRunLabel.StartRunLabelEditor(true))
            {
                if (startRunLabel.LabelEditor.LabelFileCollectionSelected != null)
                {
                    LabelEditorStarted = true;
                }
            }
        }

        internal void GenerateLabels(OAVSProjectFileItem projectFileItem)
        {
            if (LabelEditorStarted && projectFileItem.Object != null)
            {
                VStudioUtils.LogToGenOutput("Generating labels for: " + projectFileItem.Name);

                VSProjectFileNode vSProjectFileNode = (VSProjectFileNode)projectFileItem.Object;
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
                    EdtFunc.GenLabelsForEdt(StartRunLabel.LabelEditor, edt, modelInfo.Name);

                    designMetaModel.CurrentMetadataProvider.Edts.Update(edt, new ModelSaveInfo(modelInfo));
                }
                else
                {
                    switch (vSProjectFileNode.MetadataReference.MetadataType.Name)
                    {
                        #region Our Model

                        case nameof(AxTable):
                            AxTable axTable = designMetaModel.GetTable(vSProjectFileNode.FileName);

                            TableFunc.GenLabelsForTable(StartRunLabel.LabelEditor, axTable);

                            designMetaModel.CurrentMetadataProvider.Tables.Update(axTable, new ModelSaveInfo(modelInfo));
                            break;

                        case nameof(AxForm):
                            AxForm axForm = designMetaModel.GetForm(vSProjectFileNode.FileName);

                            FormFunc.GenLabelsForForm(StartRunLabel.LabelEditor, axForm, modelInfo.Name);

                            designMetaModel.CurrentMetadataProvider.Forms.Update(axForm, new ModelSaveInfo(modelInfo));
                            break;

                        case nameof(AxEnum):
                            AxEnum axEnum = designMetaModel.GetEnum(vSProjectFileNode.FileName);

                            EdtFunc.GenLabelsForEnum(StartRunLabel.LabelEditor, axEnum, modelInfo.Name);

                            designMetaModel.CurrentMetadataProvider.Enums.Update(axEnum, new ModelSaveInfo(modelInfo));
                            break;

                        case nameof(AxView):
                            AxView axView = designMetaModel.GetView(vSProjectFileNode.FileName);

                            ViewFunc.GenLabelsForView(StartRunLabel.LabelEditor, axView);

                            designMetaModel.CurrentMetadataProvider.Views.Update(axView, new ModelSaveInfo(modelInfo));
                            break;

                        case nameof(AxMap):
                            AxMap axMap = designMetaModel.GetMap(vSProjectFileNode.FileName);

                            MapFunc.GenLabelsForMap(StartRunLabel.LabelEditor, axMap);

                            designMetaModel.CurrentMetadataProvider.Maps.Update(axMap, new ModelSaveInfo(modelInfo));
                            break;

                        case nameof(AxDataEntityView):
                            AxDataEntityView axDataEntityView = designMetaModel.GetDataEntityView(vSProjectFileNode.FileName);

                            DataFunc.GenLabelsForDataEntity(StartRunLabel.LabelEditor, axDataEntityView, modelInfo.Name);

                            designMetaModel.CurrentMetadataProvider.DataEntityViews.Update(axDataEntityView, new ModelSaveInfo(modelInfo));
                            break;

                        case nameof(AxMenuItemAction):
                            AxMenuItemAction axMenuItemAction = designMetaModel.GetMenuItemAction(vSProjectFileNode.FileName);

                            MenuFunc.GenLabelsForActionMenuItem(StartRunLabel.LabelEditor, axMenuItemAction);

                            designMetaModel.CurrentMetadataProvider.MenuItemActions.Update(axMenuItemAction, new ModelSaveInfo(modelInfo));
                            break;

                        case nameof(AxMenuItemDisplay):
                            AxMenuItemDisplay axMenuItemDisplay = designMetaModel.GetMenuItemDisplay(vSProjectFileNode.FileName);

                            MenuFunc.GenLabelsForDisplayMenuItem(StartRunLabel.LabelEditor, axMenuItemDisplay);

                            designMetaModel.CurrentMetadataProvider.MenuItemDisplays.Update(axMenuItemDisplay, new ModelSaveInfo(modelInfo));
                            break;

                        case nameof(AxMenuItemOutput):
                            AxMenuItemOutput axMenuItemOutput = designMetaModel.GetMenuItemOutput(vSProjectFileNode.FileName);

                            MenuFunc.GenLabelsForOutputMenuItem(StartRunLabel.LabelEditor, axMenuItemOutput);

                            designMetaModel.CurrentMetadataProvider.MenuItemOutputs.Update(axMenuItemOutput, new ModelSaveInfo(modelInfo));
                            break;

                        case nameof(AxMenu):
                            AxMenu axMenu = designMetaModel.GetMenu(vSProjectFileNode.FileName);

                            MenuFunc.GenLabelsForMenu(StartRunLabel.LabelEditor, axMenu);

                            designMetaModel.CurrentMetadataProvider.Menus.Update(axMenu, new ModelSaveInfo(modelInfo));
                            break;

                        case nameof(AxTile):
                            AxTile axTile = designMetaModel.GetTile(vSProjectFileNode.FileName);

                            TileFunc.GenLabelsForTile(StartRunLabel.LabelEditor, axTile);

                            designMetaModel.CurrentMetadataProvider.Tiles.Update(axTile, new ModelSaveInfo(modelInfo));
                            break;

                        case nameof(AxSecurityPrivilege):
                            AxSecurityPrivilege axSecurityPrivilege = designMetaModel.GetSecurityPrivilege(vSProjectFileNode.FileName);

                            SecurityFunc.GenLabelsForPrivilege(StartRunLabel.LabelEditor, axSecurityPrivilege);

                            designMetaModel.CurrentMetadataProvider.SecurityPrivileges.Update(axSecurityPrivilege, new ModelSaveInfo(modelInfo));
                            break;

                        case nameof(AxSecurityDuty):
                            AxSecurityDuty axSecurityDuty = designMetaModel.GetSecurityDuty(vSProjectFileNode.FileName);

                            SecurityFunc.GenLabelsForDuty(StartRunLabel.LabelEditor, axSecurityDuty);

                            designMetaModel.CurrentMetadataProvider.SecurityDuties.Update(axSecurityDuty, new ModelSaveInfo(modelInfo));
                            break;

                        case nameof(AxSecurityRole):
                            AxSecurityRole axSecurityRole = designMetaModel.GetSecurityRole(vSProjectFileNode.FileName);

                            SecurityFunc.GenLabelsForRole(StartRunLabel.LabelEditor, axSecurityRole);

                            designMetaModel.CurrentMetadataProvider.SecurityRoles.Update(axSecurityRole, new ModelSaveInfo(modelInfo));
                            break;

                        case nameof(AxSecurityPolicy):
                            AxSecurityPolicy axSecurityPolicy = designMetaModel.GetSecurityPolicy(vSProjectFileNode.FileName);

                            SecurityFunc.GenLabelsForPolicy(StartRunLabel.LabelEditor, axSecurityPolicy);

                            designMetaModel.CurrentMetadataProvider.SecurityPolicies.Update(axSecurityPolicy, new ModelSaveInfo(modelInfo));
                            break;

                        #endregion

                        #region Extension

                        case nameof(AxEdtExtension):
                            AxEdtExtension axEdtExt = designMetaModel.CurrentMetadataProvider.EdtExtensions.Read(vSProjectFileNode.Name);

                            EdtFunc.GenLabelsForEdtExt(StartRunLabel.LabelEditor, axEdtExt, modelInfo.Name);

                            designMetaModel.CurrentMetadataProvider.EdtExtensions.Update(axEdtExt, new ModelSaveInfo(modelInfo));
                            break;

                        case nameof(AxTableExtension):
                            AxTableExtension axTableExtension = designMetaModel.GetTableExtension(vSProjectFileNode.FileName);

                            TableFunc.GenLabelsForTableExt(StartRunLabel.LabelEditor, axTableExtension, modelInfo.Name);

                            designMetaModel.CurrentMetadataProvider.TableExtensions.Update(axTableExtension, new ModelSaveInfo(modelInfo));
                            break;

                        case nameof(AxFormExtension):
                            AxFormExtension axFormExtension = designMetaModel.GetFormExtension(vSProjectFileNode.FileName);

                            FormFunc.GenLabelsForFormExt(StartRunLabel.LabelEditor, axFormExtension, modelInfo.Name);

                            designMetaModel.CurrentMetadataProvider.FormExtensions.Update(axFormExtension, new ModelSaveInfo(modelInfo));
                            break;

                        case nameof(AxEnumExtension):
                            AxEnumExtension axEnumExtension = designMetaModel.GetEnumExtension(vSProjectFileNode.FileName);

                            EdtFunc.GenLabelsForEnumExt(StartRunLabel.LabelEditor, axEnumExtension, modelInfo.Name);

                            designMetaModel.CurrentMetadataProvider.EnumExtensions.Update(axEnumExtension, new ModelSaveInfo(modelInfo));
                            break;

                        case nameof(AxViewExtension):
                            AxViewExtension axViewExtension = designMetaModel.GetViewExtension(vSProjectFileNode.FileName);

                            ViewFunc.GenLabelsForViewExt(StartRunLabel.LabelEditor, axViewExtension, modelInfo.Name);

                            designMetaModel.CurrentMetadataProvider.ViewExtensions.Update(axViewExtension, new ModelSaveInfo(modelInfo));
                            break;

                        case nameof(AxDataEntityViewExtension):
                            AxDataEntityViewExtension axDataEntityViewExtension = designMetaModel.GetDataEntityViewExtension(vSProjectFileNode.FileName);

                            DataFunc.GenLabelsForDataEntityExt(StartRunLabel.LabelEditor, axDataEntityViewExtension, modelInfo.Name);

                            designMetaModel.CurrentMetadataProvider.DataEntityViewExtensions.Update(axDataEntityViewExtension, new ModelSaveInfo(modelInfo));
                            break;

                        case nameof(AxMenuItemActionExtension):
                            AxMenuItemActionExtension axMenuItemActionExtension = designMetaModel.CurrentMetadataProvider.MenuItemActionExtensions.Read(vSProjectFileNode.Name);

                            MenuFunc.GenLabelsForActionMenuItemExt(StartRunLabel.LabelEditor, axMenuItemActionExtension, modelInfo.Name);

                            designMetaModel.CurrentMetadataProvider.MenuItemActionExtensions.Update(axMenuItemActionExtension, new ModelSaveInfo(modelInfo));
                            break;

                        case nameof(AxMenuItemDisplayExtension):
                            AxMenuItemDisplayExtension axMenuItemDisplayExtension = designMetaModel.CurrentMetadataProvider.MenuItemDisplayExtensions.Read(vSProjectFileNode.Name);

                            MenuFunc.GenLabelsForDisplayMenuItemExt(StartRunLabel.LabelEditor, axMenuItemDisplayExtension, modelInfo.Name);

                            designMetaModel.CurrentMetadataProvider.MenuItemDisplayExtensions.Update(axMenuItemDisplayExtension, new ModelSaveInfo(modelInfo));
                            break;

                        case nameof(AxMenuItemOutputExtension):
                            AxMenuItemOutputExtension axMenuItemOutputExtension = designMetaModel.CurrentMetadataProvider.MenuItemOutputExtensions.Read(vSProjectFileNode.Name);

                            MenuFunc.GenLabelsForOutputMenuItemExt(StartRunLabel.LabelEditor, axMenuItemOutputExtension, modelInfo.Name);

                            designMetaModel.CurrentMetadataProvider.MenuItemOutputExtensions.Update(axMenuItemOutputExtension, new ModelSaveInfo(modelInfo));
                            break;

                        case nameof(AxMenuExtension):
                            AxMenuExtension axMenuExtension = designMetaModel.GetMenuExtension(vSProjectFileNode.FileName);

                            MenuFunc.GenLabelsForMenuExt(StartRunLabel.LabelEditor, axMenuExtension);

                            designMetaModel.CurrentMetadataProvider.MenuExtensions.Update(axMenuExtension, new ModelSaveInfo(modelInfo));
                            break;

                            #endregion
                    }
                }
            }
        }
    }
}
