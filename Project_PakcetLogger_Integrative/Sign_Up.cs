using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using MailKit.Net.Smtp;
using Microsoft.Win32;
using MimeKit;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Tls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls.WebParts;
using System.Windows.Forms;


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
                string specialCharacters = @"!@#$%^&*()_+-=[]{}|;':\"",.<>/?`~";
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
                                            OTP_LOGIN otp = new OTP_LOGIN(email, password);
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

        private async void pictureBox3_Click(object sender, EventArgs e)
        {
            try
            {

                // cancellation token
                CancellationTokenSource cancel_token = new CancellationTokenSource(TimeSpan.FromSeconds(60));
                //Holding the id and client secret for api
                

                // using Client secrets to authenticate the user and get the credentials
                new ClientSecrets { ClientId = clientId, ClientSecret = clientSecret };
                UserCredential User_Credentials = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    // used for the client secrets and the scopes for the API access
                    new ClientSecrets { ClientId = clientId, ClientSecret = clientSecret },
                    // using this to get email user premissions
                    new[] { "email" },
                    "user",
                    // time limit for the user to authenticate and get the credentials
                    cancel_token.Token
                        
                );
                // use for getting the gmail address
                



            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while handling the picture box click: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void Sign_up_Load(object sender, EventArgs e)
        {
            try
            {
                string database = "Server = 127.0.0.1; Port = 3306; Database = packetlogger_login; Uid = root; Pwd = P@55W0RD";
                string command = "SELECT packet_gmail FROM packetlogger_users WHERE packet_gmail = @Email LIMIT 1";
                using (MySqlConnection connection = new MySqlConnection(database)) 
                {
                    //database.Open
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while loading the Sign-up form: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
