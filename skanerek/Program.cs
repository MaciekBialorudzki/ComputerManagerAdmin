using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace RemoteComputerManager
{
    class Program
    {
        static void Main(string[] args)
        {
            // Get the IP address of the local machine
            IPAddress localAddress = GetLocalIPAddress();

            // Display the local IP address to the user
            Console.WriteLine("Local IP Address: {0}", localAddress.ToString());

            // Prompt the user to enter the IP address of the remote machine
            Console.WriteLine("Enter the IP address of the remote machine:");
            string remoteAddressString = Console.ReadLine();
            IPAddress remoteAddress = IPAddress.Parse(remoteAddressString);

            // Prompt the user to enter the URL of the web page to open
            Console.WriteLine("Enter the URL of the web page to open:");
            string url = Console.ReadLine();

            // Build the command to open the web page remotely
            string command = string.Format("start {0}", url);

            // Create a new process to run the command
            ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/c " + command);
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;

            // Create a new client socket to connect to the remote machine
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Connect to the remote machine
            clientSocket.Connect(new IPEndPoint(remoteAddress, 8888));

            // Send the command to the remote machine
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(command);
            clientSocket.Send(buffer);

            // Close the socket
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }

        static IPAddress GetLocalIPAddress()
        {
            // Get the host name
            string hostName = Dns.GetHostName();

            // Get the IP addresses for the host
            IPHostEntry hostEntry = Dns.GetHostEntry(hostName);

            // Loop through the IP addresses and return the first non-localhost address
            foreach (IPAddress address in hostEntry.AddressList)
            {
                if (!IPAddress.IsLoopback(address) && address.AddressFamily == AddressFamily.InterNetwork)
                {
                    return address;
                }
            }

            // If no non-localhost addresses were found, return the first address in the list
            return hostEntry.AddressList[0];
        }
    }
}
