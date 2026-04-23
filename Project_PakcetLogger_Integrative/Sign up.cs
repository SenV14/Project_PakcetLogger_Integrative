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
        private void sign_Up()
        {
            try
            {
                string packet_email = txt_Signup.Text.Trim();
                string packet_password = txt_Password.Text.Trim();
                if (packet_email == "" || packet_password == "")
                {
                    MessageBox.Show("Please fill in all fields.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else
                {
                    try
                    {
                        string constring = "server=127.0.0.1;port=3306;uid=root;pwd=P@55W0RD;database=packetlogger_login";
                        using (MySqlConnection con = new MySqlConnection(constring))
                            try
                            {
                                con.Open();
                                if (con.State == ConnectionState.Open)
                                {
                                    string query = "INSERT INTO packetlogger_users (packet_gmail, packet_password) VALUES (@Email, @Password)";
                                    using (MySqlCommand Addcommandsql = new MySqlCommand(query, con))
                                    {
                                        Addcommandsql.Parameters.AddWithValue("@Email", packet_email);
                                        Addcommandsql.Parameters.AddWithValue("@Password", packet_password);
                                        int result = Addcommandsql.ExecuteNonQuery();
                                        if (result > 0)
                                        {
                                            MessageBox.Show("Sign-up successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                            _loginform.Show();
                                            this.Hide();
                                        }
                                        else
                                        {
                                            MessageBox.Show("Sign-up failed. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("An error occurred while executing the query: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;

                            }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An error occurred while connecting to the database: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred during the sign-up process: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    One_Time_Password otp = new One_Time_Password();
                    otp.Show();
                    this.Hide();
                }     
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while processing the sign-up: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
               

        
    }
}
