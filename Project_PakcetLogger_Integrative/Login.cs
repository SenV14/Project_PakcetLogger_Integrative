using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace Project_PakcetLogger_Integrative
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }

        public void Opening_Form()
        {
            One_Time_Password otp = new One_Time_Password();
            otp.Show();
            this.Hide();
        }
        public void Login_Load(object sender, EventArgs e) // Confirmation in opening the OTP form after clicking the "Confirm" button
        {
            try
            {
                Opening_Form();
            }
            catch (Exception ex) 
            {
                MessageBox.Show("An error occurred while opening the Sign-up form: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Sign_up signup = new Sign_up(this);
                signup.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while opening the Sign-up form: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_Confirm_Click(object sender, EventArgs e)
        {
            Login_Load(sender, e);
        }
    }
}
