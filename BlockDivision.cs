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
    public partial class BlockDivision : Form
    {
        public static string text = "";
        public string s = "";
        public BlockDivision()
        {
            InitializeComponent();
        }
        public BlockDivision(string fulltext, string status)
        {
            InitializeComponent();
            text = fulltext;
            
            s = status;
            timer1.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        public void keywordsplit(string w)
        {
            string[] words = w.Split(' ');

            for (int i = 0; i < words.Count(); i++)
            {
                if (words[i] != "")
                {
                    lwords.Items.Add(words[i].ToString());
                }
            }

            for (int i = 0; i < lwords.Items.Count; i++)
            {
                lwords.SelectedIndex = i;
                string q = RemoveSpecialCharacters(lwords.SelectedItem.ToString());
                listBox1.Items.Add(q);
            }


        }

        //code to remove special characters from a query...........
        public static string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                // if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_' || c == ' ')
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            keywordsplit(text);
            timer1.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string tosend = "";
            for (int i = 0; i < listBox1.Items.Count; i++)
            {
                listBox1.SelectedIndex = i;
                if (listBox1.SelectedItem.ToString() != "")
                {
                    tosend = tosend + listBox1.SelectedItem.ToString() + ",";
                }
                
            }

            Matching obj = new Matching(tosend, s);
            ActiveForm.Hide();
            obj.Show();
        }
    }
}
