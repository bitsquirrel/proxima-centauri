using System;
using System.ComponentModel.Design;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace Functions_for_Dynamics_Operations
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class ExistsCommand : BaseCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 4129;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("89d99d81-7b70-40ce-8965-5c095a127236");

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static ExistsCommand Instance { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExistsCommand"/> class.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private ExistsCommand(AsyncPackage package, OleMenuCommandService commandService)
            : base(package, commandService, CommandSet, CommandId)
        {
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            Instance = await InitializeCommandAsync(package, CommandSet, CommandId,
                (pkg, cmdService) => new ExistsCommand(pkg, cmdService));
        }

        /// <summary>
        /// Executes the command logic.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        protected override void ExecuteCommand(object sender, EventArgs e)
        {
            DTE dte = Package.GetGlobalService(typeof(SDTE)) as DTE;

            TextSelection textSelection = (TextSelection)dte.ActiveDocument.Selection;

            DynType dynType = DynaxUtils.GetObjectTypeAndName(dte.ActiveDocument.Name);

            Microsoft.Dynamics.AX.Metadata.MetaModel.AxTable table = VStudioUtils.GetDesignMetaModelService().CurrentMetaModelService.GetTable(dynType.Name);
            if (!(table is null))
            {   // only works on tables
                DynTableFieldEdt tablefieldedt = (DynaxUtils.GetAXTableFieldAndEdtAltIndex(table) ?? DynaxUtils.GetAXTableFieldAndEdtPriIndex(table)) ?? DynaxUtils.GetAXTableFieldAndEdtRecId(table);

                if (tablefieldedt != null)
                    textSelection.Insert(DynaxUtils.ExistsCode(tablefieldedt), 1);

                dte.ActiveDocument.Save();
            }
        }

        /// <summary>
        /// Gets the error message to log when command execution fails.
        /// </summary>
        /// <returns>Error message string.</returns>
        protected override string GetErrorMessage()
        {
            return "Unable to create exists method";
        }
    }
}
