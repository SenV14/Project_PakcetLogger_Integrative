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
                string connection = "";
                string query = "SELECT packet_gmail, packet_password FROM packetlogger_users WHERE packet_gmail = @Email AND packet_password = @Password LIMIT 1";
                using (MySqlConnection connect = new MySqlConnection(connection))
                {
                    using(MySqlCommand command = new MySqlCommand(query, connect))
                    {
                        command.Parameters//
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
