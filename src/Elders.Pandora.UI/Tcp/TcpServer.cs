using Elders.Pandora.Box;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Elders.Pandora.UI.Tcp
{
    public class TcpServer
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(TcpServer));

        private TcpListener tcpListener;
        private Thread listenThread;
        private List<TcpClient> clients;
        BlockingCollection<Byte[]> bcinfo = new BlockingCollection<byte[]>();

        public void SendToAllClients(byte[] info)
        {
            bcinfo.Add(info);
        }

        private void Pulse()
        {
            foreach (var item in bcinfo)
            {
                foreach (var client in clients)
                {
                    try
                    {
                        if (client.Connected)
                            client.Client.Send(item);
                    }
                    catch (Exception ex)
                    {
                        log.Fatal("Something happend. Can not send to client", ex);
                    }
                }
            }
        }

        private Thread pulse;

        public TcpServer()
        {

        }

        public void Start()
        {
            this.clients = new List<TcpClient>();
            //this.tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 3000);
            this.tcpListener = new TcpListener(IPAddress.Any, 3000);

            this.listenThread = new Thread(new ThreadStart(startListen));
            this.listenThread.Start();
            this.pulse = new Thread(Pulse);
            this.pulse.Start();
        }

        private void startListen()
        {
            this.tcpListener.Start();

            while (true)
            {
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

                //Thread clientThread = new Thread(new ParameterizedThreadStart(handleCommunication));
                //clientThread.Start(client);
            }
        }

        //private void handleCommunication(object client)
        //{
        //    TcpClient tcpClient = (TcpClient)client;
        //    var netStream = tcpClient.GetStream();

        //    while (true)
        //    {
        //        try
        //        {
        //            if (netStream.CanRead)
        //            {
        //                byte[] bytes = new byte[tcpClient.ReceiveBufferSize];
        //                netStream.Read(bytes, 0, (int)tcpClient.ReceiveBufferSize);

        //                string message = Encoding.UTF8.GetString(bytes);

        //                Console.WriteLine(message);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            break;
        //        }
        //    }
        //}
    }
}