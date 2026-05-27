using Functions_for_Dynamics_Operations.Functions;
using Functions_for_Dynamics_Operations.Utilities;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Core;
using Microsoft.Internal.VisualStudio.PlatformUI;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Functions_for_Dynamics_Operations
{
    /// <summary>
    /// Represents an object type that can be selected for code search
    /// </summary>
    public class ObjectTypeItem : INotifyPropertyChanged
    {
        private bool _isSelected;

        public string DisplayName { get; set; }
        public string FolderName { get; set; }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Interaction logic for CodeSearchControl.
    /// </summary>
    public partial class CodeSearchControl : UserControl
    {
        private ListSortDirection _dir = ListSortDirection.Ascending;
        private string _sortCol = null;
        private List<ObjectTypeItem> _objectTypes;

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeSearchControl"/> class.
        /// </summary>
        /// <summary>Applies the current Visual Studio theme colors to all labels and data grids.</summary>
        public void ApplyVsTheme() => EditorColorHelper.ApplyVsTheme(this);

        /// <summary>
        /// Applies user-configured foreground/background colors to all labels, the root background, and data grids.
        /// </summary>
        public void ApplyColors(System.Drawing.Color foreColor, System.Drawing.Color backColor, System.Drawing.Color gridForeColor, System.Drawing.Color gridBackColor)
        {
            EditorColorHelper.Apply(this, foreColor, backColor, gridForeColor, gridBackColor);
        }

        public CodeSearchControl()
        {
            this.InitializeComponent();

            InitializeObjectTypes();

            SearchText.KeyDown += SearchText_KeyDown;
            // Allow the user to resize
            SearchDataGrid.AllowUserToResizeColumns = true;

            SearchDataGrid.CellMouseDoubleClick += SearchDataGrid_CellMouseDoubleClick;

            SearchDataGrid.ColumnHeaderMouseClick += SearchDataGrid_ColumnHeaderMouseClick;
        }

        private void InitializeObjectTypes()
        {
            _objectTypes = new List<ObjectTypeItem>
            {
                new ObjectTypeItem { DisplayName = "Classes", FolderName = "axclass", IsSelected = true },
                new ObjectTypeItem { DisplayName = "Tables", FolderName = "axtable", IsSelected = true },
                new ObjectTypeItem { DisplayName = "Forms", FolderName = "axform", IsSelected = true },
                new ObjectTypeItem { DisplayName = "Queries", FolderName = "axquery", IsSelected = true },
                new ObjectTypeItem { DisplayName = "Views", FolderName = "axview", IsSelected = true },
                new ObjectTypeItem { DisplayName = "Maps", FolderName = "axmap", IsSelected = true },
                new ObjectTypeItem { DisplayName = "Data Entity Views", FolderName = "axdataentityview", IsSelected = true },
                new ObjectTypeItem { DisplayName = "Form Parts", FolderName = "axformpart", IsSelected = true },
                new ObjectTypeItem { DisplayName = "Info Parts", FolderName = "axinfopart", IsSelected = true },
                new ObjectTypeItem { DisplayName = "Tiles", FolderName = "axtile", IsSelected = true }
            };

            ObjectTypesListBox.ItemsSource = _objectTypes;
        }

        private void SelectAllBtn_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _objectTypes)
            {
                item.IsSelected = true;
            }
        }

        private void DeselectAllBtn_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _objectTypes)
            {
                item.IsSelected = false;
            }
        }

        private List<string> GetSelectedFolders()
        {
            return _objectTypes.Where(x => x.IsSelected).Select(x => x.FolderName).ToList();
        }

        private void SearchDataGrid_ColumnHeaderMouseClick(object sender, System.Windows.Forms.DataGridViewCellMouseEventArgs e)
        {
            var col = SearchDataGrid.Columns[e.ColumnIndex];
            var name = col.DataPropertyName;  // or col.Name
            var list = (List<CodeSearchFound>)SearchDataGrid.DataSource;

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
               ? list.OrderBy(x => typeof(CodeSearchFound)
                                  .GetProperty(name)
                                  .GetValue(x))
                     .ToList()
               : list.OrderByDescending(x => typeof(CodeSearchFound)
                                            .GetProperty(name)
                                            .GetValue(x))
                     .ToList();

            // Rebind
            SearchDataGrid.DataSource = sorted;
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
                    string textLine = GridUtils.GetGridRowValue(SearchDataGrid, e.RowIndex, 2);
                    // Using the line of text in the grid gives a better context and reduces the chance of opening the wrong object
                    new CodeViewUtils(metaModelService, textLine, objectName, objectType).OpenSource();
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
                // Get the selected folders
                List<string> selectedFolders = GetSelectedFolders();

                if (selectedFolders.Count == 0)
                {
                    VStudioUtils.LogToGenOutput($"Please select at least one object type to search");
                    return;
                }

                // This triggers opening of other tools for no reason
                CodeViewUtils.DoNotLaunchOtherTools = true;

                VStudioUtils.LogToGenOutput($"Searching for code : {SearchText.Text}");

                SearchDataGrid.DataSource = null;
                // This is an async task to search code and resetting the Do Not Launch cannot be done here
                Task t = new CodeSearchController(SearchText.Text, selectedFolders).FindCodeAsync(SearchDataGrid);
            }
            catch (ExceptionVsix ex)
            {
                ex.Log();
            }
        }
    }
}