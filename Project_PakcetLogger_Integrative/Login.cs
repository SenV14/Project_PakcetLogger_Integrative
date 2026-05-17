using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Oauth2.v2;
using Google.Apis.Oauth2.v2.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using Octokit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls.WebParts;
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

            var config = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("Database.json", optional: false)
                   .Build();
            string @database = config.GetConnectionString("DefaultConnection");
            string @selecting_method = "SELECT packet_gmail, packet_password FROM packetlogger_users WHERE packet_gmail = @gmail LIMIT 1";
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
                       

                        using (MySqlDataReader reader = sql_identity.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string storedEmail = reader.GetString("packet_gmail");
                                string stored_hash = reader.GetString("packet_password");

                                if (storedEmail == email && BCrypt.Net.BCrypt.Verify(password, stored_hash))
                                {
                                    MessageBox.Show("Login successful! Welcome, " + storedEmail, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    MessageBox.Show("Password Correct and successfully logged in!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    MessageBox.Show("Please wait while we redirect you to the OTP form.", "Redirecting", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    this.Hide();
                                    OTP_LOGIN otpForm = new OTP_LOGIN(email, password);
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
            }
        }


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
                MessageBox.Show("Please put on the right password in this account before continue", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        private bool HandleUserAuthentication(string email, string accessToken = null)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    MessageBox.Show("Email is required for authentication.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("Database.json", optional: false)
                    .Build();
                string CONNECTION_STRING = config.GetConnectionString("DefaultConnection");
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

            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while handling the picture box click: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }





        private async void pcb_Google_Click(object sender, EventArgs e)
        {
            try
            {
                // Resolve the authentication file from the application's base directory so it works after build/publish
                var authPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AUTHENTICATION.json");
                if (!File.Exists(authPath))
                {
                    MessageBox.Show($"Authentication file not found: {authPath}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                using (var stream = new FileStream(authPath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    var secrets = GoogleClientSecrets.FromStream(stream).Secrets;
                    var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                    {
                        ClientSecrets = secrets,
                        Scopes = new[] { Oauth2Service.Scope.UserinfoEmail },
                        DataStore = new FileDataStore("DataApp")
                    });

                    var app = new AuthorizationCodeInstalledApp(flow, new LocalServerCodeReceiver());
                    var credential = await app.AuthorizeAsync("user", CancellationToken.None);
                    // Use or store `credential` as needed
                    // 1. Get user email from Google API
                    var oauthService = new Oauth2Service(new BaseClientService.Initializer { HttpClientInitializer = credential });
                    Userinfo userInfo = await oauthService.Userinfo.Get().ExecuteAsync();

                    // 2. Validate with Database and Switch Form
                    if (HandleUserAuthentication(userInfo.Email, credential.Token.AccessToken))
                    {
                        this.Hide();
                        Packet_Logger mainApp = new Packet_Logger();
                        mainApp.FormClosed += (s, args) => this.Close(); // This is the most important line
                        
                        mainApp.Show();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while accessing the authentication file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void pcb_Microsoft_Click(object sender, EventArgs e)
        {
            try
            {
                string json = File.ReadAllText("github.json");

                var root = JObject.Parse(json);

                if (root == null)
                {
                    MessageBox.Show("Client ID not found in github.json", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                var git = root["GitHub"];

                string clientId = git["ClientId"].ToString();
                string clientSecret = git["ClientSecret"].ToString();
                string redirectUri = git["RedirectUri"].ToString();
                string reponame = git["RepositoryName"].ToString();
                string Owner = git["Owner"].ToString();

                //start the authentication process

                var listener = new System.Net.HttpListener();
                listener.Prefixes.Add(redirectUri);
                listener.Start();

                //OPEMIMH THE BROWSER WITH OAUTH
                string authorization = $"https://github.com/login/oauth/authorize?client_id={clientId}&redirect_uri={redirectUri}&scope=user,repo";
                Process.Start(new ProcessStartInfo(authorization) { UseShellExecute = true });

                var context = await listener.GetContextAsync();
                string code = context.Request.QueryString["code"];

                byte[] buffer = System.Text.Encoding.UTF8.GetBytes("Authentication successful! You can close this window.");
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                context.Response.OutputStream.Close();
                listener.Stop();

                var Gitclient = new GitHubClient(new ProductHeaderValue("YourAppName"));
                var tokenRequest = new OauthTokenRequest(clientId, clientSecret, code);
                var TOKEN = await Gitclient.Oauth.CreateAccessToken(tokenRequest);

                //
                Gitclient.Credentials = new Credentials(TOKEN.AccessToken);
                var user = await Gitclient.User.Current();
                var repos = await Gitclient.Repository.GetAllForCurrent();
                string email = user.Email ?? user.Login;

                // 2. Validate with Database and Switch Form
                if (HandleUserAuthentication(email, TOKEN.AccessToken))
                {
                    MessageBox.Show($"Logged in as {user.Login}", "Success");
                    this.Hide();
                    Packet_Logger mainApp = new Packet_Logger();
                    mainApp.FormClosed += (s, args) => this.Close();
                    mainApp.Show();
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while reading the GitHub configuration: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Login_Load_1(object sender, EventArgs e)
        {

        }
    }
}