using EnvDTE;
using Functions_for_Dynamics_Operations.Utilities;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace Functions_for_Dynamics_Operations
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(LabelEditor))]
    [ProvideToolWindow(typeof(LabelSearch))]
    [ProvideToolWindow(typeof(BPEditor))]
    [ProvideOptionPage(typeof(OptionPageCustom), "Functions 365", "Options", 0, 0, true)]
    [ProvideToolWindow(typeof(CodeSearch))]
    [ProvideToolWindow(typeof(CreateFormCommand))]
    public sealed class Functions_for_Dynamics_OperationsPackage : AsyncPackage, IVsSolutionEvents
    {
        private IVsSolution solution = null;
        private uint _hSolutionEvents = uint.MaxValue;

        /// <summary>
        /// Functions_for_Dynamics_OperationsPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "28619b35-d8e1-4feb-a99a-9dec64c935a7";

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            await LabelEditorCommand.InitializeAsync(this).ConfigureAwait(false);
            await LabelSearchCommand.InitializeAsync(this).ConfigureAwait(false);
            await CodeSearchCommand.InitializeAsync(this).ConfigureAwait(false);
            await BPEditorCommand.InitializeAsync(this).ConfigureAwait(false);

            await AppExpTableToContractCommand.InitializeAsync(this).ConfigureAwait(true);
            await CreateLabelInCodeCommand.InitializeAsync(this).ConfigureAwait(true);
            await CreateParmMethodCommand.InitializeAsync(this).ConfigureAwait(true);
            await RunBestPracticeCommand.InitializeAsync(this).ConfigureAwait(false);
            await CreateMenuItemCommand.InitializeAsync(this).ConfigureAwait(false);
            await TableToContractCommand.InitializeAsync(this).ConfigureAwait(true);
            await ConstructMethodCommand.InitializeAsync(this).ConfigureAwait(true);
            await FindReferencesCommand.InitializeAsync(this).ConfigureAwait(true);
            await TryCatchFinalCommand.InitializeAsync(this).ConfigureAwait(true);
            await CreateBPFileCommand.InitializeAsync(this).ConfigureAwait(false);
            await FindPrimaryCommand.InitializeAsync(this).ConfigureAwait(false);
            await FindRecIdCommand.InitializeAsync(this).ConfigureAwait(false);
            await CommentsCommand.InitializeAsync(this).ConfigureAwait(true);
            await TryCatchCommand.InitializeAsync(this).ConfigureAwait(true);
            await LabelsCommand.InitializeAsync(this).ConfigureAwait(false);
            await LookupCommand.InitializeAsync(this).ConfigureAwait(true);
            await ExistsCommand.InitializeAsync(this).ConfigureAwait(true);
            await DefaultModel.InitializeAsync(this).ConfigureAwait(false);
            await FindCommand.InitializeAsync(this).ConfigureAwait(true);
            await ForCommand.InitializeAsync(this).ConfigureAwait(true);
            await CreateFormCommand.InitializeAsync(this).ConfigureAwait(false);
            await CreatePrivielegesCommand.InitializeAsync(this).ConfigureAwait(false);
            await ExportModels.InitializeAsync(this).ConfigureAwait(false);
            await SearchLabelInCode.InitializeAsync(this).ConfigureAwait(false);
            await CreateDataEntity.InitializeAsync(this).ConfigureAwait(false);
            await GenerateToolboxCommand.InitializeAsync(this).ConfigureAwait(false);
            await GenLabelsProjectCommand.InitializeAsync(this).ConfigureAwait(false);

            AdviseSolutionEvents();
        }

        public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            // For some weird reason this method is called when opening a window - which is not a tool
            if (!CodeViewUtils.DoNotLaunchOtherTools)
                OnOpenToolWindow();
            // If the do not open is active -0 deactivate it
            CodeViewUtils.DoNotLaunchOtherTools = false;

            return VSConstants.S_OK;
        }

        public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            OnCloseToolWindow();
            return VSConstants.S_OK;
        }

        public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        { return VSConstants.S_OK; }

        public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        { return VSConstants.S_OK; }

        public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        { return VSConstants.S_OK; }

        public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        { return VSConstants.S_OK; }

        public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        { return VSConstants.S_OK; }

        public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        { return VSConstants.S_OK; }

        public int OnBeforeCloseSolution(object pUnkReserved)
        { return VSConstants.S_OK; }

        public int OnAfterCloseSolution(object pUnkReserved)
        { return VSConstants.S_OK; }

        #endregion

        private void AdviseSolutionEvents()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            UnadviseSolutionEvents();

            solution = (IVsSolution)ServiceProvider.GetGlobalServiceAsync(typeof(SVsSolution)).ConfigureAwait(false).GetAwaiter().GetResult();

            solution?.AdviseSolutionEvents(this, out _hSolutionEvents);
        }

        private void UnadviseSolutionEvents()
        {
            // This is not working - the solution must remain open otherwise the events are not triggered
        }

        private void OnOpenToolWindow()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // For now we will not allow the project to open the tool windows
            /*
            StartRunLabelEditorFunc startRunLabel = new StartRunLabelEditorFunc(this);
            // When called from here, do not create a new instance of the editor
            if (startRunLabel.StartRunLabelEditor(false))
            {
                IVsWindowFrame windowFrame = (IVsWindowFrame)startRunLabel.Window.Frame;
                ErrorHandler.ThrowOnFailure(windowFrame.Show());
            }

            StartRunBPEditorFunc startRun = new StartRunBPEditorFunc(this);
            // When called from here, do not create a new instance of the editor
            if (startRun.StartRunBPEditor(false))
            {
                IVsWindowFrame windowFrame = (IVsWindowFrame)startRun.Window.Frame;
                ErrorHandler.ThrowOnFailure(windowFrame.Show());
            }
            */
        }

        private void OnCloseToolWindow()
        {
            // We need to close the tool windows when the project is closed, otherwise the events are triggered randomly opening the tool windows
            new StartRunLabelSearchFunc(this).StopLabelSearch();

            new StartRunLabelEditorFunc(this).StopLabelEditor();

            new StartRunCodeSearchFunc(this).StopCodeSearch();

            new StartRunBPEditorFunc(this).StopBPEditor();
        }
    }
}
