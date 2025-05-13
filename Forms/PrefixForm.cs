using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Functions_for_Dynamics_Operations.Forms
{
    public partial class PrefixForm : Form
    {
        public string Prefix;
        public bool Ok;

        public PrefixForm()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            Prefix = PrefixTextBox.Text;
            Ok = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            Ok = false;
            this.Close();
        }
    }
}
