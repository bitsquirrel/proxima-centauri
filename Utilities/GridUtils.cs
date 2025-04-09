using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Windows.Forms;

namespace Functions_for_Dynamics_Operations
{
    /// <summary>
    /// GridView utilities
    /// </summary>
    public class GridUtils
    {
        public static DataGridViewRow CreateRowFromRawLabel(string language, string id, string text, string description, string descriptionExisted)
        {
            DataGridViewRow row = new DataGridViewRow();

            SetRowCellValue(row, 0, language);
            SetRowCellValue(row, 1, id);
            SetRowCellValue(row, 2, text);
            SetRowCellValue(row, 3, description);
            SetRowCellValue(row, 4, id);
            SetRowCellValue(row, 5, descriptionExisted);

            return row;
        }

        public static string GridRowToRawLabel(DataGridViewRow viewRow)
        {
            return $"{viewRow.Cells[2].Value}={viewRow.Cells[3].Value}{Environment.NewLine} ;{viewRow.Cells[4].Value}{Environment.NewLine}";
        }

        /// <summary>
        /// Set the cell value of a row
        /// </summary>
        /// <param name="row">Row of the grid</param>
        /// <param name="cell">Cell to be filled in the above row</param>
        /// <param name="value">Value to assign to the cell</param>
        public static void SetRowCellValue(DataGridViewRow row, int cell, string value)
        {
            row.Cells[cell].Value = value;
        }

        /// <summary>
        /// Set the cell value of a row
        /// </summary>
        /// <param name="row">Row of the grid</param>
        /// <param name="cell">Cell to be filled in the above row</param>
        /// <param name="value">Value to assign to the cell</param>
        public static void SetRowCellValue(DataGridViewRow row, int cell, int value)
        {
            row.Cells[cell].Value = value;
        }

        /// <summary>
        /// Object the grid row value specified in the form of a string
        /// </summary>
        /// <param name="grid">Grid from which the value must be obtained</param>
        /// <param name="selected">Indicates the row from which the value must be objtained</param>
        /// <param name="field">Indicates the column or cell in the row from which to obtian a value</param>
        /// <returns>the string value of the cell</returns>
        public static string GetGridRowValue(DataGridView grid, int selected, int field)
        {
            if (grid.Rows[selected].Cells.Count >= field + 1 && (grid.Rows[selected].Cells[field] == null))
            {
                return "";
            }

            if (grid.Rows[selected].Cells[field].Value == null)
            {
                return "";
            }

            if (grid.Rows[selected].Cells[field].Value.GetType() == typeof(DBNull))
            {
                return "";
            }

            string value = (string)grid.Rows[selected].Cells[field].Value;

            return value.Trim(' ');
        }

        /// <summary>
        /// Set the folus of the interaction to a newly generated row and the first cell
        /// </summary>
        /// <param name="dataGridView">Grid on which to focus</param>
        public static void SetFocus(DataGridView dataGridView)
        {
            dataGridView.ClearSelection();
            int nRowIndex = dataGridView.Rows.Count - 1;

            if (nRowIndex > 1)
            {
                dataGridView.Rows[nRowIndex].Selected = true;
                dataGridView.Rows[nRowIndex].Cells[0].Selected = true;
            }
        }
        
        /// <summary>
        /// Load the label file data to a data table for binding to the grid
        /// </summary>
        /// <param name="labelid">Label id being loaded</param>
        /// <param name="labelfile">Label file to load</param>
        /// <returns>fully loaded data grid for binding to a grid</returns>
        public static DataTable LoadData(string labelId, DLabelFileCollection fileCollection, DLabelFile labelFile)
        {
            return SetRowValues(GetAXDataTableEditor(labelId), fileCollection, labelFile);
        }
       
        public static DataTable SetRowValues(DataTable table, DLabelFileCollection fileCollection, DLabelFile labelFile)
        {
            foreach (KeyValuePair<string, Dictionary<string, Label>> labels in fileCollection.Labels)
            {
                Dictionary<string, Label> labelLanguages = labels.Value;

                if (labelLanguages.TryGetValue(labelFile.Language, out Label label))
                {
                    SetRowValue(table, labelFile.Language, labels.Key, label.Text, label.Description, label.DescriptionExisted);
                }
                else
                    Thread.Sleep(1);
            }

            return table;
        }

        /// <summary>
        /// Set the row values of the data table being built
        /// </summary>
        /// <param name="table">table to be filled</param>
        /// <param name="language">language for the table</param>
        /// <param name="id">id of the label</param>
        /// <param name="text">text of the label</param>
        /// <param name="description">description of the label</param>
        /// <param name="descriptionExisted">check if the description existed</param>
        public static void SetRowValue(DataTable table, string language, string id, string text, string description, string descriptionExisted)
        {
            DataRow row = table.NewRow();

            row[0] = language;
            row[1] = id;
            row[2] = text;
            row[3] = description;
            row[4] = id;
            row[5] = descriptionExisted;

            table.Rows.Add(row);
        }

        /// <summary>
        /// Align the columns and controls and resize the,
        /// </summary>
        /// <param name="dataGridView">Grid to rearrange</param>
        /// <param name="readOnly">Not spelled incorrectly - readOnly is a reserved word</param>
        public static void SetGridLayoutEditor(DataGridView dataGridView, Boolean readOnly = false)
        {
            dataGridView.BackgroundColor = System.Drawing.Color.DarkGray;

            dataGridView.Columns[0].Width = 70;
            dataGridView.Columns[0].ReadOnly = true;

            dataGridView.Columns[1].Width = 300;
            dataGridView.Columns[1].ReadOnly = readOnly;

            dataGridView.Columns[2].Width = 600;

            dataGridView.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            // Original label id field
            dataGridView.Columns[4].Visible = false;
            // Original label Description field
            dataGridView.Columns[5].Visible = false;

            dataGridView.RowsDefaultCellStyle.ForeColor = System.Drawing.Color.Black;

            dataGridView.RowsDefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;

            dataGridView.DefaultCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.True;

            dataGridView.BorderStyle = BorderStyle.Fixed3D;
        }

        /// <summary>
        /// Instantiate a data table to bind to a grid
        /// </summary>
        /// <param name="labelId">Set the table id to the label id</param>
        /// <returns>data table used to bind to the a grid</returns>
        public static DataTable GetAXDataTableEditor(string labelId)
        {
            DataTable table = new DataTable(labelId);

            table.Columns.Add(new DataColumn("Language") { Caption = "Language" });
            table.Columns.Add(new DataColumn("Label Id") { Caption = "Label Id" });
            table.Columns.Add(new DataColumn("Label") { Caption = "Label" });
            table.Columns.Add(new DataColumn("Description") { Caption = "Description" });
            table.Columns.Add(new DataColumn("IdOrig") { Caption = "IdOrig" });
            table.Columns.Add(new DataColumn("DescriptionOrig") { Caption = "DescriptionOrig" });

            return table;
        }

        public static DataTable GetAXDataTableSearch(string language)
        {
            DataTable table = new DataTable(language);

            table.Columns.Add(new DataColumn("FileId") { Caption = "Label File" });
            table.Columns.Add(new DataColumn("Label Id") { Caption = "Label Id" });
            table.Columns.Add(new DataColumn("Label") { Caption = "Label" });

            return table;
        }

        public static DataTable GetAXDataTableBP()
        {
            DataTable table = new DataTable("BestPractice");

            table.Columns.Add(new DataColumn("Severity") { Caption = "Severity" });
            table.Columns.Add(new DataColumn("Path") { Caption = "Path" });
            table.Columns.Add(new DataColumn("ElementType") { Caption = "Alement type" });
            table.Columns.Add(new DataColumn("Message") { Caption = "Message" });
            table.Columns.Add(new DataColumn("Moniker") { Caption = "Label" });
            table.Columns.Add(new DataColumn("DiagnosticType") { Caption = "Diagnostic type" });

            return table;
        }

        public static void SetRowValueBP(DataTable table, string severity, string path, string elementType, string message, string moniker, string diagnosticType)
        {
            DataRow row = table.NewRow();

            row[0] = severity;
            row[1] = path;
            row[2] = elementType;
            row[3] = message;
            row[4] = moniker;
            row[5] = diagnosticType;

            table.Rows.Add(row);
        }

        public static void SetGridLayoutSearch(DataGridView dataGridView)
        {
            dataGridView.BackgroundColor = System.Drawing.Color.DarkGray;

            dataGridView.Columns[0].Name = "Model";
            dataGridView.Columns[0].Width = 120;
            dataGridView.Columns[0].ReadOnly = true;

            dataGridView.Columns[1].Name = "Label Id";
            dataGridView.Columns[1].Width = 300;
            dataGridView.Columns[1].ReadOnly = true;
            dataGridView.Columns[1].SortMode = DataGridViewColumnSortMode.Automatic;

            dataGridView.Columns[2].Name = "Label";
            dataGridView.Columns[2].Width = 600;
            dataGridView.Columns[2].SortMode = DataGridViewColumnSortMode.Automatic;
            dataGridView.Columns[2].ReadOnly = true;

            dataGridView.Columns[3].Name = "Comment";
            dataGridView.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView.Columns[3].ReadOnly = true;

            dataGridView.RowsDefaultCellStyle.ForeColor = System.Drawing.Color.Black;
        }

        public static void SetGridLayoutSearchCode(DataGridView dataGridView)
        {
            dataGridView.BackgroundColor = System.Drawing.Color.DarkGray;

            dataGridView.Columns[0].Name = "Type";
            dataGridView.Columns[0].Width = 200;
            dataGridView.Columns[0].ReadOnly = true;

            dataGridView.Columns[1].Name = "Name";
            dataGridView.Columns[1].Width = 200;
            dataGridView.Columns[1].ReadOnly = true;

            dataGridView.Columns[2].Name = "Snippet";
            dataGridView.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView.Columns[2].ReadOnly = true;

            dataGridView.Columns[3].Name = "FullName";
            dataGridView.Columns[3].Visible = false;

            dataGridView.RowsDefaultCellStyle.ForeColor = System.Drawing.Color.Black;
        }

        public static void SetGridLayoutBP(DataGridView dataGridView)
        {
            dataGridView.BackgroundColor = System.Drawing.Color.DarkGray;

            dataGridView.Columns[0].HeaderText = "Severity";
            dataGridView.Columns[0].Name = "Severity";
            dataGridView.Columns[0].Width = 120;
            dataGridView.Columns[0].SortMode = DataGridViewColumnSortMode.Automatic;

            dataGridView.Columns[1].HeaderText = "Path";
            dataGridView.Columns[1].Name = "Path";
            dataGridView.Columns[1].Width = 400;
            dataGridView.Columns[1].SortMode = DataGridViewColumnSortMode.Automatic;

            dataGridView.Columns[2].HeaderText = "Element type";
            dataGridView.Columns[2].Name = "ElementType";
            dataGridView.Columns[2].Width = 120;

            dataGridView.Columns[3].HeaderText = "Message";
            dataGridView.Columns[3].Name = "Message";
            dataGridView.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            // Hide the moniker
            dataGridView.Columns[4].Visible = false;
            // Hide the moniker
            dataGridView.Columns[5].Visible = false;

            dataGridView.RowsDefaultCellStyle.ForeColor = System.Drawing.Color.Black;
        }

        public void GridTesting(DataGridView gridView)
        {
            DataGridViewRow row = gridView.Rows[0];

            DataGridViewCell cell = row.Cells[0];
           
            cell.Style.Font = new System.Drawing.Font("", float.Parse(""));
        }
    }
}
