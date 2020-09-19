using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleLed
{
    public partial class DummyForm : Form
    {
        public DummyForm()
        {
            InitializeComponent();
        }

        private void DummyForm_Load(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void DummyForm_Activated(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void DummyForm_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                this.Hide();
            }
        }
    }
}
