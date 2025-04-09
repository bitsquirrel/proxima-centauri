using EnvDTE;
using Microsoft.Dynamics.AX.Metadata.Core.MetaModel;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.Dynamics.Framework.Tools.Core;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Core;
using Microsoft.Dynamics.Framework.Tools.ProjectSystem.Navigation;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace Functions_for_Dynamics_Operations.Utilities
{
    internal class CodeViewUtils
    {
        public static bool DoNotLaunchOtherTools;

        internal string SearchCode, ObjectName, ObjectType;
        internal IDesignMetaModelService ModelService;

        public CodeViewUtils(IDesignMetaModelService modelService, string searchCode, string objectName, string objectType)
        {
            ModelService = modelService;
            SearchCode = searchCode;
            ObjectName = objectName;
            ObjectType = objectType;
        }

        public void OpenSource()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            DoNotLaunchOtherTools = true;

            Window window = null;

            try
            {
                IMetadataInfoProvider imetadataInfoProvider = AxServiceProvider.GetService<IMetadataInfoProvider>();

                (ModelInfo modelInfo, Type type) = GetModelInfoObject(imetadataInfoProvider);

                string xppSourceFile = imetadataInfoProvider.GetXppSourceFilePath(ObjectName, type, modelInfo.Name);

                string xmlSourceFile = imetadataInfoProvider.GetXmlArtifactFilePath(xppSourceFile);

                window = new NavigationUtil(AxServiceProvider.ServiceProvider).OpenXppSourceFileFromXml(xmlSourceFile);

                /* -- THIS FAILS TO OPEN CERTAIN DOCUMENT TYPES
                DTE dte = (DTE)Package.GetGlobalService(typeof(SDTE));
                window = dte.ItemOperations.OpenFile(xmlSourceFile);
                */
            }
            catch (ExceptionVsix ex)
            {
                ex.Log();
            }
            finally
            {
                DoNotLaunchOtherTools = false;
            }
            
            if (window != null)
            {
                TextDocument doc = (TextDocument)window.Document.Object(nameof(TextDocument));

                var p = doc.StartPoint.CreateEditPoint();
                string text = p.GetText(doc.EndPoint);
                // Fix the case of the search text cause lower was used
                string caseCorrectedSearch = text.Substring(text.ToLower().IndexOf(SearchCode.ToLower()), SearchCode.Length);

                string code = text.Substring(0, text.IndexOf(caseCorrectedSearch) + caseCorrectedSearch.Length);

                EnvDTE.TextSelection selection = (EnvDTE.TextSelection)window.Document.Selection;
                // Highlight the text rather than just placing the cursor
                selection.FindText(caseCorrectedSearch);
            }
        }

        internal (ModelInfo, Type) GetModelInfoObject(IMetadataInfoProvider imetadataInfoProvider)
        {
            Type type = null;

            switch (ObjectType)
            {
                case nameof(AxClass):
                    type = typeof(AxClass);
                    break;
                case nameof(AxTable):
                    type = typeof(AxTable);
                    break;
                case nameof(AxView):
                    type = typeof(AxView);
                    break;
                case nameof(AxQuery):
                    type = typeof(AxQuery);
                    break;
                case nameof(AxMap):
                    type = typeof(AxMap);
                    break;
                case nameof(AxForm):
                    type = typeof(AxForm);
                    break;
                case nameof(AxDataEntity):
                    type = typeof(AxDataEntity);
                    break;
                case nameof(AxDataEntityView):
                    type = typeof(AxDataEntityView);
                    break;
            }

            ModelInfoCollection modelInfos = imetadataInfoProvider.GetModelInfoForArtifact(type, ObjectName);

            return (modelInfos.FirstOrDefault(), type);
        }

        internal (int line, int column) GetCharFromPosition(string text)
        {
            string[] lines = text.Split('\n');

            string lastLine = lines[lines.Length - 1];

            return (lines.Length, lastLine.IndexOf(SearchCode) + 1);
        }
    }
}
