using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.Dynamics.Framework.Tools.ProjectSystem;

namespace Functions_for_Dynamics_Operations
{
    public class QueryFunc : ClassFunc
    {
        public static new void ApplyComments(string queryName)
        {
            VSProjectNode vSProjectNode = VStudioUtils.GetSelectedProjectOrFirstActiveProject();

            AxQuery query = vSProjectNode.DesignMetaModelService.GetQuery(queryName);
            if (query != null)
            {
                IModelReference modelReference = vSProjectNode.GetProjectsModelInfo();

                if (!query.SourceCode.Declaration.Contains("///"))
                {
                    query.SourceCode.Declaration = ClassFunc.CommentDeclaration(query.Name, query.GetType().Name, modelReference.Name, query.SourceCode.Declaration);
                }

                MethodFunc.CommentMethods(query.Methods);

                vSProjectNode.DesignMetaModelService.CurrentMetadataProvider.Queries.Update(query, new ModelSaveInfo(modelReference));
            }
        }
    }
}
