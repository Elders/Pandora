using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Linq;

namespace Elders.Pandora.Api.Tcp
{
    public class Server
    {
        private TcpListener tcpListener;
        private Thread listenThread;
        private List<TcpClient> clients;

        public List<TcpClient> Clients
        {
            get
            {
                return clients;
            }
        }

        public Server()
        {
            this.clients = new List<TcpClient>();
            //this.tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 3000);
            this.tcpListener = new TcpListener(IPAddress.Any, 3000);
            this.listenThread = new Thread(new ThreadStart(startListen));
            this.listenThread.Start();
        }

        private void startListen()
        {
            this.tcpListener.Start();

            while (true)
            {
                //blocks until a client has connected to the server
                TcpClient client = this.tcpListener.AcceptTcpClient();

                var existing = clients.SingleOrDefault(x => x.Client.RemoteEndPoint == client.Client.RemoteEndPoint);

                if (existing != null)
                {
                    if (existing.Connected)
                        continue;
                    else
                    {
                        clients.Remove(existing);
                        clients.Add(client);
                    }
                }
                else
                {
                    clients.Add(client);
                }
                //create a thread to handle communication 
                //with connected client
                //Thread clientThread = new Thread(new ParameterizedThreadStart(handleCommunication));
                //clientThread.Start(client);
            }
        }

        private void handleCommunication(object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            var netStream = tcpClient.GetStream();

            while (true)
            {
                try
                {
                    if (netStream.CanRead)
                    {
                        byte[] bytes = new byte[tcpClient.ReceiveBufferSize];

                        // Read can return anything from 0 to numBytesToRead.  
                        // This method blocks until at least one byte is read.
                        netStream.Read(bytes, 0, (int)tcpClient.ReceiveBufferSize);

                        // Returns the data received from the host to the console. 
                        string message = Encoding.UTF8.GetString(bytes);

                        Console.WriteLine(message);
                    }
                }
                catch
                {
                    //a socket error has occured
                    break;
                }
            }
        }
    }
}