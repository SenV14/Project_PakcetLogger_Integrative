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
    
        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void btn_Confirm_otp_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txt_One_time_Permit.Text))
            {
                MessageBox.Show("Please enter the OTP.");
                return;
            }

            try
            {

                string OTP_permit = txt_One_time_Permit.Text;
                // Check your Port! Default is 3306.
                string connection = "Server=127.0.0.1;Port=3308;Database=packetlogger_login;Uid=root;Pwd=p@55w0rd23!4@;";

                // Added OTP_PACKET to the SELECT list
                string query = "SELECT OTP_PACKET FROM packetlogger_users WHERE  packet_gmail = @Email LIMIT 1";

                using (MySqlConnection connect = new MySqlConnection(connection))
                {
                    connect.Open();
                    using (MySqlCommand command = new MySqlCommand(query, connect))
                    {
                        command.Parameters.AddWithValue("@Email", email);
      
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string storedOTP = reader.GetString("OTP_PACKET");
                                string user_otp_typed =  txt_One_time_Permit.Text.Trim();
                                if (BCrypt.Net.BCrypt.Verify(user_otp_typed, storedOTP))
                                {
                                    MessageBox.Show("OTP verified successfully!");
                                    // Open your logger form here
                                }
                                else
                                {
                                    MessageBox.Show("Invalid OTP. The numbers don't match the hash.");
                                }
                            }
                            else
                            {
                                MessageBox.Show("No user found with the provided email.");
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                // This will now tell you if "OTP_PACKET" was missing or if the Port was wrong
                MessageBox.Show("Database Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OTP_LOGIN_Load(object sender, EventArgs e)
        {
            
        }
    }
}
