using Microsoft.Dynamics.AX.Metadata.MetaModel;

namespace Functions_for_Dynamics_Operations
{
    public class ContractParms
    {
        public string Edt { get; set; }
        public string Name { get; set; }
        public ContractParms()
        {
            Edt = "";
            Name = "";
        }
        internal void Trim()
        {
            Edt = Edt.Trim();
            Name = Name.Trim();
        }
    }

    public class AOTNodeTypeAndName
    {
        public string Types { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }

        public AOTNodeTypeAndName(string[] nodes) 
        {
            Types = nodes.Length >= 3 ? nodes[2] : "";
            Name = nodes.Length >= 4 ? nodes[3] : "";

            PluralToType();
        }

        protected void PluralToType()
        {
            switch (Types)
            {
                case "Tables":
                    Type = "table";
                    break;
                case "Table Extensions":
                    Type = "tableExtension";
                    break;
                case "Views":
                    Type = "view";
                    break;
                case "View Extensions":
                    Type = "viewExtension";
                    break;
                case "Queries":
                    Type = "query";
                    break;
                case "Query Extensions":
                    Type = "queryExtension";
                    break;
                case "Data Entities":
                    Type = "dataEntity";
                    break;
                case "Data Entity Extensions":
                    Type = "dataEntityExtension";
                    break;
                case "Composite Data Entities":
                    Type = "compositeDataEntity";
                    break;
                case "Aggregate Data Entities":
                    Type = "aggregateDataEntity";
                    break;
                case "Maps":
                    Type = "map";
                    break;
                case "TableCollections":
                    Type = "tableCollection";
                    break;
                case "Classes":
                    Type = "class";
                    break;
                case "Macros":
                    Type = "macro";
                    break;
                case "Forms":
                    Type = "form";
                    break;
                case "Form Extensions":
                    Type = "formExtension";
                    break;
                case "Tiles":
                    Type = "tile";
                    break;
                case "Menus":
                    Type = "menu";
                    break;
                case "Menu Extensions":
                    Type = "menuExtension";
                    break;
                case "Menu Items":
                    Type = "menuItem";
                    break;
                case "Menu Item Extensions":
                    Type = "menuItemExtension";
                    break;
                default: 
                    Type = "";
                    break;
            }
        }
    }

    public class FOObjects
    {
        public static string GetObjectTypeGuid(string nameOfActiveDocument)
        {
            nameOfActiveDocument = nameOfActiveDocument.Replace(".xpp", "");

            string objectName = nameOfActiveDocument.Substring(0, nameOfActiveDocument.IndexOf("_"));

            switch (objectName)
            {
                case "AxClass":
                    return AxClass.MetaClassId;
                case "AxTable":
                    return AxTable.MetaClassId;
                case "AxTableExtension":
                    return AxTableExtension.MetaClassId;
                case "AxDataEntity":
                    return AxDataEntity.MetaClassId;
                case "AxDataEntityView":
                    return AxDataEntityView.MetaClassId;
                case "AxEdt":
                    return AxEdt.MetaClassId;
                case "AxEdtDate":
                    return AxEdtDate.MetaClassId;
                case "AxEdtEnum":
                    return AxEdtEnum.MetaClassId;
                case "AxEdtGuid":
                    return AxEdtGuid.MetaClassId;
                case "AxEdtInt":
                    return AxEdtInt.MetaClassId;
                case "AxEdtInt64":
                    return AxEdtInt64.MetaClassId;
                case "AxEdtReal":
                    return AxEdtReal.MetaClassId;
                case "AxEdtString":
                    return AxEdtString.MetaClassId;
                case "AxEdtTime":
                    return AxEdtTime.MetaClassId;
                case "AxEdtUtcDateTime":
                    return AxEdtUtcDateTime.MetaClassId;
                case "AxEnum":
                    return AxEnum.MetaClassId;
                case "AxEnumExtension":
                    return AxEnumExtension.MetaClassId;
                case "AxForm":
                    return AxForm.MetaClassId;
                case "AxFormControl":
                    return AxFormControl.MetaClassId;
                case "AxFormControlExtension":
                    return AxFormControlExtension.MetaClassId;
                case "AxFormDataSource":
                    return AxFormDataSource.MetaClassId;
                case "AxFormDataSourceField":
                    return AxFormDataSourceField.MetaClassId;
                case "AxFormExtension":
                    return AxFormExtension.MetaClassId;
                case "AxFormExtensionControl":
                    return AxFormExtensionControl.MetaClassId;
                case "AxFormPart":
                    return AxFormPart.MetaClassId;
                case "AxMap":
                    return AxMap.MetaClassId;
                case "AxMapExtension":
                    return AxMapExtension.MetaClassId;
                case "AxMenu":
                    return AxMenu.MetaClassId;
                case "AxMenuExtension":
                    return AxMenuExtension.MetaClassId;
                case "AxMenuItem":
                    return AxMenuItem.MetaClassId;
                case "AxMenuItemExtension":
                    return AxMenuItemExtension.MetaClassId;
                case "AxQuery":
                    return AxQuery.MetaClassId;
                default:
                    return "";
            }
        }

        public static string TypeGuidToName(string typeGuid)
        {
            switch (typeGuid) 
            {
                case AxClass.MetaClassId:
                    return "class";
                case AxTable.MetaClassId:
                    return "table";
                case AxTableExtension.MetaClassId:
                    return "tableExtension";
                case AxDataEntity.MetaClassId:
                    return "dataEntity";
                case AxDataEntityView.MetaClassId:
                    return "dataEntity";
                case AxEdt.MetaClassId:
                    return "edt";
                case AxEdtDate.MetaClassId:
                    return "edtDate";
                case AxEdtEnum.MetaClassId:
                    return "edtEnum";
                case AxEdtGuid.MetaClassId:
                    return "edtGuid";
                case AxEdtInt.MetaClassId:
                    return "edtInt";
                case AxEdtInt64.MetaClassId:
                    return "edtInt64";
                case AxEdtReal.MetaClassId:
                    return "edtReal";
                case AxEdtString.MetaClassId:
                    return "edtString";
                case AxEdtTime.MetaClassId:
                    return "edtTime";
                case AxEdtUtcDateTime.MetaClassId:
                    return "edtUtcDateTime";
                case AxEnum.MetaClassId:
                    return "enum";
                case AxEnumExtension.MetaClassId:
                    return "enumExtension";
                case AxForm.MetaClassId:
                    return "form";
                case AxFormControl.MetaClassId:
                    return "formControl";
                case AxFormControlExtension.MetaClassId:
                    return "formControlExtension";
                case AxFormDataSource.MetaClassId:
                    return "formDataSource";
                case AxFormDataSourceField.MetaClassId:
                    return "formDataSourceField";
                case AxFormExtension.MetaClassId:
                    return "formExtension";
                case AxFormExtensionControl.MetaClassId:
                    return "formExtensionControl";
                case AxFormPart.MetaClassId:
                    return "formPart";
                case AxMap.MetaClassId:
                    return "map";
                case AxMapExtension.MetaClassId:
                    return "mapExtension";
                case AxMenu.MetaClassId:
                    return "menu";
                case AxMenuExtension.MetaClassId:
                    return "menuExtension";
                case AxMenuItem.MetaClassId:
                    return "menuItem";
                case AxMenuItemExtension.MetaClassId:
                    return "menuItemExtension";
                case AxQuery.MetaClassId:
                    return "query";
                default:
                    return "";
            }
        }
    }
}
