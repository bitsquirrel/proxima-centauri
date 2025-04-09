using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Functions_for_Dynamics_Operations
{
    public partial class CreateFormForm : Form
    {
        public string FormPattern;
        public bool Ok, Full;

        public CreateFormForm()
        {
            InitializeComponent();

            FormPatternComboBox.DataSource = Patterns.GetPatterns();
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            FormPattern = FormPatternComboBox.SelectedItem.ToString();

            Ok = true;

            Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void FullPatternCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Full = FullPatternCheckBox.Checked;
        }
    }
}
