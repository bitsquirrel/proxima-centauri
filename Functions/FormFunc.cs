using Microsoft.Dynamics.AX.Metadata.MetaModel.Extensions;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Core;

namespace Functions_for_Dynamics_Operations
{
    public class FormFunc : ClassFunc
    {
        public static void GenLabelsForForm(LabelEditorControl labelEditor, AxForm axForm, string model)
        {
            string labelId = $"@{labelEditor.LabelFileCollectionSelected.Id}:";

            if (IsNotLabelOrEmpty(axForm.Design.Caption, labelId))
            {
                axForm.Design.Caption = labelEditor.AddLabelFromTextInCode($"{axForm.Name}~{axForm.Design.Caption}", "", true);
            }

            foreach (AxFormControl control in axForm.GetAllControls())
            {
                GenLabelsForControl(labelEditor, control, axForm.Name, model);
            }
        }

        public static void LoopChildControls(LabelEditorControl labelEditor, AxFormContainerControl control, string formName, string model)
        {
            foreach (var subControl in control.Controls)
            {
                GenLabelsForControl(labelEditor, subControl, formName, model);
            }
        }

        public static void GenLabelsForControl(LabelEditorControl labelEditor, AxFormControl control, string formName, string model)
        {
            string labelId = $"@{labelEditor.LabelFileCollectionSelected.Id}:";

            if (control is AxFormActionPaneControl paneControl)
            {
                if (IsNotLabelOrEmpty(paneControl.Caption, labelId))
                {
                    paneControl.Caption = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, formName + paneControl.Name)}~{paneControl.Caption}", "", true);
                }

                LoopChildControls(labelEditor, paneControl, formName, model);
            }
            else if (control is AxFormActionPaneTabControl paneTabControl)
            {
                if (IsNotLabelOrEmpty(paneTabControl.Caption, labelId))
                {
                    paneTabControl.Caption = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, formName + paneTabControl.Name)}~{paneTabControl.Caption}", "", true);
                }

                LoopChildControls(labelEditor, paneTabControl, formName, model);
            }
            else if (control is AxFormButtonControl buttonControl)
            {
                if (IsNotLabelOrEmpty(buttonControl.Text, labelId))
                {
                    buttonControl.Text = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, formName + buttonControl.Name)}~{buttonControl.Text}", "", true);
                }

                if (IsNotLabelOrEmpty(buttonControl.HelpText, labelId))
                {
                    buttonControl.HelpText = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, formName + buttonControl.Name)}Help~{buttonControl.HelpText}", "", true);
                }
            }
            else if (control is AxFormButtonGroupControl buttonGroupControl)
            {
                if (IsNotLabelOrEmpty(buttonGroupControl.Caption, labelId))
                {
                    buttonGroupControl.Caption = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, formName + buttonGroupControl.Name)}~{buttonGroupControl.Caption}", "", true);
                }

                if (IsNotLabelOrEmpty(buttonGroupControl.HelpText, labelId))
                {
                    buttonGroupControl.HelpText = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, formName + buttonGroupControl.Name)}Help~{buttonGroupControl.HelpText}", "", true);
                }

                LoopChildControls(labelEditor, buttonGroupControl, formName, model);
            }
            else if (control is AxFormCheckBoxControl checkBoxControl)
            {
                if (IsNotLabelOrEmpty(checkBoxControl.Label, labelId))
                {
                    checkBoxControl.Label = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, formName + checkBoxControl.Name)}~{checkBoxControl.Label}", "", true);
                }

                if (IsNotLabelOrEmpty(checkBoxControl.HelpText, labelId))
                {
                    checkBoxControl.HelpText = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, formName + checkBoxControl.Name)}Help~{checkBoxControl.HelpText}", "", true);
                }
            }
            else if (control is AxFormComboBoxControl comboBoxControl)
            {
                if (IsNotLabelOrEmpty(comboBoxControl.Label, labelId))
                {
                    comboBoxControl.Label = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, formName + comboBoxControl.Name)}~{comboBoxControl.Label}", "", true);
                }

                if (IsNotLabelOrEmpty(comboBoxControl.HelpText, labelId))
                {
                    comboBoxControl.HelpText = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, formName + comboBoxControl.Name)}Help~{comboBoxControl.HelpText}", "", true);
                }
            }
            else if (control is AxFormCommandButtonControl commandButtonControl)
            {
                if (IsNotLabelOrEmpty(commandButtonControl.Text, labelId))
                {
                    commandButtonControl.Text = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, formName + commandButtonControl.Name)}~{commandButtonControl.Text}", "", true);
                }

                if (IsNotLabelOrEmpty(commandButtonControl.HelpText, labelId))
                {
                    commandButtonControl.HelpText = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, formName + commandButtonControl.Name)}Help~{commandButtonControl.HelpText}", "", true);
                }
            }
            else if (control is AxFormGridControl gridControl)
            {
                if (IsNotLabelOrEmpty(gridControl.HelpText, labelId))
                {
                    gridControl.HelpText = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, formName + gridControl.Name)}Help~{gridControl.HelpText}", "", true);
                }
            }
            else if (control is AxFormGroupControl groupControl)
            {
                if (IsNotLabelOrEmpty(groupControl.Caption, labelId))
                {
                    groupControl.Caption = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, formName + groupControl.Name)}~{groupControl.Caption}", "", true);
                }

                if (IsNotLabelOrEmpty(groupControl.HelpText, labelId))
                {
                    groupControl.HelpText = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, formName + groupControl.Name)}Help~{groupControl.HelpText}", "", true);
                }

                LoopChildControls(labelEditor, groupControl, formName, model);
            }
            else if (control is AxFormGuidControl guidControl)
            {
                if (IsNotLabelOrEmpty(guidControl.Label, labelId))
                {
                    guidControl.Label = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, formName + guidControl.Name)}~{guidControl.Label}", "", true);
                }

                if (IsNotLabelOrEmpty(guidControl.HelpText, labelId))
                {
                    guidControl.HelpText = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, formName + guidControl.Name)}Help~{guidControl.HelpText}", "", true);
                }
            }
            else if (control is AxFormInt64Control int64Control)
            {
                if (IsNotLabelOrEmpty(int64Control.Label, labelId))
                {
                    int64Control.Label = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, formName + int64Control.Name)}~{int64Control.Label}", "", true);
                }

                if (IsNotLabelOrEmpty(int64Control.HelpText, labelId))
                {
                    int64Control.HelpText = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, formName + int64Control.Name)}Help~{int64Control.HelpText}", "", true);
                }
            }
            else if (control is AxFormIntegerControl integerControl)
            {
                if (IsNotLabelOrEmpty(integerControl.Label, labelId))
                {
                    integerControl.Label = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, formName + integerControl.Name)}~{integerControl.Label}", "", true);
                }

                if (IsNotLabelOrEmpty(integerControl.HelpText, labelId))
                {
                    integerControl.HelpText = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, formName + integerControl.Name)}Help~{integerControl.HelpText}", "", true);
                }
            }
            else if (control is AxFormListBoxControl listBoxControl)
            {
                if (IsNotLabelOrEmpty(listBoxControl.Label, labelId))
                {
                    listBoxControl.Label = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, formName + listBoxControl.Name)}~{listBoxControl.Label}", "", true);
                }

                if (IsNotLabelOrEmpty(listBoxControl.HelpText, labelId))
                {
                    listBoxControl.HelpText = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, formName + listBoxControl.Name)}Help~{listBoxControl.HelpText}", "", true);
                }
            }
            else if (control is AxFormListViewControl listViewControl)
            {
                if (IsNotLabelOrEmpty(listViewControl.HelpText, labelId))
                {
                    listViewControl.HelpText = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, formName + listViewControl.Name)}Help~{listViewControl.HelpText}", "", true);
                }
            }
            else if (control is AxFormMenuButtonControl menuButtonControl)
            {
                if (IsNotLabelOrEmpty(menuButtonControl.Text, labelId))
                {
                    menuButtonControl.Text = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, formName + menuButtonControl.Name)}~{menuButtonControl.Text}", "", true);
                }

                if (IsNotLabelOrEmpty(menuButtonControl.HelpText, labelId))
                {
                    menuButtonControl.HelpText = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, formName + menuButtonControl.Name)}Help~{menuButtonControl.HelpText}", "", true);
                }
            }
            else if (control is AxFormMenuFunctionButtonControl menuFunctionButtonControl)
            {
                if (IsNotLabelOrEmpty(menuFunctionButtonControl.Text, labelId))
                {
                    menuFunctionButtonControl.Text = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, formName + menuFunctionButtonControl.Name)}~{menuFunctionButtonControl.Text}", "", true);
                }

                if (IsNotLabelOrEmpty(menuFunctionButtonControl.HelpText, labelId))
                {
                    menuFunctionButtonControl.HelpText = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, formName + menuFunctionButtonControl.Name)}Help~{menuFunctionButtonControl.HelpText}", "", true);
                }
            }
            else if (control is AxFormRadioButtonControl radioButtonControl)
            {
                if (IsNotLabelOrEmpty(radioButtonControl.Caption, labelId))
                {
                    radioButtonControl.Caption = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, formName + radioButtonControl.Name)}~{radioButtonControl.Caption}", "", true);
                }

                if (IsNotLabelOrEmpty(radioButtonControl.HelpText, labelId))
                {
                    radioButtonControl.HelpText = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, formName + radioButtonControl.Name)}Help~{radioButtonControl.HelpText}", "", true);
                }
            }
            else if (control is AxFormRealControl realControl)
            {
                if (IsNotLabelOrEmpty(realControl.Label, labelId))
                {
                    realControl.Label = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, formName + realControl.Name)}~{realControl.Label}", "", true);
                }

                if (IsNotLabelOrEmpty(realControl.HelpText, labelId))
                {
                    realControl.HelpText = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, formName + realControl.Name)}Help~{realControl.HelpText}", "", true);
                }
            }
            else if (control is AxFormReferenceGroupControl referenceGroupControl)
            {
                if (IsNotLabelOrEmpty(referenceGroupControl.Label, labelId))
                {
                    referenceGroupControl.Label = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, formName + referenceGroupControl.Name)}~{referenceGroupControl.Label}", "", true);
                }

                if (IsNotLabelOrEmpty(referenceGroupControl.HelpText, labelId))
                {
                    referenceGroupControl.HelpText = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, formName + referenceGroupControl.Name)}Help~{referenceGroupControl.HelpText}", "", true);
                }

                LoopChildControls(labelEditor, referenceGroupControl, formName, model);
            }
            else if (control is AxFormSegmentedEntryControl segmentedEntryControl)
            {
                if (IsNotLabelOrEmpty(segmentedEntryControl.Label, labelId))
                {
                    segmentedEntryControl.Label = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, formName + segmentedEntryControl.Name)}~{segmentedEntryControl.Label}", "", true);
                }

                if (IsNotLabelOrEmpty(segmentedEntryControl.HelpText, labelId))
                {
                    segmentedEntryControl.HelpText = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, formName + segmentedEntryControl.Name)}Help~{segmentedEntryControl.HelpText}", "", true);
                }
            }
            else if (control is AxFormStaticTextControl staticTextControl)
            {
                if (IsNotLabelOrEmpty(staticTextControl.Text, labelId))
                {
                    staticTextControl.Text = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, formName + staticTextControl.Name)}~{staticTextControl.Text}", "", true);
                }

                if (IsNotLabelOrEmpty(staticTextControl.HelpText, labelId))
                {
                    staticTextControl.HelpText = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, formName + staticTextControl.Name)}Help~{staticTextControl.HelpText}", "", true);
                }
            }
            else if (control is AxFormStringControl stringControl)
            {
                if (IsNotLabelOrEmpty(stringControl.Label, labelId))
                {
                    stringControl.Label = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, formName + stringControl.Name)}~{stringControl.Label}", "", true);
                }

                if (IsNotLabelOrEmpty(stringControl.HelpText, labelId))
                {
                    stringControl.HelpText = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, formName + stringControl.Name)}Help~{stringControl.HelpText}", "", true);
                }
            }
            else if (control is AxFormTabControl tabControl)
            {
                if (IsNotLabelOrEmpty(tabControl.HelpText, labelId))
                {
                    tabControl.HelpText = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, formName + tabControl.Name)}Help~{tabControl.HelpText}", "", true);
                }

                LoopChildControls(labelEditor, tabControl, formName, model);
            }
            else if (control is AxFormTabPageControl tabPageControl)
            {
                if (IsNotLabelOrEmpty(tabPageControl.Caption, labelId))
                {
                    tabPageControl.Caption = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, formName + tabPageControl.Name)}~{tabPageControl.Caption}", "", true);
                }

                if (IsNotLabelOrEmpty(tabPageControl.HelpText, labelId))
                {
                    tabPageControl.HelpText = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, formName + tabPageControl.Name)}Help~{tabPageControl.HelpText}", "", true);
                }

                LoopChildControls(labelEditor, tabPageControl, formName, model);
            }
            else if (control is AxFormTableControl tableControl)
            {
                if (IsNotLabelOrEmpty(tableControl.HelpText, labelId))
                {
                    tableControl.HelpText = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, formName + tableControl.Name)}Help~{tableControl.HelpText}", "", true);
                }
            }
            else if (control is AxFormTimeControl timeControl)
            {
                if (IsNotLabelOrEmpty(timeControl.HelpText, labelId))
                {
                    timeControl.HelpText = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, formName + timeControl.Name)}Help~{timeControl.HelpText}", "", true);
                }
            }
            else if (control is AxFormTreeControl treeControl)
            {
                if (IsNotLabelOrEmpty(treeControl.HelpText, labelId))
                {
                    treeControl.HelpText = labelEditor.AddLabelFromTextInCode($"{CorrectLabel(model, formName + treeControl.Name)}Help~{treeControl.HelpText}", "", true);
                }
            }
        }

        public static void GenLabelsForFormExt(LabelEditorControl labelEditor, AxFormExtension axForm, string model)
        {
            string labelId = $"@{labelEditor.LabelFileCollectionSelected.Id}:";

            foreach (AxFormExtensionControl control in axForm.Controls)
            {
                GenLabelsForControl(labelEditor, control.FormControl, axForm.Name, model);
            }

            foreach (Microsoft.Dynamics.AX.Metadata.MetaModel.AxPropertyModification propertyMod in axForm.PropertyModifications)
            {
                switch (propertyMod.Name)
                {
                    case "Label":
                    case "Caption":
                        if (IsNotLabelOrEmpty(propertyMod.Value, labelId))
                        {
                            propertyMod.Value = labelEditor.AddLabelFromTextInCode($"{axForm.Name.Substring(0, axForm.Name.IndexOf("."))}{axForm.Name}~{propertyMod.Value}", "", true);
                        }
                        break;
                    case "HelpText":
                        if (IsNotLabelOrEmpty(propertyMod.Value, labelId))
                        {
                            propertyMod.Value = labelEditor.AddLabelFromTextInCode($"{axForm.Name.Substring(0, axForm.Name.IndexOf("."))}{axForm.Name}Help~{propertyMod.Value}", "", true);
                        }
                        break;
                }
            }

            foreach (Microsoft.Dynamics.AX.Metadata.MetaModel.AxExtensionModification axExtensionModification in axForm.ControlModifications)
            {
                foreach (Microsoft.Dynamics.AX.Metadata.MetaModel.AxPropertyModification propertyMod in axExtensionModification.PropertyModifications)
                {
                    switch (propertyMod.Name)
                    {
                        case "Label":
                        case "Caption":
                            if (IsNotLabelOrEmpty(propertyMod.Value, labelId))
                            {
                                propertyMod.Value = labelEditor.AddLabelFromTextInCode($"{axForm.Name.Substring(0, axForm.Name.IndexOf("."))}{axExtensionModification.Name}~{propertyMod.Value}", "", true);
                            }
                            break;
                        case "HelpText":
                            if (IsNotLabelOrEmpty(propertyMod.Value, labelId))
                            {
                                propertyMod.Value = labelEditor.AddLabelFromTextInCode($"{axForm.Name.Substring(0, axForm.Name.IndexOf("."))}{axExtensionModification.Name}Help~{propertyMod.Value}", "", true);
                            }
                            break;
                    }
                }
            }
        }

        public static new void ApplyComments(string formName)
        {
            IDesignMetaModelService designMetaModelService = VStudioUtils.GetDesignMetaModelService();
            AxForm form = designMetaModelService.GetForm(formName);
            if (form != null)
            {
                IModelReference modelReference = VStudioUtils.GetActiveAXProjectModelInfo();

                if (!form.SourceCode.Declaration.Contains("///"))
                {
                    form.SourceCode.Declaration = ClassFunc.CommentDeclaration(form.Name, form.GetType().Name, modelReference.Name, form.SourceCode.Declaration);
                }

                MethodFunc.CommentMethods(form.Methods);

                designMetaModelService.CurrentMetadataProvider.Forms.Update(form, new ModelSaveInfo(modelReference));
            }
        }
    }
}
