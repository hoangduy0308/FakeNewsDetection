using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;


namespace Regression.Linear
{
    public partial class Matching : Form
    {
        public static string status = "";
        public  string wbull = "";
        public  int wc = 0;
        public int oc = 0;
        public Matching()
        {
            InitializeComponent();
        }
        public Matching(string optimized, string s)
        {
            InitializeComponent();
            filllist(optimized);
            status = s;
        }
        public void filllist(string tex)
        {
            string[] a = tex.Split(',');
            for (int i = 0; i < a.Count(); i++)
            {
                listBox1.Items.Add(a[i].ToString());
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            progressBar1.Maximum = listBox1.Items.Count-1;
            for (int i = 0; i < listBox1.Items.Count-1; i++)
            {
                wc++;
                progressBar1.Value++;

                listBox1.SelectedIndex = i;
                string word = listBox1.SelectedItem.ToString();
                //if (word.StartsWith("A") || word.StartsWith("a"))
                //{
                    listBox2.Items.Clear();
                    label1.Text = "Dictionary of A";
                    label1.Refresh();
                    string dictionary = File.ReadAllText(Application.StartupPath + "\\dictionary\\a.txt");
                    string column = dictionary.Replace("\n", "");
                    string column2 = dictionary.Replace("\r", ".");
                    string[] splitwords = column2.Split('.');
                    for (int j = 0; j < splitwords.Count(); j++)
                    {
                        listBox2.Items.Add(splitwords[j].Replace("\n", "").ToString());
                    }
                    progressBar2.Maximum = listBox2.Items.Count;
                    progressBar2.Value = 0;
                    int index = 0;
                    int count = 0;
                    for (int k = 0; k < listBox2.Items.Count; k++)
                    {

                        listBox2.SelectedIndex = k;
                        progressBar2.Value++;
                        if (string.Equals(word, listBox2.SelectedItem.ToString(), StringComparison.CurrentCultureIgnoreCase) == true)
                        {
                            wbull= wbull+word + ",";
                            index = k;
                            count = 1;
                        }
                    }
                    if (count != 1)
                    {
                        richTextBox2.AppendText(word + " ");
                        richTextBox2.Refresh();
                    }
                    else
                    {
                        if (status == "1")
                        {
                            oc++;
                            richTextBox2.AppendText("***** ");
                            richTextBox2.Refresh();
                        }
                        if (status == "2") 
                        {
                            oc++;
                            richTextBox2.AppendText(" ");
                            richTextBox2.Refresh();
                        }
                        if (status == "0")
                        {
                            oc++;
                            
                            richTextBox2.Refresh();
                        }
                    }
                    index = 0;
                    count = 0;


               // }
                
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //MessageBox.Show(wc.ToString());
            //MessageBox.Show(oc.ToString());
            FinalText obj = new FinalText(richTextBox2.Text,wc,oc,wbull);
            ActiveForm.Hide();
            obj.Show();
        }
    }
}
