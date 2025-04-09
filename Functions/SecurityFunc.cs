using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Dynamics.AX.Metadata.MetaModel;

namespace Functions_for_Dynamics_Operations
{
    public enum AccessLevel
    {
        [Description("No Access")]
        NoAccess,
        [Description("Read")]
        Read,
        [Description("Update")]
        Update,
        [Description("Create")]
        Create,
        [Description("Correct")]
        Correct,
        [Description("Delete")]
        Delete
    }

    public class SecurityFunc : ClassFunc
    {
        public static void GenLabelsForPrivilege(LabelEditorControl labelEditor, AxSecurityPrivilege axPrivilege)
        {
            string labelId = $"@{labelEditor.LabelFileCollectionSelected.Id}:";

            if (IsNotLabelOrEmpty(axPrivilege.Label, labelId))
            {
                axPrivilege.Label = labelEditor.AddLabelFromTextInCode($"{axPrivilege.Name}~{axPrivilege.Label}", "", true);
            }

            if (IsNotLabelOrEmpty(axPrivilege.Description, labelId))
            {
                axPrivilege.Description = labelEditor.AddLabelFromTextInCode($"{axPrivilege.Name}Help~{axPrivilege.Description}", "", true);
            }
        }

        public static void GenLabelsForDuty(LabelEditorControl labelEditor, AxSecurityDuty axSecurityDuty)
        {
            string labelId = $"@{labelEditor.LabelFileCollectionSelected.Id}:";

            if (IsNotLabelOrEmpty(axSecurityDuty.Label, labelId))
            {
                axSecurityDuty.Label = labelEditor.AddLabelFromTextInCode($"{axSecurityDuty.Name}~{axSecurityDuty.Label}", "", true);
            }

            if (IsNotLabelOrEmpty(axSecurityDuty.Description, labelId))
            {
                axSecurityDuty.Description = labelEditor.AddLabelFromTextInCode($"{axSecurityDuty.Name}Help~{axSecurityDuty.Description}", "", true);
            }
        }

        public static void GenLabelsForDutyExt(LabelEditorControl labelEditor, AxSecurityDutyExtension axSecurityDuty)
        {
            string labelId = $"@{labelEditor.LabelFileCollectionSelected.Id}:";

            foreach (var prop in axSecurityDuty.PropertyModifications)
            {
                switch (prop.Name)
                {
                    case "Label":
                        if (IsNotLabelOrEmpty(prop.Value, labelId))
                        {
                            prop.Value = labelEditor.AddLabelFromTextInCode($"{axSecurityDuty.Name.Substring(0, axSecurityDuty.Name.IndexOf("."))}{prop.Name}~{prop.Value}", "", true);
                        }
                        break;
                    case "HelpText":
                        if (IsNotLabelOrEmpty(prop.Value, labelId))
                        {
                            prop.Value = labelEditor.AddLabelFromTextInCode($"{axSecurityDuty.Name.Substring(0, axSecurityDuty.Name.IndexOf("."))}{prop.Name}~{prop.Value}", "", true);
                        }
                        break;
                }
            }
        }

        public static void GenLabelsForRole(LabelEditorControl labelEditor, AxSecurityRole axSecurityRole)
        {
            string labelId = $"@{labelEditor.LabelFileCollectionSelected.Id}:";

            if (IsNotLabelOrEmpty(axSecurityRole.Label, labelId))
            {
                axSecurityRole.Label = labelEditor.AddLabelFromTextInCode($"{axSecurityRole.Name}~{axSecurityRole.Label}", "", true);
            }

            if (IsNotLabelOrEmpty(axSecurityRole.Description, labelId))
            {
                axSecurityRole.Description = labelEditor.AddLabelFromTextInCode($"{axSecurityRole.Name}Help~{axSecurityRole.Description}", "", true);
            }
        }

        public static void GenLabelsForRoleExt(LabelEditorControl labelEditor, AxSecurityRoleExtension axSecurityRole)
        {
            string labelId = $"@{labelEditor.LabelFileCollectionSelected.Id}:";

            foreach (var prop in axSecurityRole.PropertyModifications)
            {
                switch (prop.Name)
                {
                    case "Label":
                        if (IsNotLabelOrEmpty(prop.Value, labelId))
                        {
                            prop.Value = labelEditor.AddLabelFromTextInCode($"{axSecurityRole.Name.Substring(0, axSecurityRole.Name.IndexOf("."))}{prop.Name}~{prop.Value}", "", true);
                        }
                        break;
                    case "HelpText":
                        if (IsNotLabelOrEmpty(prop.Value, labelId))
                        {
                            prop.Value = labelEditor.AddLabelFromTextInCode($"{axSecurityRole.Name.Substring(0, axSecurityRole.Name.IndexOf("."))}{prop.Name}~{prop.Value}", "", true);
                        }
                        break;
                }
            }
        }

        public static void GenLabelsForPolicy(LabelEditorControl labelEditor, AxSecurityPolicy axSecurityPolicy)
        {
            string labelId = $"@{labelEditor.LabelFileCollectionSelected.Id}:";

            if (IsNotLabelOrEmpty(axSecurityPolicy.Label, labelId))
            {
                axSecurityPolicy.Label = labelEditor.AddLabelFromTextInCode($"{axSecurityPolicy.Name}~{axSecurityPolicy.Label}", "", true);
            }

            if (IsNotLabelOrEmpty(axSecurityPolicy.HelpText, labelId))
            {
                axSecurityPolicy.HelpText = labelEditor.AddLabelFromTextInCode($"{axSecurityPolicy.Name}Help~{axSecurityPolicy.HelpText}", "", true);
            }
        }

        public static List<string> GetAccessLevels()
        {
            return Enum.GetNames(typeof(AccessLevel)).ToList();
        }

        public static Microsoft.Dynamics.AX.Metadata.Core.MetaModel.AccessGrant MarshalToAccessGrant(AccessLevel level)
        {
            switch (level)
            {
                case AccessLevel.NoAccess:
                    return Microsoft.Dynamics.AX.Metadata.Core.MetaModel.AccessGrant.ConstructDenyAll();
                case AccessLevel.Read:
                    return Microsoft.Dynamics.AX.Metadata.Core.MetaModel.AccessGrant.ConstructGrantRead();
                case AccessLevel.Update:
                    return Microsoft.Dynamics.AX.Metadata.Core.MetaModel.AccessGrant.ConstructGrantUpdate();
                case AccessLevel.Create:
                    return Microsoft.Dynamics.AX.Metadata.Core.MetaModel.AccessGrant.ConstructGrantCreate();
                case AccessLevel.Correct:
                    return Microsoft.Dynamics.AX.Metadata.Core.MetaModel.AccessGrant.ConstructGrantCorrect();
                case AccessLevel.Delete:
                    return Microsoft.Dynamics.AX.Metadata.Core.MetaModel.AccessGrant.ConstructGrantDelete();
                default:
                    return null;
            }
        }

        /*
        public void Run()
        {
            EnvDTE80.DTE2 dte = Package.GetGlobalService(typeof(SDTE)) as EnvDTE80.DTE2;

            if (dte.SelectedItems.Count > 0)
            {   // Only support a single item
                // SelectedItem item = dte.SelectedItems.Item(1);

                PrivilegesForm privSelect = new PrivilegesForm();

                Microsoft.Dynamics.AX.Metadata.MetaModel.AxSecurityPrivilege privilege = null;

                privSelect.SetPrivileges(VStudioUtils.GetPrivileges());
                privSelect.ShowDialog();

                if (privSelect.closedOkay)
                    privilege = privSelect.GetPrivilegeSelected().Length > 1 ? DynaxUtils.GetAXSecurityPrivilege(privSelect.GetPrivilegeSelected()) : null;

                Boolean updated = false;

                if (privilege != null)
                {
                    IEnumerator menuitems = VStudioUtils.FindMenuItemsInProject().GetEnumerator();
                    while (menuitems.MoveNext())
                    {
                        DynType menuitem = (DynType)menuitems.Current;

                        Microsoft.Dynamics.AX.Metadata.MetaModel.AxSecurityEntryPointReference entryPoint = new Microsoft.Dynamics.AX.Metadata.MetaModel.AxSecurityEntryPointReference
                        {
                            Grant = MarshalToAccessGrant(privSelect.GetAccessLevelSelected()),
                            ObjectType = DynaxUtils.GetEntryPointType(menuitem.TypeObj),
                            ObjectName = menuitem.Name,
                            Name = menuitem.Name
                        };

                        Boolean alreadyAdded = false;

                        foreach (var entryPointFnd in privilege.EntryPoints)
                        {
                            if (entryPointFnd.Name.ToLower() == entryPoint.Name.ToLower())
                                alreadyAdded = true;
                        }

                        if (!alreadyAdded)
                        {
                            privilege.EntryPoints.Add(entryPoint);
                            updated = true;
                        }
                    }

                    if (updated)
                    {   // update the privilege
                        Microsoft.Dynamics.Framework.Tools.ProjectSystem.VSProjectFileNode vsprojFile = VStudioUtils.GetVSProjectNodefile(VStudioUtils.GetAOVSProjectNode(), privilege.Name);
                        if (vsprojFile != null)
                        { 
                            VStudioUtils.GetDesignMetaModelService().CurrentMetadataProvider.SecurityPrivileges.Update(privilege, new Microsoft.Dynamics.AX.Metadata.MetaModel.ModelSaveInfo(vsprojFile.MetadataReference.ModelReference));
                            // Save the changes
                            Project proj = VStudioUtils.GetActiveProject(dte.DTE);
                            proj.Save();
                        }
                    }
                }
            }
        }
        */
    }
}
