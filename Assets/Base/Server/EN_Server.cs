using System;
using System.Collections.Generic;
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


        UdpClient udp_server;
        TcpListener tcp_server;
        IPEndPoint source;
        Dictionary<int, IPEndPoint> udp_clients;
        List<TcpClient> tcp_clients;
        List<byte[]> packet_stack;


        public EN_Server()
        {
            udp_server = new UdpClient(EN_ServerSettings.PORT);
            tcp_server = new TcpListener(IPAddress.Any,EN_ServerSettings.PORT);
            source = new IPEndPoint(IPAddress.Any, 0);
            udp_clients = new Dictionary<int, IPEndPoint>();
            tcp_clients = new List<TcpClient>();
            packet_stack = new List<byte[]>();

            Console.WriteLine("Waiting for a connection...");
            tcp_server.Start();
            tcp_clients.Add(tcp_server.AcceptTcpClient());


            while (true)
            {
                // Quit if we pressed Escape
                // TODO: Disconnect all clients and maybe some cleanup (?)
                if ((Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)){ break; }

                RecieveTCP();
                RecieveUDP();
            }

            udp_server.Close();
        }

        private void RecieveUDP()
        {
            if (udp_server.Available > 0)
            {
                byte[] bytes = udp_server.Receive(ref source);

                if (bytes.Length > 0)
                {
                    // Get & translate first 4 bytes
                    EN_PACKET_TYPE packet_type = EN_Protocol.BytesToType(bytes);

                    // Print packet info
                    Console.WriteLine("UDP " + source.Address.ToString() + ": " + TranslateUDP(source, packet_type, bytes));
                }
            }
        }

        private void RecieveTCP()
        {
            for (int i = 0; i < tcp_clients.Count; i++)
            {
                int bytes_available = tcp_clients[i].Available;
                if (bytes_available > 0)
                {
                    NetworkStream stream = tcp_clients[i].GetStream();
                    byte[] bytes = new byte[bytes_available];
                    stream.Read(bytes, 0, bytes_available);


                    EN_PACKET_TYPE packet_type = EN_Protocol.BytesToType(bytes);

                    Console.WriteLine("TCP " + ((IPEndPoint)tcp_clients[i].Client.RemoteEndPoint).Address.ToString() + ": " + TranslateTCP(tcp_clients[i], source, packet_type, bytes));
                }
            }
        }

        private string TranslateUDP(IPEndPoint source, EN_PACKET_TYPE type, byte[] bytes)
        {
            string s = "";
            if (type == EN_PACKET_TYPE.TRANSFORM)
            {
                EN_PacketTransform packet = EN_Protocol.BytesToObject<EN_PacketTransform>(bytes);
                BroadcastUDP(bytes);
            }

            return s;
        }

        private string TranslateTCP(TcpClient client, IPEndPoint source, EN_PACKET_TYPE type, byte[] bytes)
        {
            string s = "";
            if (type == EN_PACKET_TYPE.CONNECT)
            {
                EN_PacketConnect packet = EN_Protocol.BytesToObject<EN_PacketConnect>(bytes);
                packet.packet_client_id = udp_clients.Count;

                byte[] message = new byte[bytes.Length - 8 - 16];
                Buffer.BlockCopy(bytes, 8 + 16, message, 0, message.Length);
                string name = EN_Protocol.BytesToString(message);

                // Add this new client to our list
                //TcpClient client = new TcpClient(source);
                //tcp_clients.Add(client);

                // Resend older important messages from before
                BroadcastStackTCP(client);

                // Setup an ID to replace the old -1 ID from the packet
                byte[] newID = new byte[4];
                newID = BitConverter.GetBytes(tcp_clients.Count);
                for (int i = 0; i < 4; i++)
                {
                    // Good ol-fashioned byte swap to insert the new ID
                    byte b = newID[i];
                    bytes[4 + i] = b;
                }

                // Send out the connection packet to the rest of the clients
                BroadcastTCP(bytes);

                // Add connect request to the stack of important messages
                packet_stack.Add(bytes);

                s = name + " connected.";
            }
            if (type == EN_PACKET_TYPE.DISCONNECT)
            {
                EN_PacketDisconnect packet = EN_Protocol.BytesToObject<EN_PacketDisconnect>(bytes);

                s = " disconnected.";
                //TODO: remove source from clients list
            }
            if (type == EN_PACKET_TYPE.MESSAGE)
            {
                EN_PacketMessage packet;
                packet.packet_type = EN_PACKET_TYPE.MESSAGE;

                byte[] message = new byte[bytes.Length-4];
                Buffer.BlockCopy(bytes, 4, message, 0, message.Length);
                packet.packet_data = message;
                s = EN_Protocol.BytesToString(packet.packet_data);
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

            BroadcastTCP(bytes);

            packet_stack.Add(bytes);
        }

        private void BroadcastUDP(byte[] bytes)
        {
            foreach (KeyValuePair<int, IPEndPoint> c in udp_clients)
            {
                udp_server.Send(bytes, bytes.Length, c.Value);
            }
        }

        private void BroadcastTCP(byte[] bytes)
        {
            for (int i = 0; i < tcp_clients.Count; i++)
            {
                NetworkStream stream = tcp_clients[i].GetStream();
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        private void BroadcastStackTCP(TcpClient client)
        {
            NetworkStream stream = client.GetStream();

            // Re-broadcast all old messages
            for (int i = 0; i < packet_stack.Count; i++)
            {
                stream.Write(packet_stack[i], 0, packet_stack[i].Length);
            }
        }
    }
#endif
}