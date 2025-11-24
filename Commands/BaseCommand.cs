using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.Threading.Tasks;

namespace Functions_for_Dynamics_Operations
{
    /// <summary>
    /// Base class for all command handlers to eliminate boilerplate code duplication.
    /// Provides common initialization and error handling patterns.
    /// </summary>
    internal abstract class BaseCommand
    {
        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        protected readonly AsyncPackage package;

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        protected IAsyncServiceProvider ServiceProvider => package;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseCommand"/> class.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        /// <param name="commandSet">Command set GUID.</param>
        /// <param name="commandId">Command ID.</param>
        protected BaseCommand(AsyncPackage package, OleMenuCommandService commandService, Guid commandSet, int commandId)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(commandSet, commandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                ExecuteCommand(sender, e);
            }
            catch (ExceptionVsix ex)
            {
                ex.Log(GetErrorMessage());
            }
        }

        /// <summary>
        /// Executes the command logic. Override this method in derived classes.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        protected abstract void ExecuteCommand(object sender, EventArgs e);

        /// <summary>
        /// Gets the error message to log when command execution fails.
        /// Override this method to provide a custom error message.
        /// </summary>
        /// <returns>Error message string.</returns>
        protected virtual string GetErrorMessage()
        {
            return $"Unable to execute {GetType().Name}";
        }

        /// <summary>
        /// Helper method to initialize a command asynchronously.
        /// </summary>
        /// <typeparam name="T">The command type to initialize.</typeparam>
        /// <param name="package">Owner package.</param>
        /// <param name="commandSet">Command set GUID.</param>
        /// <param name="commandId">Command ID.</param>
        /// <param name="factory">Factory function to create the command instance.</param>
        /// <returns>The initialized command instance.</returns>
        protected static async Task<T> InitializeCommandAsync<T>(
            AsyncPackage package,
            Guid commandSet,
            int commandId,
            Func<AsyncPackage, OleMenuCommandService, T> factory) where T : BaseCommand
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            return factory(package, commandService);
        }
    }
}
