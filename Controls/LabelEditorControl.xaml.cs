using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows;
using System.Linq;
using System.Xml;
using System;

namespace Functions_for_Dynamics_Operations
{
    /// <summary>
    /// Interaction logic for LabelEditorControl
    /// </summary>
    public partial class LabelEditorControl : System.Windows.Controls.UserControl
    {
        private int RowRightClick, RowRightClickSec, RowAdded;
        private string DescriptionRightClick, CopyDelete;
        private LangTranslate Translate;
        public bool Scrubbed;

        /// <summary>
        /// Do not evaluate the function
        /// </summary>
        public bool DoNotEvaluate;

        /// <summary>
        /// Label editor is primed and labels are loaded
        /// </summary>
        public bool Primed;

        /// <summary>
        /// Allow editing of labels
        /// </summary>
        private bool EnableSelected;

        /// <summary>
        /// All the labels of the models in the solution
        /// </summary>
        private List<DLabelFileCollection> LabelFileCollections;

        /// <summary>
        /// Label File Collection selected
        /// </summary>
        public DLabelFileCollection LabelFileCollectionSelected;

        /// <summary>
        /// The selected label File with labels
        /// </summary>
        public DLabelFile LabelFileSelected;

        /// <summary>
        /// Current model opened for label editor
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Label Directory for loading the labels
        /// </summary>
        public string LabelDirectory { get; set; }

        /// <summary>
        /// Exclude a certain langauge from being translated
        /// </summary>
        public string NoTranslateLanguage { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LabelEditorControl"/> class.
        /// </summary>
        public LabelEditorControl(string model)
        {
            Model = model;

            InitializeComponent();

            InitEvents();

            InitForm();
        }

        /// <summary>
        /// Initialize the label editor spinning up the label cache
        /// </summary>
        public void InitForm()
        {
            if (LabelDirectory != null && LabelDirectory != "")
            {   // Initialize the translator
                Translate = new LangTranslate();

                LoadlabelIds();

                PrimaryLabelGridView.ClipboardCopyMode = DataGridViewClipboardCopyMode.Disable;
                SecondaryLabelGridView.ClipboardCopyMode = DataGridViewClipboardCopyMode.Disable;

                SecondaryLabelGridView.AllowUserToAddRows = false;
                SecondaryLabelGridView.AllowUserToDeleteRows = false;

                InitLastValues();

                ExcludeLanguageTranslate();
            }
        }

        private void ExcludeLanguageTranslate()
        {
            List<string> languages = new List<string>
            {
                ""
            };

            foreach (var labelFile in LabelFileCollections)
            {
                foreach (var file in labelFile.Files)
                {
                    languages.Add(file.Language);
                }
            }

            ExcludeComB.ItemsSource = languages;
        }

        /// <summary>
        /// Init all events used in the label editor
        /// </summary>
        private void InitEvents()
        {
            AutoTranslate.Checked += AutoTranslate_Checked;
            AutoTranslate.Unchecked += AutoTranslate_Unchecked;
            AutoPropagate.Checked += AutoPropagate_Checked;
            AutoPropagate.Unchecked += AutoPropagate_Unchecked;

            DefaultTxt.PreviewMouseDown += DefaultTxt_PreviewMouseDown;

            PrimaryLabelGridView.SelectionChanged += PrimaryLabelGridView_SelectionChanged;

            PrimaryLabelGridView.CellLeave += PrimaryLabelGridView_CellLeave;

            PrimaryLabelGridView.RowsAdded += PrimaryLabelGridView_RowsAdded;
            PrimaryLabelGridView.UserAddedRow += PrimaryLabelGridView_UserAddedRow;
            PrimaryLabelGridView.KeyDown += PrimaryLabelGridView_KeyDown;
            PrimaryLabelGridView.CellEndEdit += PrimaryLabelGridView_CellEndEdit;
            PrimaryLabelGridView.CellMouseClick += PrimaryLabelGridView_CellMouseClick;
            PrimaryLabelGridView.UserDeletingRow += PrimaryLabelGridView_UserDeletingRow;
            PrimaryLabelGridView.CellValueChanged += PrimaryLabelGridView_CellValueChanged;

            SecondaryLabelGridView.CellMouseClick += SecondaryLabelGridView_CellMouseClick;
            SecondaryLabelGridView.CellValueChanged += SecondaryLabelGridView_CellValueChanged;
        }

        private void PrimaryLabelGridView_CellLeave(object sender, DataGridViewCellEventArgs e)
        {
            /*
            DataGridView dgv = (DataGridView)sender;
            if (dgv != null)
            {
                if (dgv.Rows != null && dgv.Rows.Count > 0 && dgv.Rows[0] != null)
                {
                    if (RowAdded != dgv.Rows[0].Index)
                    {
                        dgv.Rows[RowAdded].Selected = true;
                    }
                }

            }
            */
        }

        private void PrimaryLabelGridView_SelectionChanged(object sender, EventArgs e)
        {
            /*
            if (RowAdded != 0)
            {
                DataGridView dgv = (DataGridView)sender;
                if (dgv.Rows != null && dgv.Rows.Count > 0 && dgv.Rows[0] != null)
                {
                    if (RowAdded != dgv.Rows[0].Index)
                    {
                        // dgv.Rows[RowAdded].Selected = true;
                    }
                }
            }
            */
        }

        private void AutoPropagate_Unchecked(object sender, RoutedEventArgs e)
        {
            AutoPropagateSet();
        }

        private void AutoPropagate_Checked(object sender, RoutedEventArgs e)
        {
            AutoPropagateSet();
        }

        private void AutoPropagateSet()
        {
            Settings settings = VStudioCache.GetSettings(Model);

            if (settings.AutoProp != AutoPropagate.IsChecked)
            {
                settings.AutoProp = (bool)AutoPropagate.IsChecked;

                VStudioCache.SaveSettings(settings);
            }
        }

        private void AutoTranslate_Checked(object sender, RoutedEventArgs e)
        {
            AutoTranslateSet();
        }

        private void AutoTranslate_Unchecked(object sender, RoutedEventArgs e)
        {
            AutoTranslateSet();
        }

        private void AutoTranslateSet()
        {
            Settings settings = VStudioCache.GetSettings(Model);

            if (settings.AutoTrans != AutoTranslate.IsChecked)
            {
                settings.AutoTrans = (bool)AutoTranslate.IsChecked;

                VStudioCache.SaveSettings(settings);
            }
        }

        /// <summary>
        /// Allows creation of labels from inline code via right click and 'create label'
        /// </summary>
        /// <param name="labelTextToCreateLabel">label and id in specific format</param>
        /// <returns>newly created label id</returns>
        public string AddLabelFromTextInCode(string labelTextToCreateLabel, string descript = "", bool silent = false)
        {
            try
            {
                if (!labelTextToCreateLabel.IsNullOrEmpty())
                {
                    string[] labelAndText = labelTextToCreateLabel.Split('~');

                    if (LabelFileCollectionSelected != null)
                    {
                        string id = labelAndText[0], text = labelAndText[1], description = descript != "" ? descript : DefaultTxt.Text;

                        if (!Constants.Constants.AlwaysNewLabel)
                        {
                            if (LabelUtils.CheckTextAlreadyExists(LabelFileCollectionSelected, LabelFileSelected, text, out string labelidExisting))
                            {   // Check if there is already a lebel with the text required
                                if (labelidExisting != "" && !silent)
                                {
                                    if (!LabelUtils.CreateOrUseExistingLabel(LabelFileCollectionSelected, labelidExisting, text))
                                    {
                                        return $"@{LabelFileCollectionSelected.Id}:{labelidExisting}";
                                    }
                                }
                            }
                        }

                        PrimaryLabelGridView.CurrentCell = null;

                        if (LabelFileCollectionSelected.Labels.TryGetValue(id, out Dictionary<string, Label> labelValues))
                        {   // contains the label id already 
                            if (labelValues.TryGetValue(LabelFileSelected.Language, out Label label))
                            {   // See if the language for that label already exists
                                if (label.Text.ToLower() == text.ToLower())
                                    labelTextToCreateLabel = $"@{LabelFileCollectionSelected.Id}:{id}";
                                else
                                    labelTextToCreateLabel = $"TEXT DIFFERS || {text} || {label.Text}";
                            }
                            else
                            {   // Language text for the id does not exist yet - New label has no description that existed
                                labelValues.Add(LabelFileSelected.Language, new Label(text, description, ""));
                                // Return the new label id
                                labelTextToCreateLabel = $"@{LabelFileCollectionSelected.Id}:{id}";
                            }
                        }
                        else
                        {   // Id and label text is new 
                            LabelFileCollectionSelected.Labels.Add(id, new Dictionary<string, Label>() { { LabelFileSelected.Language, new Label(text, description, "") } });
                            // Return the new label id
                            labelTextToCreateLabel = $"@{LabelFileCollectionSelected.Id}:{id}";
                        }
                        // Add to the other languages
                        if ((bool)AutoTranslate.IsChecked)
                        {
                            LabelCRUD.CreateNewLabelsSecondaryInCache(LabelFileCollectionSelected, LabelFileSelected, Translate, id, NoTranslateLanguage);
                        }
                        // Dump the cache to the actual label file
                        LabelCRUD.DumpLabelsToFilesToBuff(LabelFileCollectionSelected);
                        // Add to the primary grid
                        LoadPrimaryLabels();
                    }
                }

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }

            return labelTextToCreateLabel;
        }

        private void PrimaryLabelGridView_UserAddedRow(object sender, DataGridViewRowEventArgs e)
        {
            RowAdded = e.Row.Index;
            NewRecordValues(e.Row.Index);
        }

        private void PrimaryLabelGridView_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            if (RowAdded != 0 && e.RowIndex != RowAdded)
            {
                PrimaryLabelGridView.Rows[e.RowIndex].Dispose();
            }

            if (PrimaryLabelGridView.Rows[e.RowIndex].Cells.Count >= 8)
            {
                string idLocal = PrimaryLabelGridView.Rows[e.RowIndex].Cells[2].Value.ToString();

                if (idLocal == "")
                {
                    NewRecordValues(e.RowIndex);
                }
            }
        }

        private void NewRecordValues(int rowIndex)
        {
            TagRowLockUlockCells(rowIndex, true, false);

            DataGridViewRow gridRow = PrimaryLabelGridView.Rows[rowIndex];

            gridRow.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.LightSalmon;
        }

        private void DefaultTxt_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Right)
            {
                DescriptionRightClick = "Default";

                DescriptionOptions();
            }
        }

        private void PrimaryLabelGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1)
            {
                DataGridView dgv = sender as DataGridView;

                string id = GridUtils.GetGridRowValue(dgv, e.RowIndex, 1);
                string text = GridUtils.GetGridRowValue(dgv, e.RowIndex, 2);

                if (id == "")
                {
                    TagRowLockUlockCells(e.RowIndex, true, false, text);
                }
                else if (!new Regex("^[a-zA-Z][a-zA-Z0-9_]*$").IsMatch(id) || id.Contains(" ") || (LabelFileCollectionSelected.IdExists(id) && text == ""))
                {
                    TagRowLockUlockCells(e.RowIndex, true, true, text);
                }
                else
                {
                    TagRowLockUlockCells(e.RowIndex, false, false, text);
                }
            }
        }

        /// <summary>
        /// Lock the cells for any incorrect and or invalid id or charachters
        /// </summary>
        /// <param name="rowSelected"></param>
        /// <param name="lockUlock"></param>
        private void TagRowLockUlockCells(Int32 rowSelected, bool lockUlock, bool error, string text = "")
        {
            DataGridViewRow gridRow = PrimaryLabelGridView.Rows[rowSelected];

            gridRow.ErrorText = error ? "Label Id exists or invalid charachters specified" : "";

            // There needs to be a global lock on adding records
            PrimaryLabelGridView.AllowUserToAddRows = !error;

            if (gridRow != null)
            {
                gridRow.Cells[2].ReadOnly = lockUlock;
                gridRow.Cells[3].ReadOnly = lockUlock;
            }

            if (error)
            {
                gridRow.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.Red;
            }
            else
            {
                if (text == "")
                {
                    gridRow.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.LightSalmon;
                }
            }
        }

        private void SecondaryLabelGridView_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.ColumnIndex != 3 && e.ColumnIndex != -1 && e.RowIndex != -1 && SecondaryLabelGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value != null)
            {
                CopyDelete = "Secondary";
                RowRightClick = e.RowIndex;

                ContextMenuStrip mnu = new ContextMenuStrip();

                ToolStripMenuItem copy = new ToolStripMenuItem("Copy");
                copy.Click += Copy_Click;

                mnu.Items.Add(copy);
                mnu.Show(new System.Drawing.Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y));
            }

            if (e.Button == MouseButtons.Right && e.ColumnIndex == 3 && e.RowIndex != -1 && SecondaryLabelGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value != null)
            {
                DescriptionRightClick = "Secondary";
                RowRightClickSec = e.RowIndex;

                DescriptionOptions();
            }
        }

        private void Primary_OnSelectionChanged(object sender, System.EventArgs e)
        {
            DataGridView dgv = sender as DataGridView;

            if (EnableSelected && dgv.SelectedRows.Count > 0)
            {   // Create labels for all languages in cache
                string id = GridUtils.GetGridRowValue(dgv, dgv.SelectedRows[0].Index, 1);
                string text = GridUtils.GetGridRowValue(dgv, dgv.SelectedRows[0].Index, 2);
                string description = GridUtils.GetGridRowValue(dgv, dgv.SelectedRows[0].Index, 3);

                if (id != "" && !id.Contains(" "))
                {
                    LoadSecondaryLabels(id, text, description);
                }
                else
                {
                    SecondaryLabelGridView.DataSource = null;
                }
            }
        }

        private void PrimaryLabelGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (!DoNotEvaluate)
            {
                DataGridView dgv = sender as DataGridView;

                if (dgv.Rows[e.RowIndex].ErrorText == "" && dgv.SelectedRows != null && dgv.SelectedRows.Count != 0 && dgv.SelectedRows[0] != null)
                {
                    dgv.Rows[e.RowIndex].DefaultCellStyle.SelectionBackColor = new System.Drawing.Color();

                    // Hold on errors
                    string language = GridUtils.GetGridRowValue(dgv, dgv.SelectedRows[0].Index, 0);
                    string id = GridUtils.GetGridRowValue(dgv, dgv.SelectedRows[0].Index, 1);
                    string text = GridUtils.GetGridRowValue(dgv, dgv.SelectedRows[0].Index, 2);
                    string description = GridUtils.GetGridRowValue(dgv, dgv.SelectedRows[0].Index, 3);
                    string descriptionExisted = GridUtils.GetGridRowValue(dgv, dgv.SelectedRows[0].Index, 5);

                    if (language == "" && id != "" && text != "")
                    {   // New label because Language empty, if not empty, label not empty
                        if (description == "")
                        {
                            description = DefaultTxt.Text;
                        }

                        if (text != "")
                        {
                            // Option to not check if a label exists, just create a new one
                            if (!Constants.Constants.AlwaysNewLabel)
                            {
                                if (LabelUtils.CheckTextAlreadyExists(LabelFileCollectionSelected, LabelFileSelected, text, out string labelidExisting))
                                {
                                    if (labelidExisting != "")
                                    {
                                        if (!LabelUtils.CreateOrUseExistingLabel(LabelFileCollectionSelected, labelidExisting, text))
                                        {
                                            System.Windows.Clipboard.SetText($"@{LabelFileCollectionSelected.Id}:{labelidExisting}");

                                            dgv.Rows.RemoveAt(e.RowIndex);
                                            return;
                                        }
                                    }
                                }
                            }
                        }

                        LabelCRUD.CreateNewLabelsFromRowInCache(LabelFileCollectionSelected, LabelFileSelected, id, text, description, Translate, NoTranslateLanguage, descriptionExisted, (bool)AutoTranslate.IsChecked);

                        LabelCRUD.DumpLabelsToFilesToBuff(LabelFileCollectionSelected);

                        DoNotEvaluate = true;

                        dgv.Rows[dgv.SelectedRows[0].Index].Cells[0].Value = LabelFileSelected.Language;
                        dgv.Rows[dgv.SelectedRows[0].Index].Cells[3].Value = description;
                        dgv.Rows[dgv.SelectedRows[0].Index].Cells[4].Value = id;
                        dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = System.Drawing.Color.White;
                        dgv.Rows[e.RowIndex].ErrorText = "";

                        DoNotEvaluate = false;

                        LoadSecondaryLabels(id, text, description);
                    }

                    if (!DoNotEvaluate && language != "" && id != "" && text != "")
                    {
                        string idOrig = GridUtils.GetGridRowValue(dgv, dgv.SelectedRows[0].Index, 4);
                        // Check if there is a diff between the label already loaded and the values in grid
                        UpdatePrimaryLabel(id, idOrig, text, description);
                        // ID may have changed so tag it with the new one
                        DoNotEvaluate = true;
                        // Do not evaluate
                        dgv.Rows[dgv.SelectedRows[0].Index].Cells[4].Value = id;

                        DoNotEvaluate = false;
                        // Reload the labels in the secondary grid
                        LoadSecondaryLabels(id, text, description);
                    }
                }
            }
        }

        private void Secondary_OnSelectionChanged(object sender, System.EventArgs e)
        {

        }

        private void PrimaryLabelGridView_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            // If a non saved label is deleted then allow adding rows
            PrimaryLabelGridView.AllowUserToAddRows = true;

            if (e.Row != null && e.Row.Index != -1)
            {
                DataGridView dgv = sender as DataGridView;

                string id = GridUtils.GetGridRowValue(dgv, dgv.SelectedRows[0].Index, 1);
                if (id != "")
                {   // Delete from DAX - And Label Cache
                    DeleteLabel(id, false);
                }
            }
        }

        private void PrimaryLabelGridView_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            // Default the new row style for empty rows
            if (e.RowIndex > 0)
            {
                string id = GridUtils.GetGridRowValue(PrimaryLabelGridView, e.RowIndex, 1);
                string text = GridUtils.GetGridRowValue(PrimaryLabelGridView, e.RowIndex, 2);

                if (id == "" && text == "")
                {
                    PrimaryLabelGridView.Rows[e.RowIndex].DefaultCellStyle.SelectionBackColor = System.Drawing.Color.LightSalmon;
                }
            }

            if (!DoNotEvaluate)
            {
                if (e.Button == MouseButtons.Right && e.ColumnIndex != 3 && e.ColumnIndex != -1 && e.RowIndex != -1 && PrimaryLabelGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value != null)
                {
                    if (PrimaryLabelGridView.Rows[e.RowIndex].Cells[0].Value != null && PrimaryLabelGridView.Rows[e.RowIndex].Cells[0].Value.GetType() != typeof(DBNull))
                    {   // No right click if the language is not filled
                        CopyDelete = "Primary";
                        RowRightClick = e.RowIndex;

                        ContextMenuStrip mnu = new ContextMenuStrip();

                        ToolStripMenuItem copy = new ToolStripMenuItem("Copy");
                        copy.Click += Copy_Click;

                        ToolStripMenuItem delete = new ToolStripMenuItem("Delete");
                        delete.Click += Delete_Click;

                        ToolStripMenuItem findreference = new ToolStripMenuItem("Find Reference");
                        findreference.Click += FindReference_Click;

                        mnu.Items.Add(copy);
                        mnu.Items.Add(delete);
                        mnu.Items.Add(findreference);

                        mnu.Show(new System.Drawing.Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y));
                    }
                }
                else if (e.Button == MouseButtons.Right && e.ColumnIndex == 3 && e.RowIndex != -1 && PrimaryLabelGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value != null)
                {
                    DescriptionRightClick = "Primary";
                    RowRightClick = e.RowIndex;

                    DescriptionOptions();
                }
                else if (e.Button == MouseButtons.Right && e.RowIndex > 0)
                {
                    CopyDelete = "Primary";
                    RowRightClick = e.RowIndex;

                    ContextMenuStrip mnu = new ContextMenuStrip();

                    ToolStripMenuItem delete = new ToolStripMenuItem("Delete");
                    delete.Click += Delete_Click;

                    mnu.Items.Add(delete);

                    mnu.Show(new System.Drawing.Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y));
                }
            }
        }

        private void DescriptionOptions()
        {
            ContextMenuStrip mnu = new ContextMenuStrip();

            ToolStripMenuItem label = new ToolStripMenuItem("Label");
            label.Click += Label_Click;
            mnu.Items.Add(label);

            ToolStripMenuItem comment = new ToolStripMenuItem("Comment");
            comment.Click += Comment_Click;
            mnu.Items.Add(comment);

            ToolStripMenuItem help = new ToolStripMenuItem("Help text");
            help.Click += Help_Click;
            mnu.Items.Add(help);

            ToolStripMenuItem locked = new ToolStripMenuItem("{Locked}");
            locked.Click += Locked_Click;
            mnu.Items.Add(locked);

            mnu.Show(new System.Drawing.Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y));
        }

        private void Locked_Click(object sender, EventArgs e)
        {
            SetDescription("{Locked}");
        }

        private void Help_Click(object sender, EventArgs e)
        {
            SetDescription("Help text");
        }

        private void Comment_Click(object sender, EventArgs e)
        {
            SetDescription("Comment");
        }

        private void Label_Click(object sender, EventArgs e)
        {
            SetDescription("Label");
        }

        private void SetDescription(string description)
        {
            if (DescriptionRightClick == "Primary")
            {
                if (RowRightClick != -1 && PrimaryLabelGridView.Rows[RowRightClick] != null)
                {
                    PrimaryLabelGridView.Rows[RowRightClick].Cells[3].Value = description;
                    RowRightClick = -1;
                }
            }
            else if (DescriptionRightClick == "Secondary")
            {
                if (RowRightClickSec != -1 && SecondaryLabelGridView.Rows[RowRightClickSec] != null)
                {
                    SecondaryLabelGridView.Rows[RowRightClickSec].Cells[3].Value = description;
                    RowRightClickSec = -1;
                }
            }
            else if (DescriptionRightClick == "Default")
            {
                DefaultTxt.Text = description;
            }
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            if (RowRightClick != -1 && PrimaryLabelGridView.Rows[RowRightClick] != null)
            {
                string id = GridUtils.GetGridRowValue(PrimaryLabelGridView, RowRightClick, 1);
                if (id != "")
                {
                    DeleteLabel(id, true);
                }
            }
        }

        private void Copy_Click(object sender, EventArgs e)
        {
            switch (CopyDelete)
            {
                case "Primary":
                    if (RowRightClick != -1 && PrimaryLabelGridView.Rows[RowRightClick] != null)
                    {
                        string id = GridUtils.GetGridRowValue(PrimaryLabelGridView, RowRightClick, 1);

                        System.Windows.Clipboard.SetText($"@{LabelFileCollectionSelected.Id}:{id}");
                    }
                    break;
                case "Secondary":
                    if (RowRightClick != -1 && SecondaryLabelGridView.Rows[RowRightClick] != null)
                    {
                        string id = GridUtils.GetGridRowValue(SecondaryLabelGridView, RowRightClick, 1);

                        System.Windows.Clipboard.SetText($"@{LabelFileCollectionSelected.Id}:{id}");
                    }
                    break;
                default:
                    break;
            }
        }

        private void FindReference_Click(object sender, EventArgs e)
        {
            string id = "";

            switch (CopyDelete)
            {
                case "Primary":
                    if (RowRightClick != -1 && PrimaryLabelGridView.Rows[RowRightClick] != null)
                    {
                        id = GridUtils.GetGridRowValue(PrimaryLabelGridView, RowRightClick, 1);

                        id = $"@{LabelFileCollectionSelected.Id}:{id}";
                    }
                    break;
                case "Secondary":
                    if (RowRightClick != -1 && SecondaryLabelGridView.Rows[RowRightClick] != null)
                    {
                        id = GridUtils.GetGridRowValue(SecondaryLabelGridView, RowRightClick, 1);

                        id = $"@{LabelFileCollectionSelected.Id}:{id}";
                    }
                    break;
                default:
                    break;
            }

            // Find the references of the label id
            DynaxUtils.FindReferences(id);
        }

        private void PrimaryLabelGridView_KeyDown(object sender, KeyEventArgs e)
        {
            if (PrimaryLabelGridView.SelectedCells.Count > 0)
            {
                int rowIndex = PrimaryLabelGridView.SelectedCells[0].RowIndex;
                if (rowIndex >= 1 && PrimaryLabelGridView.SelectedRows.Count != 0)
                {
                    string language = GridUtils.GetGridRowValue(PrimaryLabelGridView, rowIndex, 0);
                    string id = GridUtils.GetGridRowValue(PrimaryLabelGridView, rowIndex, 1);

                    if (e.KeyCode == Keys.C && e.Modifiers == Keys.Control)
                    {
                        System.Windows.Clipboard.SetText($"@{LabelFileCollectionSelected.Id}:{id}");
                    }
                    else if (e.KeyCode == Keys.Control && e.Modifiers == Keys.S)
                    {
                        if (id != "" && language == "")
                        {

                            // TODO : New label
                            // AddLabel(PrimaryLabelGridView.SelectedRows[0].Index);
                        }
                    }
                    else if (e.KeyCode == Keys.Tab)
                    {
                        if (id != "" && language == "")
                        {
                            // TODO : New label
                            // AddLabel(PrimaryLabelGridView.SelectedRows[0].Index);
                        }
                    }
                }
            }
        }

        private void SecondaryLabelGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            string language = GridUtils.GetGridRowValue(SecondaryLabelGridView, e.RowIndex, 0);
            string id = GridUtils.GetGridRowValue(SecondaryLabelGridView, e.RowIndex, 1);
            string text = GridUtils.GetGridRowValue(SecondaryLabelGridView, e.RowIndex, 2);
            string description = GridUtils.GetGridRowValue(SecondaryLabelGridView, e.RowIndex, 3);
            string descriptionExisted = GridUtils.GetGridRowValue(SecondaryLabelGridView, e.RowIndex, 5);

            if (language != "")
            {
                Label label = LabelFileCollectionSelected.Labels[id].First(k => k.Key == language).Value;

                if (label.Text != text || label.Description != description)
                {
                    label.Text = text;
                    label.Description = description;
                    // Edited label means update to cache and the physical file
                    LabelCRUD.DumpLabelFileSingleToBuff(LabelFileCollectionSelected, LabelFileCollectionSelected.Files.First(l => l.Language == language));
                }
            }
        }

        /// <summary>
        /// Delete labels 
        /// </summary>
        /// <param name="id">Id of label to delete</param>
        /// <param name="manuallyRemove">manually removed as apposed to delete key</param>
        private void DeleteLabel(string id, bool manuallyRemove)
        {
            _ = LabelFileCollectionSelected.Labels.Remove(id);

            LabelCRUD.DumpLabelsToFilesToBuff(LabelFileCollectionSelected);
            // Only remove if right click was used
            if (manuallyRemove)
                PrimaryLabelGridView.Rows.RemoveAt(RowRightClick);
            // Flush the Secondary grid of all labels
            SecondaryLabelGridView.DataSource = null;
        }

        /// <summary>
        /// Update the primary label 
        /// </summary>
        /// <param name="id">Id of the label to be edited</param>
        /// <param name="idOrig">Original id if the id was changed</param>
        /// <param name="text">Text of the label</param>
        /// <param name="description">Description of the label</param>
        private void UpdatePrimaryLabel(string id, string idOrig, string text, string description)
        {
            if (LabelFileCollectionSelected.Labels[idOrig].TryGetValue(LabelFileSelected.Language, out Label label))
            {
                if (id != idOrig)
                {
                    Dictionary<string, Label> languageValues = LabelFileCollectionSelected.Labels[idOrig];
                    // Scrub the label id
                    LabelFileCollectionSelected.Labels.Remove(idOrig);
                    // Bump to the new id
                    LabelFileCollectionSelected.Labels.Add(id, languageValues);
                }

                if (label.Text != text || label.Description != description)
                {
                    label.Text = text;
                    label.Description = description;

                    LabelCRUD.UpdateLabelSecondaryInCache(LabelFileCollectionSelected, LabelFileSelected, Translate, id);
                }

                LabelCRUD.DumpLabelsToFilesToBuff(LabelFileCollectionSelected);
            }
        }

        /// <summary>
        /// Update was made to the secondary grid labels
        /// </summary>
        /// <param name="language">Language that is being edited</param>
        /// <param name="id">Id of the label that has been amended</param>
        /// <param name="text">New text that has been edited</param>
        /// <param name="description">New description that has been edited</param>
        private void UpdateSecondaryLabel(string language, string id, string text, string description)
        {
            Label label = LabelFileCollectionSelected.Labels[id].First(e => e.Key == language).Value;

            label.Text = text;
            label.Description = description;

            LabelCRUD.DumpLabelFileSingleToBuff(LabelFileCollectionSelected, LabelFileCollectionSelected.Files.First(e => e.Language == language));
        }

        private void LoadlabelIds()
        {
            if (LabelFileCollections == null)
                LabelFileCollections = new List<DLabelFileCollection>();
            // Load all the label files XML defenitions
            foreach (string labelFileName in System.IO.Directory.GetFiles(LabelDirectory))
            {
                // Parse the XML
                XmlDocument xmlLabelDef = new XmlDocument();
                xmlLabelDef.Load(labelFileName);

                LoadLabelFile(xmlLabelDef, labelFileName);
            }

            Primed = true;
        }

        private void LoadLabelFile(XmlDocument xmlLabelDef, string folder)
        {
            string resourceName = "", language = "", name = "", id = "";
            XmlNodeList fileDefValues = xmlLabelDef.GetElementsByTagName("AxLabelFile")[0].ChildNodes;

            foreach (XmlNode node in fileDefValues)
            {
                switch (node.Name)
                {
                    case "Name":
                        name = node.InnerText;
                        break;
                    case "Language":
                        language = node.InnerText;
                        break;
                    case "LabelFileId":
                        id = node.InnerText;
                        break;
                    case "LabelContentFileName":
                        resourceName = node.InnerText;
                        break;
                }
            }

            if (language == "" && name.Contains("en-US"))
                language = "en-US";

            DLabelFile labelFile = new DLabelFile()
            {
                Name = name,
                Language = language,
                ResourceName = resourceName,
                FilePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(folder), "LabelResources", language, resourceName)
            };

            DLabelFileCollection labelFileCollection = null;

            if (!LabelFileCollections.Any(item => item.Id == id))
            {
                // New Id
                labelFileCollection = new DLabelFileCollection(id);
                LabelFileCollections.Add(labelFileCollection);

                // Tag the selection combo box
                _ = LabelsComB.Items.Add(id);
            }
            else
            {   // Existing Id
                labelFileCollection = LabelFileCollections.FirstOrDefault(p => p.Id == id);
            }

            labelFileCollection.Files.Add(labelFile);
        }

        private void LabelsComB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LabelfilesComB.Items.Clear();

            string label = (string)LabelsComB.SelectedItem;

            if (label != null)
            {
                if (label != "")
                {   // Save the selection of the user
                    Settings settings = VStudioCache.GetSettings(Model);

                    if (settings.Label != label)
                    {
                        settings.Label = label;

                        VStudioCache.SaveSettings(settings);
                    }
                }

                // Get the labelFile selected from the collection
                foreach (DLabelFileCollection labelFileCollection in LabelFileCollections)
                {
                    if (labelFileCollection.Id == label)
                    {
                        LabelFileCollectionSelected = labelFileCollection;

                        foreach (DLabelFile labelFile in labelFileCollection.Files)
                        {
                            _ = LabelfilesComB.Items.Add(labelFile.ResourceName);
                        }
                    }
                }
            }
        }

        private void LoadPrimaryLabels()
        {
            System.Data.DataTable dataTable = GridUtils.LoadData("Primary", LabelFileCollectionSelected, LabelFileSelected);

            PrimaryLabelGridView.DataSource = dataTable;
            // Set width of columns 
            GridUtils.SetGridLayoutEditor(PrimaryLabelGridView);
            // End of the grid to add new label
            GridUtils.SetFocus(PrimaryLabelGridView);

            EnableSelected = true;
        }

        private void LabelfilesComB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DLabelFile dLabelFile = LabelFileCollectionSelected.Files.FirstOrDefault(a => a.ResourceName == (string)LabelfilesComB.SelectedItem);

            if (dLabelFile != null && dLabelFile.Language == NoTranslateLanguage)
            {
                // Do not allow a use to select a language that does not allow translation
                LabelfilesComB.SelectedItem = LabelFileSelected.ResourceName;
                // The selection change will simply reload the original value
                return;
            }

            PrimaryLabelGridView.DataSource = null;
            SecondaryLabelGridView.DataSource = null;

            string labelIdFileSelected = (string)LabelfilesComB.SelectedItem;

            if (labelIdFileSelected != null)
            {   // Scrub the buff
                if (labelIdFileSelected != "")
                {   // Save the selection of the user
                    Settings settings = VStudioCache.GetSettings(Model);

                    if (settings.File != labelIdFileSelected)
                    {
                        settings.File = labelIdFileSelected;

                        VStudioCache.SaveSettings(settings);
                    }
                }

                LabelFileCollectionSelected.Labels = new SortedDictionary<string, Dictionary<string, Label>>();

                foreach (DLabelFile labelFile in LabelFileCollectionSelected.Files)
                {
                    LabelUtils.LoadLabelFile(LabelFileCollectionSelected, labelFile);

                    if (labelFile.ResourceName == labelIdFileSelected)
                    {   // Set the selected file 
                        LabelFileSelected = labelFile;
                    }
                }

                foreach (var label in LabelFileCollectionSelected.Labels)
                {
                    if (label.Value.Count != LabelFileCollectionSelected.Files.Count)
                    {
                        VStudioUtils.LogToGenOutput($"Count differs for label {label.Key}");
                    }
                }

                // Load the primary labels for edit
                try
                {
                    LoadPrimaryLabels();
                }
                catch (Exception ex)
                {
                    VStudioUtils.LogToOutput($"Error loading labels - {ex}{Environment.NewLine}");
                }
            }
        }

        private void LoadSecondaryLabels(string id, string text, string description)
        {
            System.Data.DataTable dataTable = GridUtils.GetAXDataTableEditor("Secondary");
            // Load the secondary grid
            foreach (DLabelFile dlabel in LabelFileCollectionSelected.Files)
            {
                if (dlabel.Language != LabelFileSelected.Language)
                {
                    if (LabelFileCollectionSelected.Labels.ContainsKey(id))
                    {
                        if (LabelFileCollectionSelected.Labels[id].ContainsKey(dlabel.Language) && LabelFileCollectionSelected.Labels[id].TryGetValue(dlabel.Language, out Label label))
                        {
                            // Automatically create a translation if the other language is empty
                            if (label.Text == "")
                            {
                                LabelFileCollectionSelected.Labels[id].TryGetValue(LabelFileSelected.Language, out Label labelorig);
                                if (labelorig != null)
                                {
                                    label.Text = Translate.TranslateAzure(text, LabelFileSelected.Language, dlabel.Language);
                                    label.Description = labelorig.Description;

                                    LabelCRUD.DumpLabelFileSingleToBuff(LabelFileCollectionSelected, dlabel);
                                }
                            }

                            GridUtils.SetRowValue(dataTable, dlabel.Language, id, label.Text, label.Description, label.DescriptionExisted);
                        }
                        else
                        {
                            if ((bool)AutoTranslate.IsChecked)
                            {
                                if (AzureTranslate.Url != null && AzureTranslate.Url != "" && AzureTranslate.Secret != null && AzureTranslate.Secret != "")
                                {
                                    Label newLabel = new Label(Translate.TranslateAzure(text, LabelFileSelected.Language, dlabel.Language), Translate.TranslateAzure(description, LabelFileSelected.Language, dlabel.Language), "");
                                    LabelFileCollectionSelected.Labels[id].Add(dlabel.Language, newLabel);

                                    LabelCRUD.DumpLabelFileSingleToBuff(LabelFileCollectionSelected, dlabel);

                                    GridUtils.SetRowValue(dataTable, dlabel.Language, id, newLabel.Text, newLabel.Description, newLabel.DescriptionExisted);
                                }
                                else
                                {
                                    VStudioUtils.LogToOutput("Azure transalate options not set, please set the D365 Functions for Finance");
                                }
                            }
                        }
                    }
                }
            }

            SecondaryLabelGridView.DataSource = dataTable;

            GridUtils.SetGridLayoutEditor(SecondaryLabelGridView, true);
        }

        private void ClearSearchCmd_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = "";

            SetFocus();
        }

        private void SetFocus()
        {
            PrimaryLabelGridView.ClearSelection();

            int nRowIndex = PrimaryLabelGridView.Rows.Count - 1;

            if (nRowIndex > 1)
            {
                PrimaryLabelGridView.Rows[nRowIndex].Selected = true;
                PrimaryLabelGridView.Rows[nRowIndex].Cells[0].Selected = true;
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetRowsWhereTextMatches();
        }

        private void SetRowsWhereTextMatches()
        {
            EnableSelected = false;

            PrimaryLabelGridView.CurrentCell = null;

            foreach (DataGridViewRow row in PrimaryLabelGridView.Rows)
            {
                if ((row.Cells[3].Value != null && row.Cells[3].Value.ToString().ToLower().Contains(SearchTextBox.Text.ToLower())) ||
                    (row.Cells[2].Value != null && row.Cells[2].Value.ToString().ToLower().Contains(SearchTextBox.Text.ToLower())) ||
                    (row.Cells[1].Value != null && row.Cells[1].Value.ToString().ToLower().Contains(SearchTextBox.Text.ToLower())))
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
            // Scrub the secondary grid
            SecondaryLabelGridView.DataSource = null;

            EnableSelected = true;
        }

        /// <summary>
        /// Initialize the last values used in the editor
        /// </summary>
        private void InitLastValues()
        {
            // Do not set the selected buffs here - that will be handled on 'SelectionChanged'
            if (Model != null && Model != "")
            {
                try
                {
                    Settings settings = VStudioCache.GetSettings(Model);

                    if (settings.Model == "")
                    {
                        settings.Model = Model;

                        VStudioCache.SaveSettings(settings);
                    }

                    if (settings.Label != "")
                    {
                        if (settings.File != "")
                        {
                            LabelsComB.SelectedItem = settings.Label;
                            LabelfilesComB.SelectedItem = settings.File;

                            DefaultTxt.Text = settings.Default;

                            AutoPropagate.IsChecked = settings.AutoProp;
                            AutoTranslate.IsChecked = settings.AutoTrans;
                            ExcludeComB.SelectedItem = settings.NoTransLang;
                        }
                    }
                }
                catch (Exception ex)
                {
                    VStudioUtils.LogToOutput($"Error loading last values - {ex}{Environment.NewLine}");

                    ScrubLastVales();
                }
            }
        }

        /// <summary>
        /// Clear the current cached values when changing projects or solutions between models
        /// </summary>
        private void ScrubLastVales()
        {   // Wipe the last values clean
            Settings settings = new Settings
            {
                Model = Model,
                Label = "",
                File = "",
                Default = "",
                NoTransLang = "",
                AutoProp = false,
                AutoTrans = false,
                NoDefaultDesc = false
            };

            VStudioCache.SaveSettings(settings);
        }

        private void DefaultTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            Settings settings = VStudioCache.GetSettings(Model);

            if (settings.Default != DefaultTxt.Text)
            {
                settings.Default = DefaultTxt.Text;

                VStudioCache.SaveSettings(settings);
            }
        }

        private void SyncEmpty_Click(object sender, RoutedEventArgs e)
        {
            foreach (DLabelFile dLabel in LabelFileCollectionSelected.Files)
            {
                // Get the next language to deal with
                if (dLabel.Language != LabelFileSelected.Language)
                {
                    // Grab the label for the label file being checked
                    foreach (var lab in LabelFileCollectionSelected.Labels)
                    {
                        // Pick up the default label
                        var labelLangDefault = lab.Value.FirstOrDefault(a => a.Key == LabelFileSelected.Language);
                        // No parse every empty text label to update
                        foreach (var label in lab.Value.Where(a => a.Key != LabelFileSelected.Language && a.Value.Text == ""))
                        {
                            label.Value.Text = Translate.TranslateAzure(labelLangDefault.Value.Text, LabelFileSelected.Language, dLabel.Language);

                            label.Value.Description = labelLangDefault.Value.Description;
                        }
                    }
                }

                LabelCRUD.DumpLabelFileSingleToBuff(LabelFileCollectionSelected, dLabel);
            }
            // Reload all
            LoadPrimaryLabels();
        }

        private void ExcludeComB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string noTransLang = ExcludeComB.SelectedItem.ToString();

            if (LabelFileSelected != null && noTransLang != LabelFileSelected.Language)
            {
                NoTranslateLanguage = noTransLang;

                Settings settings = VStudioCache.GetSettings(Model);

                if (settings.NoTransLang != NoTranslateLanguage)
                {
                    settings.NoTransLang = NoTranslateLanguage;

                    VStudioCache.SaveSettings(settings);
                }
            }
            else
            {
                // Do not allow selection of the current primary language
                ExcludeComB.SelectedItem = "";
            }
        }

        /// <summary>
        /// Cleear the form cached values for changing between projects and solutions linked to different models
        /// </summary>
        public void ScrubForm()
        {
            if (Model != null)
                Model = string.Empty;

            if (LabelDirectory != null)
                LabelDirectory = string.Empty;

            if (LabelFileCollections != null)
                LabelFileCollections = null;

            if (LabelFileSelected != null)
                LabelFileSelected = null;

            if (PrimaryLabelGridView != null && PrimaryLabelGridView.Rows != null)
                PrimaryLabelGridView.DataSource = null;

            if (SecondaryLabelGridView != null && SecondaryLabelGridView.Rows != null)
                SecondaryLabelGridView.DataSource = null;

            if (LabelsComB != null && LabelsComB.Items != null)
                LabelsComB.Items.Clear();

            if (LabelfilesComB != null && LabelfilesComB.Items != null)
                LabelfilesComB.Items.Clear();

            if (ExcludeComB != null && ExcludeComB.Items != null)
                ExcludeComB.SelectedItem = "";

            if (SearchTextBox != null)
                SearchTextBox.Text = string.Empty;

            if (DefaultTxt != null)
                DefaultTxt.Text = string.Empty;

            if (AutoTranslate != null)
                AutoTranslate.IsChecked = false;

            Scrubbed = true;
            Primed = false;
        }
    }
}