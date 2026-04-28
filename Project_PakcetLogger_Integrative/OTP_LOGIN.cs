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

namespace Project_PakcetLogger_Integrative
{
    public partial class OTP_LOGIN : Form
    {
        
        public string email { get; set; }
        public string password { get; set; }
        public OTP_LOGIN(string email, string password)
        {
            InitializeComponent();
      
        }
        private void identify_otp(string email, string password)
        {
            try
            {
                string OTP_permit = txt_One_time_Permit.Text;
                string connection = "Server=127.0.0.1;Port=3306;Database=packetlogger_login;Uid=root;Pwd=P@55W0RD;"; ;
                string query = "SELECT packet_gmail, packet_password FROM packetlogger_users WHERE packet_gmail = @Email AND OTP_PACKET = @OTP LIMIT 1";
                using (MySqlConnection connect = new MySqlConnection(connection))
                {
                    using (MySqlCommand command = new MySqlCommand(query, connect))
                    {
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@OTP", OTP_permit);
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string emailed = reader.GetString("packet_gmail");
                                string otp = reader.GetString("OTP_PACKET");
                                if (emailed == email && otp == OTP_permit)
                                {
                                    MessageBox.Show("OTP verified successfully! Welcome, " + emailed, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    Packet_Logger loggerForm = new Packet_Logger();
                                    loggerForm.Show();
                                    this.Hide();
                                    
                                }
                                else
                                {
                                    MessageBox.Show("Invalid OTP. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    txt_One_time_Permit.Clear();
                                }
                            }
                        }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while identifying the OTP: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }   
        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void btn_Confirm_otp_Click(object sender, EventArgs e)
        {

        }

        private void OTP_LOGIN_Load(object sender, EventArgs e)
        {
            identify_otp(email,password);
        }
    }
}
