using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.Dynamics.Framework.Tools.ProjectSystem;

namespace Functions_for_Dynamics_Operations.ToolbaxCode
{
    internal class EnumsCode : GenToolbox
    {
        private const string V = @"application/xml",
            V1 = @"application/json",
            V2 = @"application/x-www-form-urlencoded";

        public EnumsCode(VSProjectNode vSProjectNode, string prefix)
        {
            VSProjectNode = VStudioUtils.GetFirstActiveVSProjectNode();
            Prefix = prefix;
        }

        public void CreateEnums()
        {
            // Create the enums
            CreateErrorTypeCode();
            CreateMediaTypeCode();
        }

        public void CreateErrorTypeCode()
        {
            // Add the Enum for the error types
            AxEnum errorEnum = VStudioUtils.GetDesignMetaModelService().GetEnum($"{Prefix}ExceptionType");
            if (errorEnum == null)
            {
                AxEnum axEnum = new AxEnum
                {
                    Name = $"{Prefix}ExceptionType",
                    Label = "Exception Type",
                    Help = "Exception Type for the request",
                    IsExtensible = true,
                    UseEnumValue = Microsoft.Dynamics.AX.Metadata.Core.MetaModel.NoYes.No
                };

                axEnum.AddEnumValue(new AxEnumValue() { Name = "All", Value = 0, Label = "All" });
                axEnum.AddEnumValue(new AxEnumValue() { Name = "Errors", Value = 1, Label = "Errors" });
                axEnum.AddEnumValue(new AxEnumValue() { Name = "ErrorsAndWarnings", Value = 2, Label = "Errors and Warnings" });
                // Save the enum
                AddToProject(axEnum);
            }
            else
            {
                VStudioUtils.LogToGenOutput($"Enum: {Prefix}ExceptionType already exists.{Environment.NewLine}");
            }
        }

        public void CreateMediaTypeCode()
        {
            AxEnum mediaTypeEnum = VStudioUtils.GetDesignMetaModelService().GetEnum($"{Prefix}MediaType");
            if (mediaTypeEnum == null)
            {
                AxEnum axEnum = new AxEnum
                {
                    Name = $"{Prefix}MediaType",
                    Label = "Media Type",
                    Help = "Media Type for the request",
                    IsExtensible = true,
                    UseEnumValue = Microsoft.Dynamics.AX.Metadata.Core.MetaModel.NoYes.No
                };
                // Add the enum values
                axEnum.AddEnumValue(new AxEnumValue() { Name = "AppXml", Value = 0, Label = V });
                axEnum.AddEnumValue(new AxEnumValue() { Name = "AppJson", Value = 1, Label = V1 });
                axEnum.AddEnumValue(new AxEnumValue() { Name = "AppFormUrlEncoded", Value = 2, Label = V2 });
                // Save the enum
                AddToProject(axEnum);
            }
            else
            {
                VStudioUtils.LogToGenOutput($"Enum: {Prefix}MediaType already exists.{Environment.NewLine}");
            }
        }

        internal void AddToProject(AxEnum axEnum)
        {
            VStudioUtils.LogToGenOutput($"Creating enum: {axEnum.Name}{Environment.NewLine}");
            // Need to create the object in the model
            VSProjectNode.DesignMetaModelService.CurrentMetadataProvider.Enums.Create(axEnum, new ModelSaveInfo(VSProjectNode.GetProjectsModelInfo()));
            // Save the enum to the project
            new ClassFunc().SaveToProject(axEnum.Name, axEnum.GetType(), false);
        }
    }
}
