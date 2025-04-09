using Microsoft.Dynamics.AX.Metadata.Core.MetaModel;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.Dynamics.Framework.Tools.ProjectSystem;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Core;
using Microsoft.VisualStudio.Shell;

namespace Functions_for_Dynamics_Operations.Functions
{
    internal class PrivilegeFunc : ClassFunc
    {
        public void CheckCreateActionMenuItemAndPrivileges(IDesignMetaModelService designMetaModel, VSProjectFileNode vSProjectFileNode, EntryPointType pointType, string label)
        {
            AxMenuItemAction menuItem = designMetaModel.GetMenuItemAction(vSProjectFileNode.FileName);
            if (menuItem == null)
            {
                AxClass axClass = designMetaModel.GetClass(vSProjectFileNode.FileName);
                if (!axClass.IsNull())
                {
                    new MenuItemsFunc().CreateMenuItemAction(axClass);

                    CreateMaintainViewPrivileges(designMetaModel, vSProjectFileNode, pointType, label);
                }
            }
        }

        public void CheckCreateDisplayMenuItemAndPrivileges(IDesignMetaModelService designMetaModel, VSProjectFileNode vSProjectFileNode, EntryPointType pointType, string label)
        {
            AxMenuItemDisplay menuItem = designMetaModel.GetMenuItemDisplay(vSProjectFileNode.FileName);
            if (menuItem == null)
                new MenuItemsFunc().CreateMenuItemDisplay(vSProjectFileNode.FileName, label);

            CreateMaintainViewPrivileges(designMetaModel, vSProjectFileNode, pointType, label);
        }

        public void CreateMaintainViewPrivileges(IDesignMetaModelService designMetaModel, VSProjectFileNode vSProjectFileNode, EntryPointType pointType, string label)
        {
            CreatePrivilegeMaintain(designMetaModel, vSProjectFileNode, pointType, label);

            CreatePrivilegeView(designMetaModel, vSProjectFileNode, pointType, label);
        }

        public AxSecurityPrivilege CreatePrivilegeMaintain(IDesignMetaModelService designMetaModel, VSProjectFileNode vSProjectFileNode, EntryPointType pointType, string label)
        {
            AxSecurityPrivilege securityPrivilege = designMetaModel.GetSecurityPrivilege($"{vSProjectFileNode.FileName}Maintain");
            if (securityPrivilege == null)
            {
                string labelMod = label.Contains("@") ? label : $"{label} maintain";

                CreateEntryPointPrivilegeFromName($"{vSProjectFileNode.FileName}Maintain", vSProjectFileNode.FileName, AccessGrant.ConstructGrantDelete(), pointType, labelMod, "Maintain privilege");
            }

            return securityPrivilege;
        }

        public AxSecurityPrivilege CreatePrivilegeView(IDesignMetaModelService designMetaModel, VSProjectFileNode vSProjectFileNode, EntryPointType pointType, string label)
        {
            AxSecurityPrivilege securityPrivilege = designMetaModel.GetSecurityPrivilege($"{vSProjectFileNode.FileName}View");
            if (securityPrivilege == null)
            {
                string labelMod = label.Contains("@") ? label : $"{label} view";

                CreateEntryPointPrivilegeFromName($"{vSProjectFileNode.FileName}View", vSProjectFileNode.FileName, AccessGrant.ConstructGrantRead(), pointType, labelMod, "View privilege");
            }

            return securityPrivilege;
        }

        public AxSecurityPrivilege CreateEntryPointPrivilegeFromName(string nameOfPrivilege, string objectToGrantAccessTo, AccessGrant accessToGrantOnObject, EntryPointType pointType, string label, string description = "")
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            AxSecurityPrivilege privilege = new AxSecurityPrivilege
            {
                Label = label,
                Name = nameOfPrivilege,
                Description = description
            };

            AxSecurityEntryPointReference entryPointReference = new AxSecurityEntryPointReference()
            {
                ObjectName = objectToGrantAccessTo,
                Grant = accessToGrantOnObject,
                ObjectType = pointType,
                Name = nameOfPrivilege
            };

            switch (pointType)
            {
                case EntryPointType.MenuItemDisplay:
                    entryPointReference.Forms.Add(new AxSecurityEntryPointReferenceForm() { Name = objectToGrantAccessTo });
                    break;
                case EntryPointType.MenuItemOutput:
                    // entryPointReference.Forms.Add(new AxSecurity () { Name = objectToGrantAccessTo });
                    break;
            }

            privilege.EntryPoints.Add(entryPointReference);

            VSProjectNode vSProjectNode = VStudioUtils.GetSelectedProjectOrFirstActiveProject();
            vSProjectNode.DesignMetaModelService.CurrentMetadataProvider.SecurityPrivileges.Create(privilege, new ModelSaveInfo(vSProjectNode.GetProjectsModelInfo()));

            SaveToProject(privilege.Name, privilege.GetType(), false);

            return privilege;
        }

        public AxSecurityPrivilege CreateDataEntityPrivilegeFromName(string nameOfPrivilege, string objectToGrantAccessTo, AccessGrant accessToGrantOnObject, string label, string description = "")
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            AxSecurityPrivilege privilege = new AxSecurityPrivilege
            {
                Label = label,
                Name = nameOfPrivilege,
                Description = description
            };

            AxSecurityDataEntityPermission dataEntityPermission = new AxSecurityDataEntityPermission()
            {
                IntegrationMode = IntegrationMode.All,
                Grant = accessToGrantOnObject,
                Name = objectToGrantAccessTo
            };

            privilege.DataEntityPermissions.Add(dataEntityPermission);

            VSProjectNode vSProjectNode = VStudioUtils.GetSelectedProjectOrFirstActiveProject();
            vSProjectNode.DesignMetaModelService.CurrentMetadataProvider.SecurityPrivileges.Create(privilege, new ModelSaveInfo(vSProjectNode.GetProjectsModelInfo()));

            SaveToProject(privilege.Name, privilege.GetType(), false);

            return privilege;
        }
    }
}
