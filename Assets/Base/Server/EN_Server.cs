using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using ErnstNetworking.Server;
using ErnstNetworking.Protocol;



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
#if !UNITY_EDITOR && !UNITY_5 && !UNITY_STANDALONE
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
#if !UNITY_EDITOR && !UNITY_5 && !UNITY_STANDALONE
                    //Console.WriteLine(Encoding.ASCII.GetString(bytes));

                    // Get & translate first 4 bytes
                    EN_PACKET_TYPE packet_type = EN_Protocol.BytesToType(bytes);

                    // Print
                    Console.WriteLine(source.ToString() + ": " + TranslateMessage(packet_type, bytes));
#endif
                }
            }
        }

        private string TranslateMessage(EN_PACKET_TYPE type, byte[] bytes)
        {
            string s = "";
            if (type == EN_PACKET_TYPE.CONNECT)
            {
                EN_PacketConnect packet = EN_Protocol.BytesToObject<EN_PacketConnect>(bytes);
                s = "CONNECTED!";
            }
            if (type == EN_PACKET_TYPE.DISCONNECT)
            {
                EN_PacketDisconnect packet = EN_Protocol.BytesToObject<EN_PacketDisconnect>(bytes);
                s = "DISCONNECTED!";
            }
            if (type == EN_PACKET_TYPE.MESSAGE)
            {
                EN_PacketMessage packet;
                packet.packet_type = EN_Protocol.BytesToType(bytes);

                byte[] message = new byte[bytes.Length-4];
                Buffer.BlockCopy(bytes, 4, message, 0, message.Length);
                packet.packet_data = message;
                s = Encoding.ASCII.GetString(packet.packet_data);
            }

            return s;
        }
    }
}