using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Xml;

namespace Functions_for_Dynamics_Operations
{
    /// <summary>
    /// Interaction logic for BPEditorControl.
    /// </summary>
    public partial class BPEditorControl : System.Windows.Controls.UserControl
    {
        private BestPracticeFunc BestPracticeFunc;
        private int RowRightClick;

        /// <summary>
        /// Initializes a new instance of the <see cref="BPEditorControl"/> class.
        /// </summary>
        public BPEditorControl()
        {
            InitializeComponent();

            Init();
        }

        protected void Init()
        {
            BPDataGrid.ReadOnly = true;
            BPDataGrid.AllowUserToAddRows = false;

            BPDataGrid.CellMouseClick += BPDataGrid_CellMouseClick;
        }

        private void BPDataGrid_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right && e.ColumnIndex != -1 && e.RowIndex != -1 && BPDataGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value != null)
            {
                if (BPDataGrid.Rows[e.RowIndex].Cells[0].Value != null && BPDataGrid.Rows[e.RowIndex].Cells[0].Value.GetType() != typeof(DBNull))
                {   // No right click if the language is not filled
                    RowRightClick = e.RowIndex;

                    System.Windows.Forms.ToolStripMenuItem supress = new System.Windows.Forms.ToolStripMenuItem("Suppress non critical");
                    supress.Click += Supress_Click;

                    System.Windows.Forms.ContextMenuStrip mnu = new System.Windows.Forms.ContextMenuStrip();
                    mnu.Items.Add(supress);
                    mnu.Show(new System.Drawing.Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y));
                }
            }
        }

        private void Supress_Click(object sender, EventArgs e)
        {
            if (RowRightClick != -1 && BPDataGrid.Rows[RowRightClick] != null)
            {
                System.Windows.Forms.DataGridViewRow row = BPDataGrid.Rows[RowRightClick];

                BestPracticeFound bestPracticeFound = new BestPracticeFound
                {
                    Severity = row.Cells[0].Value.ToString(),
                    Path = row.Cells[1].Value.ToString(),
                    Moniker = row.Cells[4].Value.ToString(),
                    DiagnosticType = row.Cells[5].Value.ToString(),
                    Justification = $"Non critical suppression, User : {Environment.UserName}, Machine : {Environment.MachineName}, at : {DateTime.UtcNow}"
                };

                BestPracticeFunc.WriteToBPSuppressionFile(bestPracticeFound);

                BPDataGrid.Rows.RemoveAt(RowRightClick);
            }
        }

        private void LoadCmd_Click(object sender, RoutedEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            ModelInfo modelInfo = VStudioUtils.GetActiveAXProjectModelInfo();
            if (modelInfo != null)
            {
                if (BPDataGrid.Rows.Count > 0)
                    BPDataGrid.DataSource = null;

                BestPracticeFunc = new BestPracticeFunc(modelInfo);
                BPDataGrid.DataSource = BestPracticeFunc.LoadBPFile();

                GridUtils.SetGridLayoutBP(BPDataGrid);
            }
        }

        private void SaveCmd_Click(object sender, RoutedEventArgs e)
        {
            BestPracticeFound bestPracticeFound = new BestPracticeFound
            {
                Severity = BPDataGrid.CurrentRow.Cells[0].Value.ToString(),
                Path = BPDataGrid.CurrentRow.Cells[1].Value.ToString(),
                Moniker = BPDataGrid.CurrentRow.Cells[4].Value.ToString(),
                DiagnosticType = BPDataGrid.CurrentRow.Cells[5].Value.ToString(),
                Justification = $"{IgnoreTxtBox.Text}, User : {Environment.UserName}, Machine : {Environment.MachineName}, at : {DateTime.UtcNow}"
            };

            BestPracticeFunc.WriteToBPSuppressionFile(bestPracticeFound);

            BPDataGrid.Rows.RemoveAt(BPDataGrid.CurrentRow.Index);

            IgnoreTxtBox.Text = "";
        }

        public void ScrubForm()
        {
            if (BPDataGrid != null)
                BPDataGrid.DataSource = null;

            if (IgnoreTxtBox != null)
                IgnoreTxtBox.Text = "";
        }

        private void SearchTxtBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetRowsWhereTextMatches();
        }

        private void SetRowsWhereTextMatches()
        {
            BPDataGrid.CurrentCell = null;

            foreach (DataGridViewRow row in BPDataGrid.Rows)
            {
                if ((row.Cells[3].Value != null && row.Cells[4].Value.ToString().ToLower().Contains(SearchTxtBox.Text.ToLower())) ||
                    (row.Cells[3].Value != null && row.Cells[3].Value.ToString().ToLower().Contains(SearchTxtBox.Text.ToLower())) ||
                    (row.Cells[2].Value != null && row.Cells[2].Value.ToString().ToLower().Contains(SearchTxtBox.Text.ToLower())) ||
                    (row.Cells[1].Value != null && row.Cells[1].Value.ToString().ToLower().Contains(SearchTxtBox.Text.ToLower())))
                {
                    try
                    {
                        row.Visible = true;
                    }
                    catch (Exception)
                    {
                        // Do nothing for now
                    }
                }
                else
                {
                    try
                    {
                        row.Visible = false;
                    }
                    catch (Exception)
                    {
                        // Do nothing for now
                    }
                }
            }
        }

        private void ClearCmd_Click(object sender, RoutedEventArgs e)
        {
            SearchTxtBox.Text = "";

            SetFocus();
        }

        private void SetFocus()
        {
            BPDataGrid.ClearSelection();

            int nRowIndex = BPDataGrid.Rows.Count - 1;

            if (nRowIndex > 1)
            {
                BPDataGrid.Rows[nRowIndex].Selected = true;
                BPDataGrid.Rows[nRowIndex].Cells[0].Selected = true;
            }
        }
    }
}