using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.Dynamics.AX.Metadata.Core.MetaModel;
using System;
using System.Linq;
using Microsoft.Dynamics.AX.Metadata.Patterns;
using Microsoft.Dynamics.AX.Metadata.MetaModel.Extensions;

namespace Functions_for_Dynamics_Operations.Utilities
{
    public class FormUtils
    {
        protected PatternFactory PatternFactory;
        protected AxForm Form;

        public FormUtils(PatternFactory patternFactory, AxForm form)
        {
            PatternFactory = patternFactory;
            Form = form;
        }

        public static object GetFormPropertyValue(string property, string value)
        {
            AxForm form = null;
            //form.Design. =         

            switch (property)
            {
                case "HeightMode":
                    Enum.TryParse(value, out FormWidthHeightMode formWidthHeightMode);
                    return formWidthHeightMode;
                case "LeftMode":
                    Enum.TryParse(value, out FormLeftMode formLeftMode);
                    return formLeftMode;
                case "AllowDocking":
                    Enum.TryParse(value, out NoYes noYes);
                    return noYes;
                case "AllowFormCompanyChange":
                    Enum.TryParse(value, out NoYes noYes1);
                    return noYes1;
                case "Mode":
                    Enum.TryParse(value, out FormMode_ITxt formMode_ITxt);
                    return formMode_ITxt;
                case "AlwaysOnTop":
                    Enum.TryParse(value, out NoYes noYes2);
                    return noYes2;
                case "ColumnsMode":
                    Enum.TryParse(value, out FormColumnsMode formColumnsMode);
                    return formColumnsMode;
                case "ArrangeWhen":
                    Enum.TryParse(value, out ArrangeWhen_ITxt arrangeWhen_ITxt);
                    return arrangeWhen_ITxt;
                case "Style":
                    Enum.TryParse(value, out FormStyle formStyle);
                    return formStyle;
                case "ArrangeMethod":
                    Enum.TryParse(value, out ArrangeMethod_ITxt arrangeMethod_ITxt);
                    return arrangeMethod_ITxt;
                case "ViewEditMode":
                    Enum.TryParse(value, out ViewEditMode viewEditMode);
                    return viewEditMode;
                case "WidthMode":
                    Enum.TryParse(value, out FormWidthHeightMode formWidthHeight);
                    return formWidthHeight;
                case "ShowNewButton":
                    Enum.TryParse(value, out AutoNoYes autoNoYes);
                    return autoNoYes;
                case "ShowDeleteButton":
                    Enum.TryParse(value, out AutoNoYes autoNoYes1);
                    return autoNoYes1;
                case "DialogSize":
                    Enum.TryParse(value, out DialogSize dialogSize);
                    return dialogSize;
                case "WindowType":
                    Enum.TryParse(value, out WindowType_ITxt windowType_ITxt);
                    return windowType_ITxt;
                case "NewRecordAction":
                    return value;
                case "Width":
                case "Height":
                    return Convert.ToInt32(value);
                case "Columns":
                    return Convert.ToInt32(value);
                default:
                    throw new NotImplementedException();
            }
        }
        public AxFormControl CreateControl(string type, string name)
        {
            switch (type)
            {
                case "ActionPane":
                    return new AxFormActionPaneControl();
                case "ActionPaneTab":
                    return new AxFormActionPaneTabControl();
                case "ActiveX":
                    return new AxFormActiveXControl();
                case "Animate":
                    return new AxFormAnimateControl();
                case "Button":
                case "$Button":
                    return new AxFormButtonControl();
                case "ButtonGroup":
                    return new AxFormButtonGroupControl();
                case "ButtonSeparator":
                    return new AxFormButtonSeparatorControl();
                case "CheckBox":
                    return new AxFormCheckBoxControl();
                case "CommandButton":
                    AxFormCommandButtonControl commandButtonControl = new AxFormCommandButtonControl
                    {
                        Visible = NoYes.No,
                        Command = ClientTaskType.DetailsView
                    };

                    if (name.Contains("DefaultAction"))
                    {
                        AxFormGridControl mainGrid = (AxFormGridControl)Form.GetAllControls().FirstOrDefault(c => c.Name == name.Replace("DefaultAction", ""));
                        if (mainGrid != null)
                        {
                            // We need the default action from the command button
                            mainGrid.DefaultAction = name;
                            Form.Design.DefaultAction = name;
                        }
                    }

                    return commandButtonControl;
                case "Container":
                    return new AxFormContainerControl();
                case "Date":
                    return new AxFormDateControl();
                case "Grid":
                    AxFormGridControl grid = new AxFormGridControl();

                    if (Form.DataSources != null)
                        grid.DataSource = Form.DataSources[0].Name;

                    switch (name)
                    {
                        case "ListStyleGrid":
                            grid.WidthMode = FormWidthHeightMode.SizeToContent;
                            grid.ShowRowLabels = NoYes.No;
                            grid.MultiSelect = NoYes.No;
                            grid.Style = GridStyle.List;
                            grid.AllowEdit = NoYes.No;
                            break;
                    }

                    return grid;
                case "Group":
                    AxFormGroupControl groupControl = new AxFormGroupControl();

                    if (Form.DataSources != null)
                        groupControl.DataSource = Form.DataSources[0].Name;

                    // Some groups are generic but require certain properties to be defaulted
                    switch (name)
                    {
                        case "TitleGroup":
                            groupControl.WidthMode = FormWidthHeightMode.SizeToAvailable;
                            break;
                    }

                    // The group below does not have the quick filter as a mandatory pattern, so we handle this ourselves
                    if (name.Contains("Filter"))
                    {
                        var pattern = PatternFactory.AllPatterns.FirstOrDefault(i => i.Name == "CustomAndQuickFilters");
                        if (pattern != null)
                        {
                            // The pattern is not listed in the grou pattern but is required on fitler groups
                            groupControl.Pattern = pattern.Name;
                            groupControl.PatternVersion = pattern.Version;
                            groupControl.WidthMode = FormWidthHeightMode.SizeToAvailable;
                            groupControl.FrameType = FrameType_ITxt.None;
                            // The pattern on the Form type does not contain the quick filter either so add one
                            groupControl.Controls.Add(GetQuickFilterControl($"{name.Replace("Group", "")}QuickFilterControl"));
                        }
                    }

                    return groupControl;
                case "StaticText":
                    return new AxFormStaticTextControl();
                case "Tab":
                    return new AxFormTabControl();
                case "Table":
                    return new AxFormTableControl();
                case "TabPage":
                    return new AxFormTabPageControl();
                case "String":
                    return new AxFormStringControl();
                case "QuickFilterControl":
                    return GetQuickFilterControl("QuickFilterControl");
                default:
                    throw new NotImplementedException();
            }
        }

        protected AxFormControl GetQuickFilterControl(string name)
        {
            AxFormControlExtension extension = new AxFormControlExtension
            {
                Name = "QuickFilterControl"
            };

            extension.ExtensionProperties.Add(new AxFormControlExtensionProperty() { Name = "defaultColumnName", Type = CompilerBaseType.String });
            extension.ExtensionProperties.Add(new AxFormControlExtensionProperty() { Name = "targetControlName", Type = CompilerBaseType.String });
            extension.ExtensionProperties.Add(new AxFormControlExtensionProperty() { Name = "placeholderText", Type = CompilerBaseType.String });

            AxFormControl quickFilter = new AxFormControl
            {
                FormControlExtension = extension,
                Name = name
            };

            return quickFilter;
        }

        public void AddFormControl(AxFormControl parentControl, AxFormControl childControl)
        {
            switch (parentControl.Type.ToString())
            {
                case "ActionPane":
                    AxFormActionPaneControl apc = (AxFormActionPaneControl)parentControl;

                    if (apc.Controls.FirstOrDefault(a => a.Name == childControl.Name) == null)
                        apc.AddControl(childControl);
                    break;
                case "ActionPaneTab":
                    AxFormActionPaneTabControl aptc = (AxFormActionPaneTabControl)parentControl;

                    if (aptc.Controls.FirstOrDefault(a => a.Name == childControl.Name) == null)
                        aptc.AddControl(childControl);
                    break;
                case "ButtonGroup":
                    AxFormButtonGroupControl bg = (AxFormButtonGroupControl)parentControl;

                    if (bg.Controls.FirstOrDefault(a => a.Name == childControl.Name) == null)
                        bg.AddControl(childControl);
                    break;
                case "Container":
                    AxFormContainerControl c = (AxFormContainerControl)parentControl;

                    if (c.Controls.FirstOrDefault(a => a.Name == childControl.Name) == null)
                        c.AddControl(childControl);
                    break;
                case "Group":
                    AxFormGroupControl g = (AxFormGroupControl)parentControl;

                    if (g.Controls.FirstOrDefault(a => a.Name == childControl.Name) == null)
                        g.AddControl(childControl);
                    break;
                case "Tab":
                    AxFormTabControl t = (AxFormTabControl)parentControl;

                    if (t.Controls.FirstOrDefault(a => a.Name == childControl.Name) == null)
                        t.AddControl(childControl);
                    break;
                case "TabPage":
                    AxFormTabPageControl tp = (AxFormTabPageControl)parentControl;

                    if (tp.Controls.FirstOrDefault(a => a.Name == childControl.Name) == null)
                        tp.AddControl(childControl);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public AxFormControl GetFormControl(string name, string type)
        {
            switch (type)
            {
                case "ActionPane":
                    return (AxFormActionPaneControl)Form.GetAllControls().FirstOrDefault(c => c.Name == name);
                case "ActionPaneTab":
                    return (AxFormActionPaneTabControl)Form.GetAllControls().FirstOrDefault(c => c.Name == name);
                case "ActiveX":
                    return (AxFormActiveXControl)Form.GetAllControls().FirstOrDefault(c => c.Name == name);
                case "Animate":
                    return (AxFormAnimateControl)Form.GetAllControls().FirstOrDefault(c => c.Name == name);
                case "Button":
                case "$Button":
                    return (AxFormButtonControl)Form.GetAllControls().FirstOrDefault(c => c.Name == name);
                case "ButtonGroup":
                    return (AxFormButtonGroupControl)Form.GetAllControls().FirstOrDefault(c => c.Name == name);
                case "ButtonSeparator":
                    return (AxFormButtonSeparatorControl)Form.GetAllControls().FirstOrDefault(c => c.Name == name);
                case "CheckBox":
                    return (AxFormCheckBoxControl)Form.GetAllControls().FirstOrDefault(c => c.Name == name);
                case "CommandButton":
                    return (AxFormCommandButtonControl)Form.GetAllControls().FirstOrDefault(c => c.Name == name);
                case "Container":
                    return (AxFormContainerControl)Form.GetAllControls().FirstOrDefault(c => c.Name == name);
                case "Date":
                    return (AxFormDateControl)Form.GetAllControls().FirstOrDefault(c => c.Name == name);
                case "Grid":
                    return (AxFormGridControl)Form.GetAllControls().FirstOrDefault(c => c.Name == name);
                case "Group":
                    return (AxFormGroupControl)Form.GetAllControls().FirstOrDefault(c => c.Name == name);
                case "StaticText":
                    return (AxFormStaticTextControl)Form.GetAllControls().FirstOrDefault(c => c.Name == name);
                case "Tab":
                    return (AxFormTabControl)Form.GetAllControls().FirstOrDefault(c => c.Name == name);
                case "Table":
                    return (AxFormTableControl)Form.GetAllControls().FirstOrDefault(c => c.Name == name);
                case "TabPage":
                    return (AxFormTabPageControl)Form.GetAllControls().FirstOrDefault(c => c.Name == name);
                case "QuickFilterControl":
                    return Form.GetAllControls().FirstOrDefault(c => c.Name == name);
                case "String":
                    return (AxFormStringControl)Form.GetAllControls().FirstOrDefault(c => c.Name == name);
                default:
                    throw new NotImplementedException();
            }
        }

        public Microsoft.Dynamics.AX.Metadata.Core.MetaModel.FormControlType GetFormControlType(string type)
        {
            switch (type)
            {
                case "ActionPane":
                    return FormControlType.ActionPane;
                case "ActionPaneTab":
                    return FormControlType.ActionPaneTab;
                case "ActiveX":
                    return FormControlType.ActiveX;
                case "Animate":
                    return FormControlType.Animate;
                case "Button":
                case "$Button":
                    return FormControlType.Button;
                case "ButtonGroup":
                    return FormControlType.ButtonGroup;
                case "ButtonSeparator":
                    return FormControlType.ButtonSeparator;
                case "CheckBox":
                    return FormControlType.CheckBox;
                case "CommandButton":
                    return FormControlType.CommandButton;
                case "Container":
                    return FormControlType.Container;
                case "Custom":
                    return FormControlType.Custom;
                case "Date":
                    return FormControlType.Date;
                case "FastTabHeader":
                    return FormControlType.FastTabHeader;
                case "Grid":
                    return FormControlType.Grid;
                case "Group":
                    return FormControlType.Group;
                case "RichText":
                    return FormControlType.RichText;
                case "StaticText":
                    return FormControlType.StaticText;
                case "Tab":
                    return FormControlType.Tab;
                case "Table":
                    return FormControlType.Table;
                case "TabPage":
                    return FormControlType.TabPage;
                case "String":
                    return FormControlType.String;
                case "Tree":
                    return FormControlType.Tree;
                case "SysChart":
                case "QuickFilterControl":
                    return FormControlType.Custom;
                default:
                    throw new NotImplementedException();
            }
        }

        internal static object GetFormControlPropertyValue(string type, string property, string value)
        {
            AxFormControl formControl = null;
           
            switch (property)
            {
                case "AlignControl":
                    Enum.TryParse(value, out NoYes noYes);
                    return noYes;
                case "AllowEdit":
                    Enum.TryParse(value, out NoYes noYes1);
                    return noYes1;
                case "Enabled":
                    Enum.TryParse(value, out NoYes noYes2);
                    return noYes2;
                case "ExtendedStyle":
                    return value;
                case "HeightMode":
                    Enum.TryParse(value, out FormWidthHeightMode formWidthHeightMode);
                    return formWidthHeightMode;
                case "LeftMode":
                    Enum.TryParse(value, out FormLeftMode formLeftMode);
                    return formLeftMode;
                case "Skip":
                    Enum.TryParse(value, out NoYes noYes3);
                    return noYes3;
                case "TopMode":
                    Enum.TryParse(value, out FormTopMode formTopMode);
                    return formTopMode;
                case "Type":
                    Enum.TryParse(value, out FormControlType formControlType);
                    return formControlType;
                case "Visible":
                    Enum.TryParse(value, out NoYes noYes4);
                    return noYes4;
                case "WidthMode":
                    Enum.TryParse(value, out FormWidthHeightMode formWidthHeightMode1);
                    return formWidthHeightMode1;
                case "Width":
                    return Convert.ToInt32(value);
                case "Height":
                    return Convert.ToInt32(value);
                case "ShowLabel":
                    Enum.TryParse(value, out NoYes noYes5);
                    return noYes5;
                case "Command":
                    Enum.TryParse(value, out ClientTaskType clientTaskType);
                    return clientTaskType;
                case "Text":
                    return value;
                case "ColumnsMode":
                    Enum.TryParse(value, out FormColumnsMode formColumnsMode);
                    return formColumnsMode;
                case "ArrangeMethod":
                    Enum.TryParse(value, out ArrangeMethod_ITxt arrangeMethod_ITxt);
                    return arrangeMethod_ITxt;
                case "FrameType":
                    Enum.TryParse(value, out FrameType_ITxt frameType_ITxt);
                    return frameType_ITxt;
                case "Columns":
                    return Convert.ToInt32(value);
                case "VisibleRowsMode":
                    Enum.TryParse(value, out FormAutoMode formAutoMode);
                    return formAutoMode;
                case "ShowRowLabels":
                    Enum.TryParse(value, out NoYes noYes6);
                    return noYes6;
                case "MultiSelect":
                    Enum.TryParse(value, out NoYes noYes7);
                    return noYes7;
                case "HighlightActive":
                    Enum.TryParse(value, out NoYes noYes8);
                    return noYes8;
                case "VisibleRows":
                    return Convert.ToInt32(value);
                case "FrameOptionButton":
                    Enum.TryParse(value, out FormFrameOptionButton_ITxt formFrameOptionButton_ITxt);
                    return formFrameOptionButton_ITxt;
                case "AutoDeclaration":
                    Enum.TryParse(value, out NoYes noYes9);
                    return noYes9;
                case "ShowTabs":
                    Enum.TryParse(value, out NoYes noYes10);
                    return noYes10;
                case "Tab":
                    return Convert.ToInt32(value);
                case "PanelStyle":
                    Enum.TryParse(value, out PanelStyle panelStyle);
                    return panelStyle;
                case "FastTabExpanded":
                    Enum.TryParse(value, out FastTabExpanded fastTabExpanded);
                    return fastTabExpanded;
                case "HideIfEmpty":
                    Enum.TryParse(value, out NoYes noYes11);
                    return noYes11;
                default:
                    return GetFormControlPropertyPerControl(type, property, value);
            }
        }

        internal static object GetFormControlPropertyPerControl(string type, string property, string value)
        {
            switch (type)
            {
                case "ActionPane":
                    return GetPropertyForActoinPane(property, value);
                case "ActionPaneTab":
                    return GetPropertyForActionPaneTab(property, value);
                case "Button":
                case "$Button":
                    return GetPropertyForButton(property, value);
                case "ButtonGroup":
                    return GetPropertyForButtonGroup(property, value);  
                case "ButtonSeparator":
                    return GetPropertyForButtonSeparator(property, value);  
                case "CheckBox":
                    return GetPropertyForCheckBox(property, value);
                case "CommandButton":
                    return GetPropertyForCommandButton(property, value);
                case "Container":
                    return GetPropertyForContainer(property, value);
                case "Grid":
                    return GetPropertyForGrid(property, value);
                case "Group":
                    return GetPropertyForGroup(property, value);
                case "StaticText":
                    return GetPropertyForStaticText(property, value);
                case "Tab":
                    return GetPropertyForTab(property, value);
                case "Table":
                    return GetPropertyForTable(property, value);
                case "TabPage":
                    return GetPropertyForTabPage(property, value);  
                case "String":
                    return GetPropertyForString(property, value);
                case "QuickFilterControl":
                    return GetPropertyForQuickFilterControl(property, value);
                default:
                    throw new NotImplementedException();
            }
        }

        internal static object GetPropertyForActoinPane(string property, string value)
        {
            switch (property)
            {
                case "Style":
                    Enum.TryParse(value, out ActionPaneStyle actionPaneStyle);
                    return actionPaneStyle;
                default:
                    throw new NotImplementedException();
            }
        }

        internal static object GetPropertyForActionPaneTab(string property, string value)
        {
            switch (property)
            {
                default:
                    throw new NotImplementedException();
            }
        }

        internal static object GetPropertyForButton(string property, string value)
        {
            switch (property)
            {
                case "Style":
                    Enum.TryParse(value, out ButtonStyle buttonStyle);
                    return buttonStyle;
                default:
                    throw new NotImplementedException();
            }
        }

        internal static object GetPropertyForButtonGroup(string property, string value)
        {
            AxFormButtonGroupControl but = null;
  
            switch (property)
            {
                case "Style":
                    Enum.TryParse(value, out ButtonGroupStyle buttonGroupStyle);
                    return buttonGroupStyle;
                default:
                    throw new NotImplementedException();
            }
        }

        internal static object GetPropertyForButtonSeparator(string property, string value)
        {
            switch (property)
            {
                default:
                    throw new NotImplementedException();
            }
        }

        internal static object GetPropertyForCheckBox(string property, string value)
        {
            switch (property)
            {
                case "Style":
                    Enum.TryParse(value, out CheckBoxStyle checkBoxStyle);
                    return checkBoxStyle;
                default:
                    throw new NotImplementedException();
            }
        }

        internal static object GetPropertyForCommandButton(string property, string value)
        {
            AxFormCommandButtonControl cb = null;
          
            switch (property)
            {
                case "Style":
                    Enum.TryParse(value, out ButtonStyle buttonStyle);
                    return buttonStyle;
                default:
                    throw new NotImplementedException();
            }
        }

        internal static object GetPropertyForContainer(string property, string value)
        {
            AxFormActionPaneControl axFormActionPaneControl = null;

            switch (property)
            {
                case "Style":
                    Enum.TryParse(value, out ActionPaneStyle actionPaneStyle);
                    return actionPaneStyle;
                default:
                    throw new NotImplementedException();
            }
        }

        internal static object GetPropertyForGrid(string property, string value)
        {
            AxFormGridControl axFormGrid = null;

            switch (property)
            {
                case "Style":
                    Enum.TryParse(value, out GridStyle gridStyle);
                    return gridStyle;
                default:
                    throw new NotImplementedException();
            }
        }

        internal static object GetPropertyForGroup(string property, string value)
        {
            AxFormGroupControl axFormGroup = null;
            //axFormGroup. = 
            switch (property)
            {
                case "Style":
                    Enum.TryParse(value, out GroupStyle groupStyle);
                    return groupStyle;
                case "ViewEditMode":
                    Enum.TryParse(value, out ViewEditMode viewEditMode);
                    return viewEditMode;
                case "Breakable":
                    Enum.TryParse(value, out NoYes noYes);
                    return noYes;
                default:
                    throw new NotImplementedException();
            }
        }

        internal static object GetPropertyForStaticText(string property, string value)
        {
            switch (property)
            {
                case "Style":
                    Enum.TryParse(value, out StaticTextStyle staticTextStyle);
                    return staticTextStyle;
                default:
                    throw new NotImplementedException();
            }
        }

        internal static object GetPropertyForTab(string property, string value)
        {
            switch (property)
            {
                case "Style":
                    Enum.TryParse(value, out TabStyle tabStyle);
                    return tabStyle;
                default:
                    throw new NotImplementedException();
            }
        }

        internal static object GetPropertyForTable(string property, string value)
        {
            switch (property)
            {
                default:
                    throw new NotImplementedException();
            }
        }

        internal static object GetPropertyForTabPage(string property, string value)
        {
            switch (property)
            {
                case "Style":
                    Enum.TryParse(value, out TabPageStyle tabPageStyle);
                    return tabPageStyle;
                default:
                    throw new NotImplementedException();
            }
        }

        internal static object GetPropertyForString(string property, string value)
        {
            switch (property)
            {
                case "Style":
                    Enum.TryParse(value, out ControlStyle controlStyle);
                    return controlStyle;
                default:
                    throw new NotImplementedException();
            }
        }

        internal static object GetPropertyForQuickFilterControl(string property, string value)
        {
            switch (property)
            {
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
