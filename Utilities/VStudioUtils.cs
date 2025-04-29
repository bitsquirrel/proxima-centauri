using EnvDTE;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Core;
using Microsoft.Dynamics.Framework.Tools.ProjectSupport;
using Microsoft.Dynamics.Framework.Tools.ProjectSystem;
using Microsoft.Dynamics.AX.Data.Management.ADE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Dynamics.AX.Metadata.Service;
using Microsoft.Dynamics.AX.Metadata.Static.Access;
using Microsoft.Dynamics.Framework.Tools.Core;
using Functions_for_Dynamics_Operations.Functions;

namespace Functions_for_Dynamics_Operations
{
    public class TestClass
    {
        public static void Test()
        {
            /*
            Microsoft.Dynamics.Framework.Tools.MetaModel.Core.LicensingUtility.CheckAccessLicense(new ModelInfo());

            IDesignMetaModelService modelService = VStudioUtils.GetDesignMetaModelService();

            ManagedSyncADEWorker managedSyncADEWorker = new ManagedSyncADEWorker(modelService.CurrentConfiguration.BusinessDatabaseConnectionString, 
                modelService.CurrentMetadataProvider,
                LogCallBack);

            List<Microsoft.Dynamics.AX.Data.Management.SyncRequest> syncRequests = new List<Microsoft.Dynamics.AX.Data.Management.SyncRequest>(); 

            foreach (string table in VStudioUtils.GetDesignMetaModelService().CurrentMetadataProvider.Tables.ListObjectsForModel(VStudioUtils.GetActiveAXProjectModelName()))
            {
                syncRequests.Add(new Microsoft.Dynamics.AX.Data.Management.SyncRequest(Microsoft.Dynamics.AX.Data.Management.Enum.SyncRequestType.SyncAll, table));
            }

            Microsoft.Dynamics.AX.Data.Management.ManagedSyncWorkerManager managedSyncWorkerManager = new Microsoft.Dynamics.AX.Data.Management.ManagedSyncWorkerManager(true);
           

            Microsoft.Dynamics.AX.Data.Management.ManagedSyncTableWorker managedSyncTableWorker = new Microsoft.Dynamics.AX.Data.Management.ManagedSyncTableWorker(modelService.CurrentConfiguration.BusinessDatabaseConnectionString,
                modelService.CurrentMetadataProvider);

            IEnumerable<Exception> exceptions = managedSyncTableWorker.SyncIncremental(syncRequests, new List<string>());

            foreach (var ex in exceptions)
            {
                
            }
            */
        }

        public static void LogCallBack(Microsoft.Dynamics.AX.Data.Management.Enum.LogEntryType logEntryType, string log)
        {
            
        }
    }

    public static class Tools
    {
        public static bool IsNull<T>(this T obj)
        {
            return (obj == null);
        }

        public static bool IsNullOrEmpty<T>(this T obj)
        {
            return (obj == null || obj.ToString() == "");
        }
    }

    public class VStudioUtils
    {
        public static void LogToOutput(string text)
        {
            if (Package.GetGlobalService(typeof(SVsOutputWindow)) is IVsOutputWindow outWindow)
            {
                Guid generalPaneGuid = VSConstants.GUID_OutWindowGeneralPane;

                outWindow.GetPane(ref generalPaneGuid, out IVsOutputWindowPane generalPane);
                if (generalPane != null)
                {
                    _ = generalPane.OutputStringThreadSafe(text);
                    // Brings this pane into view
                    generalPane.Activate(); 
                }
            }
        }

        public static void LogToGenOutput(string text)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            IVsOutputWindow outWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;

            // Use e.g. Tools -> Create GUID to make a stable, but unique GUID for your pane.
            // Also, in a real project, this should probably be a static constant, and not a local variable
            Guid customGuid = new Guid("0F44E2D1-F5FA-4d2d-AB30-22BE8ECD9789");
            string customTitle = "9A Developer Tools ( Dynamics 365 F&O )";
            outWindow.CreatePane(ref customGuid, customTitle, 1, 1);

            IVsOutputWindowPane customPane;
            outWindow.GetPane(ref customGuid, out customPane);

            customPane.OutputString($"{Environment.NewLine}{text}");
            customPane.Activate(); // Brings this pane into view
        }

        public static List<string> GetPrivileges()
        {
            List<string> privilegeNames = new List<string>
            {
                ""
            };

            foreach (var privilege in VStudioUtils.FindPrivilegesInProject())
            {
                privilegeNames.Add(privilege.Name);
            }

            return privilegeNames;
        }

        public static List<TSource> ToList<TSource>(IEnumerable<TSource> source)
        {
            if (source == null)
            {
                throw new Exception("source empty");
            }

            return new List<TSource>(source);
        }

        public static Microsoft.Dynamics.Framework.Tools.MetaModel.Core.IDesignMetaModelService GetDesignMetaModelService()
        {
            VSProjectNode projectNode = GetSelectedProjectOrFirstActiveProject();
            if (projectNode != null)
            {
                return projectNode.DesignMetaModelService;
            }

            return null;
        }

        public static OleServiceProvider GetOLEServiceProvider()
        {
            VSProjectNode projectNode = GetSelectedProjectOrFirstActiveProject();
            if (projectNode != null)
            {
                return projectNode.OleServiceProvider;
            }

            return null;
        }

        public static List<DynType> FindMenuItemsInProject()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            List<DynType> menuItems = new List<DynType>();

            try
            {
                if (Package.GetGlobalService(typeof(SDTE)) is DTE dte)
                {
                    EnvDTE.Project project = GetActiveProject(dte);
                    if (project != null && project.Object is VSProjectNode)
                    {
                        OAVSProject oavsProject = (OAVSProject)project;
                        VSProjectNode projectNode = (VSProjectNode)oavsProject.Object;

                        foreach (var itemInProject in oavsProject.ProjectItems)
                        {
                            if (itemInProject is Microsoft.Dynamics.Framework.Tools.ProjectSupport.Automation.OAFolderItem folder)
                            {
                                foreach (var itemInFolder in folder.ProjectItems)
                                {
                                    if (itemInFolder is OAVSProjectFileItem item)
                                    {
                                        DynType menuItem = HandleMenuItems(projectNode, item);
                                        if (menuItem != null)
                                            menuItems.Add(menuItem);
                                    }
                                }
                            }
                            else if (itemInProject is OAVSProjectFileItem oAVSProjectFileItem)
                            {
                                DynType menuItem = HandleMenuItems(projectNode, oAVSProjectFileItem);
                                if (menuItem != null)
                                    menuItems.Add(menuItem);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
                Debug.AutoFlush = true;
                Debug.Indent();
                Debug.WriteLine(ex.ToString());
                Debug.Unindent();
            }

            return menuItems;
        }

        public static OAVSProjectFileItem FindItemInProject(string name)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                if (Package.GetGlobalService(typeof(SDTE)) is DTE dte)
                {
                    Project project = GetActiveProject(dte);
                    if (project != null && project.Object is VSProjectNode)
                    {
                        OAVSProject oavsProject = (OAVSProject)project;
                        VSProjectNode projectNode = (VSProjectNode)oavsProject.Object;

                        foreach (var itemInProject in oavsProject.ProjectItems)
                        {
                            if (itemInProject is Microsoft.Dynamics.Framework.Tools.ProjectSupport.Automation.OAFolderItem folder)
                            {
                                foreach (var itemInFolder in folder.ProjectItems)
                                {
                                    if (itemInFolder is OAVSProjectFileItem)
                                    {
                                        if (itemInFolder is OAVSProjectFileItem item)
                                        {
                                            if (item.Name == name)
                                                return item;
                                        }
                                    }
                                }
                            }
                            else if (itemInProject is OAVSProjectFileItem)
                            {
                                if (itemInProject is OAVSProjectFileItem item)
                                {
                                    if (item.Name == name)
                                        return item;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
                Debug.AutoFlush = true;
                Debug.Indent();
                Debug.WriteLine(ex.ToString());
                Debug.Unindent();
            }

            return null;
        }

        public static List<DynType> FindPrivilegesInProject()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            List<DynType> privileges = new List<DynType>();

            try
            {
                if (Package.GetGlobalService(typeof(SDTE)) is DTE dte)
                {
                    Project project = GetActiveProject(dte);
                    if (project != null && project.Object is VSProjectNode)
                    {
                        OAVSProject oavsProject = (OAVSProject)project;
                        VSProjectNode projectNode = (VSProjectNode)oavsProject.Object;

                        foreach (var itemInProject in oavsProject.ProjectItems)
                        {
                            if (itemInProject is Microsoft.Dynamics.Framework.Tools.ProjectSupport.Automation.OAFolderItem folder)
                            {
                                foreach (var itemInFolder in folder.ProjectItems)
                                {
                                    if (itemInFolder is OAVSProjectFileItem oAVSProjectFileItem)
                                    {
                                        DynType privilege = HandlePrivileges(projectNode, oAVSProjectFileItem);
                                        if (privilege != null)
                                            privileges.Add(privilege);
                                    }
                                }
                            }
                            else if (itemInProject is OAVSProjectFileItem aVSProjectFileItem)
                            {
                                DynType privilege = HandleMenuItems(projectNode, aVSProjectFileItem);
                                if (privilege != null)
                                    privileges.Add(privilege);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
                Debug.AutoFlush = true;
                Debug.Indent();
                Debug.WriteLine(ex.ToString());
                Debug.Unindent();
            }

            return privileges;
        }

        public static DynType HandleMenuItems(VSProjectNode projectNode, OAVSProjectFileItem item)
        {
            VSProjectFileNode vsProjectFileNode = (VSProjectFileNode)item.Object;

            if (vsProjectFileNode.MetadataReference.MetadataType.Name == typeof(AxMenuItemAction).Name)
            {
                return new DynType(typeof(AxMenuItemAction), vsProjectFileNode.Name);
            }
            else if (vsProjectFileNode.MetadataReference.MetadataType.Name == typeof(AxMenuItemActionExtension).Name)
            {
                return new DynType(typeof(AxMenuItemActionExtension), vsProjectFileNode.Name);
            }
            else if (vsProjectFileNode.MetadataReference.MetadataType.Name == typeof(AxMenuItemDisplay).Name)
            {
                return new DynType(typeof(AxMenuItemDisplay), vsProjectFileNode.Name);
            }
            else if (vsProjectFileNode.MetadataReference.MetadataType.Name == typeof(AxMenuItemDisplayExtension).Name)
            {
                return new DynType(typeof(AxMenuItemDisplayExtension), vsProjectFileNode.Name);
            }
            else if (vsProjectFileNode.MetadataReference.MetadataType.Name == typeof(AxMenuItemOutput).Name)
            {
                return new DynType(typeof(AxMenuItemOutput), vsProjectFileNode.Name);
            }
            else if (vsProjectFileNode.MetadataReference.MetadataType.Name == typeof(AxMenuItemOutputExtension).Name)
            {
                return new DynType(typeof(AxMenuItemOutputExtension), vsProjectFileNode.Name);
            }

            return null;
        }

        public static DynType HandlePrivileges(VSProjectNode projectNode, OAVSProjectFileItem item)
        {
            VSProjectFileNode vsProjectFileNode = (VSProjectFileNode)item.Object;

            if (vsProjectFileNode.MetadataReference.MetadataType.Name == typeof(AxSecurityPrivilege).Name)
            {
                return new DynType(typeof(AxSecurityPrivilege), vsProjectFileNode.Name);
            }

            return null;
        }

        public static bool IsFinanceOperationsProject(VSProjectNode projectNode)
        {
            if (projectNode != null && (projectNode.ProjectType == "UnifiedOperations" || projectNode.ProjectType == "FinanceOperations"))
            {
                return true;
            }

            return false;
        }

        public static VSProjectFolderNode GetVSFolder(string folderName)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            foreach (Project project in GetDTEProjects())
            {
                if (project != null && project.Object is VSProjectNode projectNode)
                {
                    OAVSProject oavsProject = (OAVSProject)project;

                    if (projectNode != null && IsFinanceOperationsProject(projectNode))
                    {
                        foreach (var itemInProject in oavsProject.ProjectItems)
                        {
                            if (itemInProject is Microsoft.Dynamics.Framework.Tools.ProjectSupport.Automation.OAFolderItem folder)
                            {
                                if (folder.Name == folderName)
                                    return (VSProjectFolderNode)folder.Object;
                            }
                        }
                    }
                }
            }

            return null;
        }

        public static Microsoft.Dynamics.Framework.Tools.ProjectSupport.Automation.OAFolderItem GetOAFolder(string foldername)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            foreach (Project project in GetDTEProjects())
            {
                if (project.Object is VSProjectNode projectNode)
                {
                    OAVSProject oavsProject = (OAVSProject)project;
                    
                    if (projectNode != null && IsFinanceOperationsProject(projectNode))
                    {
                        foreach (var itemInProject in oavsProject.ProjectItems)
                        {
                            if (itemInProject is Microsoft.Dynamics.Framework.Tools.ProjectSupport.Automation.OAFolderItem)
                            {
                                Microsoft.Dynamics.Framework.Tools.ProjectSupport.Automation.OAFolderItem folder = (Microsoft.Dynamics.Framework.Tools.ProjectSupport.Automation.OAFolderItem)itemInProject;

                                if (folder.Name == foldername)
                                    return folder;
                            }
                        }
                    }
                }
            }

            return null;
        }

        public static List<ModelInfo> GetAXProjectsModelsInfoInSolution()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            List<ModelInfo> models = new List<ModelInfo>();

            foreach (Project project in Microsoft.Dynamics.Framework.Tools.MetaModel.Core.CoreUtility.GetDynamicsProjectsInSolution())
            {
                OAVSProject oavsProject = (OAVSProject)project;
                VSProjectNode projectNode = (VSProjectNode)project.Object;
                if (projectNode != null)
                {
                    if (!models.Contains(projectNode.GetProjectsModelInfo()))
                    {
                        models.Add(projectNode.GetProjectsModelInfo());
                    }  
                }
            }

            return models;
        }

        public static List<LabelFilePath> GetAXProjectsModelsInSolutionLFP()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            List<LabelFilePath> LabelFilePaths = new List<LabelFilePath>();

            foreach (Project project in Microsoft.Dynamics.Framework.Tools.MetaModel.Core.CoreUtility.GetDynamicsProjectsInSolution())
            {
                if ((VSProjectNode)project.Object is VSProjectNode projectNode)
                {
                    ModelInfo modelInfo = projectNode.GetProjectsModelInfo();

                    string modelStore = "", notUsed = "";

                    if (RuntimeHost.IsCloudHosted())
                    {
                        (modelStore, notUsed) = RuntimeHost.GetModelStoreAndFrameworkDirectories();
                    }
                    else
                    {
                        modelStore = projectNode.DesignMetaModelService.CurrentConfiguration.ModelStoreFolder;
                    }

                    LabelFilePaths.Add(new LabelFilePath
                    {
                        FilePath = Path.Combine(modelStore, modelInfo.Key.ToString(), "AxLabelFile"),
                        Model = modelInfo.Module,
                        Id = modelInfo.Id
                    });
                }
            }

            return LabelFilePaths;
        }

        /*
        public static List<LabelFilePath> GetAXProjectsModelsInSolutionActive()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            List<LabelFilePath> lfps = new List<LabelFilePath>();

            foreach (Project project in Microsoft.Dynamics.Framework.Tools.MetaModel.Core.CoreUtility.GetDynamicsProjectsInSolution())
            {
                if ((VSProjectNode)project.Object is VSProjectNode projectNode)
                {
                    ModelInfo modelInfo = projectNode.GetProjectsModelInfo();

#warning Include cloud hosted
                    LabelFilePath lfp = new LabelFilePath
                    {
                        FilePath = Path.Combine(projectNode.DesignMetaModelService.CurrentConfiguration.ModelStoreFolder, modelInfo.Key.ToString(), "AxLabelFile"),
                        Model = modelInfo.Module,
                        Id = modelInfo.Id
                    };

                    lfps.Add(lfp);
                }
            }

            return lfps;
        }
        */

        public static string GetSolutionName()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            SVsSolution solution = (SVsSolution)Package.GetGlobalService(typeof(SVsSolution));
            IVsSolution solutionInterface = solution as IVsSolution;

            bool isSolutionOpen = GetPropertyValue<bool>(solutionInterface, __VSPROPID.VSPROPID_IsSolutionOpen);
            if (isSolutionOpen)
            {
                return GetPropertyValue<string>(solutionInterface, __VSPROPID.VSPROPID_SolutionFileName);
            }

            return "";
        }

        private static T GetPropertyValue<T>(IVsSolution solutionInterface, __VSPROPID solutionProperty)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            T result = default;

            if (solutionInterface.GetProperty((int)solutionProperty, out object value) == VSConstants.S_OK)
            {
                result = (T)value;
            }

            return result;
        }

        public static LabelFilePath GetFirstAXProjectLabelModelInfo()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            LabelFilePath labelFilePath = new LabelFilePath();

            try
            {
                foreach (EnvDTE.Project project in Microsoft.Dynamics.Framework.Tools.MetaModel.Core.CoreUtility.GetDynamicsProjectsInSolution())
                {   
                    VSProjectNode projectNode = (VSProjectNode)project.Object;
                    if (projectNode != null)
                    {
                        ModelInfo modelInfo = projectNode.GetProjectsModelInfo();
                        // Tag the model data
                        labelFilePath.Model = modelInfo.Module;
                        labelFilePath.Id = modelInfo.Id;

                        string modelStore = "", notUsed = "";

                        if (RuntimeHost.IsCloudHosted())
                        {
                            (modelStore, notUsed) = RuntimeHost.GetModelStoreAndFrameworkDirectories();                   
                        }
                        else
                        {
                            modelStore = projectNode.DesignMetaModelService.CurrentConfiguration.ModelStoreFolder;
                        }
                        // The key always consists of both folders of the model
                        labelFilePath.FilePath = Path.Combine(modelStore, modelInfo.Key.ToString(), "AxLabelFile");
                    }
                }
            }
            catch (Exception ex)
            {
                labelFilePath.Exception = ex;
            }

            return labelFilePath;
        }

        public static LabelFilePath GetActiveAXProjectLabelFilePath()
        {
            LabelFilePath lfp = new LabelFilePath();

            try
            {
                VSProjectNode projectNode = GetSelectedProjectOrFirstActiveProject();
                if (projectNode != null)
                {
                    ModelInfo modelInfo = projectNode.GetProjectsModelInfo();

                    string modelStore = "", notUsed = "";

                    if (RuntimeHost.IsCloudHosted())
                    {
                        (modelStore, notUsed) = RuntimeHost.GetModelStoreAndFrameworkDirectories();
                    }
                    else
                    {
                        modelStore = projectNode.DesignMetaModelService.CurrentConfiguration.ModelStoreFolder;
                    }

                    if (modelInfo != null)
                    {
                        lfp.FilePath = Path.Combine(modelStore, modelInfo.Key.ToString(), "AxLabelFile");
                        lfp.Model = modelInfo.Module;
                        lfp.Id = modelInfo.Id;
                    }
                }
            }
            catch (Exception ex)
            {
                lfp.Exception = ex;
            }

            return lfp;
        }

        public static string GetActiveAXProjectModelsFolder()
        {
            try
            {
                VSProjectNode projectNode = GetSelectedProjectOrFirstActiveProject();
                if (projectNode != null)
                {
                    if (RuntimeHost.IsCloudHosted())
                    {
                        // We need to pick up the current active config to find the correct model store folder
                        (string modelStore, string notUsed) = RuntimeHost.GetModelStoreAndFrameworkDirectories();

                        return modelStore;
                    }
                    else
                        return projectNode.DesignMetaModelService.CurrentConfiguration.ModelStoreFolder;
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Exception on GetModelsFolder function - {ex}");
            }

            return "";
        }

        public static OAVSProject GetAOVSProjectNode()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (Package.GetGlobalService(typeof(SDTE)) is DTE dte)
            {
                Project project = GetActiveProject(dte);
                if (project != null && (VSProjectNode)project.Object is VSProjectNode projectNode)
                {
                    if (projectNode != null)
                    {   // Return the Dynamics AX Projects
                        return (OAVSProject)project;
                    }
                }
            }

            return null;
        }

        public static VSProjectFileNode GetVSProjectNodeFile(OAVSProject oAVSProject, string itemName)
        {
            foreach (var projItems in oAVSProject.ProjectItems)
            {
                if (projItems is Microsoft.Dynamics.Framework.Tools.ProjectSupport.Automation.OAFolderItem folderItem)
                {
                    foreach (var item in folderItem.Collection)
                    {
                        if (item is OAVSProjectFileItem oAVS)
                        {
                            if (oAVS.Name == itemName)
                            {
                                return (VSProjectFileNode)oAVS.Object;
                            }
                        }
                    }
                }
            }

            return null;
        }


        public static string GetActiveAXProjectModelName()
        {
            ModelInfo modelInfo = GetActiveAXProjectModelInfo();
            if (modelInfo != null)
            {
                return modelInfo.Module;
            }

            return "";
        }

        public static string GetActiveAXProjectModelDisplayName()
        {
            ModelInfo modelInfo = GetActiveAXProjectModelInfo();
            if (modelInfo != null)
            {
                return modelInfo.DisplayName;
            }

            return "";
        }

        public static ModelInfo GetActiveAXProjectModelInfo()
        {
            VSProjectNode projectNode = GetSelectedProjectOrFirstActiveProject();
            if (projectNode != null)
            {
                return projectNode.GetProjectsModelInfo();
            }

            return null;
        }

        public static VSProjectNode GetSelectedProjectOrFirstActiveProject()
        {
            VSProjectNode vSProjectNode = GetSelectedProjectNode();

            if (vSProjectNode != null)
                return vSProjectNode;

            return GetFirstActiveVSProjectNode();
        }

        public static VSProjectNode GetSelectedProjectNode()
        {
            DTE dte = Package.GetGlobalService(typeof(SDTE)) as DTE;

            foreach (SelectedItem selectedItem in dte.SelectedItems)
            {
                if (selectedItem.Project is Project project)
                {
                    return (VSProjectNode)project.Object;
                }
                else if (selectedItem.ProjectItem is OAVSProjectFileItem fileItem)
                {
                    if (fileItem.Object is VSProjectFileNode projectFileNode)
                    {
                        return (VSProjectNode)projectFileNode.ProjectMgr;
                    }
                }
                else if (selectedItem.ProjectItem is Microsoft.Dynamics.Framework.Tools.ProjectSupport.Automation.OAFolderItem folderItem)
                {
                    if (folderItem.Object is VSProjectFolderNode projectFolderNode)
                    {
                        return (VSProjectNode)projectFolderNode.ProjectMgr;
                    }
                }
            }

            return null;
        }

        public static ProjectNode GetSelectedProjectNodeForBP()
        {
            DTE dte = Package.GetGlobalService(typeof(SDTE)) as DTE;

            foreach (SelectedItem selectedItem in dte.SelectedItems)
            {
                if (selectedItem.Project is Project project)
                {
                    return (ProjectNode)project.Object;
                }
                else if (selectedItem.ProjectItem is OAVSProjectFileItem fileItem)
                {
                    if (fileItem.Object is VSProjectFileNode projectFileNode)
                    {
                        return (VSProjectNode)projectFileNode.ProjectMgr;
                    }
                }
                else if (selectedItem.ProjectItem is Microsoft.Dynamics.Framework.Tools.ProjectSupport.Automation.OAFolderItem folderItem)
                {
                    if (folderItem.Object is VSProjectFolderNode projectFolderNode)
                    {
                        return (VSProjectNode)projectFolderNode.ProjectMgr;
                    }
                }
            }

            return null;
        }

        /*
        public static LabelFilePath GetSelectedProjectLabelFilePath()
        {
            LabelFilePath lfp = new LabelFilePath();

            try
            {
                VSProjectNode projectNode = GetSelectedProjectNode();
                if (projectNode != null)
                {
                    ModelInfo modelInfo = projectNode.GetProjectsModelInfo();

#warning Include cloud hosted

                    if (modelInfo != null)
                    {
                        lfp.FilePath = Path.Combine(projectNode.DesignMetaModelService.CurrentConfiguration.ModelStoreFolder, modelInfo.Key.ToString(), "AxLabelFile");
                        lfp.Model = modelInfo.Module;
                        lfp.Id = modelInfo.Id;
                    }
                }
            }
            catch (Exception ex)
            {
                lfp.Exception = ex;
            }

            return lfp;
        }
        */

        /// <summary>
        /// This will not retrieve the active but first project
        /// </summary>
        /// <returns></returns>
        public static VSProjectNode GetFirstActiveVSProjectNode()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // This may be a project selected or accessed from the menu above
            IVsSolution vsSolution = (IVsSolution)Package.GetGlobalService(typeof(SVsSolution));
            if (vsSolution != null)
            {
                Guid rguidEnumOnlyThisType = Guid.Empty;
                IVsHierarchy[] array = new IVsHierarchy[1];
                uint pceltFetched = 0u;

                _ = ErrorHandler.ThrowOnFailure(vsSolution.GetProjectEnum(1u, ref rguidEnumOnlyThisType, out IEnumHierarchies ppenum));

                ppenum.Reset();

                while (ppenum.Next(1u, array, out pceltFetched) == 0)
                {
                    if (pceltFetched == 1)
                    {
                        _ = ErrorHandler.ThrowOnFailure(array[0].GetProperty(4294967294u, -2027, out var pvar));

                        if (pvar != null)
                        {
                            OAVSProject oAVSProject = pvar as OAVSProject;

                            if (pvar is Project project && new Guid(project.Kind) == new Guid("FC65038C-1B2F-41E1-A629-BED71D161FFF"))
                            {
                                return (VSProjectNode)project.Object;
                            }
                        }
                    }
                }
            }

            return null;
        }

        public static VSProjectNode Del_GetActiveAXProjectNode()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            IVsSolution solution = (IVsSolution)Package.GetGlobalService(typeof(IVsSolution));
            if (solution == null)
                return null;

            DTE dte = Package.GetGlobalService(typeof(SDTE)) as DTE;
            if (dte != null)
            {
                Project project = GetActiveProject(dte);
                if (project != null && project.Object is VSProjectNode)
                {
                    VSProjectNode projectNode = (VSProjectNode)project.Object;
                    if (projectNode != null && IsFinanceOperationsProject(projectNode))
                    {
                        return projectNode;
                    }
                }
            }

            return null;
        }

        public static Project GetActiveProject(DTE dte)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            Project activeProject = null;

            if (dte.Solution != null)
            {
                try
                {   // Failure on project is simply because it is not active ???
                    Array activeSolutionProjects = dte.ActiveSolutionProjects as Array;
                    if (activeSolutionProjects != null && activeSolutionProjects.Length > 0)
                    {
                        activeProject = activeSolutionProjects.GetValue(0) as EnvDTE.Project;
                    }
                }
                catch (System.Exception)
                {
                    return null;
                }

            }

            return activeProject;
        }

        private static IEnumerable<Project> GetDTEProjects()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            foreach (IVsHierarchy hier in GetProjectsInSolution((IVsSolution)Package.GetGlobalService(typeof(IVsSolution))))
            {
                Project project = GetDTEProject(hier);
                if (project != null)
                    yield return project;
            }
        }

        private static Project GetDTEProject(IVsHierarchy hierarchy)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (hierarchy == null)
                throw new ArgumentNullException("hierarchy");

            hierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out object obj);
            return obj as Project;
        }

        private static IEnumerable<IVsHierarchy> GetProjectsInSolution(IVsSolution solution)
        {
            return GetProjectsInSolution(solution, __VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION);
        }

        private static IEnumerable<IVsHierarchy> GetProjectsInSolution(IVsSolution solution, __VSENUMPROJFLAGS flags)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (solution == null)
                yield break;

            Guid guid = Guid.Empty;
            solution.GetProjectEnum((uint)flags, ref guid, out IEnumHierarchies enumHierarchies);
            if (enumHierarchies == null)
                yield break;

            IVsHierarchy[] hierarchy = new IVsHierarchy[1];
            while (enumHierarchies.Next(1, hierarchy, out uint fetched) == VSConstants.S_OK && fetched == 1)
            {
                if (hierarchy.Length > 0 && hierarchy[0] != null)
                    yield return hierarchy[0];
            }
        }
    }
}
