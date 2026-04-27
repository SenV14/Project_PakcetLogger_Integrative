using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace Project_PakcetLogger_Integrative
{
    public partial class Login : Form
    {
     
        public Login()
        {
            InitializeComponent();
        }

        // put a continue limitre
        int number_limit = 0;

        public void Check_login(string email_confirm, string password_confirm)
        {


            string @database = "Server=127.0.0.1;Port=3306;Database=packetlogger_login;Uid=root;Pwd=P@55W0RD;";
            string @selecting_method = "SELECT packet_gmail, packet_password from packetlogger_users where packet_gmail = @gmail AND packet_password = @password LIMIT 1";
            string email = email_confirm;
            string password = password_confirm;
            try
            {
                using (var connect = new MySqlConnection(@database))
                {
                    connect.Open();

                    using (MySqlCommand sql_identity = new MySqlCommand(@selecting_method, connect))
                    {
                        sql_identity.Parameters.AddWithValue("@gmail", email);
                        sql_identity.Parameters.AddWithValue("@password", password);

                        using (MySqlDataReader reader = sql_identity.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string storedEmail = reader.GetString("packet_gmail");
                                string stored_password = reader.GetString("packet_password");
                                if (storedEmail == email && stored_password == password)
                                {
                                    MessageBox.Show("Login successful! Welcome, " + storedEmail, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    MessageBox.Show("Password Correct and successfully logged in!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    MessageBox.Show("Please wait while we redirect you to the OTP form.", "Redirecting", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    this.Hide();
                                    OTP_LOGIN otpForm = new OTP_LOGIN();
                                    otpForm.Show();
                                }
                                else
                                {
                                    MessageBox.Show("Incorrect password. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    txt_password_login.Clear();
                                    txt_Signin_Email.Clear();
                                    txt_Signin_Email.Focus();
                                    number_limit++;
                                    if (number_limit >= 3)
                                    {
                                        MessageBox.Show("You have reached the maximum number of login attempts. Please try again later.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        this.Close();
                                    }
                                }
                            }
                            else
                            {
                                MessageBox.Show("Email not found. Please check your email and try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                txt_password_login.Clear();
                                txt_Signin_Email.Clear();
                                txt_Signin_Email.Focus();
                                number_limit++;
                                if (number_limit >= 3)
                                {
                                    MessageBox.Show("You have reached the maximum number of login attempts. Please try again later.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    this.Close();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while checking the login credentials: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            } }

        
        public void Login_Load(object sender, EventArgs e) // Confirmation in opening the OTP form after clicking the "Confirm" button
        {
            //
        }//

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
            string email = txt_Signin_Email.Text.Trim();
            string password = txt_password_login.Text.Trim();
            string specialCharacters = @"!@#$%^&*()_+-=[]{}|;':"",.<>/?`~";
            if (!email.ToLower().EndsWith("@gmail.com") || string.IsNullOrEmpty(email) || password.Length < 8 || !password.Any(c => specialCharacters.Contains(specialCharacters)))
            {
                MessageBox.Show("Please put on the right password in this account before continue","Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txt_password_login.Clear();
                    txt_Signin_Email.Clear();
                    txt_Signin_Email.Focus();
                    number_limit++;
            if (number_limit >= 3)
            {
                MessageBox.Show("You have reached the maximum number of login attempts. Please try again later.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                }
            }
            else
            {
                Check_login(email, password);
            }
        }
    }
}
