using Microsoft.Dynamics.AX.Metadata.Core.MetaModel;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.Dynamics.Framework.Tools.ProjectSupport;
using Microsoft.Dynamics.Framework.Tools.ProjectSystem;
using Microsoft.VisualStudio.Debugger.Interop;
using System;
using System.Collections.Generic;
using System.Deployment.Internal;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Functions_for_Dynamics_Operations.Functions
{
    internal class DataEntityFunc
    {
        internal readonly string Name;

        public DataEntityFunc(string name) { Name = name; }

        public void CreateDataEntity()
        {
            if (Name != null && Name != "")
            {
                VSProjectNode vSProjectNode = VStudioUtils.GetSelectedProjectOrFirstActiveProject();
                AxTable table = vSProjectNode.DesignMetaModelService.GetTable(Name);

                if (table != null)
                {
                    AxDataEntityView view = vSProjectNode.DesignMetaModelService.GetDataEntityView($"{Name}Entity");

                    if (view.IsNull())
                    {
                        view = new AxDataEntityView()
                        {
                            DataManagementEnabled = Microsoft.Dynamics.AX.Metadata.Core.MetaModel.NoYes.Yes,
                            IsPublic = Microsoft.Dynamics.AX.Metadata.Core.MetaModel.NoYes.Yes,
                            DataManagementStagingTable = $"{Name}Staging",
                            PrimaryCompanyContext = "DataAreaId",
                            PublicCollectionName = $"{Name}s",
                            TitleField1 = table.TitleField1,
                            TitleField2 = table.TitleField2,
                            SubscriberAccessLevel = "Unset",
                            PrimaryKey = "EntityKey",
                            PublicEntityName = Name,
                            Name = $"{Name}Entity",
                            Label = table.Label,
                        };

                        CreateDataSource(view);

                        CreateFields(table, view);

                        SetEntityKey(table, view);

                        CreateStagingTable(table, vSProjectNode);

                        vSProjectNode.DesignMetaModelService.CurrentMetadataProvider.DataEntityViews.Create(view, new ModelSaveInfo(vSProjectNode.GetProjectsModelInfo()));
                        // Save it to the project
                        new ClassFunc().SaveToProject(view.Name, view.GetType(), false);

                        CreateAssignPrivilege(vSProjectNode);
                    }
                }
            }
        }

        internal void CreateDataSource(AxDataEntityView view)
        {
            view.ViewMetadata.AddRootDataSource(new AxQuerySimpleRootDataSource()
            {
                DynamicFields = Microsoft.Dynamics.AX.Metadata.Core.MetaModel.QueryFieldListDynamic.Yes,
                Enabled = Microsoft.Dynamics.AX.Metadata.Core.MetaModel.NoYes.Yes,
                Table = Name,
                Name = Name
            });
        }

        internal void CreateFields(AxTable table, AxDataEntityView view)
        {
            foreach (var field in table.Fields)
            {
                if (field.IsSystemGenerated == Microsoft.Dynamics.AX.Metadata.Core.MetaModel.NoYes.No)
                {
                    view.AddField(new AxDataEntityViewMappedField() { Name = field.Name, Label = field.Label, DataSource = table.Name, DataField = field.Name });
                }
            }
        }

        internal void SetEntityKey(AxTable table, AxDataEntityView view)
        {
            DynTableFieldEdt tablefieldedt = GetIndexFromTable(table);

            Microsoft.Dynamics.AX.Metadata.Core.Collections.KeyedObjectCollection<AxDataEntityViewKeyField> fields = new Microsoft.Dynamics.AX.Metadata.Core.Collections.KeyedObjectCollection<AxDataEntityViewKeyField>();

            foreach (var field in tablefieldedt.Fields)
            {
                fields.Add(new AxDataEntityViewKeyField() { DataField = field.Field, Name = field.Field });
            }

            view.Keys.Add(new AxDataEntityViewKey() { Name = "EntityKey", Fields = fields, Enabled = Microsoft.Dynamics.AX.Metadata.Core.MetaModel.NoYes.Yes });
        }

        internal void CreateStagingTable(AxTable table, VSProjectNode vSProjectNode)
        {
            AxTable tableStaging = vSProjectNode.DesignMetaModelService.GetTable($"{Name}Staging");
            if (tableStaging.IsNull())
            {
                tableStaging = new AxTable
                {
                    Name = $"{Name}Staging",
                };

                tableStaging.AddField(new AxTableFieldString() 
                { 
                    Mandatory = Microsoft.Dynamics.AX.Metadata.Core.MetaModel.NoYes.Yes,
                    AllowEdit = Microsoft.Dynamics.AX.Metadata.Core.MetaModel.NoYes.No,
                    ExtendedDataType = "DMFDefinitionGroupName",
                    Name = "DefinitionGroup"
                });

                tableStaging.AddField(new AxTableFieldString() 
                { 
                    Mandatory = Microsoft.Dynamics.AX.Metadata.Core.MetaModel.NoYes.Yes,
                    AllowEdit = Microsoft.Dynamics.AX.Metadata.Core.MetaModel.NoYes.No,
                    ExtendedDataType = "DMFExecutionId",
                    Name = "ExecutionId"
                });

                tableStaging.AddField(new AxTableFieldEnum()
                {        
                    Mandatory = Microsoft.Dynamics.AX.Metadata.Core.MetaModel.NoYes.Yes,
                    AllowEdit = Microsoft.Dynamics.AX.Metadata.Core.MetaModel.NoYes.No,
                    EnumType = "DMFTransferStatus",
                    Name = "TransferStatus"
                });

                tableStaging.AddField(new AxTableFieldEnum()
                {
                    Mandatory = Microsoft.Dynamics.AX.Metadata.Core.MetaModel.NoYes.Yes,
                    AllowEdit = Microsoft.Dynamics.AX.Metadata.Core.MetaModel.NoYes.No,
                    ExtendedDataType = "DMFIsSelected",
                    Name = "IsSelected"
                });

                foreach (var field in table.Fields)
                {
                    if (field.IsSystemGenerated == Microsoft.Dynamics.AX.Metadata.Core.MetaModel.NoYes.No)
                    {
                        tableStaging.AddField(field);
                    }
                }

                DynTableFieldEdt tablefieldedt = GetIndexFromTable(table);

                AxTableRelation relation = new AxTableRelation()
                {
                    Cardinality = Microsoft.Dynamics.AX.Metadata.Core.MetaModel.Cardinality.ExactlyOne,
                    RelatedTableCardinality = Microsoft.Dynamics.AX.Metadata.Core.MetaModel.RelatedTableCardinality.ZeroOne,
                    RelationshipType = Microsoft.Dynamics.AX.Metadata.Core.MetaModel.RelationshipType.Link,
                    RelatedTable = $"{Name}Entity",
                    Name = "DataEntity"
                };

                AxTableIndex axTableIndex = new AxTableIndex()
                {
                     Name = "StagingIdx"
                };

                axTableIndex.Fields.Add(new AxTableIndexField() { DataField = "DefinitionGroup", Name = "DefinitionGroup" });
                axTableIndex.Fields.Add(new AxTableIndexField() { DataField = "ExecutionId", Name = "ExecutionId" });

                foreach (var field in tablefieldedt.Fields)
                {
                    axTableIndex.Fields.Add(new AxTableIndexField() { DataField = field.Field, Name = field.Field });

                    relation.AddConstraint(new AxTableRelationConstraintField() { Name = field.Field, Field = field.Field, RelatedField = field.Field });
                }

                tableStaging.Indexes.Add(axTableIndex);

                tableStaging.Relations.Add(relation);
                // Need to create the object in the model
                vSProjectNode.DesignMetaModelService.CurrentMetadataProvider.Tables.Create(tableStaging, new ModelSaveInfo(vSProjectNode.GetProjectsModelInfo()));
                // Save it to the project
                new ClassFunc().SaveToProject(tableStaging.Name, tableStaging.GetType(), false);
            }
        }

        internal DynTableFieldEdt GetIndexFromTable(AxTable table)
        {
            DynTableFieldEdt tablefieldedt = DynaxUtils.GetAXTableFieldAndEdtPriIndex(table);

            if (tablefieldedt.IsNullOrEmpty())
            {
                tablefieldedt = DynaxUtils.GetAXTableFieldAndEdtAltIndex(table);
            }

            if (tablefieldedt.IsNullOrEmpty())
            {
                tablefieldedt = DynaxUtils.GetAXTableFieldAndEdtFirstIndex(table);
            }

            return tablefieldedt;
        }

        internal void CreateAssignPrivilege(VSProjectNode vSProjectNode)
        {
            AxSecurityPrivilege securityPrivilege = vSProjectNode.DesignMetaModelService.GetSecurityPrivilege($"{Name}EntityMaintain");
            if (securityPrivilege.IsNull())
            {
                _ = new PrivilegeFunc().CreateDataEntityPrivilegeFromName($"{Name}EntityMaintain",
                    $"{Name}Entity",
                    Microsoft.Dynamics.AX.Metadata.Core.MetaModel.AccessGrant.ConstructGrantDelete(),
                    $"{Name}Entity maintain privilege",
                    $"This privilege is to grant maintain access to the {Name}Entity data entity");
            }

            securityPrivilege = vSProjectNode.DesignMetaModelService.GetSecurityPrivilege($"{Name}EntityRead");
            if (securityPrivilege.IsNull())
            {
                _ = new PrivilegeFunc().CreateDataEntityPrivilegeFromName($"{Name}EntityRead",
                    $"{Name}Entity",
                    Microsoft.Dynamics.AX.Metadata.Core.MetaModel.AccessGrant.ConstructGrantRead(),
                    $"{Name}Entity read privilege",
                    $"This privilege is to grant read access to the {Name}Entity data entity");
            }
        }
    }
}
