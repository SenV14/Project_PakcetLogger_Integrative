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
using Mysqlx.Crud;

namespace Project_PakcetLogger_Integrative
{
    public partial class One_Time_Password : Form
    {
        public string ReceivedData { get; set; }
        public string password_2nd { get; set; }
        //create a variable for the otp code, so that it can be used in the otp_confirm method
        Random rnd = new Random();
        

        //limit for the otp code, if the user input the wrong otp code more than 3 times, the user will be exited in this site
        int Limit_Reset = 0;
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
        
        public void SendOTP()
        {
            int otpcode = rnd.Next(100000, 999999);
            string otp_string = otpcode.ToString();
            string otp_email = ReceivedData;


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



        private void btn_Confirm_otp_Click(object sender, EventArgs e, int otpcode)
        {
            otp_confirm(otpcode);
        }

        private void One_Time_Password_Load(object sender, EventArgs e)
        {

            SendOTP();
        }

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

        private void btn_Confirm_otp_Click(object sender, EventArgs e)
        {
            string otp_input = txt_One_time_Permit.Text.Trim();
            otp_confirm(int.Parse(otp_input));
        }
    }
}
