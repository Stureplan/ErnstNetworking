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
#if !UNITY_EDITOR && !UNITY_5 && !UNITY_STANDALONE
    class EN_Server
    {
        static EN_Server SERVER;
        static void Main(string[] args)
        {
            SERVER = new EN_Server();
        }


        UdpClient server;
        IPEndPoint source;
        Dictionary<int, IPEndPoint> clients;


        public EN_Server()
        {
            server = new UdpClient(EN_ServerSettings.PORT);
            source = new IPEndPoint(IPAddress.Any, 0);
            clients = new Dictionary<int, IPEndPoint>();

            bool run = true;
            while (run == true)
            {
                // Quit if we pressed Escape
                // TODO: Disconnect all clients and maybe some cleanup (?)
                if ((Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)){ run = false; }

                Recieve();
            }

            server.Close();
        }

        private void Recieve()
        {
            if (server.Available > 0)
            {
                byte[] bytes = server.Receive(ref source);

                if (bytes.Length > 0)
                {
                    // Get & translate first 4 bytes
                    EN_PACKET_TYPE packet_type = EN_Protocol.BytesToType(bytes);

                    // Print packet info
                    Console.WriteLine(source.Address.ToString() + ": " + TranslateMessage(source, packet_type, bytes));
                }
            }
        }

        private string TranslateMessage(IPEndPoint source, EN_PACKET_TYPE type, byte[] bytes)
        {
            string s = "";
            if (type == EN_PACKET_TYPE.CONNECT)
            {
                EN_PacketConnect packet;
                packet.packet_type = EN_PACKET_TYPE.CONNECT_CONFIRMED;
                packet.packet_client_id = clients.Count;
                byte[] message = new byte[bytes.Length - 4 - 4];
                Buffer.BlockCopy(bytes, 8, message, 0, message.Length);
                string name = Encoding.ASCII.GetString(message);

                s = name + " CONNECTED!";
                clients.Add(clients.Count, source);

                ConfirmConnection(packet, name);
            }
            if (type == EN_PACKET_TYPE.DISCONNECT)
            {
                EN_PacketDisconnect packet = EN_Protocol.BytesToObject<EN_PacketDisconnect>(bytes);
                s = "DISCONNECTED!";
                //TODO: remove source from clients list
            }
            if (type == EN_PACKET_TYPE.MESSAGE)
            {
                EN_PacketMessage packet;
                packet.packet_type = EN_PACKET_TYPE.MESSAGE;

                byte[] message = new byte[bytes.Length-4];
                Buffer.BlockCopy(bytes, 4, message, 0, message.Length);
                packet.packet_data = message;
                s = Encoding.ASCII.GetString(packet.packet_data);
            }

            return s;
        }

        private void ConfirmConnection(EN_PacketConnect packet, string name)
        {
            byte[] b1 = EN_Protocol.ObjectToBytes(packet);
            byte[] b2 = EN_Protocol.StringToBytes(name);

            byte[] bytes = new byte[b1.Length + b2.Length];
            Buffer.BlockCopy(b1, 0, bytes, 0, b1.Length);
            Buffer.BlockCopy(b2, 0, bytes, b1.Length, b2.Length);

            BroadcastPacket(bytes);
        }

        private void BroadcastPacket(byte[] bytes)
        {
            for (int i = 0; i < clients.Count; i++)
            {
                server.Send(bytes, bytes.Length, clients[i]);
            }
        }
    }
#endif
}