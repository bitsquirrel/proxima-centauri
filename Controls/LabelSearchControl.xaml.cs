using Functions_for_Dynamics_Operations.Utilities;
using Microsoft.Dynamics.AX.Metadata.Extensions.Reports;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI.WebControls.Expressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace Functions_for_Dynamics_Operations
{
    /// <summary>
    /// Interaction logic for LabelSearchControl.
    /// </summary>
    public partial class LabelSearchControl : System.Windows.Controls.UserControl
    {
        private ListSortDirection _dir = ListSortDirection.Ascending;
        private string _sortCol = null;
        private int RowRightClick;

        /// <summary>
        /// Initializes a new instance of the <see cref="LabelSearchControl"/> class.
        /// </summary>
        public LabelSearchControl(List<string> languages)
        {
            InitializeComponent();

            LanguageCombo.ItemsSource = languages;

            SearchType.ItemsSource = new List<string> { "Contains", "Exact" };
            // Default
            SearchType.SelectedItem = "Contains";

            SearchDataGrid.CellMouseClick += SearchDataGrid_CellMouseClick;
            // Default the selected language
            LanguageCombo.SelectedItem = "en-us";

            SearchText.KeyDown += SearchText_KeyDown;
            // Allow the user to resize
            SearchDataGrid.AllowUserToResizeColumns = true;

            SearchDataGrid.AllowUserToOrderColumns = true;

            SearchDataGrid.ColumnHeaderMouseClick += SearchDataGrid_ColumnHeaderMouseClick;
        }

        private void SearchDataGrid_ColumnHeaderMouseClick(object sender, System.Windows.Forms.DataGridViewCellMouseEventArgs e)
        {
            var col = SearchDataGrid.Columns[e.ColumnIndex];
            var name = col.DataPropertyName;  // or col.Name
            var list = (List<DLabelSearch>)SearchDataGrid.DataSource;

            // Toggle direction if same column clicked
            if (_sortCol == name)
                _dir = _dir == ListSortDirection.Ascending
                       ? ListSortDirection.Descending
                       : ListSortDirection.Ascending;
            else
            {
                _sortCol = name;
                _dir = ListSortDirection.Ascending;
            }

            // Use LINQ to sort
            var sorted = (_dir == ListSortDirection.Ascending)
               ? list.OrderBy(x => typeof(DLabelSearch)
                                  .GetProperty(name)
                                  .GetValue(x))
                     .ToList()
               : list.OrderByDescending(x => typeof(DLabelSearch)
                                            .GetProperty(name)
                                            .GetValue(x))
                     .ToList();

            // Rebind
            SearchDataGrid.DataSource = sorted;
        }

        private void SearchText_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                Search();
            }
        }

        private void SearchDataGrid_CellMouseClick(object sender, System.Windows.Forms.DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right && e.ColumnIndex != -1 && e.RowIndex != -1 && SearchDataGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value != null)
            {
                if (SearchDataGrid.Rows[e.RowIndex].Cells[0].Value != null && SearchDataGrid.Rows[e.RowIndex].Cells[0].Value.GetType() != typeof(DBNull))
                {   // No right click if the language is not filled
                    RowRightClick = e.RowIndex;

                    System.Windows.Forms.ContextMenuStrip mnu = new System.Windows.Forms.ContextMenuStrip();

                    System.Windows.Forms.ToolStripMenuItem copy = new System.Windows.Forms.ToolStripMenuItem("Copy");
                    copy.Click += Copy_Click;

                    System.Windows.Forms.ToolStripMenuItem findFeferences = new System.Windows.Forms.ToolStripMenuItem("Find references");
                    findFeferences.Click += FindReferences_Click; ;

                    mnu.Items.Add(copy);
                    mnu.Items.Add(findFeferences);

                    mnu.Show(new System.Drawing.Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y));
                }
            }
        }

        private void FindReferences_Click(object sender, EventArgs e)
        {
            if (RowRightClick != -1 && SearchDataGrid.Rows[RowRightClick] != null)
            {
                System.Windows.Forms.DataGridViewRow row = SearchDataGrid.Rows[RowRightClick];

                if (row.Cells[1].Value.ToString().Substring(0, 1) == "@" && row.Cells[1].Value.ToString().Contains(row.Cells[0].Value.ToString()))
                    DynaxUtils.FindReferences(string.Format("{0}", row.Cells[1].Value));
                else
                    DynaxUtils.FindReferences(string.Format("@{0}:{1}", row.Cells[0].Value, row.Cells[1].Value));
            }
        }



        private void Copy_Click(object sender, EventArgs e)
        {
            if (RowRightClick != -1 && SearchDataGrid.Rows[RowRightClick] != null)
            {
                System.Windows.Forms.DataGridViewRow row = SearchDataGrid.Rows[RowRightClick];

                if (row.Cells[1].Value.ToString().Substring(0, 1) == "@" && row.Cells[1].Value.ToString().Contains(row.Cells[0].Value.ToString()))
                    System.Windows.Forms.Clipboard.SetText(string.Format("{0}", row.Cells[1].Value));
                else
                    System.Windows.Forms.Clipboard.SetText(string.Format("@{0}:{1}", row.Cells[0].Value, row.Cells[1].Value));
            }
        }

        private void LabelSearchCmd_Click(object sender, RoutedEventArgs e)
        {
            Search();
        }

        internal void Search()
        {
            try
            {
                // This triggers opening of other tools for no reason
                CodeViewUtils.DoNotLaunchOtherTools = true;

                VStudioUtils.LogToGenOutput($"Searching for label text : {SearchText.Text}");
                // Reset when search 
                SearchDataGrid.DataSource = null;
                // This is an async task to search labels and resetting the Do Not Launch cannot be done here
                Task t = new LabelSearchController(SearchDataGrid, LanguageCombo.SelectedItem.ToString(), SearchText.Text, SearchType.SelectedItem.ToString()).FindLabels();
            }
            catch (ExceptionVsix ex)
            {
                ex.Log();
            }
        }
    }
}