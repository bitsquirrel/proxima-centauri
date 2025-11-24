using Functions_for_Dynamics_Operations.Utilities;
using Microsoft.Dynamics.AX.Metadata.Extensions.CanonicalForm;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.Dynamics.AX.Metadata.MetaModel.Extensions;
using Microsoft.Dynamics.AX.Metadata.Patterns;
using Microsoft.Dynamics.AX.Metadata.XppCompiler;
using Microsoft.Dynamics.Framework.Tools.BuildTasks;
using Microsoft.Dynamics.Framework.Tools.FormControlExtension.SysChartControl;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Core;
using Microsoft.Dynamics.Framework.Tools.ProjectSystem;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.VCProjectEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Functions_for_Dynamics_Operations
{
    internal class FormCreateFunc : ClassFunc
    {
        internal const string FormDesign = "FormDesign";

        public void CreateFormFromTable(string formName)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
             
            if (CreateForm(formName))
            {
                VSProjectNode vSProjectNode = VStudioUtils.GetSelectedProjectOrFirstActiveProject();
                // Create the form in the model
                vSProjectNode.DesignMetaModelService.CurrentMetadataProvider.Forms.Create(Form, new ModelSaveInfo(vSProjectNode.GetProjectsModelInfo()));
                // Add the form to the active project
                SaveToProject(Form.Name, Form.GetType());
            }
        }

        internal void SelectFormPatternApplyPattern(string formName)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            // The form exists but the pattern is not applied
            CreateFormForm createFormForm = ShowCreateFormDialog(formName);
            if (createFormForm.Ok)
            {
                // Apply the pattern to the form as selected in the dialog
                ApplyPattern(createFormForm.FormPattern);
            }
        }

        public void ApplyFormPattern(AxForm Axform)
        {
            Form = Axform;

            PatternFactory = new PatternFactory(true);
            FormUtil = new FormUtils(PatternFactory, Form);
            // Pick up the pattern to apply to your form
            var pattern = PatternFactory.AllPatterns.FirstOrDefault(i => i.Name.ToLower() == Form.Design.Pattern.ToLower());
            if (pattern != null)
            {
                ApplyRestrictedPropertiesToForm(pattern.Root);

                LoopNodes(pattern.Root);
                // Update the form in the model
                VSProjectNode vSProjectNode = VStudioUtils.GetSelectedProjectOrFirstActiveProject();
                vSProjectNode.DesignMetaModelService.CurrentMetadataProvider.Forms.Update(Form, new ModelSaveInfo(vSProjectNode.GetProjectsModelInfo()));
            }
            else
            {
                // Allow the user to select a pattern to apply
                SelectFormPatternApplyPattern(Form.Name);
            }
        }

        protected void CompileForm()
        {
            List<ModelElementCompilationDescriptor> modelElementCompilationDescriptors = new List<ModelElementCompilationDescriptor>
            {
                new ModelElementCompilationDescriptor(ModelElementType.Form, Form.Name)
            };

            VSProjectNode projectNode = VStudioUtils.GetSelectedProjectOrFirstActiveProject();

            IDesignMetaModelService designMetaModelService = projectNode.DesignMetaModelService;

            BuildHelper buildHelper = new BuildHelper(designMetaModelService.CurrentMetadataProvider);

            buildHelper.CompileElements(designMetaModelService.CurrentMetadataProvider, System.Threading.CancellationToken.None, modelElementCompilationDescriptors, CompileMode.Incremental, projectNode.GetProjectsModelInfo(), true);
        }

        internal override void ApplyPattern(string patternName)
        {
            var pattern = PatternFactory.AllPatterns.FirstOrDefault(i => i.FriendlyName.ToLower() == patternName.ToLower());

            if (pattern != null)
            {
                Form.Design.PatternVersion = pattern.Version;
                Form.Design.Pattern = pattern.Name;

                ApplyRestrictedPropertiesToForm(pattern.Root);

                LoopNodes(pattern.Root);
            }
        }

        internal void ApplyRestrictedPropertiesToForm(PatternNode patternNode)
        {
            foreach (var restrictedProperty in patternNode.RestrictedProperties)
            {
                PropertyInfo propertyInfo = Form.Design.GetType().GetProperty(restrictedProperty.Name);

                if (restrictedProperty.ExpectedValue != "")
                {
                    propertyInfo.SetValue(Form.Design, FormUtils.GetFormPropertyValue(restrictedProperty.Name, restrictedProperty.ExpectedValue));
                }   
            }
        }

        public void LoopNodes(PatternNode parentNode)
        {
            if (Form.Design != null)
            {
                // Now loop through the required subnodes to create them
                foreach (PatternNode node in parentNode.SubNodes)
                {
                    ProcessNode(parentNode, node);
                }
            }
        }

        protected void ProcessNode(PatternNode parentNode, PatternNode childNode)
        {
            if (childNode.RequireOne && (childNode.Type == "" || childNode.FriendlyName == null) && childNode.SubNodes != null && childNode.SubNodes.Count > 0)
            {
                // For some reason the subNode contains the pattern to apply
                childNode = childNode.SubNodes[0];
            }

            // The control can be required or the user can select to create the full pattern
            if ((childNode.RequireOne || Full) && (childNode.Type != "" || childNode.FriendlyName != null))
            {
                string childName = GetCtrlName(childNode), parentName = GetCtrlName(parentNode, true);

                // This is the only exception to design, there are duplicate Tabs with identical names
                if (Form.Design.Pattern == "DialogDouble" && childName == "FormTab1")
                    parentName += "1";

                AxFormControl formControl = GetFormControl(childName, childNode.Type.ToString());
                // Check if the subNode already exists so that we do not create it again
                if (formControl == null)
                {
                    formControl = CreateControl(childNode, childName);
                    formControl.Name = formControl.Name == "" ? childName : formControl.Name;
                    formControl.Type = FormUtil.GetFormControlType(childNode.Type);

                    // Apply design specific properties
                    ApplyRestrictedPropertiesToControls(childNode, formControl);

                    // Now add the control to the form or to another control if the parent is not the root
                    if (parentNode.Type != FormDesign)
                    {
                        FormUtil.AddFormControl(GetFormControl(parentName, parentNode.Type.ToString()), formControl);
                    }
                    else
                    {
                        // This is always the parent of the children added later
                        Form.Design.AddControl(formControl);
                    } 
                }

                LoopNodes(childNode);
            }
        }

        internal string GetCtrlName(PatternNode patternNode, bool parent = false)
        {
            string controlName = GetControlName(patternNode);

            if (parent)
                return controlName;

            string origName = controlName.ToString();
            int i = 1;
          
            if (Form.Design.Pattern.ToLower().Contains("double"))
            {
                // There are duplicate control names in the pattern
                while (CtrlNames.Exists(e => e.Equals(controlName)))
                {
                    controlName = origName + i.ToString();
                    i++;
                }
                // Add to the list of controls already created
                CtrlNames.Add(controlName);
            }

            return controlName;
        }

        internal string GetControlName(PatternNode patternNode)
        {
            string controlName = patternNode.FriendlyName != null ? patternNode.FriendlyName.Replace(" ", "") : patternNode.Type.ToString();
            return controlName.Replace("$", "");
        }

        internal void ApplyRestrictedPropertiesToControls(PatternNode patternNode, AxFormControl formControl)
        {
            foreach (var restrictedProperty in patternNode.RestrictedProperties)
            {
                if (restrictedProperty.ExpectedValue != "")
                {
                    PropertyInfo propertyInfo = formControl.GetType().GetProperty(restrictedProperty.Name);
                    if (propertyInfo != null)
                    {
                        propertyInfo.SetValue(formControl, FormUtils.GetFormControlPropertyValue(patternNode.Type, restrictedProperty.Name, restrictedProperty.ExpectedValue));
                    }
                }
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
                case "$ContainerOrDesign":
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
                case "String":
                    return (AxFormStringControl)Form.GetAllControls().FirstOrDefault(c => c.Name == name);
                case "Tree":
                    return (AxFormTreeControl)Form.GetAllControls().FirstOrDefault(c => c.Name == name);
                case "SysChart":
                case "QuickFilterControl":
                    return Form.GetAllControls().FirstOrDefault(c => c.Name == name);
                case "$Field":
                    return new AxFormControl();
                default:
                    throw new NotImplementedException();
            }
        }

        public AxFormControl CreateControl(PatternNode patternNode, string name)
        {
            switch (patternNode.Type.ToString())
            {
                case "ActionPane":
                    AxFormActionPaneControl actionPaneControl = new AxFormActionPaneControl();

                    if (Form.DataSources != null && Form.DataSources.Count > 0)
                    {
                        actionPaneControl.DataSource = Form.DataSources[0].Name;
                    };

                    return actionPaneControl;
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
                    return new AxFormCommandButtonControl();
                case "Container":
                    return new AxFormContainerControl();
                case "Date":
                    return new AxFormDateControl();
                case "Grid":
                    AxFormGridControl grid = new AxFormGridControl();

                    if (Form.DataSources != null && Form.DataSources.Count > 0)
                    {
                        if (!name.ToLower().Contains("lines"))
                        {
                            grid.DataSource = Form.DataSources[0].Name;
                        }
                    }

                    return grid;
                case "Group":
                    AxFormGroupControl groupControl = new AxFormGroupControl();

                    if (Form.DataSources != null && Form.DataSources.Count > 0)
                    {
                        groupControl.DataSource = Form.DataSources[0].Name;
                    };

                    if (name.ToLower().Contains("filter"))
                    {
                        var pattern = PatternFactory.AllPatterns.FirstOrDefault(i => i.Name == "CustomAndQuickFilters");
                        if (pattern != null)
                        {
                            // The pattern is not listed in the grou pattern but is required on fitler groups
                            groupControl.Pattern = pattern.Name;
                            groupControl.PatternVersion = pattern.Version;
                            groupControl.WidthMode = Microsoft.Dynamics.AX.Metadata.Core.MetaModel.FormWidthHeightMode.SizeToAvailable;
                            groupControl.FrameType = Microsoft.Dynamics.AX.Metadata.Core.MetaModel.FrameType_ITxt.None;
                            // The pattern on the Form type does not contain the quick filter either so add one

                            if (Form.Design.Controls.FirstOrDefault(a => a.Name == $"{name.Replace("Group", "").Replace("Filter", "")}QuickFilterControl") == null)
                                groupControl.Controls.Add(GetQuickFilterControl($"{name.Replace("Group", "").Replace("Filter", "")}QuickFilterControl"));
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
                    PropertyInfo element = patternNode.GetType().GetProperty("Element");
                    bool hasAttributeValue = false;
                    object hasAttributes = null;

                    if (element != null)
                    {
                        string[] strings = new string[1] { "HasAttributes" };

                        hasAttributes = element.GetValue(patternNode, strings);

                        if (hasAttributes != null && hasAttributes is bool)
                        {
                            hasAttributeValue = true;
                        }
                    }

                    AxFormTabPageControl formTabPage = new AxFormTabPageControl();

                    if (hasAttributeValue)
                    {
                        var pattern = PatternFactory.AllPatterns.FirstOrDefault(i => i.Name == "CustomAndQuickFilters");
                        if (pattern != null)
                        {
                            // The pattern is not listed in the grou pattern but is required on fitler groups
                            formTabPage.Pattern = pattern.Name;
                            formTabPage.PatternVersion = pattern.Version;
                        }
                    }

                    return formTabPage;
                case "String":
                    return new AxFormStringControl();
                case "Tree":
                    return new AxFormTreeControl();
                case "QuickFilterControl":
                    return GetQuickFilterControl("QuickFilterControl");
                case "SysChart":
                    return new AxFormControl();
                default:
                    throw new NotImplementedException();
            }
        }

        internal AxFormControl GetQuickFilterControl(string name)
        {
            AxFormControlExtension extension = new AxFormControlExtension
            {
                Name = "QuickFilterControl"
            };

            extension.ExtensionProperties.Add(new AxFormControlExtensionProperty() { Name = "defaultColumnName", Type = Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerBaseType.String });
            extension.ExtensionProperties.Add(new AxFormControlExtensionProperty() { Name = "targetControlName", Type = Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerBaseType.String });
            extension.ExtensionProperties.Add(new AxFormControlExtensionProperty() { Name = "placeholderText", Type = Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerBaseType.String });

            AxFormControl quickFilter = new AxFormControl
            {
                FormControlExtension = extension,
                Name = name
            };

            return quickFilter;
        }
    }
}
