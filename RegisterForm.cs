using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Data.SqlClient;

namespace Regression.Linear
{
    public partial class RegisterForm : Form
    {
        BaseConnection con = new BaseConnection();
        BaseConnection con1 = new BaseConnection();
        string uid;
        public RegisterForm()
        {
            InitializeComponent();
        }
        public string getid()
        {


            string query = "select isnull(max(userid)+1,500) from Usertb";
            SqlDataReader sd = con.ret_dr(query);
            if (sd.Read())
            {
                uid = sd[0].ToString();

            }
            return uid;
        }
        private void button1_Click(object sender, EventArgs e)
        {

            string pattern = @"^[a-z][a-z|0-9|]*([_][a-z|0-9]+)*([.][a-z|" +
                       @"0-9]+([_][a-z|0-9]+)*)?@[a-z][a-z|0-9|]*\.([a-z]" +
                       @"[a-z|0-9]*(\.[a-z][a-z|0-9]*)?)$";
            System.Text.RegularExpressions.Match match = Regex.Match(textBox1.Text.Trim(), pattern, RegexOptions.IgnoreCase);

            if (match.Success)
            {
                string uid11 = textBox3.Text;
                string status = "0";
                string str2 = "insert into Usertb values (" + uid11 + ",'" + txtcateg.Text + "','" + txtdes.Text + "','" + txtbrwse.Text + "','" + textBox2.Text + "','" + textBox4.Text + "','" + textBox1.Text + "'," + status + ")";
                con.exec(str2);
                string str3 = "insert into login values('" + txtcateg.Text + "','" + txtdes.Text + "','0')";
                con1.exec(str3);
                MessageBox.Show("insert successfully");
                txtbrwse.Text = "";
                txtcateg.Text = "";
                txtdes.Text = "";
                textBox2.Text = "";
                textBox4.Text = "";
                textBox1.Text = "";
                this.Close();

            }
            else
            {

                MessageBox.Show("Provide a valid email");
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar >= '0' && e.KeyChar <= '9') || (e.KeyChar == '-'))
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar >= 'a' && e.KeyChar <= 'z') || (e.KeyChar >= 'A' && e.KeyChar <= 'Z') || (e.KeyChar == '.') || (e.KeyChar == '-') || (e.KeyChar >= '0') && (e.KeyChar <= '9') || (e.KeyChar == ' '))
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void txtcateg_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar >= 'a' && e.KeyChar <= 'z') || (e.KeyChar >= 'A' && e.KeyChar <= 'Z') || (e.KeyChar == '.') || (e.KeyChar == '-') || (e.KeyChar >= '0') && (e.KeyChar <= '9') || (e.KeyChar == ' ') || (e.KeyChar == ','))
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void txtdes_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar >= 'a' && e.KeyChar <= 'z') || (e.KeyChar >= 'A' && e.KeyChar <= 'Z') || (e.KeyChar == '.') || (e.KeyChar == '-') || (e.KeyChar >= '0') && (e.KeyChar <= '9') || (e.KeyChar == ' '))
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void txtbrwse_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar >= 'a' && e.KeyChar <= 'z') || (e.KeyChar >= 'A' && e.KeyChar <= 'Z') || (e.KeyChar == '.'))
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void RegisterForm_Load(object sender, EventArgs e)
        {
            textBox3.Text=getid();
        }
    }
}
