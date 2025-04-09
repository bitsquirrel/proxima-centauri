using System;
using System.Windows.Forms;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Core;
using Microsoft.Dynamics.Framework.Tools.ProjectSystem;
using Microsoft.VisualStudio.Shell;

namespace Functions_for_Dynamics_Operations
{
    public class TableFunc : ClassFunc
    {
        public static void GenLabelsForTable(LabelEditorControl labelEditor, AxTable axTable)
        {
            string labelId = $"@{labelEditor.LabelFileCollectionSelected.Id}:";

            if (IsNotLabelOrEmpty(axTable.Label, labelId))
            {
                axTable.Label = labelEditor.AddLabelFromTextInCode($"{axTable.Name}~{axTable.Label}", "", true);
            }

            if (IsNotLabelOrEmpty(axTable.DeveloperDocumentation, labelId))
            {
                axTable.DeveloperDocumentation = labelEditor.AddLabelFromTextInCode($"{axTable.Name}DeveloperDocument~{axTable.DeveloperDocumentation}", "{Locked}", true);
            }

            foreach (var field in axTable.Fields)
            {
                if (IsNotLabelOrEmpty(field.Label, labelId))
                {
                    field.Label = labelEditor.AddLabelFromTextInCode($"{axTable.Name}{field.Name}~{field.Label}", "", true);
                }

                if (IsNotLabelOrEmpty(field.HelpText, labelId))
                {
                    field.HelpText = labelEditor.AddLabelFromTextInCode($"{axTable.Name}{field.Name}Help~{field.HelpText}", "", true);
                }
            }

            foreach (var fieldGroup in axTable.FieldGroups)
            {
                if (IsNotLabelOrEmpty(fieldGroup.Label, labelId))
                {
                    fieldGroup.Label = labelEditor.AddLabelFromTextInCode($"{axTable.Name}{fieldGroup.Name}~{fieldGroup.Label}", "", true);
                }
            }
        }

        public static void GenLabelsForTableExt(LabelEditorControl labelEditor, AxTableExtension axTable, string model)
        {
            string labelId = $"@{labelEditor.LabelFileCollectionSelected.Id}:";

            foreach (var field in axTable.Fields)
            {
                if (IsNotLabelOrEmpty(field.Label, labelId))
                {
                    field.Label = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, axTable.Name + field.Name)}~{field.Label}", "", true);
                }

                if (IsNotLabelOrEmpty(field.HelpText, labelId))
                {
                    field.HelpText = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, axTable.Name + field.Name)}Help~{field.HelpText}", "", true);
                }
            }

            foreach (var fieldGroup in axTable.FieldGroups)
            {
                if (IsNotLabelOrEmpty(fieldGroup.Label, labelId))
                {
                    fieldGroup.Label = labelEditor.AddLabelFromTextInCode($"{axTable.Name.Substring(0, axTable.Name.IndexOf("."))}{fieldGroup.Name}~{fieldGroup.Label}", "", true);
                }
            }

            foreach (AxExtensionModification fieldMod in axTable.FieldModifications)
            {
                foreach (AxPropertyModification propertyMod in fieldMod.PropertyModifications)
                {
                    switch (propertyMod.Name)
                    {
                        case "Label":
                            if (IsNotLabelOrEmpty(propertyMod.Value, labelId))
                            {
                                propertyMod.Value = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, axTable.Name + fieldMod.Name)}~{propertyMod.Value}", "", true);
                            }
                            break;
                        case "HelpText":
                            if (IsNotLabelOrEmpty(propertyMod.Value, labelId))
                            {
                                propertyMod.Value = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, axTable.Name + fieldMod.Name)}Help~{propertyMod.Value}", "", true);
                            }
                            break;
                    }
                }
            }
        }

        public static new void ApplyComments(string tableName)
        {
            VSProjectNode vSProjectNode = VStudioUtils.GetSelectedProjectOrFirstActiveProject();
            AxTable table = vSProjectNode.DesignMetaModelService.GetTable(tableName);
            if (table != null)
            {
                IModelReference modelReference = vSProjectNode.GetProjectsModelInfo();

                if (!table.SourceCode.Declaration.Contains("///"))
                {
                    table.SourceCode.Declaration = CommentDeclaration(table.Name, table.GetType().Name, modelReference.Name, table.SourceCode.Declaration);
                }

                MethodFunc.CommentMethods(table.Methods);

                vSProjectNode.DesignMetaModelService.CurrentMetadataProvider.Tables.Update(table, new ModelSaveInfo(modelReference));
            }
        }

        public AxClass CreateClassFromTable(AxTable tableFrom, string newClassName)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            AxClass contract = new AxClass
            {
                Name = newClassName
            };

            string variables = "";

            foreach (var field in GetAllowedFields(tableFrom.Fields))
            {
                string dataType = "";

                if (field is AxTableFieldContainer)
                {
                    dataType = field.ExtendedDataType != "" ? field.ExtendedDataType : "container";
                }
                else if (field is AxTableFieldDate)
                {
                    dataType = field.ExtendedDataType != "" ? field.ExtendedDataType : "date";
                }
                else if (field is AxTableFieldEnum enumField)
                {
                    dataType = enumField.ExtendedDataType != "" ? enumField.ExtendedDataType : enumField.EnumType;
                }
                else if (field is AxTableFieldGuid)
                {
                    dataType = field.ExtendedDataType != "" ? field.ExtendedDataType : "sysguid";
                }
                else if (field is AxTableFieldInt)
                {
                    dataType = field.ExtendedDataType != "" ? field.ExtendedDataType : "int";
                }
                else if (field is AxTableFieldInt64)
                {
                    dataType = field.ExtendedDataType != "" ? field.ExtendedDataType : "int64";
                }
                else if (field is AxTableFieldReal)
                {
                    dataType = field.ExtendedDataType != "" ? field.ExtendedDataType : "amount";
                }
                else if (field is AxTableFieldString)
                {
                    dataType = field.ExtendedDataType != "" ? field.ExtendedDataType : "string50";
                }
                else if (field is AxTableFieldTime)
                {
                    dataType = field.ExtendedDataType != "" ? field.ExtendedDataType : "timehour24";
                }
                else if (field is AxTableFieldUtcDateTime)
                {
                    dataType = field.ExtendedDataType != "" ? field.ExtendedDataType : "utcdatetime";
                }

                AxMethod method = new AxMethod
                {
                    Name = field.Name,
                    Source = DynaxUtils.ParmMethodCode(field.Name, dataType)
                };

                contract.AddMethod(method);

                variables += string.Concat("    ", dataType, " ", field.Name, ";", Environment.NewLine);
            }

            // Update the class declaration
            contract.Declaration = DynaxUtils.ClassDeclarationCode(contract.Name, variables);

            VSProjectNode vSProjectNode = VStudioUtils.GetSelectedProjectOrFirstActiveProject();

            vSProjectNode.DesignMetaModelService.CurrentMetadataProvider.Classes.Create(contract, new ModelSaveInfo(vSProjectNode.GetProjectsModelInfo()));

            return contract;
        }

        public static Microsoft.Dynamics.AX.Metadata.Core.Collections.KeyedObjectCollection<AxTableField> GetAllowedFields(Microsoft.Dynamics.AX.Metadata.Core.Collections.KeyedObjectCollection<AxTableField> fields)
        {
            Microsoft.Dynamics.AX.Metadata.Core.Collections.KeyedObjectCollection<AxTableField> allowedFields = new Microsoft.Dynamics.AX.Metadata.Core.Collections.KeyedObjectCollection<AxTableField>();

            foreach (AxTableField field in fields)
            {
                if (field.IsObsolete != Microsoft.Dynamics.AX.Metadata.Core.MetaModel.NoYes.Yes && field.Name != "RecId" && field.Name != "DataAreaId" && field.Name != "TableId")
                    allowedFields.Add(field);
            }

            return allowedFields;
        }
    }
}
