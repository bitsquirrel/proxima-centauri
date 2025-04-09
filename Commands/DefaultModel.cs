using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using MSXML;
using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Task = System.Threading.Tasks.Task;
using System.Linq;
using EnvDTE;
using Microsoft.VisualBasic.ApplicationServices;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;
using System.Windows.Media;
using Microsoft.Dynamics.Framework.Tools.ProjectSupport;
using Functions_for_Dynamics_Operations.Functions;
using System.Text.Json.Nodes;
using Microsoft.Office.Interop.Excel;
using Newtonsoft.Json.Linq;

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

            ModelInfo model = VStudioUtils.GetActiveAXProjectModelInfo();
            if (!model.IsNull())
            {
                if (RuntimeHost.IsCloudHosted())
                {
                    JObject jsonResponse = JObject.Parse(RuntimeHost.GetCloudHostedCurrentConfig());

                    jsonResponse["DefaultModelForNewProjects"] = model.Name;

                    RuntimeHost.SaveCloudHostedCurrentConfig(jsonResponse.ToString());
                }
                else
                {
                    Microsoft.Dynamics.Framework.Tools.Configuration.DevelopmentConfiguration developmentConfiguration = new Microsoft.Dynamics.Framework.Tools.Configuration.DevelopmentConfiguration
                    {
                        DefaultModelForNewProjects = model.Name
                    };

                    Microsoft.Dynamics.Framework.Tools.Configuration.ConfigurationHelper.SaveConfiguration("DefaultConfig", developmentConfiguration);
                }
            }
        }
    }
}
