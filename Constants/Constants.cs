using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Functions_for_Dynamics_Operations.Constants
{
    /// <summary>
    /// Constant values
    /// </summary>
    internal class Constants
    {
        /// <summary>
        /// Always create a new label if it already exists
        /// </summary>
        public static bool AlwaysNewLabel;
    }

    internal class Models
    {
        public List<ModelObjectCount> ModelCounts { get; set; }

        internal IDesignMetaModelService DesignMeta;

        public Models(IDesignMetaModelService designMeta)
        {
            ModelCounts = new List<ModelObjectCount>();
            DesignMeta = designMeta;

            LoopModels();
        }

        internal void LoopModels()
        {
            foreach (ModelInfo model in DesignMeta.CurrentMetaModelService.GetModels())
            {
                ModelCounts.Add(new ModelObjectCount(DesignMeta, model));
            }
        }
    }

    internal class ModelObjectCount
    {
        internal IDesignMetaModelService DesignMeta;
        internal ModelInfo ModelInfo;

        public int Enums { get; set; }
        public int EnumExtensions { get; set; }
        public int Edts { get; set; }
        public int EdtExtensions { get; set; }
        public int Tables { get; set; }
        public int TableExtensions { get; set; }
        public int Views { get; set; }
        public int ViewExtensions { get; set; }
        public int Queries { get; set; }
        public int QueryExtensions { get; set; }
        public int DataEntities { get; set; }
        public int DataEntityExtensions { get; set; }
        public int CompositeDataEntities { get; set; }
        public int AggregateDataEntities { get; set; }
        public int Maps { get; set; }
        public int TableCollections { get; set; }
        public int Classes { get; set; }
        public int Forms { get; set; }
        public int FormExtensions { get; set; }
        public int Tiles { get; set; }
        public int Menus { get; set; }
        public int MenuExtensions { get; set; }
        public int ActionMenuItems { get; set; }
        public int OutputMenuItems { get; set; }
        public int DisplayMenuItems { get; set; }
        public int ActionMenuItemExtensions { get; set; }
        public int OutputMenuItemExtensions { get; set; }
        public int DisplayMenuItemExtensions { get; set; }
        public int Perspectives { get; set; }
        public int Kpis { get; set; }
        public int Reports { get; set; }
        public int ReportStyleTemplates { get; set; }
        public int ReportDataSources { get; set; }
        public int ReportImages { get; set; }
        public int WorkflowCategories { get; set; }
        public int WorkflowApprovals { get; set; }
        public int WorkflowApprovalExtenions { get; set; }
        public int WorkflowTasks { get; set; }
        public int WorkflowTaskExtensions { get; set; }
        public int WorkflowAutomatedTasks { get; set; }
        public int WorkflowTypes { get; set; }
        public int WorkflowTypeExtensions { get; set; }
        public int Providers { get; set; }
        public int LabelFiles { get; set; }
        public int Resources { get; set; }
        public int Configurations { get; set; }
        public int SecurityRoles { get; set; }
        public int SecurityRoleExtenions { get; set; }
        public int SecurityDuties { get; set; }
        public int SecurityDutyExtenions { get; set; }
        public int SecurityPrivileges { get; set; }
        public int SecurityPolicies { get; set; }
        public int References { get; set; }
        public int Services { get; set; }
        public int ServiceGroups { get; set; }

        public ModelObjectCount(IDesignMetaModelService designMeta, ModelInfo modelInfo)
        {
            DesignMeta = designMeta;
            ModelInfo = modelInfo;

            Count();
        }

        internal void Count()
        {
            Enums = DesignMeta.CurrentMetadataProvider.Enums.ListObjectsForModel(ModelInfo.Name).Count;
            EnumExtensions = DesignMeta.CurrentMetadataProvider.EnumExtensions.ListObjectsForModel(ModelInfo.Name).Count;
            Edts = DesignMeta.CurrentMetadataProvider.Edts.ListObjectsForModel(ModelInfo.Name).Count;
            EdtExtensions = DesignMeta.CurrentMetadataProvider.EdtExtensions.ListObjectsForModel(ModelInfo.Name).Count;
            Tables = DesignMeta.CurrentMetadataProvider.Tables.ListObjectsForModel(ModelInfo.Name).Count;
            TableExtensions = DesignMeta.CurrentMetadataProvider.TableExtensions.ListObjectsForModel(ModelInfo.Name).Count;
            Views = DesignMeta.CurrentMetadataProvider.Views.ListObjectsForModel(ModelInfo.Name).Count;
            ViewExtensions = DesignMeta.CurrentMetadataProvider.ViewExtensions.ListObjectsForModel(ModelInfo.Name).Count;
            Queries = DesignMeta.CurrentMetadataProvider.Queries.ListObjectsForModel(ModelInfo.Name).Count;
            QueryExtensions = DesignMeta.CurrentMetadataProvider.QuerySimpleExtensions.ListObjectsForModel(ModelInfo.Name).Count;
            DataEntities = DesignMeta.CurrentMetadataProvider.DataEntityViews.ListObjectsForModel(ModelInfo.Name).Count;
            DataEntityExtensions = DesignMeta.CurrentMetadataProvider.DataEntityViewExtensions.ListObjectsForModel(ModelInfo.Name).Count;
            CompositeDataEntities = DesignMeta.CurrentMetadataProvider.CompositeDataEntityViews.ListObjectsForModel(ModelInfo.Name).Count;
            AggregateDataEntities = DesignMeta.CurrentMetadataProvider.AggregateDataEntities.ListObjectsForModel(ModelInfo.Name).Count;
            Maps = DesignMeta.CurrentMetadataProvider.Maps.ListObjectsForModel(ModelInfo.Name).Count;
            TableCollections = DesignMeta.CurrentMetadataProvider.TableExtensions.ListObjectsForModel(ModelInfo.Name).Count;
            Classes = DesignMeta.CurrentMetadataProvider.Classes.ListObjectsForModel(ModelInfo.Name).Count;
            Forms = DesignMeta.CurrentMetadataProvider.Forms.ListObjectsForModel(ModelInfo.Name).Count;
            FormExtensions = DesignMeta.CurrentMetadataProvider.FormExtensions.ListObjectsForModel(ModelInfo.Name).Count;
            Tiles = DesignMeta.CurrentMetadataProvider.Tiles.ListObjectsForModel(ModelInfo.Name).Count;
            Menus = DesignMeta.CurrentMetadataProvider.Menus.ListObjectsForModel(ModelInfo.Name).Count;
            MenuExtensions = DesignMeta.CurrentMetadataProvider.MenuExtensions.ListObjectsForModel(ModelInfo.Name).Count;
            ActionMenuItems = DesignMeta.CurrentMetadataProvider.MenuItemActions.ListObjectsForModel(ModelInfo.Name).Count;
            OutputMenuItems = DesignMeta.CurrentMetadataProvider.MenuItemOutputs.ListObjectsForModel(ModelInfo.Name).Count;
            DisplayMenuItems = DesignMeta.CurrentMetadataProvider.MenuItemDisplays.ListObjectsForModel(ModelInfo.Name).Count;
            ActionMenuItemExtensions = DesignMeta.CurrentMetadataProvider.MenuItemActionExtensions.ListObjectsForModel(ModelInfo.Name).Count;
            OutputMenuItemExtensions = DesignMeta.CurrentMetadataProvider.MenuItemOutputExtensions.ListObjectsForModel(ModelInfo.Name).Count;
            DisplayMenuItemExtensions = DesignMeta.CurrentMetadataProvider.MenuItemDisplayExtensions.ListObjectsForModel(ModelInfo.Name).Count;
            Perspectives = DesignMeta.CurrentMetadataProvider.AggregateCalculatedMeasureTemplateOtherPeriods.ListObjectsForModel(ModelInfo.Name).Count;
            Kpis = DesignMeta.CurrentMetadataProvider.KPIs.ListObjectsForModel(ModelInfo.Name).Count;
            Reports = DesignMeta.CurrentMetadataProvider.Reports.ListObjectsForModel(ModelInfo.Name).Count;
            ReportStyleTemplates = DesignMeta.CurrentMetadataProvider.ReportListStyleTemplates.ListObjectsForModel(ModelInfo.Name).Count;
            ReportDataSources = DesignMeta.CurrentMetadataProvider.ReportExternalDataSources.ListObjectsForModel(ModelInfo.Name).Count;
            ReportImages = DesignMeta.CurrentMetadataProvider.ReportEmbeddedImages.ListObjectsForModel(ModelInfo.Name).Count;
            WorkflowCategories = DesignMeta.CurrentMetadataProvider.WorkflowCategories.ListObjectsForModel(ModelInfo.Name).Count;
            WorkflowApprovals = DesignMeta.CurrentMetadataProvider.WorkflowApprovals.ListObjectsForModel(ModelInfo.Name).Count;
            WorkflowApprovalExtenions = DesignMeta.CurrentMetadataProvider.WorkflowApprovalExtensions.ListObjectsForModel(ModelInfo.Name).Count;
            WorkflowTasks = DesignMeta.CurrentMetadataProvider.WorkflowTasks.ListObjectsForModel(ModelInfo.Name).Count;
            WorkflowTaskExtensions = DesignMeta.CurrentMetadataProvider.WorkflowTaskExtensions.ListObjectsForModel(ModelInfo.Name).Count;
            WorkflowAutomatedTasks = DesignMeta.CurrentMetadataProvider.WorkflowAutomatedTasks.ListObjectsForModel(ModelInfo.Name).Count;
            WorkflowTypes = DesignMeta.CurrentMetadataProvider.WorkflowTemplates.ListObjectsForModel(ModelInfo.Name).Count;
            WorkflowTypeExtensions = DesignMeta.CurrentMetadataProvider.WorkflowTemplateExtensions.ListObjectsForModel(ModelInfo.Name).Count;

            Providers = DesignMeta.CurrentMetadataProvider.WorkflowDueDateCalculationProviders.ListObjectsForModel(ModelInfo.Name).Count;
            Providers += DesignMeta.CurrentMetadataProvider.WorkflowHierarchyAssignmentProviders.ListObjectsForModel(ModelInfo.Name).Count;
            Providers += DesignMeta.CurrentMetadataProvider.WorkflowParticipantAssignmentProviders.ListObjectsForModel(ModelInfo.Name).Count;
            Providers += DesignMeta.CurrentMetadataProvider.WorkflowQueueAssignmentProviders.ListObjectsForModel(ModelInfo.Name).Count;

            LabelFiles = DesignMeta.CurrentMetadataProvider.LabelFiles.ListObjectsForModel(ModelInfo.Name).Count;
            Resources = DesignMeta.CurrentMetadataProvider.Resources.ListObjectsForModel(ModelInfo.Name).Count;
            Configurations = DesignMeta.CurrentMetadataProvider.ConfigurationKeys.ListObjectsForModel(ModelInfo.Name).Count;
            SecurityRoles = DesignMeta.CurrentMetadataProvider.SecurityRoles.ListObjectsForModel(ModelInfo.Name).Count;
            SecurityRoleExtenions = DesignMeta.CurrentMetadataProvider.SecurityRoleExtensions.ListObjectsForModel(ModelInfo.Name).Count;
            SecurityDuties = DesignMeta.CurrentMetadataProvider.SecurityDuties.ListObjectsForModel(ModelInfo.Name).Count;
            SecurityDutyExtenions = DesignMeta.CurrentMetadataProvider.SecurityDutyExtensions.ListObjectsForModel(ModelInfo.Name).Count;
            SecurityPrivileges = DesignMeta.CurrentMetadataProvider.SecurityPrivileges.ListObjectsForModel(ModelInfo.Name).Count;
            SecurityPolicies = DesignMeta.CurrentMetadataProvider.SecurityPolicies.ListObjectsForModel(ModelInfo.Name).Count;
            References = DesignMeta.CurrentMetadataProvider.References.ListObjectsForModel(ModelInfo.Name).Count;
            Services = DesignMeta.CurrentMetadataProvider.Services.ListObjectsForModel(ModelInfo.Name).Count;
            ServiceGroups = DesignMeta.CurrentMetadataProvider.ServiceGroups.ListObjectsForModel(ModelInfo.Name).Count;
        }
    }
}
