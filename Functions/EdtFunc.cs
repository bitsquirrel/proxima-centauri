using Microsoft.Dynamics.AX.Metadata.MetaModel;

namespace Functions_for_Dynamics_Operations
{
    internal class EdtFunc : ClassFunc
    {
        public static void GenLabelsForEdt(LabelEditorControl labelEditor, AxEdt axEdt, string model)
        {
            string labelId = $"@{labelEditor.LabelFileCollectionSelected.Id}:";

            if (IsNotLabelOrEmpty(axEdt.Label, labelId))
            {
                axEdt.Label = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, axEdt.Name)}~{axEdt.Label}", "", true);
            }

            if (IsNotLabelOrEmpty(axEdt.HelpText, labelId))
            {
                axEdt.HelpText = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, axEdt.Name)}Help~{axEdt.HelpText}", "", true);
            }
        }

        public static void GenLabelsForEdt(LabelEditorControl labelEditor, AxEdtEnum axEdt, string model)
        {
            string labelId = $"@{labelEditor.LabelFileCollectionSelected.Id}:";

            if (IsNotLabelOrEmpty(axEdt.Label, labelId))
            {
                axEdt.Label = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, axEdt.Name)}~{axEdt.Label}", "", true);
            }

            if (IsNotLabelOrEmpty(axEdt.HelpText, labelId))
            {
                axEdt.HelpText = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, axEdt.Name)}Help~{axEdt.HelpText}", "", true);
            }

            foreach (var element in axEdt.ArrayElements)
            {
                if (IsNotLabelOrEmpty(element.Label, labelId))
                {
                    element.Label = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, axEdt.Name + element.Name)}~{element.Label}", "", true);
                }

                if (IsNotLabelOrEmpty(element.HelpText, labelId))
                {
                    element.HelpText = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, axEdt.Name + element.Name)}Help~{element.HelpText}", "", true);
                }
            }
        }

        public static void GenLabelsForEdtExt(LabelEditorControl labelEditor, AxEdtExtension axEdt, string model)
        {
            string labelId = $"@{labelEditor.LabelFileCollectionSelected.Id}:";

            foreach (var prop in axEdt.PropertyModifications)
            {
                switch (prop.Name.ToLower())
                {
                    case "label":
                        if (IsNotLabelOrEmpty(prop.Value, labelId))
                        {
                            prop.Value = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, axEdt.Name + prop.Name)}~{prop.Value}", "", true);
                        }
                        break;
                    case "helptext":
                        if (IsNotLabelOrEmpty(prop.Value, labelId))
                        {
                            prop.Value = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, axEdt.Name + prop.Name)}~{prop.Value}", "", true);
                        }
                        break;
                }
            }
        }

        public static void GenLabelsForEnum(LabelEditorControl labelEditor, AxEnum axEnum, string model)
        {
            string labelId = $"@{labelEditor.LabelFileCollectionSelected.Id}:";

            if (IsNotLabelOrEmpty(axEnum.Label, labelId))
            {
                axEnum.Label = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, axEnum.Name)}~{axEnum.Label}", "", true);
            }

            if (IsNotLabelOrEmpty(axEnum.Help, labelId))
            {
                axEnum.Help = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, axEnum.Name)}Help~{axEnum.Help}", "", true);
            }

            foreach (var enumValue in axEnum.EnumValues)
            {
                if (IsNotLabelOrEmpty(enumValue.Label, labelId))
                {
                    enumValue.Label = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, axEnum.Name + enumValue.Name)}~{enumValue.Label}", "", true);
                }
            }
        }

        public static void GenLabelsForEnumExt(LabelEditorControl labelEditor, AxEnumExtension axEnum, string model)
        {
            string labelId = $"@{labelEditor.LabelFileCollectionSelected.Id}:";

            foreach (var enumValue in axEnum.EnumValues)
            {
                if (IsNotLabelOrEmpty(enumValue.Label, labelId))
                {
                    enumValue.Label = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, axEnum.Name + enumValue.Name)}~{enumValue.Label}", "", true);
                }
            }

            foreach (AxPropertyModification propertyMod in axEnum.PropertyModifications)
            {
                switch (propertyMod.Name)
                {
                    case "Label":
                        if (IsNotLabelOrEmpty(propertyMod.Value, labelId))
                        {
                            propertyMod.Value = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, axEnum.Name + propertyMod.Name)}~{propertyMod.Value}", "", true);
                        }
                        break;
                    case "HelpText":
                        if (IsNotLabelOrEmpty(propertyMod.Value, labelId))
                        {
                            propertyMod.Value = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, axEnum.Name + propertyMod.Name)}Help~{propertyMod.Value}", "", true);
                        }
                        break;
                }
            }
        }
    }
}
