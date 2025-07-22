using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE;
using System.Windows.Forms;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.Dynamics.Framework.Tools.ProjectSystem;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Core;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Menus;
using Functions_for_Dynamics_Operations.Utilities;
using Microsoft.Dynamics.AX.Metadata.Patterns;

namespace Functions_for_Dynamics_Operations
{
    public class ObjectAndTypeDetails
    {
        public string ObjectName { get; set; }
        public string TypeGuid { get; set; }
        public List<string> Methods { get; set; }
        public object AxObject { get; set; }

        public ObjectAndTypeDetails(string objectName, string typeGuid, Object axObject = null)
        {
            Methods = new List<string>();
            ObjectName = objectName;
            AxObject = axObject;
            TypeGuid = typeGuid;
        }

        public void InitObjectFromName()
        {
            IDesignMetaModelService designMetaModelService = VStudioUtils.GetDesignMetaModelService();

            switch (TypeGuid)
            {
                case AxClass.MetaClassId:
                    AxObject = designMetaModelService.GetClass(ObjectName);
                    break;
                case AxDataEntity.MetaClassId:
                    AxObject = designMetaModelService.GetDataEntityView(ObjectName);
                    break;
                case AxDataEntityView.MetaClassId:
                    AxObject = designMetaModelService.GetDataEntityView(ObjectName);
                    break;
                case AxEdt.MetaClassId:
                    AxObject = designMetaModelService.GetExtendedDataType(ObjectName);
                    break;
                case AxEdtDate.MetaClassId:
                    AxObject = designMetaModelService.GetExtendedDataType(ObjectName);
                    break;
                case AxEdtEnum.MetaClassId:
                    AxObject = designMetaModelService.GetExtendedDataType(ObjectName);
                    break;
                case AxEdtGuid.MetaClassId:
                    AxObject = designMetaModelService.GetExtendedDataType(ObjectName);
                    break;
                case AxEdtInt.MetaClassId:
                    AxObject = designMetaModelService.GetExtendedDataType(ObjectName);
                    break;
                case AxEdtInt64.MetaClassId:
                    AxObject = designMetaModelService.GetExtendedDataType(ObjectName);
                    break;
                case AxEdtReal.MetaClassId:
                    AxObject = designMetaModelService.GetExtendedDataType(ObjectName);
                    break;
                case AxEdtString.MetaClassId:
                    AxObject = designMetaModelService.GetExtendedDataType(ObjectName);
                    break;
                case AxEdtTime.MetaClassId:
                    AxObject = designMetaModelService.GetExtendedDataType(ObjectName);
                    break;
                case AxEdtUtcDateTime.MetaClassId:
                    AxObject = designMetaModelService.GetExtendedDataType(ObjectName);
                    break;
                case AxEnum.MetaClassId:
                    AxObject = designMetaModelService.GetEnum(ObjectName);
                    break;
                case AxEnumExtension.MetaClassId:
                    AxObject = designMetaModelService.GetEnumExtension(ObjectName);
                    break;
                case AxForm.MetaClassId:
                    AxObject = designMetaModelService.GetForm(ObjectName);
                    break;
                case AxFormExtension.MetaClassId:
                    AxObject = designMetaModelService.GetFormExtension(ObjectName);
                    break;
                case AxMap.MetaClassId:
                    AxObject = designMetaModelService.GetMap(ObjectName);
                    break;
                case AxMenu.MetaClassId:
                    AxObject = designMetaModelService.GetMenu(ObjectName);
                    break;
                case AxMenuExtension.MetaClassId:
                    AxObject = designMetaModelService.GetMenuExtension(ObjectName);
                    break;
                case AxMenuItemAction.MetaClassId:
                    AxObject = designMetaModelService.GetMenuItemAction(ObjectName);
                    break;
                case AxMenuItemDisplay.MetaClassId:
                    AxObject = designMetaModelService.GetMenuItemDisplay(ObjectName);
                    break;
                case AxMenuItemOutput.MetaClassId:
                    AxObject = designMetaModelService.GetMenuItemOutput(ObjectName);
                    break;
                case AxMenuItemExtension.MetaClassId:
                    AxObject = designMetaModelService.GetMenuExtension(ObjectName);
                    break;
                case AxQuery.MetaClassId:
                    AxObject = designMetaModelService.GetQuery(ObjectName);
                    break;
            }

            Methods = ClassFunc.GetAllowedMethodNames(AxObject);
        }

        public Microsoft.Dynamics.AX.Metadata.Core.Collections.KeyedObjectCollection<AxMethod> GetAxObjectMethods()
        {
            return ClassFunc.GetAxObjectMethods(AxObject);
        }
    }

    public class ClassFunc
    {
        private readonly string NewLine = Environment.NewLine;
        protected PatternFactory PatternFactory;
        internal List<string> CtrlNames;
        protected string FormDataSource;
        public FormUtils FormUtil;
        protected AxForm Form;
        internal bool Full;

        internal virtual void CreateClassMethods()
        {
            // This is a placeholder for the class methods
        }

        internal CreateFormForm ShowCreateFormDialog(string _formName)
        {
            // This will allow the user to select the form design required
            CreateFormForm CreateFormForm = new CreateFormForm
            {
                StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
            };

            CreateFormForm.ShowDialog();
            // Return and deal with the result
            return CreateFormForm;
        }

        internal bool CreateForm(string formName)
        {
            CreateFormForm CreateFormForm = ShowCreateFormDialog(formName);

            if (CreateFormForm.Ok)
            {
                Full = CreateFormForm.Full;

                CtrlNames = new List<string>();

                Form = new AxForm
                {
                    Name = formName
                };

                Form.Methods.Add(new AxMethod() { Name = "classDeclaration", Source = $"[Form]{Environment.NewLine}public class {formName} extends FormRun{Environment.NewLine}" + "{" + Environment.NewLine + "}" });

                AxTable table = VStudioUtils.GetDesignMetaModelService().GetTable(formName);
                if (table != null)
                {
                    // Add the data source
                    AxFormDataSourceRoot dataSourceRoot = new AxFormDataSourceRoot()
                    {
                        InsertIfEmpty = Microsoft.Dynamics.AX.Metadata.Core.MetaModel.NoYes.No,
                        Table = formName,
                        Name = formName
                    };
                    // The fields need to be explicitly added for some reason
                    foreach (var field in table.Fields)
                    {
                        dataSourceRoot.Fields.Add(new AxFormDataSourceField() { Name = field.Name, DataField = field.Name });
                    }
                    // System fields are not part of the fields collection for some reason
                    dataSourceRoot.Fields.Add(new AxFormDataSourceField() { Name = "Partitian", DataField = "Partitian" });
                    dataSourceRoot.Fields.Add(new AxFormDataSourceField() { Name = "TableId", DataField = "TableId" });
                    dataSourceRoot.Fields.Add(new AxFormDataSourceField() { Name = "RecId", DataField = "RecId" });

                    if (table.SaveDataPerCompany == Microsoft.Dynamics.AX.Metadata.Core.MetaModel.NoYes.Yes)
                    {
                        dataSourceRoot.Fields.Add(new AxFormDataSourceField() { Name = "DataAreaId", DataField = "DataAreaId" });
                    }

                    Form.DataSources.Add(dataSourceRoot);

                    FormDataSource = formName;
                    // Use the label from the table
                    Form.Design.Caption = table.Label;
                }

                PatternFactory = new PatternFactory(true);
                FormUtil = new FormUtils(PatternFactory, Form);

                ApplyPattern(CreateFormForm.FormPattern);
            }

            return CreateFormForm.Ok;
        }

        internal virtual void ApplyPattern(string patternName)
        {
                        
        }

        public void SaveToProject(string name, Type type, bool open = true)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            List<MetadataReference> metadata = new List<MetadataReference>
            {
                new MetadataReference(name, type)
            };

            VStudioUtils.GetSelectedProjectOrFirstActiveProject().AddModelElementsToProject(metadata, open);
            EnvDTE80.DTE2 dte = Package.GetGlobalService(typeof(SDTE)) as EnvDTE80.DTE2;
            Project proj = VStudioUtils.GetActiveProject(dte.DTE);
            proj.Save();
        }

        /// <summary>
        /// Check if the text is not a label or empty
        /// </summary>
        /// <param name="text">text to check</param>
        /// <param name="labelId">label id of the current model</param>
        /// <returns>true if the text is not a label or empty</returns>
        public static bool IsNotLabelOrEmpty(string text, string labelId)
        {
            if (text != "" && !text.Contains(labelId) && text.Substring(0, 1) != "@")
            {   // Label is not empty and does not 
                return true;
            }

            return false;
        }

        /// <summary>
        /// Remove the stop in extensions of other models
        /// </summary>
        /// <param name="labelId">name to be amended</param>
        /// <returns>fixed object name without extension</returns>
        protected static string CorrectLabel(string model, string labelId)
        {
            // Drop any dot from extensions
            labelId = labelId.Replace(".", "");
            // Drop any model extension name from being used
            labelId = labelId.Replace(model, "");

            return labelId;
        }

        public static Microsoft.Dynamics.AX.Metadata.Core.Collections.KeyedObjectCollection<AxMethod> GetAxObjectMethods(object obj)
        {
            System.Reflection.PropertyInfo propertyInfo = obj.GetType().GetProperty("Methods");

            if (propertyInfo != null)
            {
                return (Microsoft.Dynamics.AX.Metadata.Core.Collections.KeyedObjectCollection<AxMethod>)propertyInfo.GetValue(obj);
            }

            return null;
        }

        public static List<string> GetAllowedMethodNames(object objectFrom)
        {
            Microsoft.Dynamics.AX.Metadata.Core.Collections.KeyedObjectCollection<AxMethod> methods = null;
            List<string> allowedMethods = new List<string>();

            if (objectFrom is AxClass axClass)
            {
                methods = axClass.Methods;
            }
            else if (objectFrom is AxTable axTable)
            {
                methods = axTable.Methods;
            }
            else if (objectFrom is AxQuery axQuery)
            {
                methods = axQuery.Methods;
            }
            else if (objectFrom is AxView axView)
            {
                methods = axView.Methods;
            }
            else if (objectFrom is AxDataEntity axDataEntity)
            {
                methods = axDataEntity.Methods;
            }

            if (methods != null)
            {
                foreach (AxMethod method in methods)
                {
                    if (!method.IsFinal && !method.IsExtendedMethod && !method.IsDelegate &&
                        (method.Visibility == Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerVisibility.Public || method.Visibility == Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerVisibility.Protected))
                    {
                        allowedMethods.Add(method.Name);
                    }
                }
            }

            return allowedMethods;
        }

        public static void ApplyComments(string className)
        {
            VSProjectNode vSProjectNode = VStudioUtils.GetSelectedProjectOrFirstActiveProject();
            AxClass clas = vSProjectNode.DesignMetaModelService.GetClass(className);
            if (clas != null)
            {
                IModelReference modelReference = vSProjectNode.GetProjectsModelInfo();

                if (!clas.Declaration.Contains("///"))
                    clas.Declaration = CommentDeclaration(className, clas.GetType().Name, modelReference.Name, clas.Declaration);

                MethodFunc.CommentMethods(clas.Methods);

                vSProjectNode.DesignMetaModelService.CurrentMetadataProvider.Classes.Update(clas, new ModelSaveInfo(modelReference));
            }
        }

        public static string CommentDeclaration(string name, string type, string model, string declaration)
        {
            string classDec = "/// <summary>" + Environment.NewLine;
            classDec += $"/// New {type} {name} created on {DateTime.UtcNow} for model {model} {Environment.NewLine}";
            classDec += "/// </summary>" + Environment.NewLine;

            return $"{classDec}{declaration}";
        }
        /*
        public void CheckSetPrefix()
        {
            string model = VStudioUtils.GetActiveAXProjectModelName();

            Settings settings = VStudioCache.GetSettings(model);

            if (settings.Prefix is null || settings.Prefix == "")
            {
                PrefixForm prefixForm = new PrefixForm(model);
                _ = prefixForm.ShowDialog();
            }
        }
        
        public AxClass CreateCocClass(string classNameFrom)
        {
            VSProjectNode vSProjectNode = VStudioUtils.GetSelectdProjectOrFirstActiveProject();
            AxClass classFrom = vSProjectNode.DesignMetaModelService.GetClass(classNameFrom);
            if (classFrom != null)
            {
                ClassCocForm classCocForm = new ClassCocForm(AxClass.MetaClassId, GetAllowedMethodNames(classFrom.Methods));
                _ = classCocForm.ShowDialog();

                CheckSetPrefix();

                if (classCocForm.closedOk)
                {
                    string className = string.Format($"{classNameFrom}_{VStudioCache.GetSettings(vSProjectNode.GetProjectsModelInfo().Module).Prefix}_Extension");

                    if (vSProjectNode.DesignMetaModelService.GetClass(className) == null)
                    {
                        AxClass classcreated = BuildCocClassFromAxObject(new ObjectAndTypeDetails(classFrom.Name, AxClass.MetaClassId, classFrom), className, classCocForm);

                        if (classcreated != null)
                        {
                            AddclassToProject(classcreated);
                        }
                    }
                    else
                    {
                        _ = MessageBox.Show(string.Format("Coc class {0} already exists for class {1}", className, classNameFrom), "Class already exists", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

            return null;
        }

        public AxClass BuildCocClassFromAxObject(ObjectAndTypeDetails objectAndTypeDetailsFrom, string className, ClassCocForm form)
        {
            VSProjectNode vSProjectNode = VStudioUtils.GetSelectdProjectOrFirstActiveProject();
            IModelReference modelReference = vSProjectNode.GetProjectsModelInfo();

            AxClass classCreate = new AxClass
            {
                Name = className
            };

            foreach (DataGridViewRow row in form.GetDataGridView().Rows)
            {
                if (row.Cells[0] != null && row.Cells[0].Value != null && row.Cells[0].Value.ToString() != "")
                {
                    string methodName = row.Cells[0].Value.ToString();

                    if (row.Cells[0].Value != null)
                    {
                        if (Convert.ToBoolean(row.Cells[1].Value))
                            classCreate.AddMethod(CocMethod(className, objectAndTypeDetailsFrom.GetAxObjectMethods()[methodName], modelReference.Name));
                    }
                }
            }

            classCreate.Declaration = ClassDeclarationCocSource(classCreate.Name, objectAndTypeDetailsFrom.ObjectName, AxClass.MetaClassId, modelReference.Name);

            vSProjectNode.DesignMetaModelService.CurrentMetadataProvider.Classes.Create(classCreate, new ModelSaveInfo(modelReference));

            return classCreate;
        }
        */

        public AxClass BuildEventClassFromAxObject(ObjectAndTypeDetails parent, string className, string prefix)
        {
            VSProjectNode vSProjectNode = VStudioUtils.GetSelectedProjectOrFirstActiveProject();
            IModelReference modelReference = vSProjectNode.GetProjectsModelInfo();

            AxClass classCreate = new AxClass
            {
                Name = className
            };

            AxMethodReturnType returnTypeClass = new AxMethodReturnType
            {
                Type = Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerBaseType.Class,
                TypeName = className
            };

            string variables = string.Concat($"    FormRun {prefix.ToLower()}FormRun;{NewLine}");
            // Class decleration
            classCreate.Declaration = ClassDeclarationSource("public", classCreate.Name, variables, $"Event class for form {parent.ObjectName}");
            // Construct method
            classCreate.AddMethod(Method(parent.ObjectName, Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerVisibility.Public, "construct", ConstructMethodSourceNoComments(className), null, returnTypeClass, modelReference.Name));
            // InitPost
            AxMethodReturnType returnTypeVoid = new AxMethodReturnType
            {
                Type = Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerBaseType.Void,
                TypeName = "void"
            };

            string source = string.Concat(
                $"    public void initPost(){NewLine}",
                "    {" + NewLine,
                $"        {NewLine}",
                "    }" + NewLine);

            classCreate.AddMethod(Method(parent.ObjectName, Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerVisibility.Public, "initPost", source, null, returnTypeVoid, modelReference.Name));

            List<string> parametersList = new List<string>
            {
                "FormRun"
            };

            source = string.Concat(
                $"    public static {className} newParams(FormRun _formRun){NewLine}",
                "    {" + NewLine,
                $"        {className} formHandler = {className}::construct();{NewLine}",
                $"    {NewLine}",
                $"        formHandler.parmFormRun(_formRun);{NewLine}",
                $"    {NewLine}",
                $"        return formHandler;{NewLine}",
                "    }" + NewLine);

            classCreate.AddMethod(Method(parent.ObjectName, Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerVisibility.Public, "newParams", source, parametersList, returnTypeClass, modelReference.Name));

            returnTypeClass.TypeName = "FormRun";

            source = string.Concat(
                $"    public FormRun parmFormRun(FormRun _formRun = {prefix.ToLower()}FormRun){NewLine}",
                "    {" + NewLine,
                $"        {prefix.ToLower()}FormRun = _formRun;{NewLine}",
                $"    {NewLine}",
                $"        return {prefix.ToLower()}FormRun;{NewLine}",
                "    }" + NewLine);

            classCreate.AddMethod(Method(parent.ObjectName, Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerVisibility.Public, "parmFormRun", source, parametersList, returnTypeClass, modelReference.Name));

            // Event method initialized
            parametersList = new List<string>
            {
                "xFormRun",
                "FormEventArgs"
            };

            source = string.Concat(
                $"    [FormEventHandler(formStr({parent.ObjectName}), FormEventType::Initialized)]{NewLine}",
                $"    public static void {parent.ObjectName}_OnInitialized(xFormRun sender, FormEventArgs e){NewLine}",
                "    {" + NewLine,
                $"        {className}::newParams(sender).initPost();{NewLine}",
                "    }" + NewLine);

            classCreate.AddMethod(Method(parent.ObjectName, Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerVisibility.Public, $"{parent.ObjectName}_OnInitialized", source, parametersList, returnTypeVoid, modelReference.Name));

            vSProjectNode.DesignMetaModelService.CurrentMetadataProvider.Classes.Create(classCreate, new ModelSaveInfo(modelReference));

            return classCreate;
        }

        protected void AddClassToProject(AxClass classCreated, bool openItemOnAdd = true)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            List<MetadataReference> classToAdd = new List<MetadataReference>
            {
                new MetadataReference(classCreated.Name, classCreated.GetType())
            };

            VStudioUtils.GetSelectedProjectOrFirstActiveProject().AddModelElementsToProject(classToAdd, openItemOnAdd);

            EnvDTE80.DTE2 dte = (EnvDTE80.DTE2)Package.GetGlobalService(typeof(SDTE));

            Project proj = VStudioUtils.GetActiveProject(dte.DTE);

            proj.Save();
        }

        public static AxMethod CocMethod(string parent, AxMethod methodFrom, string model)
        {
            AxMethod methodTo = new AxMethod
            {
                Visibility = methodFrom.Visibility,
                Name = methodFrom.Name,
            };

            string source = "    " + methodFrom.Visibility.ToString().ToLower();

            if (methodFrom.IsStatic)
                source += " static";

            source += string.Concat(" ", MarshalReturnType(methodFrom.ReturnType), " ", methodFrom.Name, "(");

            List<string> parametersList = new List<string>();
            string parameters = "", cocParameters = "";

            foreach (var parameter in methodFrom.Parameters)
            {   // Default values are not allowed
                if (parameters != "")
                    parameters += ", ";

                if (cocParameters != "")
                    cocParameters += ", ";

                parameters += string.Format("{0} {1}", MarshalParameterType(parameter), parameter.Name, Environment.NewLine);
                cocParameters += parameter.Name;

                parametersList.Add(parameter.Name);
            }
            // Still the original line
            source += string.Concat(parameters, ")", Environment.NewLine);
            // New lines 
            source += "    " + string.Concat("{", Environment.NewLine);
            // Turnary is not functioning correctly
            if (MarshalReturnType(methodFrom.ReturnType) == "void")
                source += "    " + string.Concat("    next ", methodFrom.Name, "(", cocParameters, ");", Environment.NewLine);
            else
                source += "    " + string.Concat("    return next ", methodFrom.Name, "(", cocParameters, ");", Environment.NewLine);

            source += "    }";
            source += "    " + Environment.NewLine;

            source = GetCommentMethod(model, parent, methodTo.Name, parametersList, MarshalReturnType(methodFrom.ReturnType)) + source;

            methodTo.Source = source;

            return methodTo;
        }

        public static AxMethod Method(
            string parent,
            Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerVisibility visibility,
            string name,
            string source,
            List<string> parametersList,
            AxMethodReturnType returnType,
            string model)
        {
            AxMethod method = new AxMethod
            {
                Visibility = visibility,
                Name = name
            };

            method.Source = $"{GetCommentMethod(model, parent, method.Name, parametersList, MarshalReturnType(returnType))}{source}";

            return method;
        }

        private static string GetCommentMethod(string model, string parent, string methodName, List<string> parameters, string returnVar)
        {
            string doubleQuote = "\"";

            string ret = string.Concat("    /// <summary>", Environment.NewLine,
                                       $"    /// Model {model} added object on {parent} named {methodName} method{Environment.NewLine}",
                                       "    /// </summary>", Environment.NewLine);

            if (!(parameters is null))
            {
                foreach (var parm in parameters)
                {
                    ret += string.Concat("    /// <param name = ", doubleQuote, parm, doubleQuote, ">Incoming parameter ", parm, "</param>", Environment.NewLine);
                }
            }

            if (returnVar.ToLower() != "void")
                ret += string.Concat("    /// <returns>Return a ", returnVar, "</returns>", Environment.NewLine);

            return ret;
        }

        private static string MarshalReturnType(AxMethodReturnType returnType)
        {
            switch (returnType.Type)
            {
                case Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerBaseType.AnyType:
                    return returnType.TypeName != "" ? returnType.TypeName : "AnyType";
                case Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerBaseType.Class:
                    return returnType.TypeName != "" ? returnType.TypeName : "Object";
                case Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerBaseType.ClrType:
                    return returnType.TypeName != "" ? returnType.TypeName : "System.Object";
                case Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerBaseType.Container:
                    return returnType.TypeName != "" ? returnType.TypeName : "Container";
                case Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerBaseType.Date:
                    return returnType.TypeName != "" ? returnType.TypeName : "TransDate";
                case Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerBaseType.DateTime:
                    return returnType.TypeName != "" ? returnType.TypeName : "utcDateTime";
                case Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerBaseType.Enum:
                    return returnType.TypeName != "" ? returnType.TypeName : returnType.Type.ToString();
                case Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerBaseType.ExtendedDataType:
                    return returnType.TypeName != "" ? returnType.TypeName : returnType.Type.ToString();
                case Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerBaseType.FormElementType:
                    return returnType.TypeName != "" ? returnType.TypeName : "Object";
                case Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerBaseType.Guid:
                    return returnType.TypeName != "" ? returnType.TypeName : "Guid";
                case Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerBaseType.Int32:
                    return returnType.TypeName != "" ? returnType.TypeName : "int";
                case Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerBaseType.Int64:
                    return returnType.TypeName != "" ? returnType.TypeName : "int64";
                case Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerBaseType.Real:
                    return returnType.TypeName != "" ? returnType.TypeName : "Real";
                case Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerBaseType.Record:
                    return returnType.TypeName != "" ? returnType.TypeName : "Common";
                case Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerBaseType.String:
                    return returnType.TypeName != "" ? returnType.TypeName : "str";
                case Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerBaseType.Time:
                    return returnType.TypeName != "" ? returnType.TypeName : "int";
                case Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerBaseType.VarArg:
                    return returnType.TypeName != "" ? returnType.TypeName : "Object";
                case Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerBaseType.VarType:
                    return returnType.TypeName != "" ? returnType.TypeName : returnType.Type.ToString();
                case Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerBaseType.Void:
                    return returnType.TypeName != "" ? returnType.TypeName : "void";
                default:
                    return "void";
            }
        }

        private static string MarshalParameterType(AxMethodParameter methodVar)
        {
            switch (methodVar.Type)
            {
                case Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerBaseType.AnyType:
                    return methodVar.TypeName != "" ? methodVar.TypeName : "AnyType";
                case Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerBaseType.Class:
                    return methodVar.TypeName != "" ? methodVar.TypeName : "Object";
                case Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerBaseType.ClrType:
                    return methodVar.TypeName != "" ? methodVar.TypeName : "System.Object";
                case Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerBaseType.Container:
                    return methodVar.TypeName != "" ? methodVar.TypeName : "Container";
                case Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerBaseType.Date:
                    return methodVar.TypeName != "" ? methodVar.TypeName : "TransDate";
                case Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerBaseType.DateTime:
                    return methodVar.TypeName != "" ? methodVar.TypeName : "utcDateTime";
                case Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerBaseType.Enum:
                    return methodVar.TypeName != "" ? methodVar.TypeName : methodVar.Type.ToString();
                case Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerBaseType.ExtendedDataType:
                    return methodVar.TypeName != "" ? methodVar.TypeName : methodVar.Type.ToString();
                case Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerBaseType.FormElementType:
                    return methodVar.TypeName != "" ? methodVar.TypeName : "Object";
                case Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerBaseType.Guid:
                    return methodVar.TypeName != "" ? methodVar.TypeName : "Guid";
                case Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerBaseType.Int32:
                    return methodVar.TypeName != "" ? methodVar.TypeName : "int";
                case Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerBaseType.Int64:
                    return methodVar.TypeName != "" ? methodVar.TypeName : "int64";
                case Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerBaseType.Real:
                    return methodVar.TypeName != "" ? methodVar.TypeName : "Real";
                case Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerBaseType.Record:
                    return methodVar.TypeName != "" ? methodVar.TypeName : "Common";
                case Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerBaseType.String:
                    return methodVar.TypeName != "" ? methodVar.TypeName : "str";
                case Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerBaseType.Time:
                    return methodVar.TypeName != "" ? methodVar.TypeName : "int";
                case Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerBaseType.VarArg:
                    return methodVar.TypeName != "" ? methodVar.TypeName : "Object";
                case Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerBaseType.VarType:
                    return methodVar.TypeName != "" ? methodVar.TypeName : methodVar.Type.ToString();
                case Microsoft.Dynamics.AX.Metadata.Core.MetaModel.CompilerBaseType.Void:
                    return methodVar.TypeName != "" ? methodVar.TypeName : "void";
                default:
                    return "void";
            }
        }

        public static string RunMethodSource()
        {
            return string.Concat("    /// <summary>", Environment.NewLine,
                     "    /// Standard class run method", Environment.NewLine,
                     "    /// </summary>", Environment.NewLine,
                     "    public void run()", Environment.NewLine,
                     "    {", Environment.NewLine,
                     "        ", Environment.NewLine,
                     "    }");
        }

        public static string NewMethodSource()
        {
            return string.Concat("    /// <summary>", Environment.NewLine,
                     "    /// New method", Environment.NewLine,
                     "    /// </summary>", Environment.NewLine,
                     "    public void new()", Environment.NewLine,
                     "    {", Environment.NewLine,
                     "        ", Environment.NewLine,
                     "    }");
        }

        public static string ConstructMethodSourceNoComments(string returnObjectName)
        {
            return string.Concat("    public static ", returnObjectName, " construct()", Environment.NewLine,
                                 "    {", Environment.NewLine,
                                 "        return new ", returnObjectName, "();", Environment.NewLine,
                                 "    }" + Environment.NewLine);
        }

        public static string ConstructMethodSource(string returnObjectName, string comment)
        {
            return string.Concat("    /// <summary>", Environment.NewLine,
                                 $"    /// {comment} {Environment.NewLine}",
                                 "    /// </summary>", Environment.NewLine,
                                 "    /// <returns>Returns ", returnObjectName, "</returns>", Environment.NewLine,
                                 "    public static ", returnObjectName, " construct()", Environment.NewLine,
                                 "    {", Environment.NewLine,
                                 "        return new ", returnObjectName, "();", Environment.NewLine,
                                 "    }");
        }

        public static string ClassDeclarationSource(string accessor, string classname, string variables, string comment)
        {
            return string.Concat($"/// <summary>{Environment.NewLine}",
                                 $"/// {comment}{Environment.NewLine}",
                                 $"/// </summary>{Environment.NewLine}",
                                 $"{accessor} class {classname}{Environment.NewLine}",
                                 "{", Environment.NewLine,
                                 $"{variables}{Environment.NewLine}",
                                 "}");
        }

        /*
        public static string ClassDeclarationSource(string accessor, string classname, string variables, ClassForm form, string model)
        {
            return string.Concat("/// <summary>", Environment.NewLine,
                                 $"/// Class {form.Name} created on {DateTime.UtcNow} for model {model}", Environment.NewLine,
                                 "/// </summary>", Environment.NewLine,
                                 accessor, " class ", classname, form.GetExtends().Length > 1 ? " extends " + form.GetExtends() : "",
                                 form.GetImpliments().Length > 1 ? " implements " + form.GetImpliments() : "", Environment.NewLine,
                                 "{", Environment.NewLine,
                                 variables,
                                 "}");
        }
        */

        public static string ClassDeclarationCocSource(string classname, string classNameFrom, string typeGuid, string model)
        {
            return string.Concat("/// <summary>", Environment.NewLine,
                                 $"/// Class {classname} created on {DateTime.UtcNow} for model {model}", Environment.NewLine,
                                 "/// </summary>", Environment.NewLine,
                                 "[ExtensionOf(", FOObjects.TypeGuidToName(typeGuid), "Str(" + classNameFrom + "))]", Environment.NewLine,
                                 "final class ", classname, Environment.NewLine,
                                 "{", Environment.NewLine,
                                 "    ", Environment.NewLine,
                                 "}");
        }

        /*
        public static string ClassDeclarationContractSource(string accessor, string classname, string variables, ClassForm form, string model)
        {
            return string.Concat("/// <summary>", Environment.NewLine,
                                 $"/// Contract class {form.Name} created on {DateTime.UtcNow} for model {model}", Environment.NewLine,
                                 "/// </summary>", Environment.NewLine,
                                 "[DataContractAttribute('", classname, "')]", Environment.NewLine,
                                 accessor, " class ", classname, form.GetExtends().Length > 1 ? " extends " + form.GetExtends() : "",
                                 form.GetImpliments().Length > 1 ? " implements " + form.GetImpliments() : "", Environment.NewLine,
                                 "{", Environment.NewLine,
                                 variables,
                                 "}");
        }
        */

        public static string ParmMethodSource(string methodName, string parmName, string edt, string parent)
        {
            if (edt != "")
            {
                return string.Concat("    /// <summary>", Environment.NewLine,
                                     $"    /// Parameter method {methodName} for class {parent}{Environment.NewLine}",
                                     "    /// </summary>", Environment.NewLine,
                                     "    /// <param name = '_", parmName, "'>Inbound parm ", parmName, "</param>", Environment.NewLine,
                                     "    /// <returns>Returns ", parmName, "</returns>", Environment.NewLine,
                                     "    public ", edt, " ", methodName, "(", edt, " _", parmName, " = ", parmName, ")", Environment.NewLine,
                                     "    {", Environment.NewLine,
                                     "        ", parmName, " = _", parmName, ";", Environment.NewLine,
                                     "        return ", parmName, ";", Environment.NewLine,
                                     "    }", Environment.NewLine);
            }
            else
            {
                return string.Concat("    /// <summary>", Environment.NewLine,
                                     $"    /// Parameter method {methodName} for class {parent}{Environment.NewLine}",
                                     "    /// </summary>", Environment.NewLine,
                                     "    public void ", methodName, "()", Environment.NewLine,
                                     "    {", Environment.NewLine,
                                     "        ", Environment.NewLine,
                                     "    }", Environment.NewLine);
            }
        }

        public static string ParmMethodContractSource(string methodName, string parmName, string edt, string model)
        {
            return string.Concat("    /// <summary>", Environment.NewLine,
                                 $"    /// Parameter method {methodName} for class {model}{Environment.NewLine}",
                                 "    /// </summary>", Environment.NewLine,
                                 "    /// <param name = '_", parmName, "'>Inbound parm ", parmName,"</param>", Environment.NewLine,
                                 "    /// <returns>Returns ", parmName, "</returns>", Environment.NewLine,
                                 "    [DataMemberAttribute('", char.ToUpper(parmName.First()) + parmName.Substring(1), "')] ", Environment.NewLine,
                                 "    public ", edt, " ", methodName, "(", edt, " _", parmName, " = ", parmName, ")", Environment.NewLine,
                                 "    {", Environment.NewLine,
                                 "        ", parmName, " = _", parmName, ";", Environment.NewLine,
                                 "        return ", parmName, ";", Environment.NewLine,
                                 "    }", Environment.NewLine);
        }
    }
}
