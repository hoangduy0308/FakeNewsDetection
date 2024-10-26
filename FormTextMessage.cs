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
    public partial class FormTextMessage : Form
    {
        public string status = "";
        public string data="";
        public FormTextMessage()
        {
            InitializeComponent();
        }
        public FormTextMessage(string str)
        {
            InitializeComponent();
            data = str;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            //if (checkBox1.Checked == true)
            //{
            //    status = "1";
            //    BlockDivision obj = new BlockDivision(richTextBox1.Text, status);
            //    ActiveForm.Hide();
            //    obj.Show();
            //}
            //else if (checkBox2.Checked == true)
            //{
            //    status = "2";
            //    BlockDivision obj = new BlockDivision(richTextBox1.Text,status);
            //    ActiveForm.Hide();
            //    obj.Show();
            //}

            //else
            //{
                status = "0";
                BlockDivision obj = new BlockDivision(richTextBox1.Text, status);
                ActiveForm.Hide();
                obj.Show();
          //  }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void FormTextMessage_Load(object sender, EventArgs e)
        {
            richTextBox1.Text = data;
            Program.pdata = data;
        }
        
    }
}
