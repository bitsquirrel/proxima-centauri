using Functions_for_Dynamics_Operations.Functions;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel.Design;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;
using System;

namespace Functions_for_Dynamics_Operations
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class DefaultModel
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 7485;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("f16df604-a4eb-458f-a357-d2ec3e1f92a7");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultModel"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private DefaultModel(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static DefaultModel Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in DefaultModel's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new DefaultModel(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                ModelInfo model = VStudioUtils.GetActiveAXProjectModelInfo();
                if (!model.IsNull())
                {
                    if (RuntimeHost.IsCloudHosted())
                    {
                        JObject jsonResponse = JObject.Parse(RuntimeHost.GetCloudHostedCurrentConfig());

                        jsonResponse["DefaultModelForNewProjects"] = model.Name;

                        RuntimeHost.SaveCloudHostedCurrentConfig(jsonResponse.ToString());

                        MessageBox.Show($"Default model set to {model.Name} in the cloud hosted environment{Environment.NewLine}You need to restart Visual Studio for this to take effect", "Default Model", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                    }
                    else
                    {
                        Microsoft.Dynamics.Framework.Tools.Configuration.DevelopmentConfiguration developmentConfiguration = new Microsoft.Dynamics.Framework.Tools.Configuration.DevelopmentConfiguration
                        {
                            DefaultModelForNewProjects = model.Name
                        };

                        string configFileName = RuntimeHost.GetOneBoxConfigFileName();
                        // Microsoft changed the name of the config file and its internal so we need to manually set it
                        Microsoft.Dynamics.Framework.Tools.Configuration.ConfigurationHelper.SaveConfiguration(configFileName, developmentConfiguration);
                    }
                }
            }
            catch (ExceptionVsix ex)
            {
                ex.Log("Unable to set default model");
            }
        }
    }
}
