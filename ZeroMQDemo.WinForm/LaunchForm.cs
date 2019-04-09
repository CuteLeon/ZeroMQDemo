using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZeroMQDemo.WinForm
{
    public partial class LaunchForm : Form
    {
        public LaunchForm()
        {
            this.InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            CSForm form = new CSForm();
            form.Show(this);
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            PubSubForm form = new PubSubForm();
            form.Show(this);
        }
    }
}
