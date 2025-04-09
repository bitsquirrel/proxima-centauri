using Microsoft.Dynamics.AX.Metadata.MetaModel;

namespace Functions_for_Dynamics_Operations
{
    internal class MenuFunc : ClassFunc
    {
        public static void GenLabelsForMenu(LabelEditorControl labelEditor, AxMenu axMenu)
        {
            string labelId = $"@{labelEditor.LabelFileCollectionSelected.Id}:";

            if (IsNotLabelOrEmpty(axMenu.Label, labelId))
            {
                axMenu.Label = labelEditor.AddLabelFromTextInCode($"{axMenu.Name}~{axMenu.Label}", "", true);
            }
        }

        public static void GenLabelsForMenuExt(LabelEditorControl labelEditor, AxMenuExtension axMenu)
        {
            string labelId = $"@{labelEditor.LabelFileCollectionSelected.Id}:";

            foreach (var customization in axMenu.Customizations)
            {
                switch (customization.Name)
                {
                    case "Label":
                        
                        break;
                    case "HelpText":
                        
                        break;
                }
            }

            foreach (var prop in axMenu.PropertyModifications)
            {
                switch (prop.Name)
                {
                    case "Label":
                        if (IsNotLabelOrEmpty(prop.Value, labelId))
                        {
                            prop.Value = labelEditor.AddLabelFromTextInCode($"{axMenu.Name.Substring(0, axMenu.Name.IndexOf("."))}{prop.Name}~{prop.Value}", "", true);
                        }
                        break;
                    case "HelpText":
                        if (IsNotLabelOrEmpty(prop.Value, labelId))
                        {
                            prop.Value = labelEditor.AddLabelFromTextInCode($"{axMenu.Name.Substring(0, axMenu.Name.IndexOf("."))}{prop.Name}~{prop.Value}", "", true);
                        }
                        break;
                }
            }
        }

        public static void GenLabelsForActionMenuItem(LabelEditorControl labelEditor, AxMenuItemAction axMenuItemAction)
        {
            string labelId = $"@{labelEditor.LabelFileCollectionSelected.Id}:";

            if (IsNotLabelOrEmpty(axMenuItemAction.Label, labelId))
            {
                axMenuItemAction.Label = labelEditor.AddLabelFromTextInCode($"{axMenuItemAction.Name}~{axMenuItemAction.Label}", "", true);
            }

            if (IsNotLabelOrEmpty(axMenuItemAction.HelpText, labelId))
            {
                axMenuItemAction.HelpText = labelEditor.AddLabelFromTextInCode($"{axMenuItemAction.Name}Help~{axMenuItemAction.HelpText}", "{Locked}", true);
            }
        }

        public static void GenLabelsForActionMenuItemExt(LabelEditorControl labelEditor, AxMenuItemActionExtension axMenuItemAction, string model)
        {
            string labelId = $"@{labelEditor.LabelFileCollectionSelected.Id}:";

            foreach (var prop in axMenuItemAction.PropertyModifications)
            {
                switch (prop.Name)
                {
                    case "Label":
                        if (IsNotLabelOrEmpty(prop.Value, labelId))
                        {
                            prop.Value = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, axMenuItemAction.Name + prop.Name)}~{prop.Value}", "", true);
                        }
                        break;
                    case "HelpText":
                        if (IsNotLabelOrEmpty(prop.Value, labelId))
                        {
                            prop.Value = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, axMenuItemAction.Name + prop.Name)}~{prop.Value}", "", true);
                        }
                        break;
                }
            }
        }

        public static void GenLabelsForDisplayMenuItem(LabelEditorControl labelEditor, AxMenuItemDisplay axMenuItemDisplay)
        {
            string labelId = $"@{labelEditor.LabelFileCollectionSelected.Id}:";

            if (IsNotLabelOrEmpty(axMenuItemDisplay.Label, labelId))
            {
                axMenuItemDisplay.Label = labelEditor.AddLabelFromTextInCode($"{axMenuItemDisplay.Name}~{axMenuItemDisplay.Label}", "", true);
            }

            if (IsNotLabelOrEmpty(axMenuItemDisplay.HelpText, labelId))
            {
                axMenuItemDisplay.HelpText = labelEditor.AddLabelFromTextInCode($"{axMenuItemDisplay.Name}Help~{axMenuItemDisplay.HelpText}", "{Locked}", true);
            }
        }

        public static void GenLabelsForDisplayMenuItemExt(LabelEditorControl labelEditor, AxMenuItemDisplayExtension axMenuItemDisplay, string model)
        {
            string labelId = $"@{labelEditor.LabelFileCollectionSelected.Id}:";

            foreach (var prop in axMenuItemDisplay.PropertyModifications)
            {
                switch (prop.Name)
                {
                    case "Label":
                        if (IsNotLabelOrEmpty(prop.Value, labelId))
                        {
                            prop.Value = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, axMenuItemDisplay.Name + prop.Name)}~{prop.Value}", "", true);
                        }
                        break;
                    case "HelpText":
                        if (IsNotLabelOrEmpty(prop.Value, labelId))
                        {
                            prop.Value = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, axMenuItemDisplay.Name + prop.Name)}~{prop.Value}", "", true);
                        }
                        break;
                }
            }
        }

        public static void GenLabelsForOutputMenuItem(LabelEditorControl labelEditor, AxMenuItemOutput axMenuItemOutput)
        {
            string labelId = $"@{labelEditor.LabelFileCollectionSelected.Id}:";

            if (IsNotLabelOrEmpty(axMenuItemOutput.Label, labelId))
            {
                axMenuItemOutput.Label = labelEditor.AddLabelFromTextInCode($"{axMenuItemOutput.Name}~{axMenuItemOutput.Label}", "", true);
            }

            if (IsNotLabelOrEmpty(axMenuItemOutput.HelpText, labelId))
            {
                axMenuItemOutput.HelpText = labelEditor.AddLabelFromTextInCode($"{axMenuItemOutput.Name}Help~{axMenuItemOutput.HelpText}", "{Locked}", true);
            }
        }

        public static void GenLabelsForOutputMenuItemExt(LabelEditorControl labelEditor, AxMenuItemOutputExtension axMenuItemOutput, string model)
        {
            string labelId = $"@{labelEditor.LabelFileCollectionSelected.Id}:";

            foreach (var prop in axMenuItemOutput.PropertyModifications)
            {
                switch (prop.Name)
                {
                    case "Label":
                        if (IsNotLabelOrEmpty(prop.Value, labelId))
                        {
                            prop.Value = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, axMenuItemOutput.Name + prop.Name)}~{prop.Value}", "", true);
                        }
                        break;
                    case "HelpText":
                        if (IsNotLabelOrEmpty(prop.Value, labelId))
                        {
                            prop.Value = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, axMenuItemOutput.Name + prop.Name)}~{prop.Value}", "", true);
                        }
                        break;
                }
            }
        }
    }
}
