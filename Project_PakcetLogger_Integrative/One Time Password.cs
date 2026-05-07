    using MailKit.Net.Smtp;
using MimeKit;
using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace Project_PakcetLogger_Integrative
{
    public partial class One_Time_Password : Form
    {
        // used for getting password and signup user name in the signup form, so that it can be used in the otp_confirm method
        public string ReceivedData { get; set; }
        public string password_2nd { get; set; }
        //create a variable for the otp code, so that it can be used in the otp_confirm method
        Random rnd = new Random();
        // this if the global public variable for the otp code, so that it can be used in the otp_confirm method
        private string otp_code_global;
        

        //limit for the otp code, if the user input the wrong otp code more than 3 times, the user will be exited in this site
        private int Limit_Reset = 0;

        public One_Time_Password(string email, string password)
        {       
            InitializeComponent();
            ReceivedData = email;
            password_2nd = password;
        }
 
        public void otp_confirm(int otpcode_confirm)
        {
            try
            {
                if(txt_One_time_Permit.Text == otpcode_confirm.ToString())
                { 
                    MessageBox.Show("OTP code confirmed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while confirming the OTP code. Please try again. {ex}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        //used for sending otp using mime and mail packages
        public void SendOTP()
        {
            string otpCode;
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                byte[] bytes = new byte[4];
                rng.GetBytes(bytes);
                int value = BitConverter.ToInt32(bytes, 0);
                // This math ensures a 6-digit number between 100000 and 999999
                otpCode = (Math.Abs(value % 900000) + 100000).ToString();
            }
            otp_code_global = otpCode;



            try
            {
                //used for message for sending the otp email
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("PacketLogger", "puppetemail875@gmail.com"));
                message.To.Add(new MailboxAddress("User", ReceivedData));
                message.Subject = "Your OTP Code for PacketLogger";

                // use for the body or text of the data
                var bodyBuilder = new BodyBuilder();
                bodyBuilder.TextBody = $"Your OTP code is: {otp_code_global}";
                message.Body = bodyBuilder.ToMessageBody();
                // used for sending the email using the SMTP client
                using (var client = new SmtpClient())
                {
                    client.Connect("smtp.gmail.com", 587, false);
                    // must use your own client email to be the sender
                    client.Authenticate("puppetemail875@gmail.com", "aafjpmcwbqrszemq");
                    // .send used for sending to the receiver
                    client.Send(message);
                    client.Disconnect(true);
                   
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while sending the OTP email. Please check your email address and try again. {ex}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


        }
        private void btn_Send_otp_Click(object sender, EventArgs e)
        {

        } 
        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }



        private void btn_Confirm_otp_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txt_One_time_Permit.Text))
                {
                    MessageBox.Show("Please enter the OTP code.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Limit_Reset++;
                    SendOTP();
                    return;
                }
                else if (txt_One_time_Permit.Text == otp_code_global)
                {
                    MessageBox.Show("Confirmation of the random number is correct, You will now go.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    //going to database function to open the database local
                    Database_Confirm_Open(otp_code_global,ReceivedData, password_2nd);

                    this.Hide();
                    
                }
                else if (Limit_Reset > 3)
                {
                    

                    MessageBox.Show($"You have exceeded the amount of tries on the otp, you will be exited in this site.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("There is an error, try again", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    SendOTP();
                    Limit_Reset++;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while confirming the OTP code. Please try again. {ex}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //automatic open of function to send otp
        private void One_Time_Password_Load(object sender, EventArgs e)
        {

            SendOTP();
        }

        // linklabel to resend 3-4 times when forgotten otp code, if the user click the linklabel more than 3 times, the user will be exited in this site
        private void lbl_Resend_code_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
    
                Limit_Reset++;
                SendOTP();
                MessageBox.Show("OTP code resent successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                if (Limit_Reset > 3)
                {
                    MessageBox.Show($"You have exceeded the amount of tries on the otp, you will be exited in this site.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while sending the OTP email. Please check your email address and try again. {ex}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        
            
            
        }
        public void Database_Confirm_Open(string otp_code, string receivedData, string password)
        {
            try
            {
                string Code = otp_code;
                string Email = receivedData;    
                string Password = password;
                string @database = "Server=127.0.0.1;Port=3308;Database=packetlogger_login;Uid=root;Pwd=p@55w0rd23!4@;";      
                var databases = new MySqlConnection(@database);
                databases.Open();
                if (databases.State == ConnectionState.Open)
                {
                    MessageBox.Show("You are in, Congratulations! on your own", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DatabaseCommand(Code, Email, Password);
                }
               
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while connecting to the database. Please check your database connection and try again. {ex}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        public void DatabaseCommand(string Code,string  Email, string Password)
        {
            try
            {
                
                DateTime patch_date = DateTime.Now;
                string @database = "Server=127.0.0.1;Port=3308;Database=packetlogger_login;Uid=root;Pwd=p@55w0rd23!4@;";
                string select_method = "INSERT INTO packetlogger_users (packet_gmail, packet_password, OTP_PACKET) VALUES (@Email, @Password,@Code)";
                using(MySqlConnection @connection = new MySqlConnection(@database))
                {
                    connection.Open();
                    using (MySqlCommand INSERT = new MySqlCommand(select_method, connection))
                    {
                        INSERT.Parameters.AddWithValue("@Code", Code);
                        INSERT.Parameters.AddWithValue("@Email", Email);
                        INSERT.Parameters.AddWithValue("@Password", Password);
                        INSERT.ExecuteNonQuery();
                        Login login = new Login();
                        login.Show();
                        this.Hide();
                    }
                  
                }
              
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while executing the database command. Please check your database command and try again. {ex}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }
    }
}
