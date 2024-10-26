using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Regression.Linear
{
    public partial class FinalText : Form
    {
        public FinalText()
        {
            InitializeComponent();
        }
        public FinalText(string ss, int wc, int oc, string w)
        {
            InitializeComponent();
          //  richTextBox1.Text = ss.ToString();
            richTextBox2.Text = Program.pdata.ToString();
           // label1.Text = wc.ToString();
           // richTextBox3.Text = w;
            label3.Text = oc.ToString();
            if (oc >= 1)
            {
                label3.ForeColor = Color.Red;
                label3.Text = "Fake News Detected";
            }
            else
            {
                label3.ForeColor = Color.Green;
                label3.Text = "Real News";
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
