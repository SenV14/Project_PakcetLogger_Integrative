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
using Microsoft.Extensions.Configuration;
using Google.Apis.Oauth2.v2;
using Google.Apis.Oauth2.v2.Data;
using Octokit;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Drawing.Text;


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
                //
                CancellationTokenSource ctc = new CancellationTokenSource(TimeSpan.FromSeconds(30));

                CancellationToken token = ctc.Token;
                // this is used to find the json file
                var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("AUTHENTICATION.json", optional: false).Build();
                // creating variable for the client and finding client secret in file in order to push rep and authenticate
                var secrets = new ClientSecrets
                {
                    ClientId = config["Installed:client_id"],
                    ClientSecret = config["Installed:client_secret"]
                };
                //using User credential for signing in with google account and requesting the email scope basically user consent
                UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    secrets,
                    new[] { Oauth2Service.Scope.UserinfoEmail }, //permission to get the email of the user
                    "user",
                    token,
                    new FileDataStore("GoogleOAuth")
                );
                // getting the email after successful authentication and putting in mysqlworkbench for double security
                var oauth2Service = new Oauth2Service(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "integ-project"
                });
                // used to get the userinfo in the credentials and then get the email from the userinfo
                Userinfo userinfo = await oauth2Service.Userinfo.Get().ExecuteAsync();
                string email = userinfo.Email;
                Sign_up_Load(email);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while handling the picture box click: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void Sign_up_Load(string email)
        {
            try
            {
                string database = "Server = 127.0.0.1; Port = 3306; Database = packetlogger_login; Uid = root; Pwd = P@55W0RD";
                string command = "SELECT email_info FROM packet_logger_authentication WHERE email_info = @Email LIMIT 1";
                using (MySqlConnection connection = new MySqlConnection(database))
                {
                    connection.Open();
                    using (MySqlCommand select = new MySqlCommand(command, connection))
                    {
                        select.Parameters.AddWithValue("@Email", email);
                        using (MySqlDataReader reader = select.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                MessageBox.Show("This email is already registered.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                            else if (!reader.HasRows)
                            {
                                MessageBox.Show("Email is not available for registration.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                OTP_LOGIN otp = new OTP_LOGIN(email, null);
                                otp.Show();
                                this.Hide();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while loading the Sign-up form: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void pictureBox4_Click(object sender, EventArgs e)
        {
            pictureBox4.Enabled = false;
            try
            {
                var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("github.json", optional: false)
                .Build();

                var clientId = config["GitHub:ClientId"];
                var clientSecret = config["GitHub:ClientSecret"];
                var client = new GitHubClient(new ProductHeaderValue("Cybersec-integ"));

                var request = new OauthLoginRequest(clientId)
                {
                    Scopes = { "user:email" }
                };
                Uri loginURL = client.Oauth.GetGitHubLoginUrl(request);

                System.Diagnostics.Process.Start(
                new System.Diagnostics.ProcessStartInfo(loginURL.ToString())
                {
                    UseShellExecute = true
                }
                    );

            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while handling the picture box click: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
