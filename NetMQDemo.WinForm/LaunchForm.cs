using System;
using System.Windows.Forms;

namespace NetMQDemo.WinForm
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

        private void Button3_Click(object sender, EventArgs e)
        {
            PushPullForm form = new PushPullForm();
            form.Show(this);
        }
    }
}
