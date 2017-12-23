using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using ErnstNetworking.Settings;


namespace ErnstNetworking
{
    class EN_Server
    {
        static EN_Server server;
        static void Main(string[] args)
        {
            server = new EN_Server();
        }


        UdpClient client;
        IPEndPoint source;

        public EN_Server()
        {
            client = new UdpClient(EN_ServerSettings.PORT);
            source = new IPEndPoint(IPAddress.Any, 0);


            bool run = true;
            while (run == true)
            {
#if !UNITY_EDITOR
                if ((Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)){ run = false; }
#endif

                Recieve();
            }

            client.Close();
        }

        private void Recieve()
        {
            if (client.Available > 0)
            {
                byte[] bytes = client.Receive(ref source);

                if (bytes.Length > 0)
                {
#if !UNITY_EDITOR
                    Console.WriteLine(Encoding.ASCII.GetString(bytes));
                    Console.WriteLine("recieved from: " + source.ToString());
#endif
                }
            }
        }
    }
}