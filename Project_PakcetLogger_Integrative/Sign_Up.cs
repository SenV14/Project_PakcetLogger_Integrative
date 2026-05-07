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
                        string @database = "Server=127.0.0.1;Port=3308;Database=packetlogger_login;Uid=root;Pwd=P@55W0RD";
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
                string database = "Server = 127.0.0.1; Port = 3308; Database = packetlogger_login; Uid = root; Pwd = P@55W0RD";
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

                if (string.IsNullOrWhiteSpace(email))
                    email = (user?.Login ?? "unknown") + "@github";

                SaveEncryptedToken(email, token.AccessToken);
                MessageBox.Show($"Authenticated as {user?.Login} (email: {email})", "GitHub OAuth", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void SaveEncryptedToken(string email, string token)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token)) return;

            // Protect token with DPAPI for current user/machine
            var protectedBytes = ProtectedData.Protect(Encoding.UTF8.GetBytes(token), null, DataProtectionScope.CurrentUser);
            var protectedBase64 = Convert.ToBase64String(protectedBytes);

            var cs = Environment.GetEnvironmentVariable("DB_CONN"); // don't hardcode
            using (var conn = new MySqlConnection(cs))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("INSERT INTO packet_logger_authentication (email_info, token_enc) VALUES (@e,@t) ON DUPLICATE KEY UPDATE token_enc=@t", conn))
                {
                    cmd.Parameters.AddWithValue("@e", email);
                    cmd.Parameters.AddWithValue("@t", protectedBase64);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
