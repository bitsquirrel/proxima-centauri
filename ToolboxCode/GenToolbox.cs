using System;
using Functions_for_Dynamics_Operations.Forms;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.Dynamics.Framework.Tools.ProjectSystem;

namespace Functions_for_Dynamics_Operations.ToolboxCode
{
    internal class GenToolbox : ClassFunc
    {
        internal VSProjectNode VSProjectNode;
        internal AxClass axClass;
        internal string Prefix;

        internal void GenerateToolbox()
        {
            VSProjectNode = VStudioUtils.GetFirstActiveVSProjectNode();

            PrefixForm prefixForm = GetPrefix();

            if (prefixForm.Ok == false || string.IsNullOrEmpty(prefixForm.Prefix))
            {
                return;
            }
            // We need the enums to be created first as these are used in the toolbox code (creates and adds them to the project open at the time)
            new EnumsCode(VSProjectNode, prefixForm.Prefix).CreateEnums();
            // Generate the toolbox code
            new UtilsCode(prefixForm.Prefix).CreateClass();
            new SysOpBaseCode(prefixForm.Prefix).CreateClass();
            new ExceptionCode(prefixForm.Prefix).CreateClass();
            new DimensionCode(prefixForm.Prefix).CreateClass();
            new SysOpContractCode(VSProjectNode, prefixForm.Prefix).CreateClass();
        }

        internal PrefixForm GetPrefix()
        {
            // This will allow the user to select the form design required
            PrefixForm prefixForm = new PrefixForm
            {
                StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
            };

            prefixForm.ShowDialog();
            // Return and deal with the result
            return prefixForm;

        }

        internal virtual string ClassName()
        {
            // Override this to return the class name
            return "";
        }

        internal virtual string GetClassCode()
        {
            return "";
        }

        internal void CreateClass()
        {
            axClass = VStudioUtils.GetDesignMetaModelService().GetClass($"{Prefix}{ClassName()}");
            if (axClass == null)
            {
                axClass = new AxClass
                {
                    Name = $"{Prefix}{ClassName()}",
                    Declaration = GetClassCode()
                };

                CreateClassMethods();

                VSProjectNode vSProjectNode = VStudioUtils.GetFirstActiveVSProjectNode();
                IModelReference modelReference = vSProjectNode.GetProjectsModelInfo();
                // Log the creation of the class so user can see it
                VStudioUtils.LogToGenOutput($"Creating class: {axClass.Name}{Environment.NewLine}");
                // Create the class in the model
                vSProjectNode.DesignMetaModelService.CurrentMetadataProvider.Classes.Create(axClass, new ModelSaveInfo(modelReference));

                AddClassToProject(axClass, false);
            }
            else
            {
                VStudioUtils.LogToGenOutput($"Class: {axClass.Name}, already exists{Environment.NewLine}");
            }
        }
    }
}
