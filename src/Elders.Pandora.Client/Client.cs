using Elders.Pandora.Box;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Elders.Pandora.Client
{
    public static class Client
    {
        public static void Connect(string ipAddress, int port)
        {
            var tcpClient = new TcpClient();
            tcpClient.Connect(new IPEndPoint(IPAddress.Parse(ipAddress), port));

            if (tcpClient.Connected)
            {
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

                            var cfg = JsonConvert.DeserializeObject<Jar>(message);

                            var box = Elders.Pandora.Box.Box.Mistranslate(cfg);

                            var pandora = new Pandora(box).Open("local", Environment.MachineName);
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
}