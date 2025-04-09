using System.Windows.Forms;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.Dynamics.Framework.Tools.ProjectSystem;

namespace Functions_for_Dynamics_Operations
{
    public class DataFunc : ClassFunc
    {
        public static void GenLabelsForDataEntity(LabelEditorControl labelEditor, AxDataEntityView axDataEntityView, string model)
        {
            string labelId = $"@{labelEditor.LabelFileCollectionSelected.Id}:";

            if (IsNotLabelOrEmpty(axDataEntityView.Label, labelId))
            {
                axDataEntityView.Label = labelEditor.AddLabelFromTextInCode($"{axDataEntityView.Name}~{axDataEntityView.Label}", "", true);
            }

            if (IsNotLabelOrEmpty(axDataEntityView.DeveloperDocumentation, labelId))
            {
                axDataEntityView.DeveloperDocumentation = labelEditor.AddLabelFromTextInCode($"{axDataEntityView.Name}DeveloperDocument~{axDataEntityView.DeveloperDocumentation}", "{Locked}", true);
            }

            foreach (var field in axDataEntityView.Fields)
            {
                if (IsNotLabelOrEmpty(field.Label, labelId))
                {
                    field.Label = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, axDataEntityView.Name + field.Name)}~{field.Label}", "", true);
                }

                if (IsNotLabelOrEmpty(field.HelpText, labelId))
                {
                    field.HelpText = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, axDataEntityView.Name + field.Name)}{field.Name}Help~{field.HelpText}", "", true);
                }
            }

            foreach (var fieldGroup in axDataEntityView.FieldGroups)
            {
                if (IsNotLabelOrEmpty(fieldGroup.Label, labelId))
                {
                    fieldGroup.Label = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, axDataEntityView.Name + fieldGroup.Name)}~{fieldGroup.Label}", "", true);
                }
            }
        }

        public static void GenLabelsForDataEntityExt(LabelEditorControl labelEditor, AxDataEntityViewExtension axDataEntityView, string model)
        {
            string labelId = $"@{labelEditor.LabelFileCollectionSelected.Id}:";

            foreach (var field in axDataEntityView.Fields)
            {
                if (IsNotLabelOrEmpty(field.Label, labelId))
                {
                    field.Label = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, axDataEntityView.Name + field.Name)}~{field.Label}", "", true);
                }

                if (IsNotLabelOrEmpty(field.HelpText, labelId))
                {
                    field.HelpText = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, axDataEntityView.Name + field.Name)}{field.Name}Help~{field.HelpText}", "", true);
                }
            }

            foreach (var fieldGroup in axDataEntityView.FieldGroups)
            {
                if (IsNotLabelOrEmpty(fieldGroup.Label, labelId))
                {
                    fieldGroup.Label = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, axDataEntityView.Name + fieldGroup.Name)}~{fieldGroup.Label}", "", true);
                }
            }

            foreach (AxExtensionModification fieldMod in axDataEntityView.FieldModifications)
            {
                foreach (AxPropertyModification propertyMod in fieldMod.PropertyModifications)
                {
                    switch (propertyMod.Name)
                    {
                        case "Label":
                            if (IsNotLabelOrEmpty(propertyMod.Value, labelId))
                            {
                                propertyMod.Value = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, axDataEntityView.Name + fieldMod.Name)}~{propertyMod.Value}", "", true);
                            }
                            break;
                        case "HelpText":
                            if (IsNotLabelOrEmpty(propertyMod.Value, labelId))
                            {
                                propertyMod.Value = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, axDataEntityView.Name + fieldMod.Name)}Help~{propertyMod.Value}", "", true);
                            }
                            break;
                    }
                }
            }
        }

        public static new void ApplyComments(string dataEntityName)
        {
            VSProjectNode vSProjectNode = VStudioUtils.GetSelectedProjectOrFirstActiveProject();
            AxDataEntityView dataEntity = vSProjectNode.DesignMetaModelService.GetDataEntityView(dataEntityName);
            if (dataEntity != null)
            {
                IModelReference modelReference = vSProjectNode.GetProjectsModelInfo();

                if (!dataEntity.SourceCode.Declaration.Contains("///"))
                {
                    dataEntity.SourceCode.Declaration = ClassFunc.CommentDeclaration(dataEntity.Name, dataEntity.GetType().Name, modelReference.Name, dataEntity.SourceCode.Declaration);
                }

                MethodFunc.CommentMethods(dataEntity.Methods);

                vSProjectNode.DesignMetaModelService.CurrentMetadataProvider.DataEntityViews.Update(dataEntity, new ModelSaveInfo(modelReference));
            }
        }
    }
}
