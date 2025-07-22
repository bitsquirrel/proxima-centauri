using EnvDTE;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.Dynamics.Framework.Tools.ProjectSystem;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Functions_for_Dynamics_Operations.Functions
{
    internal class MenuItemsFunc : ClassFunc
    {
        public AxMenuItemDisplay CreateMenuItemDisplay(string name, string label, string helpText = "")
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            AxMenuItemDisplay axMenuItemDisplay = new AxMenuItemDisplay
            {
                Name = name,
                Object = name,
                Label = label,
                HelpText = helpText,
                ObjectType = Microsoft.Dynamics.AX.Metadata.Core.MetaModel.MenuItemObjectType.Form
            };

            VSProjectNode vSProjectNode = VStudioUtils.GetSelectedProjectOrFirstActiveProject();

            vSProjectNode.DesignMetaModelService.CurrentMetadataProvider.MenuItemDisplays.Create(axMenuItemDisplay, new ModelSaveInfo(vSProjectNode.GetProjectsModelInfo()));

            SaveToProject(axMenuItemDisplay.Name, axMenuItemDisplay.GetType(), false);

            return axMenuItemDisplay;
        }

        public AxMenuItemAction CreateMenuItemAction(AxClass _class)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            AxMenuItemAction axMenuItem = new AxMenuItemAction()
            {
                Name = _class.Name,
                HelpText = $"{_class.Name} menu item help",
                ObjectType = Microsoft.Dynamics.AX.Metadata.Core.MetaModel.MenuItemObjectType.Class
            };

            string label = string.Empty;

            if (_class.Extends != null && _class.Extends != "" && _class.Extends.ToLower() == "sysoperationservicebase")
            {
                axMenuItem.Name = _class.Name;
                axMenuItem.EnumParameter = "Synchronous";
                axMenuItem.Label = $"{_class.Name} menu item";
                axMenuItem.Parameters = $"{_class.Name}.runService";
                axMenuItem.Object = "SysOperationServiceController";
                axMenuItem.EnumTypeParameter = "SysOperationExecutionMode";
            }
            else
            {
                axMenuItem.Object = _class.Name;
            }

            axMenuItem.Label = label;

            VSProjectNode vSProjectNode = VStudioUtils.GetSelectedProjectOrFirstActiveProject();

            vSProjectNode.DesignMetaModelService.CurrentMetadataProvider.MenuItemActions.Create(axMenuItem, new ModelSaveInfo(vSProjectNode.GetProjectsModelInfo()));

            SaveToProject(axMenuItem.Name, axMenuItem.GetType(), false);

            return axMenuItem;
        }
    }
}
