using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpPcap;
using PacketDotNet;
using Microsoft.VisualBasic;
using MySql.Data.MySqlClient;
using System.Linq.Expressions;
namespace Project_PakcetLogger_Integrative

{
    public partial class Packet_Logger : Form
    {

        CaptureDeviceList devices = CaptureDeviceList.Instance;
        ILiveDevice device;


        public Packet_Logger()
        {
            InitializeComponent();

        }



        private void txt_start_Click(object sender, EventArgs e)
        {
            try
            {
                if (devices.Count == 0)
                {
                    MessageBox.Show("No network devices found.");
                    return;
                }

                string deviceList = "";
                for (int i = 0; i < devices.Count; i++)
                {
                    deviceList += $"{i}: {devices[i].Description}\n";
                }

                // Now that the 'Interaction' property is gone, this will work:
                string input = Microsoft.VisualBasic.Interaction.InputBox(
                    "Enter device index:\n\n" + deviceList,
                    "Select Device",
                    "0");

                if (int.TryParse(input, out int index) && index >= 0 && index < devices.Count)
                {
                    device = devices[index];
                    device.OnPacketArrival += new PacketArrivalEventHandler(device_OnPacketArrival);

                    // Open device in Promiscuous mode
                    device.Open(DeviceModes.Promiscuous, 1000);
                    device.StartCapture();

                    // UI Updates (Ensure these names match your designer buttons)
                    txt_start.Enabled = false;
                    btn_stop.Enabled = true;
                }
                else if (!string.IsNullOrEmpty(input))
                {
                    MessageBox.Show("Invalid index selected.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error starting capture: " + ex.Message);
            }
        }


        private void device_OnPacketArrival(object sender, PacketCapture e)
        {
            try
            {

                var rawPacket = e.GetPacket();
                var packet = Packet.ParsePacket(rawPacket.LinkLayerType, rawPacket.Data);

                // Extracting IP to get more useful info (Source/Dest IP)
                var ipPacket = packet.Extract<IPPacket>();
                var tcpPacket = packet.Extract<TcpPacket>();
                var transportPacket = packet.Extract<TransportPacket>();

                if (ipPacket != null && tcpPacket != null)
                {
                    string srcIP = ipPacket.SourceAddress.ToString();
                    string destIP = ipPacket.DestinationAddress.ToString();
                    string protocol = ipPacket.Protocol.ToString();
                    int port = transportPacket?.DestinationPort ?? 0;

                    // 1. Log to Database to complete the task
                    SavePacketToDb(srcIP, destIP, protocol, port);
                    string logEntry = $"[{DateTime.Now:HH:mm:ss}] {ipPacket.SourceAddress}:{tcpPacket.SourcePort} -> {ipPacket.DestinationAddress}:{tcpPacket.DestinationPort} | Len: {tcpPacket.PayloadData.Length}";

                    // Safe UI update from a background thread
                    if (lst_listpacket.InvokeRequired)
                    {
                        lst_listpacket.Invoke(new Action(() => lst_listpacket.Items.Add(logEntry)));
                        if (lst_listpacket.Items.Count > 100)
                        {
                            lst_listpacket.Items.RemoveAt(0);
                        }

                    }
                    else
                    {
                        lst_listpacket.Items.Add(logEntry);
                        if (lst_listpacket.Items.Count > 100) lst_listpacket.Items.RemoveAt(0);
                        lst_listpacket.TopIndex = lst_listpacket.Items.Count - 1;
                    }
                }
              
            }



            catch (Exception ex)
            {
                // Background errors are best handled silently or logged to a file
                Console.WriteLine("Parsing error: " + ex.Message);
            }
        }

        private void btn_stop_Click(object sender, EventArgs e)
        {
            try
            {
                // Fixed logic: Only stop if device is actually running
                if (device != null && device.Started)
                {
                    device.StopCapture();
                    device.Close();

                    txt_start.Enabled = true;
                    btn_stop.Enabled = false;
                    MessageBox.Show("Capture Stopped.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error stopping capture: " + ex.Message);
            }
        }

        private void btn_Close_Click(object sender, EventArgs e)
        {
            try
            {
                if (device != null && device.Started)
                {
                    device.StopCapture();
                    device.Close();
                }
                this.Close();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error closing application: " + ex.Message);
            }
        }
        private void SavePacketToDb(string src, string dest, string proto, int port)
        {
            try
            {
                string connString = "Server=127.0.0.1;Port=3308;Database=packetlogger_login;Uid=root;Pwd=p@55w0rd23!4@;";

                string query = "INSERT INTO PacketLogs (SourceIP, DestIP, Protocol, Port) VALUES (@src, @dest, @proto, @port);";
            
                    using (MySqlConnection conn = new MySqlConnection(connString))
                    {
                        // Using the Stored Procedure for "Advanced Implementation"
                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        {
                            cmd.CommandType = CommandType.Text;

                            cmd.Parameters.AddWithValue("@src", src);
                            cmd.Parameters.AddWithValue("@dest", dest);
                            cmd.Parameters.AddWithValue("@proto", proto);
                            cmd.Parameters.AddWithValue("@port", port);

                            conn.Open();
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
           
            catch (Exception e)
            {
                Console.WriteLine("Database error: " + e.Message);
            }
        }
    }
}
