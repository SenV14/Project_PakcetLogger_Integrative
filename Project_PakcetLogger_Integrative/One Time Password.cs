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
    public partial class One_Time_Password : Form
    {
        public string ReceivedData { get; set; }
        public One_Time_Password(string email)
        {
            ReceivedData = email;
            InitializeComponent();
        }
        public void SendOTP()
        {
            Random rnd = new Random();
            string otp_email = ReceivedData;
             int otpcode = rnd.Next(100000, 999999);
            string otp_string = otpcode.ToString();
            try
            {
                //used for message for sending the otp email
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("PacketLogger", "puppetemail875@gmail.com"));
                message.To.Add(new MailboxAddress("User", ReceivedData));
                message.Subject = "Your OTP Code for PacketLogger";
                // use for the body or text of the data
                var bodyBuilder = new BodyBuilder();
                bodyBuilder.TextBody = $"Your OTP code is: {otp_string}";
                message.Body = bodyBuilder.ToMessageBody();
                // used for sending the email using the SMTP client
                using (var client = new SmtpClient())
                {
                    client.Connect("smtp.gmail.com", 587, false);
                    client.Authenticate("puppetemail875@gmail.com", "aafjpmcwbqrszemq");
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

        }

        private void One_Time_Password_Load(object sender, EventArgs e)
        {

            SendOTP();
        }
    }
}
