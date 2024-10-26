using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace Regression.Linear
{
    public partial class adminlogin : Form
    {
        BaseConnection con = new BaseConnection();
        public adminlogin()
        {
            InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox2.Text == "admin" && textBox1.Text == "admin")
            {
                MinerHome obj = new MinerHome();
                ActiveForm.Hide();
                obj.Show();
            }
            else
            {



                string query = "select * from login where username='" + textBox1.Text + "'";
                SqlDataReader sd = con.ret_dr(query);
                if (sd.Read())
                {
                    if ((textBox1.Text == sd[0].ToString()) && textBox2.Text == sd[1].ToString())
                    {



                        DetectCyber obj = new DetectCyber();
                        ActiveForm.Hide();
                        obj.Show();
                    }

                }
                else
                {
                    MessageBox.Show("Invalid Password or username...");
                    textBox1.Text = "";
                    textBox2.Text = "";
                }
            }
        }

        private void adminlogin_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            textBox2.Text = "";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            RegisterForm obj = new RegisterForm();
            obj.Show();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            RegisterForm obj = new RegisterForm();
            obj.Show();
        }

        
       

  
    }
}
