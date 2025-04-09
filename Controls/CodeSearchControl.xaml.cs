using Functions_for_Dynamics_Operations.Functions;
using Functions_for_Dynamics_Operations.Utilities;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Core;
using Microsoft.Internal.VisualStudio.PlatformUI;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Functions_for_Dynamics_Operations
{
    /// <summary>
    /// Interaction logic for CodeSearchControl.
    /// </summary>
    public partial class CodeSearchControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeSearchControl"/> class.
        /// </summary>
        public CodeSearchControl()
        {
            this.InitializeComponent();

            SearchText.KeyDown += SearchText_KeyDown;
            // Allow the user to resize
            SearchDataGrid.AllowUserToResizeColumns = true;

            SearchDataGrid.CellMouseDoubleClick += SearchDataGrid_CellMouseDoubleClick;
        }

        private void SearchDataGrid_CellMouseDoubleClick(object sender, System.Windows.Forms.DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left && e.ColumnIndex != -1 && e.RowIndex != -1 && SearchDataGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value != null)
            {
                try
                {
                    IDesignMetaModelService metaModelService = VStudioUtils.GetDesignMetaModelService();

                    string objectType = GridUtils.GetGridRowValue(SearchDataGrid, e.RowIndex, 0);
                    string objectName = GridUtils.GetGridRowValue(SearchDataGrid, e.RowIndex, 1);

                    // string filePath = GridUtils.GetGridRowValue(SearchDataGrid, e.RowIndex, 3);

                    new CodeViewUtils(metaModelService, SearchText.Text, objectName, objectType).OpenSource();
                }
                catch (ExceptionVsix ex)
                {
                    ex.Log();
                }
            }
        }

        private void SearchText_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                SearchForCode();
            }
        }

        private void CodeSearchCmd_Click(object sender, RoutedEventArgs e)
        {
            SearchForCode();
        }

        internal void SearchForCode()
        {
            try
            {
                // This triggers opening of other tools for no reason
                CodeViewUtils.DoNotLaunchOtherTools = true;

                VStudioUtils.LogToGenOutput($"Searching for code : {SearchText.Text}");

                SearchDataGrid.DataSource = null;
                // This is an async task to search code and resetting the Do Not Launch cannot be done here
                Task t = new CodeSearchController(SearchText.Text).FindCodeAsync(SearchDataGrid);
            }
            catch (ExceptionVsix ex)
            {
                ex.Log();
            }
        }
    }
}