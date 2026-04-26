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
using MailKit.Net.Smtp;
using MimeKit;

namespace Project_PakcetLogger_Integrative
{
    public partial class Sign_up : Form
    {
        private Form _loginform;
        public Sign_up(Form login)
        {
            InitializeComponent();
            _loginform = login;
       
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                _loginform.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while opening the Login form: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string email = txt_Signup.Text.Trim();
                string password = txt_Password.Text.Trim();
                string specialCharacters = @"!@#$%^&*()_+-=[]{}|;':"",.<>/?`~";
                if (!email.ToLower().EndsWith("@gmail.com") || string.IsNullOrEmpty(email) || password.Length < 8 || !password.Any(c => specialCharacters.Contains(specialCharacters)))
                {

                    MessageBox.Show("Please enter a valid Gmail address.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txt_Password.Clear();
                    txt_Signup.Clear();
                    txt_Password.Focus();
                    return;

                }
                else if (password.Length < 8 || password.Any(c => specialCharacters.Contains(specialCharacters)) || email.ToLower().EndsWith("@gmail.com"))
                {
                    try
                    {
                        string @database = "Server=127.0.0.1;Port=3306;Database=packetlogger_login;Uid=root;Pwd=P@55W0RD";
                        string select_method = "SELECT packet_gmail FROM packetlogger_users WHERE packet_gmail = @Email LIMIT 1";
                        using (MySqlConnection @connection = new MySqlConnection(@database))
                        {
                            try
                            {   
                                @connection.Open();
                                using (MySqlCommand command = new MySqlCommand(select_method, @connection))
                                {
                                    
                                    command.Parameters.AddWithValue("@Email", email);
                                    command.Parameters.AddWithValue("@Password", password);
                                    using (MySqlDataReader reader = command.ExecuteReader())
                                    {
                                        if (reader.HasRows)
                                        {
                                            MessageBox.Show("This email is already registered.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                            return;
                                        }
                                        else if (!reader.HasRows)
                                        {
                                            MessageBox.Show("Email is not available for registration.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                            One_Time_Password otp = new One_Time_Password(email, password);
                                            otp.Show();
                                            this.Hide();
                                        }
                                    }

                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("An error occurred while checking data from storage: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An error occurred while processing the sign-up: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while processing the sign-up: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
               

        
    }
}
