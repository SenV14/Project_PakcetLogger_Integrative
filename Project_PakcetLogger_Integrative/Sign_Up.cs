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
using System.Drawing.Text;
using System.Net;
using BCrypt.Net;


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
                if (!email.ToLower().EndsWith("@gmail.com") || string.IsNullOrEmpty(email) || password.Length < 8 || !password.Any(c => specialCharacters.Contains(c)))
                {

                    MessageBox.Show("Please enter a valid Gmail address.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txt_Password.Clear();
                    txt_Signup.Clear();
                    txt_Password.Focus();
                    return;

                }
                else 
                {
                    try
                    {
                        string @database = "Server=127.0.0.1;Port=3308;Database=packetlogger_login;Uid=root;Pwd=p@55w0rd23!4@";
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
                                            MessageBox.Show("Email is available for registration.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
                                            One_Time_Password otp = new One_Time_Password(email, hashedPassword);
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

        private bool HandleUserAuthentication(string email, string accessToken = null)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    MessageBox.Show("Email is required for authentication.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                string CONNECTION_STRING = "Server=127.0.0.1; Port=3308; Database=packetlogger_login; Uid=root; Pwd=p@55w0rd23!4@";
                using (var connect = new MySqlConnection(CONNECTION_STRING))
                {
                    connect.Open();
                    string protectedBase64 = null;


                        if (!string.IsNullOrEmpty(accessToken))
                    {
                        var protectedBytes = ProtectedData.Protect(Encoding.UTF8.GetBytes(accessToken), null, DataProtectionScope.CurrentUser);
                        protectedBase64 = Convert.ToBase64String(protectedBytes);
                    }

                    // 3. Upsert into database
                    string upsertQuery = @"INSERT INTO packet_logger_authentication (email_info, token_enc) 
                                 VALUES (@email, @token) 
                                 ON DUPLICATE KEY UPDATE token_enc = @token";

                    using (MySqlCommand upsertCmd = new MySqlCommand(upsertQuery, connect))
                    {
                        upsertCmd.Parameters.AddWithValue("@email", email);
                        // Send DBNull if there is no token (common for Google flow)
                        upsertCmd.Parameters.AddWithValue("@token", (object)protectedBase64 ?? DBNull.Value);
                        upsertCmd.ExecuteNonQuery();
                        
                    }
                    
                }
                return true;

            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while handling user authentication: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
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
                var oauth2Service = new Oauth2Service(new BaseClientService.Initializer { HttpClientInitializer = credential });
                Userinfo userinfo = await oauth2Service.Userinfo.Get().ExecuteAsync();

                string googleToken = credential.Token.AccessToken;

                if (HandleUserAuthentication(userinfo.Email, googleToken))
                {
                    this.Hide();
                    Packet_Logger mainApp = new Packet_Logger();
                    mainApp.FormClosed += (s, args) => this.Close();
                    mainApp.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while handling the picture box click: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
       

        private async void pictureBox4_Click(object sender, EventArgs eventArgs)
        {
            pictureBox4.Enabled = false;
            HttpListener listener = null;

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
                    // If your Octokit version supports it, you can set RedirectUri here:
                    // RedirectUri = "http://localhost:5001/"
                };

                Uri loginURL = client.Oauth.GetGitHubLoginUrl(request);

                // Start a local HTTP listener to receive the OAuth redirect.
                // Make sure this exact prefix is registered as a redirect URL in your GitHub OAuth app.
                var redirectPrefix = "http://localhost:5001/";
                listener = new HttpListener();
                listener.Prefixes.Add(redirectPrefix);
                listener.Start();

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(loginURL.ToString())
                {
                    UseShellExecute = true
                });

                // Wait for the incoming OAuth redirect request
                var context = await listener.GetContextAsync();
                var code = context.Request.QueryString.Get("code");

                // Respond to the browser so the user sees something
                var responseString = "<html><body>Authentication complete. You can close this window.</body></html>";
                var buffer = Encoding.UTF8.GetBytes(responseString);
                context.Response.ContentLength64 = buffer.Length;
                await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                context.Response.OutputStream.Close();

                var tokenRequest = new OauthTokenRequest(clientId, clientSecret, code);
                var token = await client.Oauth.CreateAccessToken(tokenRequest);
                if (token == null || string.IsNullOrWhiteSpace(token.AccessToken))
                {
                    MessageBox.Show("Failed to obtain access token.", "GitHub OAuth", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                client.Credentials = new Credentials(token.AccessToken);
                var user = await client.User.Current();

                string email = user?.Email;
                if (string.IsNullOrWhiteSpace(email))
                {
                    try
                    {
                        var emails = await client.User.Email.GetAll();
                        var primary = emails?.FirstOrDefault(e =>
                        {
                            if (e is null)
                            {
                                throw new ArgumentNullException(nameof(e));
                            }

                            return e.Primary;
                        }) ?? emails?.FirstOrDefault();
                        if (primary != null) email = primary.Email;
                    }
                    catch { }
                }

                if (string.IsNullOrWhiteSpace(email)) email = (user?.Login ?? "unknown") + "@github";

                if (HandleUserAuthentication(email, token.AccessToken))
                {
                    MessageBox.Show($"Authenticated as {user?.Login}", "Success");
                    this.Hide();
                    Packet_Logger mainApp = new Packet_Logger();
                    mainApp.FormClosed += (s, args) => this.Close();
                    mainApp.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while handling the picture box click: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                try { listener?.Stop(); listener?.Close(); } catch { }
                pictureBox4.Enabled = true;
            }
        }

        private void Sign_up_Load(object sender, EventArgs e)
        {

        }
    }
}
