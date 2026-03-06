using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using Task = System.Threading.Tasks.Task;

namespace Functions_for_Dynamics_Operations
{
    /// <summary>
    /// Hooks into the D365 element designer property window to intercept the built-in label browser
    /// command and redirect it to the 365 Label Editor instead.
    ///
    /// <para>
    /// When a user clicks the label picker button ("...") next to a Label property in the D365
    /// element designer's Properties pane, D365 fires a DTE command identified by
    /// <see cref="D365LabelPickerCommandGuid"/> / <see cref="D365LabelPickerCommandId"/>.
    /// By subscribing to <see cref="CommandEvents.BeforeExecute"/> for that specific command,
    /// this hook can cancel D365's default label browser and open the custom 365 Label Editor.
    /// </para>
    ///
    /// <para>
    /// <b>Discovering the exact command GUID / ID:</b>
    /// If the label picker does not appear to be intercepted, enable <see cref="LogAllCommands"/>
    /// and reproduce the click. The Output window will show the GUID and ID of every fired command
    /// so you can update <see cref="D365LabelPickerCommandGuid"/> and
    /// <see cref="D365LabelPickerCommandId"/> accordingly.
    /// </para>
    /// </summary>
    internal sealed class D365LabelPropertyHook
    {
        /// <summary>
        /// GUID of the D365 element designer properties pane command set.
        /// This matches <c>propertiesLabelCmdSet</c> in the VSCT file.
        /// </summary>
        public static readonly Guid D365LabelPickerCommandGuid = new Guid("A72BD644-1979-4CBC-A620-EA4112198A66");

        /// <summary>
        /// Command ID of the D365 built-in label browser within the properties pane.
        /// Update this value if the interception does not trigger; see the class summary for
        /// instructions on how to discover the correct ID at runtime.
        /// </summary>
        public const int D365LabelPickerCommandId = 1282;

        /// <summary>
        /// When <c>true</c>, every DTE command execution is written to the VS Output window so
        /// that the correct D365 label picker GUID / ID can be identified.
        /// Set to <c>false</c> for production use.
        /// </summary>
        public static bool LogAllCommands = false;

        private readonly AsyncPackage package;

        // Keep a strong reference so the event subscription is not garbage-collected.
        // These fields intentionally persist for the lifetime of the VS package, matching
        // the lifetime of the DTE CommandEvents COM object they wrap.
        private CommandEvents d365LabelCommandEvents;
        private CommandEvents allCommandEvents;

        private D365LabelPropertyHook(AsyncPackage package)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
        }

        /// <summary>
        /// Gets the singleton instance of the hook.
        /// </summary>
        public static D365LabelPropertyHook Instance { get; private set; }

        /// <summary>
        /// Creates and registers the hook. Must be called on the UI thread.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            Instance = new D365LabelPropertyHook(package);
            Instance.Register();
        }

        /// <summary>
        /// Subscribes to the DTE command events that intercept the D365 label picker.
        /// </summary>
        private void Register()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                DTE dte = Package.GetGlobalService(typeof(SDTE)) as DTE;
                if (dte == null)
                {
                    VStudioUtils.LogToGenOutput("D365LabelPropertyHook: DTE service unavailable; label picker interception will not be active.");
                    return;
                }

                // Subscribe to the specific D365 label picker command.
                string guidString = D365LabelPickerCommandGuid.ToString("B").ToUpper();
                d365LabelCommandEvents = dte.Events.CommandEvents[guidString, D365LabelPickerCommandId];
                d365LabelCommandEvents.BeforeExecute += OnD365LabelPickerBeforeExecute;

                // Optionally subscribe to all commands to help discover the correct GUID/ID.
                if (LogAllCommands)
                {
                    allCommandEvents = dte.Events.CommandEvents;
                    allCommandEvents.BeforeExecute += OnAnyCommandBeforeExecute;
                }
            }
            catch (Exception ex)
            {
                VStudioUtils.LogToGenOutput($"D365LabelPropertyHook: Error registering command events - {ex}");
            }
        }

        /// <summary>
        /// Fired before the D365 label picker command executes. Cancels the default D365
        /// label browser and opens the 365 Label Editor instead.
        /// </summary>
        private void OnD365LabelPickerBeforeExecute(
            string guid,
            int id,
            object customIn,
            object customOut,
            ref bool cancelDefault)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                StartRunLabelEditorFunc startRunLabel = new StartRunLabelEditorFunc(package);

                if (startRunLabel.StartRunLabelEditor(true))
                {
                    IVsWindowFrame windowFrame = (IVsWindowFrame)startRunLabel.Window.Frame;
                    Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());

                    // Cancel the D365 default label browser so it does not open on top.
                    cancelDefault = true;
                }
            }
            catch (ExceptionVsix ex)
            {
                ex.Log("D365LabelPropertyHook: Unable to open label editor");
            }
        }

        /// <summary>
        /// Diagnostic handler: logs every DTE command to the Output window.
        /// Only active when <see cref="LogAllCommands"/> is <c>true</c>.
        /// </summary>
        private void OnAnyCommandBeforeExecute(
            string guid,
            int id,
            object customIn,
            object customOut,
            ref bool cancelDefault)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            VStudioUtils.LogToGenOutput($"D365LabelPropertyHook [discovery]: command GUID={guid}, ID={id}");
        }
    }
}
