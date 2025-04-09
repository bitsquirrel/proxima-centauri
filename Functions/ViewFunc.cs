using System.Windows.Forms;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.Dynamics.Framework.Tools.ProjectSystem;

namespace Functions_for_Dynamics_Operations
{
    public class ViewFunc : TableFunc
    {
        public static void GenLabelsForView(LabelEditorControl labelEditor, AxView axView)
        {
            string labelId = $"@{labelEditor.LabelFileCollectionSelected.Id}:";

            if (IsNotLabelOrEmpty(axView.Label, labelId))
            {
                axView.Label = labelEditor.AddLabelFromTextInCode($"{axView.Name}~{axView.Label}", "", true);
            }

            if (IsNotLabelOrEmpty(axView.DeveloperDocumentation, labelId))
            {
                axView.DeveloperDocumentation = labelEditor.AddLabelFromTextInCode($"{axView.Name}DeveloperDocument~{axView.DeveloperDocumentation}", "{Locked}", true);
            }

            foreach (var field in axView.Fields)
            {
                if (IsNotLabelOrEmpty(field.Label, labelId))
                {
                    field.Label = labelEditor.AddLabelFromTextInCode($"{axView.Name}{field.Name}~{field.Label}", "", true);
                }

                if (IsNotLabelOrEmpty(field.HelpText, labelId))
                {
                    field.HelpText = labelEditor.AddLabelFromTextInCode($"{axView.Name}{field.Name}Help~{field.HelpText}", "", true);
                }
            }

            foreach (var fieldGroup in axView.FieldGroups)
            {
                if (IsNotLabelOrEmpty(fieldGroup.Label, labelId))
                {
                    fieldGroup.Label = labelEditor.AddLabelFromTextInCode($"{axView.Name}{fieldGroup.Name}~{fieldGroup.Label}", "", true);
                }
            }
        }

        public static void GenLabelsForViewExt(LabelEditorControl labelEditor, AxViewExtension axView, string model)
        {
            string labelId = $"@{labelEditor.LabelFileCollectionSelected.Id}:";

            foreach (var field in axView.Fields)
            {
                if (IsNotLabelOrEmpty(field.Label, labelId))
                {
                    field.Label = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, axView.Name + field.Name)}~{field.Label}", "", true);
                }

                if (IsNotLabelOrEmpty(field.HelpText, labelId))
                {
                    field.HelpText = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, axView.Name + field.Name)}Help~{field.HelpText}", "", true);
                }
            }

            foreach (var fieldGroup in axView.FieldGroups)
            {
                if (IsNotLabelOrEmpty(fieldGroup.Label, labelId))
                {
                    fieldGroup.Label = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, axView.Name + fieldGroup.Name)}~{fieldGroup.Label}", "", true);
                }
            }

            foreach (AxExtensionModification fieldMod in axView.FieldModifications)
            {
                foreach (AxPropertyModification propertyMod in fieldMod.PropertyModifications)
                {
                    switch (propertyMod.Name)
                    {
                        case "Label":
                            if (IsNotLabelOrEmpty(propertyMod.Value, labelId))
                            {
                                propertyMod.Value = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, axView.Name + fieldMod.Name)}~{propertyMod.Value}", "", true);
                            }
                            break;
                        case "HelpText":
                            if (IsNotLabelOrEmpty(propertyMod.Value, labelId))
                            {
                                propertyMod.Value = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, axView.Name + fieldMod.Name)}Help~{propertyMod.Value}", "", true);
                            }
                            break;
                    }
                }
            }
        }

        public static new void ApplyComments(string viewName)
        {
            VSProjectNode vSProjectNode = VStudioUtils.GetSelectedProjectOrFirstActiveProject();
            AxView view = vSProjectNode.DesignMetaModelService.GetView(viewName);
            if (view != null)
            {
                IModelReference modelReference = vSProjectNode.GetProjectsModelInfo();

                if (!view.SourceCode.Declaration.Contains("///"))
                {
                    view.SourceCode.Declaration = CommentDeclaration(view.Name, view.GetType().Name, modelReference.Name, view.SourceCode.Declaration);
                }

                MethodFunc.CommentMethods(view.Methods);

                vSProjectNode.DesignMetaModelService.CurrentMetadataProvider.Views.Update(view, new ModelSaveInfo(modelReference));
            }
        }

        /*
        public AxClass CreateCocView(string viewName)
        {
            VSProjectNode vSProjectNode = VStudioUtils.GetSelectdProjectOrFirstActiveProject();
            AxView viewFrom = vSProjectNode.DesignMetaModelService.GetView(viewName);
            if (viewFrom != null)
            {
                ClassCocForm classCocForm = new ClassCocForm(AxView.MetaClassId, GetAllowedMethodNames(viewFrom.Methods));
                classCocForm.ShowDialog();

                CheckSetPrefix();

                if (classCocForm.closedOk)
                {
                    string className = string.Format($"{viewName}_{VStudioCache.GetSettings(vSProjectNode.GetProjectsModelInfo().Module).Prefix}_{FOObjects.TypeGuidToName(AxView.MetaClassId)}_Extension");

                    if (vSProjectNode.DesignMetaModelService.GetClass(className) == null)
                    {
                        AxClass classcreated = BuildCocClassFromAxObject(new ObjectAndTypeDetails(viewFrom.Name, AxView.MetaClassId, viewFrom), className, classCocForm);

                        if (classcreated != null)
                        {
                            AddclassToProject(classcreated);
                        }
                    }
                    else
                    {
                        MessageBox.Show(string.Format("Coc class {0} already exists for view {1}", className, viewName), "Class already exists", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

            return null;
        }
        */
    }
}
