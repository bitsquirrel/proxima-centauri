using Microsoft.Dynamics.AX.Metadata.Extensions.CanonicalForm;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.Dynamics.Framework.Tools.BuildTasks;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Core;
using Microsoft.Dynamics.Framework.Tools.ProjectSystem;
using Microsoft.Dynamics.AX.Metadata.XppCompiler;
using System;
using System.Collections.Generic;
using System.Linq;

/*                       Notes                              */
/*  #D365 = Microsoft Dynamics 365 Finance and Operations   */

namespace Functions_for_Dynamics_Operations
{
    /// <summary>
    /// Dynamic type object
    /// </summary>
    public class DynType
    {
        public Type TypeObject { get; set; }
        public string Name { get; set; }

        public DynType(Type typeObject, string name)
        {
            TypeObject = typeObject;
            Name = name;
        }
    }

    /// <summary>
    /// Dynamic table field extended data type object
    /// </summary>
    public class DynTableFieldEdt
    {
        public string Table { get; set; }

        public List<DynField> Fields { get; set; }

        public DynTableFieldEdt()
        {
            Fields = new List<DynField>();
        }
    }

    public class DynField
    {
        public string Field { get; set; }
        public string Edt { get; set; }
    }

    /// <summary>
    /// Dynamic extended data type variable object
    /// </summary>
    public class DynEdtVariable
    {
        public string Edt { get; set; }
        public string Variable { get; set; }
    }

    /// <summary>
    /// Dynamic utilies class
    /// </summary>
    public class DynaxUtils
    {
        /// <summary>
        /// Compile label files
        /// </summary>
        /// <param name="dlabelFileCollection">label file collection in the current model</param>
        public static void CompileLabel(DLabelFile dlabelFile)
        {
            List<ModelElementCompilationDescriptor> modelElementCompilationDescriptors = new List<ModelElementCompilationDescriptor>
            {
                new ModelElementCompilationDescriptor(ModelElementType.LabelFile, dlabelFile.Name)
            };

            CompileElemCompDescr(modelElementCompilationDescriptors, CompileMode.Incremental);
        }

        /// <summary>
        /// Compile label files
        /// </summary>
        /// <param name="dlabelFileCollection">label file collection in the current model</param>
        public static void CompileLabels(DLabelFileCollection dlabelFileCollection)
        {
            List<ModelElementCompilationDescriptor> modelElementCompilationDescriptors = new List<ModelElementCompilationDescriptor>();

            foreach (var labelFile in dlabelFileCollection.Files)
            {
                modelElementCompilationDescriptors.Add(new ModelElementCompilationDescriptor(ModelElementType.LabelFile, labelFile.Name));
            }

            CompileElemCompDescr(modelElementCompilationDescriptors, CompileMode.Incremental);
        }
       
        public static void CompileElemCompDescr(List<ModelElementCompilationDescriptor> modelElementCompilationDescriptors, CompileMode compileMode)
        {
            VSProjectNode projectNode = VStudioUtils.GetSelectedProjectOrFirstActiveProject();

            IDesignMetaModelService designMetaModelService = projectNode.DesignMetaModelService;

            BuildHelper buildHelper = new BuildHelper(designMetaModelService.CurrentMetadataProvider);

            buildHelper.CompileElements(designMetaModelService.CurrentMetadataProvider, System.Threading.CancellationToken.None, modelElementCompilationDescriptors, compileMode, projectNode.GetProjectsModelInfo(), true);
        }

        /// <summary>
        /// Find all references for the label file
        /// </summary>
        /// <param name="labelid"></param>
        internal static void FindReferences(string labelid)
        {
            Microsoft.Dynamics.Framework.Tools.MetaModel.MetaModelUtility.FindReferencesOnLabelId(labelid);
        }

        /// <summary>
        /// Clear label cache of the model instance in the project currently open in Visual Studio
        /// </summary>
        public void ClearLabelFileCache()
        {
            VStudioUtils.GetDesignMetaModelService().CurrentMetadataProvider.LabelFiles.ClearCache();
        }

        /// <summary>
        /// Obtian the DynEdtVariable object from the string provided
        /// </summary>
        /// <param name="selection">string selected to be marshalled to object</param>
        /// <returns>instantiated object from the string provided</returns>
        public static DynEdtVariable GetEdtVariable(string selection)
        {
            DynEdtVariable edtvariable = new DynEdtVariable();

            selection = selection.Replace(";", "");

            foreach (var text in selection.Split(' '))
            {
                if (edtvariable.Edt == null)
                {
                    edtvariable.Edt = text;
                }

                if (edtvariable.Variable == null && edtvariable.Edt != null)
                {
                    edtvariable.Variable = text;
                }
            }

            return edtvariable;
        }

        /// <summary>
        /// Get AX table field with extended data type information
        /// </summary>
        /// <param name="table">#D365 table</param>
        /// <returns>our object representing the table field and data type</returns>
        public static DynTableFieldEdt GetAXTableFieldAndEdtAltIndex(AxTable table)
        {
            if (table != null)
            {
                foreach (var index in table.Indexes)
                {
                    if (index.AlternateKey == Microsoft.Dynamics.AX.Metadata.Core.MetaModel.NoYes.Yes)
                    {
                        DynTableFieldEdt dynTableFieldEdt = new DynTableFieldEdt { Table = table.Name };

                        foreach (var field in index.Fields)
                        {
                            AxTableField axTableField = table.Fields.FirstOrDefault(a => a.Name == field.Name);

                            dynTableFieldEdt.Fields.Add(new DynField() { Field = axTableField.Name, Edt = axTableField.ExtendedDataType });
                        }

                        return dynTableFieldEdt;
                    }
                }
            }
            // If the index is not found or a cluster
            return null;
        }

        public static DynTableFieldEdt GetAXTableFieldAndEdtFirstIndex(AxTable table)
        {
            if (table != null)
            {
                foreach (var index in table.Indexes)
                {
                    DynTableFieldEdt dynTableFieldEdt = new DynTableFieldEdt { Table = table.Name };

                    foreach (var field in index.Fields)
                    {
                        AxTableField axTableField = table.Fields.FirstOrDefault(a => a.Name == field.Name);

                        dynTableFieldEdt.Fields.Add(new DynField() { Field = axTableField.Name, Edt = axTableField.ExtendedDataType });
                    }

                    return dynTableFieldEdt;
                }
            }
            // If the index is not found or a cluster
            return null;
        }

        /// <summary>
        /// Get AX table field with extended data type information
        /// </summary>
        /// <param name="table">#D365 table</param>
        /// <returns>our object representing the table field and data type</returns>
        public static DynTableFieldEdt GetAXTableFieldAndEdtPriIndex(AxTable table)
        {
            if (table != null)
            {
                foreach (var index in table.Indexes)
                {
                    if (index.Name == table.PrimaryIndex)
                    {   // Use the primary index
                        DynTableFieldEdt dynTableFieldEdt = new DynTableFieldEdt { Table = table.Name };

                        foreach (var field in index.Fields)
                        {
                            AxTableField axTableField = table.Fields.FirstOrDefault(a => a.Name == field.Name);

                            dynTableFieldEdt.Fields.Add(new DynField() { Field = axTableField.Name, Edt = axTableField.ExtendedDataType });
                        }

                        return dynTableFieldEdt;
                    }
                }
            }
            // If the index is not found or a cluster
            return null;
        }

        /// <summary>
        /// Get AX table field with extended data type information
        /// </summary>
        /// <param name="table">#D365 table</param>
        /// <returns>our object representing the table field and data type</returns>
        public static DynTableFieldEdt GetAXTableFieldAndEdtRecId(AxTable table)
        {
            // If the index is not found or a cluster
            return new DynTableFieldEdt { Table = table.Name, Fields = new List<DynField>() { new DynField() { Field = "RecId", Edt = "RefRecId" } } };
        }

        /// <summary>
        /// Obtian the #D365 entrypoint of the type provided
        /// </summary>
        /// <param name="type">The type from #D365</param>
        /// <returns>returns the #D365 endipoint type</returns>
        public static Microsoft.Dynamics.AX.Metadata.Core.MetaModel.EntryPointType GetEntryPointType(Type type)
        {
            if (type == typeof(AxMenuItemAction) || type == typeof(AxMenuItemActionExtension))
                return Microsoft.Dynamics.AX.Metadata.Core.MetaModel.EntryPointType.MenuItemAction;
            else if (type == typeof(AxMenuItemDisplay) || type == typeof(AxMenuItemDisplayExtension))
                return Microsoft.Dynamics.AX.Metadata.Core.MetaModel.EntryPointType.MenuItemDisplay;
            else if (type == typeof(AxMenuItemOutput) || type == typeof(AxMenuItemOutputExtension))
                return Microsoft.Dynamics.AX.Metadata.Core.MetaModel.EntryPointType.MenuItemOutput;

            return Microsoft.Dynamics.AX.Metadata.Core.MetaModel.EntryPointType.None;
        }

        /// <summary>
        /// Get the object type and name from the string provided
        /// </summary>
        /// <param name="objectname">The full object name to be split into the correct values</param>
        /// <returns>the ynamics type we require</returns>
        public static DynType GetObjectTypeAndName(string objectname)
        {
            switch (objectname.Substring(0, 8))
            {
                case "AxClass_":
                    return new DynType(typeof(AxClass), objectname.Replace("AxClass_", "").Replace(".xpp", ""));
                case "AxTable_":
                    return new DynType(typeof(AxTable), objectname.Replace("AxTable_", "").Replace(".xpp", ""));
                case "AxForm_":
                    return new DynType(typeof(AxForm), objectname.Replace("AxForm_", "").Replace(".xpp", ""));
                default:
                    return null;
            }

        }

        public static AxSecurityPrivilege GetAXSecurityPrivilege(VSProjectFileNode projfileNode)
        {
            if (projfileNode.MetadataReference.MetadataType.Name == typeof(AxSecurityPrivilege).Name)
            {
                AxSecurityPrivilege privilege = VStudioUtils.GetDesignMetaModelService().GetSecurityPrivilege(projfileNode.Name);
                if (privilege != null)
                {
                    return privilege;
                }
            }

            return null;
        }

        public static AxSecurityPrivilege GetAXSecurityPrivilege(string name)
        {
            return VStudioUtils.GetDesignMetaModelService().GetSecurityPrivilege(name);
        }

        public static IList<string> GetAXSecurityPrivileges()
        {
            return VStudioUtils.GetDesignMetaModelService().CurrentMetaModelService.GetSecurityPrivilegeNames();
        }

        public static AxClass TableToContract(VSProjectFileNode projfileNode)
        {
            if (projfileNode.MetadataReference.MetadataType.Name == typeof(AxTable).Name)
            {
                AxTable table = VStudioUtils.GetSelectedProjectOrFirstActiveProject().DesignMetaModelService.GetTable(projfileNode.FileName);
                if (table != null)
                {
                    return CreateClass(projfileNode, table.Fields);
                }
            }
            else if (projfileNode.MetadataReference.MetadataType.Name == typeof(AxTableExtension).Name)
            {
                AxTableExtension table = VStudioUtils.GetSelectedProjectOrFirstActiveProject().DesignMetaModelService.GetTableExtension(projfileNode.FileName);
                if (table != null)
                {
                    return CreateClass(projfileNode, table.Fields);
                }
            }
            else if (projfileNode.MetadataReference.MetadataType.Name == typeof(AxDataEntity).Name)
            {
                
            }

            return null;
        }

        public static AxClass CreateClass(VSProjectFileNode projfileNode, Microsoft.Dynamics.AX.Metadata.Core.Collections.KeyedObjectCollection<AxTableField> fields)
        {
            IDesignMetaModelService designMetaModelService = VStudioUtils.GetDesignMetaModelService();
            if (designMetaModelService.GetTable(projfileNode.FileName + "Class") == null)
            {   // Class does not exist yet
                AxClass contract = new AxClass
                {
                    Name = projfileNode.FileName + "Class"
                };

                string variables = "";

                foreach (var field in fields)
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
                        Source = ParmMethodCode(field.Name, dataType)
                    };

                    contract.AddMethod(method);

                    variables += string.Concat("    ", dataType, " ", field.Name, ";", Environment.NewLine);
                }

                // Update the class declaration
                contract.Declaration = ClassDeclarationCode(contract.Name, variables);

                designMetaModelService.CurrentMetadataProvider.Classes.Create(contract, new ModelSaveInfo(projfileNode.MetadataReference.ModelReference));

                return contract;
            }

            return null;
        }

        public static string GetWhiteSpace(int columns)
        {
            return string.Empty.PadLeft(columns - 1);
        }

        public static string EmptyCode()
        {
            return string.Concat("", Environment.NewLine,
                                 "    ", Environment.NewLine,
                                 "    ", Environment.NewLine,
                                 "    ", Environment.NewLine,
                                 "    ");
        }

        public static string ClassDeclarationCode(string classname, string variables)
        {
            return string.Concat("/// <summary>", Environment.NewLine,
                                 "/// Autogenerated contract class", Environment.NewLine,
                                 "/// </summary>", Environment.NewLine,
                                 "[DataContractAttribute('", classname, "')]", Environment.NewLine,
                                 "public class ", classname, Environment.NewLine,
                                 "{", Environment.NewLine,
                                 variables,
                                 "}");
        }

        public static string ParmMethodCode(string name, string edt)
        {
            return string.Concat("    /// <summary>", Environment.NewLine,
                                 "    /// Autogenerated data contract member method", Environment.NewLine,
                                 "    /// </summary>", Environment.NewLine,
                                 "    /// <param name = '_", name, "'></param>", Environment.NewLine,
                                 "    /// <returns>Returns ", name, "</returns>", Environment.NewLine,
                                 "    [DataMemberAttribute('", name, "')] ", Environment.NewLine,
                                 "    public ", edt, " ", name.ToLower(), "(", edt, " _", name.ToLower(), " = ", name, ")", Environment.NewLine,
                                 "    {", Environment.NewLine,
                                 "        ", name, " = _", name, ";", Environment.NewLine,
                                 "        return ", name, ";", Environment.NewLine,
                                 "    }");
        }

        public static string ParmMethodShortCode(string name, string edt)
        {
            return string.Concat("    [DataMemberAttribute('", name, "')] ", Environment.NewLine,
                                 "    public ", edt, " ", name.ToLower(), ";", Environment.NewLine);
        }

        public static string ForLoopCode(int column)
        {
            return string.Concat("for (int i = 0; i < #; i++)", Environment.NewLine,
                                 GetWhiteSpace(column), "{", Environment.NewLine,
                                 GetWhiteSpace(column), "    // Autogenerated code", Environment.NewLine,
                                 GetWhiteSpace(column), "}");
        }

        public static string TryCatchStandardCode(int column)
        {
            return string.Concat("try", Environment.NewLine,
                                 GetWhiteSpace(column), "{", Environment.NewLine,
                                 GetWhiteSpace(column), "    // Do Something cool", Environment.NewLine,
                                 GetWhiteSpace(column), "}", Environment.NewLine,
                                 GetWhiteSpace(column), "catch", Environment.NewLine,
                                 GetWhiteSpace(column), "{", Environment.NewLine,
                                 GetWhiteSpace(column), "    // Handle exception", Environment.NewLine,
                                 GetWhiteSpace(column), "}");
        }

        public static string TryCatchFinalStandardCode(int column)
        {
            return string.Concat("try", Environment.NewLine,
                                 GetWhiteSpace(column), "{", Environment.NewLine,
                                 GetWhiteSpace(column), "    // Do Something cool", Environment.NewLine,
                                 GetWhiteSpace(column), "}", Environment.NewLine,
                                 GetWhiteSpace(column), "catch", Environment.NewLine,
                                 GetWhiteSpace(column), "{", Environment.NewLine,
                                 GetWhiteSpace(column), "    // Handle exception", Environment.NewLine,
                                 GetWhiteSpace(column), "}", Environment.NewLine,
                                 GetWhiteSpace(column), "finally", Environment.NewLine,
                                 GetWhiteSpace(column), "{", Environment.NewLine,
                                 GetWhiteSpace(column), "    // Finalize", Environment.NewLine,
                                 GetWhiteSpace(column), "}");
        }

        public static string TryCatchClrCode(int column)
        {
            return string.Concat("new InteropPermission(InteropKind::ClrInterop).assert();", Environment.NewLine,
                                 GetWhiteSpace(column), "", Environment.NewLine,
                                 GetWhiteSpace(column), "System.Exception ex;", Environment.NewLine,
                                 GetWhiteSpace(column), "", Environment.NewLine,
                                 GetWhiteSpace(column), "try", Environment.NewLine,
                                 GetWhiteSpace(column), "{", Environment.NewLine,
                                 GetWhiteSpace(column), "    // Do Something cool", Environment.NewLine,
                                 GetWhiteSpace(column), "}", Environment.NewLine,
                                 GetWhiteSpace(column), "catch (Exception::CLRError)", Environment.NewLine,
                                 GetWhiteSpace(column), "{", Environment.NewLine,
                                 GetWhiteSpace(column), "    throw error(YourFunctionClass::GetClrError());", Environment.NewLine,
                                 GetWhiteSpace(column), "}", Environment.NewLine,
                                 GetWhiteSpace(column), "catch (ex)", Environment.NewLine,
                                 GetWhiteSpace(column), "{", Environment.NewLine,
                                 GetWhiteSpace(column), "    // Handle standard exception", Environment.NewLine,
                                 GetWhiteSpace(column), "}", Environment.NewLine,
                                 GetWhiteSpace(column), "", Environment.NewLine,
                                 GetWhiteSpace(column), "CodeAccessPermission::revertAssert();");
        }

        public static string ConstructCode(string classname)
        {
            return string.Concat("/// <summary>", Environment.NewLine,
                                 "    /// Autogenerated standard construct method", Environment.NewLine,
                                 "    /// </summary>", Environment.NewLine,
                                 "    /// <returns>Instance of this Class</returns>", Environment.NewLine,
                                 "    public static ", classname, " construct()", Environment.NewLine,
                                 "    {", Environment.NewLine,
                                 "        return new ", classname, "();", Environment.NewLine,
                                 "    }");
        }

        public static string LookupCode()
        {
            return string.Concat("/// <summary>", Environment.NewLine,
                                 "    /// Autogenerated Lookup Method", Environment.NewLine,
                                 "    /// </summary>", Environment.NewLine,
                                 "    /// <param name = '_formControl'>Inbound generic FormControl</param>", Environment.NewLine,
                                 "    public static void lookupMethod(FormControl _formControl)", Environment.NewLine,
                                 "    {", Environment.NewLine,
                                 "        Query q = new Query();", Environment.NewLine,
                                 "        QueryBuildDataSource qbds = q.addDataSource(Tablenum(Common));", Environment.NewLine,
                                 "        qbds.addRange(fieldnum(Common, RecId)).value(queryvalue(0));", Environment.NewLine,
                                 "        ", Environment.NewLine,
                                 "        SysTableLookup sysTableLookup = SysTableLookup::newParameters(Tablenum(Common), _formControl);", Environment.NewLine,
                                 "        SysTableLookup.addLookupfield(fieldnum(Common, RecId));", Environment.NewLine,
                                 "        sysTableLookup.parmQuery(q);", Environment.NewLine,
                                 "        systableLookup.performFormLookup();", Environment.NewLine,
                                 "    }");
        }

        public static string FindCode(DynTableFieldEdt tablefieldedt, string methodName)
        {
            string method = $"/// <summary>{Environment.NewLine}";
            method += $"    /// Standard find method based on the alternate index{Environment.NewLine}";
            method += $"    /// </summary>{Environment.NewLine}";

            foreach (var field in tablefieldedt.Fields)
            {
                method += $"    /// <param name = '_{field.Field.ToLower()}'>Field as part of the alternate index</param>{Environment.NewLine}";
            }

            method += $"    /// <param name = '_update'>Sets the select on update</param>{Environment.NewLine}";
            method += $"    /// <returns>Record Found</returns>{Environment.NewLine}";

            string select = $"            select firstonly {tablefieldedt.Table.ToLower()}";
            string parms = "";
            string ifS = "        if (";

            foreach (var field in tablefieldedt.Fields)
            {
                if (select != "")
                    select += Environment.NewLine;

                if (parms == "")
                {
                    select += $"                where {tablefieldedt.Table.ToLower()}.{field.Field.ToLower()} == _{field.Field.ToLower()}";
                    parms = $"    public static {tablefieldedt.Table} {methodName}({field.Edt} _{field.Field.ToLower()}";
                    ifS += $"_{field.Field.ToLower()}";
                }
                else
                {
                    select += $"                    && {tablefieldedt.Table.ToLower()}.{field.Field.ToLower()} == _{field.Field.ToLower()}";
                    parms += $", {field.Edt} _{field.Field.ToLower()}";
                    ifS += $" && _{field.Field.ToLower()}";
                }
            }

            select += $";{Environment.NewLine}";
            parms += $", boolean _update = false){Environment.NewLine}";

            method += parms;

            method += "    {" + Environment.NewLine;
            method += $"        {tablefieldedt.Table} {tablefieldedt.Table.ToLower()};{Environment.NewLine}";
            method += $"        {Environment.NewLine}";

            ifS += $"){Environment.NewLine}";

            method += ifS;

            method += "        {" + Environment.NewLine;
            method += $"            {tablefieldedt.Table.ToLower()}.selectforupdate(_update);{Environment.NewLine}";
            method += $"        {Environment.NewLine}";

            method += select;

            method += "        }" + Environment.NewLine;
            method += $"        {Environment.NewLine}";
            method += $"        return {tablefieldedt.Table.ToLower()};{Environment.NewLine}";
            method += "    }";

            return method;
        }

        public static string ExistsCode(DynTableFieldEdt tablefieldedt)
        {
            string method = $"/// <summary>{Environment.NewLine}";
            method += $"    /// Standard find method based on the alternate index{Environment.NewLine}";
            method += $"    /// </summary>{Environment.NewLine}";

            foreach (var field in tablefieldedt.Fields)
            {
                method += $"    /// <param name = '_{field.Field.ToLower()}'>Field as part of the alternate index</param>{Environment.NewLine}";
            }

            method += $"    /// <returns>Record exists</returns>{Environment.NewLine}";

            string select = $"            select firstonly RecId from {tablefieldedt.Table.ToLower()}{Environment.NewLine}";
            string parms = "";
            string ifS = "        if (";

            foreach (var field in tablefieldedt.Fields)
            {
                if (parms == "")
                {
                    select += $"                where {tablefieldedt.Table.ToLower()}.{field.Field.ToLower()} == _{field.Field.ToLower()}";
                    parms = $"    public static Boolean exist({field.Edt} _{field.Field.ToLower()}";
                    ifS += $"_{field.Field.ToLower()}";
                }
                else
                {
                    select += $"                    && {tablefieldedt.Table.ToLower()}.{field.Field.ToLower()} == _{field.Field.ToLower()}";
                    parms += $", {field.Edt} _{field.Field.ToLower()}";
                    ifS += $" && {field.Field.ToLower()}";
                }
            }

            select += $";{Environment.NewLine}";
            parms += $"){Environment.NewLine}";

            method += parms;

            method += "    {" + Environment.NewLine;
            method += $"        {tablefieldedt.Table} {tablefieldedt.Table.ToLower()};{Environment.NewLine}";
            method += $"        {Environment.NewLine}";

            ifS += $"){Environment.NewLine}";

            method += ifS;

            method += "        {" + Environment.NewLine;

            method += select;

            method += $"            {Environment.NewLine}";
            method += $"            if ({tablefieldedt.Table.ToLower()}.RecId != 0){Environment.NewLine}";
            method += $"                return true;{Environment.NewLine}";

            method += "        }" + Environment.NewLine;
            method += $"        {Environment.NewLine}";
            method += $"        return false;{Environment.NewLine}";
            method += "    }";

            return method;
        }
    }
}
