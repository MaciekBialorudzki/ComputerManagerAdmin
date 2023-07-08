using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.DirectoryServices;
using System.Net.NetworkInformation;


namespace ComputerManagement
{
    public partial class MainWindow : Form
    {


        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            
            await LoadComputerIPAddressesAsync();
            SetupDataGridView();

        }

        private void SetupDataGridView()
        {

            dataGridView.DataSource = ComputerList;
            dataGridView.Columns["IPAddress"].HeaderText = "IP Address";
            dataGridView.Columns["ComputerName"].HeaderText = "Computer Name";
            dataGridView.Columns["IsSelected"].HeaderText = "Select";
            dataGridView.Columns["IsSelected"].Width = 50;

            // Set the checkbox column to data-bound mode
            dataGridView.Columns["IsSelected"].DataPropertyName = "IsSelected";
            dataGridView.Columns["IsSelected"].ReadOnly = false;

            // Adjust the table size to fit the grid both horizontally and vertically
            dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        }


        public class Computer
        {
            public string ComputerName { get; set; }
            public string IPAddress { get; set; }
            public bool IsSelected { get; set; }
        }


        private List<Computer> ComputerList { get; set; } = new List<Computer>();


        private async Task LoadComputerIPAddressesAsync()
        {
            await Task.Run(() =>
            {
                DirectoryEntry root = new DirectoryEntry("WinNT:");

                foreach (DirectoryEntry computers in root.Children)
                {
                    foreach (DirectoryEntry computer in computers.Children)
                    {
                        if (computer.Name != "Schema")
                        {
                            string computerName = computer.Name;
                            string ipAddress = GetIPAddress(computerName);

                            Computer newComputer = new Computer
                            {
                                ComputerName = computerName,
                                IPAddress = ipAddress
                            };

                            ComputerList.Add(newComputer);

                        


                        }
                        
                    }
                }
            });
        }


        static string GetIPAddress(string computerName)
        {
            try
            {
                Ping ping = new Ping();
                PingReply reply = ping.Send(computerName);

                if (reply.Status == IPStatus.Success)
                {
                    return reply.Address.ToString();
                }
            }
            catch (PingException)
            {

            }

            return "Offline";
        }


        private void label1_Click(object sender, EventArgs e)
        {

        }



        private List<DataGridViewRow> GetSelectedRows()
        {
            List<DataGridViewRow> selectedRows = new List<DataGridViewRow>();
            int selectColumnIndex = dataGridView.Columns["IsSelected"].Index;

            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                DataGridViewCheckBoxCell checkBoxCell = row.Cells[selectColumnIndex] as DataGridViewCheckBoxCell;

                if (checkBoxCell != null && (bool)checkBoxCell.FormattedValue)
                {
                    selectedRows.Add(row);
                }
            }

            return selectedRows;
        }



        private void sendUrlButton_Click(object sender, EventArgs e)
        {

            List<DataGridViewRow> selectedRows = GetSelectedRows();



            foreach (DataGridViewRow selectedRow in selectedRows)
            {
                // working code below
                label1.Text = selectedRow.ToString();
                string remoteAddressString = selectedRow.Cells["IPAddress"]?.Value?.ToString() ?? "";
                IPAddress remoteAddress = IPAddress.Parse(remoteAddressString);

                label1.Text = remoteAddressString;

                string url = urlTextBox.Text;

                // command
                string command = string.Format("{0}", url);

                ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/c " + command);
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;

                Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                clientSocket.Connect(new IPEndPoint(remoteAddress, 8888));
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(command);
                clientSocket.Send(buffer);

                // Close the socket
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }
        }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private async void button1_Click(object sender, EventArgs e)
        {
            ComputerList.Clear();
            await LoadComputerIPAddressesAsync();
            SetupDataGridView();
        }
    }
}