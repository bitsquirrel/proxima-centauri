using System;
using System.ComponentModel;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Core;
using Microsoft.VisualStudio.Shell;

namespace Functions_for_Dynamics_Operations
{
    /// <summary>
    /// A wrapper around Microsoft's <see cref="ILabelService"/> that forwards every call to the
    /// original implementation, EXCEPT <see cref="OpenLabelEditor(string, string)"/> which is
    /// rerouted so the default "Edit labels" button/command in the Dynamics 365 F&amp;O extension
    /// opens our custom multi-language <c>LabelEditorControl</c> tool window instead of the
    /// stock single-language Microsoft label editor.
    /// </summary>
    public class CustomLabelService : ILabelService
    {
        /// <summary>
        /// The original Microsoft label service. All non-overridden calls are delegated to it.
        /// </summary>
        private readonly ILabelService _inner;

        /// <summary>
        /// The hosting async package, used to launch our custom label editor on the UI thread.
        /// </summary>
        private readonly AsyncPackage _package;

        /// <summary>
        /// Creates a new <see cref="CustomLabelService"/> wrapping the supplied Microsoft
        /// <see cref="ILabelService"/> implementation.
        /// </summary>
        /// <param name="inner">The original Microsoft <see cref="ILabelService"/> instance.</param>
        /// <param name="package">The hosting async package (used to show the custom tool window).</param>
        public CustomLabelService(ILabelService inner, AsyncPackage package)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _package = package ?? throw new ArgumentNullException(nameof(package));
        }

        /// <summary>
        /// Intercepts the default label-editor entry point and opens our custom multi-language
        /// editor instead of Microsoft's single-language one.
        /// </summary>
        /// <param name="labelFilePath">Path passed by Microsoft's caller (reserved for future use).</param>
        /// <param name="searchText">Initial search text passed by Microsoft's caller (reserved for future use).</param>
        public void OpenLabelEditor(string labelFilePath, string searchText)
        {
            // Marshal back to the UI thread; some VS callers invoke ILabelService off-thread.
            _ = _package.JoinableTaskFactory.RunAsync(async () =>
            {
                await _package.JoinableTaskFactory.SwitchToMainThreadAsync();

                try
                {
                    StartRunLabelEditorFunc starter = new StartRunLabelEditorFunc(_package);
                    starter.StartRunLabelEditor(true);
                }
                catch (Exception ex)
                {
                    // Don't let an editor-launch failure tear down the calling VS command.
                    System.Diagnostics.Debug.WriteLine(
                        "CustomLabelService.OpenLabelEditor failed: " + ex);
                }
            });
        }

        // --- Pass-through members ------------------------------------------------------------

        public void RefreshContext() => _inner.RefreshContext();

        /// <summary>
        /// Intercepts the property-grid "edit label" button (the "..." button next to label
        /// properties). The default Microsoft <see cref="LabelUITypeEditor"/> calls this method
        /// via <c>CoreUtility.ServiceProvider.GetService(typeof(ILabelService))</c>; if the
        /// returned dialog is <c>null</c> Microsoft simply skips showing its own search dialog,
        /// which is exactly what we want — we open our custom multi-language editor instead.
        /// </summary>
        public ILabelSearchDialog GetLabelSearchDialog(ITypeDescriptorContext context, object value)
        {
            // Launch the custom editor on the UI thread (fire-and-forget; the property editor
            // returns immediately and leaves the property unchanged).
            _ = _package.JoinableTaskFactory.RunAsync(async () =>
            {
                await _package.JoinableTaskFactory.SwitchToMainThreadAsync();

                try
                {
                    StartRunLabelEditorFunc starter = new StartRunLabelEditorFunc(_package);
                    starter.StartRunLabelEditor(true);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "CustomLabelService.GetLabelSearchDialog failed: " + ex);
                }
            });

            // Returning null tells the LabelUITypeEditor not to show Microsoft's search dialog.
            return null;
        }

        public void OpenSearchToolWindow() => _inner.OpenSearchToolWindow();

        public void CloseSearchToolWindow() => _inner.CloseSearchToolWindow();

        public void RegisterSearchToolWindowPackage(Package package) =>
            _inner.RegisterSearchToolWindowPackage(package);

        public bool IsValidLabelId(string labelID) => _inner.IsValidLabelId(labelID);
    }
}
