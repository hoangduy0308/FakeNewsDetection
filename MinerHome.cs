using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Regression.Linear;


namespace Regression
{
    public partial class MinerHome : Form
    {
        public MinerHome()
        {
            InitializeComponent();
        }
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;
                return cp;
            }

        }
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            toolStripButton1.Enabled = true;
            toolStripButton7.Enabled = true;
            toolStripButton9.Enabled = true;
            BrowseFile obj = new BrowseFile();
            obj.Show();
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
             Application.Exit();
            //StartForm obj = new StartForm();
            //ActiveForm.Hide();
            //obj.Show();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {

            FeatureGeneration obj = new FeatureGeneration();
            obj.Show();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            //MinerSearchTranscation obj = new MinerSearchTranscation();
            //obj.Show();
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            MainForm fg1 = new MainForm();
            fg1.Show();
        }

        private void toolStripButton9_Click(object sender, EventArgs e)
        {
            AccuracyGraph obj = new AccuracyGraph();
            obj.Show();
        }

        private void MinerHome_Load(object sender, EventArgs e)
        {
            toolStripButton1.Enabled = false;
            toolStripButton7.Enabled = false;
            toolStripButton9.Enabled = false;
        }
    }
}
