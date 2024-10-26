using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Regression.Linear;
using System.IO;

namespace Regression
{
    public partial class BrowseFile : Form
    {
        public BrowseFile()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DataPreprocessing obj = new DataPreprocessing();
            ActiveForm.Hide();
            obj.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {

                OpenFileDialog dig = new OpenFileDialog();
                // dig.Filter = "(*.arff)|*.arff";
                if (dig.ShowDialog() == DialogResult.OK)
                {

                    txtfilepath.Text = dig.FileName;
                    string text1 = File.ReadAllText(txtfilepath.Text);
                    richTextBox1.AppendText(text1);
                    Program.fpath1 = txtfilepath.Text;
                    //label5.Text = "Reading CSV file Completed";

                    if (File.Exists(Application.StartupPath + "\\Excel\\daaset.xls"))
                    {
                        File.Delete(Application.StartupPath + "\\Excel\\daaset.xls");
                    }
                    button2.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("an error occured.....");
            }
        }
    }
}
