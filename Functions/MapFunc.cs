using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.Dynamics.Framework.Tools.ProjectSystem;

namespace Functions_for_Dynamics_Operations
{
    /// <summary>
    /// Functions that apply to map objects
    /// </summary>
    class MapFunc : ClassFunc
    {
        public static void GenLabelsForMap(LabelEditorControl labelEditor, AxMap axMap)
        {
            string labelId = $"@{labelEditor.LabelFileCollectionSelected.Id}:";

            if (IsNotLabelOrEmpty(axMap.Label, labelId))
            {
                axMap.Label = labelEditor.AddLabelFromTextInCode($"{axMap.Name}~{axMap.Label}", "", true);
            }

            if (IsNotLabelOrEmpty(axMap.DeveloperDocumentation, labelId))
            {
                axMap.DeveloperDocumentation = labelEditor.AddLabelFromTextInCode($"{axMap.Name}DeveloperDocument~{axMap.DeveloperDocumentation}", "{Locked}", true);
            }

            foreach (var field in axMap.Fields)
            {
                if (IsNotLabelOrEmpty(field.Label, labelId))
                {
                    field.Label = labelEditor.AddLabelFromTextInCode($"{axMap.Name}{field.Name}~{field.Label}", "", true);
                }

                if (IsNotLabelOrEmpty(field.HelpText, labelId))
                {
                    field.HelpText = labelEditor.AddLabelFromTextInCode($"{axMap.Name}{field.Name}Help~{field.HelpText}", "", true);
                }
            }

            foreach (var fieldGroup in axMap.FieldGroups)
            {
                if (IsNotLabelOrEmpty(fieldGroup.Label, labelId))
                {
                    fieldGroup.Label = labelEditor.AddLabelFromTextInCode($"{axMap.Name}{fieldGroup.Name}~{fieldGroup.Label}", "", true);
                }
            }
        }

        /// <summary>
        /// Apply comments to a map
        /// </summary>
        /// <param name="mapName">Name of the map to apply comments to</param>
        public static new void ApplyComments(string mapName)
        {
            VSProjectNode vSProjectNode = VStudioUtils.GetSelectedProjectOrFirstActiveProject();
            AxMap map = vSProjectNode.DesignMetaModelService.GetMap(mapName);
            if (map != null)
            {
                IModelReference modelReference = vSProjectNode.GetProjectsModelInfo();

                if (!map.SourceCode.Declaration.Contains("///"))
                {
                    map.SourceCode.Declaration = ClassFunc.CommentDeclaration(map.Name, map.GetType().Name, modelReference.Name, map.SourceCode.Declaration);
                }

                MethodFunc.CommentMethods(map.Methods);

                vSProjectNode.DesignMetaModelService.CurrentMetadataProvider.Maps.Update(map, new ModelSaveInfo(modelReference));
            }
        }
    }
}
