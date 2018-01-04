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
        List<IPEndPoint> udp_clients;
        List<TcpClient> tcp_clients;
        List<EN_ClientInfo> clients;
        List<byte[]> packet_stack;
        
        public EN_Server()
        {
            EN_ClientInfo c1 = new EN_ClientInfo(Guid.NewGuid(), "123");
            EN_ClientInfo c2 = new EN_ClientInfo(Guid.NewGuid(), "123456");

            byte[] b1 = EN_Protocol.ObjectToBytes(c1);
            byte[] b2 = EN_Protocol.ObjectToBytes(c2);

            EN_ClientInfo bc1 = EN_Protocol.BytesToObject<EN_ClientInfo>(b1);
            EN_ClientInfo bc2 = EN_Protocol.BytesToObject<EN_ClientInfo>(b2);
            //TODO: Marshal the shit out of these EN_ClientInfos.
            // This allows us to send them straight over the network as byte[] in sequence.


            udp_server = new UdpClient(EN_ServerSettings.PORT);
            tcp_server = new TcpListener(IPAddress.Any,EN_ServerSettings.PORT);
            source = new IPEndPoint(IPAddress.Any, 0);
            udp_clients = new List<IPEndPoint>();
            tcp_clients = new List<TcpClient>();
            packet_stack = new List<byte[]>();
            clients = new List<EN_ClientInfo>();

            Console.WriteLine("\t\t::ErnstNetworking Server::\n");
            Console.WriteLine("Waiting for connections...");

            tcp_server.Start();

            while (true)
            {
                // Quit if we pressed Escape
                // TODO: Disconnect all clients and maybe some cleanup (?)
                if ((Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)){ break; }

                // Searches the tcp listener for new connections.
                DiscoverClients();

                // Check if clients have disconnected.
                PollClients();

                // Receive messages over TCP & UDP
                ReceiveTCP();
                ReceiveUDP();
            }

            udp_server.Close();
            for (int i = 0; i < tcp_clients.Count; i++)
            {
                tcp_clients[i].GetStream().Close();
                tcp_clients[i].Close();
            }
        }

        private async void DiscoverClients()
        {
            TcpClient client = await tcp_server.AcceptTcpClientAsync();
            tcp_clients.Add(client);
        }

        private void PollClients()
        {
            for (int i = 0; i < tcp_clients.Count; i++)
            {
                if (tcp_clients[i].Client.Poll(0, SelectMode.SelectRead) == true)
                {
                    byte[] bytes = new byte[1];
                    if (tcp_clients[i].Client.Receive(bytes, SocketFlags.Peek) == 0)
                    {
                        DisconnectClient(tcp_clients[i]);
                    }
                }
            }
        }

        private void DisconnectClient(TcpClient client)
        {
            Console.WriteLine("SERVER: User disconnected.");

            client.GetStream().Close();
            client.Close();
            tcp_clients.Remove(client);


        }

        private void ReceiveUDP()
        {
            if (udp_server.Available > 0)
            {
                byte[] bytes = udp_server.Receive(ref source);

                if (bytes.Length > 0)
                {
                    // Get & translate first 4 bytes
                    EN_UDP_PACKET_TYPE packet_type = EN_Protocol.BytesToUDPType(bytes);

                    // Print packet info
                    Console.WriteLine("UDP " + source.Address.ToString() + ": " + TranslateUDP(source, packet_type, bytes));
                }
            }
        }

        private void ReceiveTCP()
        {
            for (int i = 0; i < tcp_clients.Count; i++)
            {
                if (tcp_clients[i].Available > 0)
                {
                    NetworkStream stream = tcp_clients[i].GetStream();


                    byte[] bytes_size = new byte[4];
                    stream.Read(bytes_size, 0, 4);
                    int bytesize = BitConverter.ToInt32(bytes_size, 0);

                    byte[] bytes_data = new byte[bytesize];
                    stream.Read(bytes_data, 0, bytesize);


                    EN_TCP_PACKET_TYPE packet_type = EN_Protocol.BytesToTCPType(bytes_data, 0);
                    Console.WriteLine("TCP " + ((IPEndPoint)tcp_clients[i].Client.RemoteEndPoint).Address.ToString() + ": " + TranslateTCP(tcp_clients[i], source, packet_type, bytes_data));
                }
            }
        }

        private string TranslateUDP(IPEndPoint source, EN_UDP_PACKET_TYPE type, byte[] bytes)
        {
            string s = "";
            if (type == EN_UDP_PACKET_TYPE.TRANSFORM)
            {
                EN_PacketTransform packet = EN_Protocol.BytesToObject<EN_PacketTransform>(bytes);
                BroadcastUDP(bytes);
            }

            return s;
        }

        private string TranslateTCP(TcpClient client, IPEndPoint source, EN_TCP_PACKET_TYPE type, byte[] bytes)
        {
            string s = "";
            if (type == EN_TCP_PACKET_TYPE.CONNECT)
            {
                EN_PacketConnect packet = EN_Protocol.BytesToObject<EN_PacketConnect>(bytes);

                // Resend older important messages from before
                ResendStackTCP(client);

                // Send out this connection packet to the rest of the clients
                BroadcastTCP(bytes);

                // Add connect request to the stack of important messages
                packet_stack.Add(bytes);

                s = packet.packet_client_name + " connected.";

                // Add client to list of unique ID's
                clients.Add(new EN_ClientInfo(client, packet.packet_client_guid, packet.packet_client_name));
            }
            if (type == EN_TCP_PACKET_TYPE.MESSAGE)
            {
                EN_PacketMessage packet = EN_Protocol.BytesToObject<EN_PacketMessage>(bytes);
                s = packet.packet_message;
            }

            return s;
        }

        private void BroadcastUDP(byte[] bytes)
        {
            for (int i = 0; i < udp_clients.Count; i++)
            {
                udp_server.Send(bytes, bytes.Length, udp_clients[i]);
            }
        }

        private void BroadcastTCP(byte[] bytes)
        {
            for (int i = 0; i < tcp_clients.Count; i++)
            {
                NetworkStream stream = tcp_clients[i].GetStream();
                
                stream.Write(BitConverter.GetBytes(bytes.Length), 0, 4);
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        private void ResendStackTCP(TcpClient client)
        {
            NetworkStream stream = client.GetStream();

            // Re-broadcast all old messages
            for (int i = 0; i < packet_stack.Count; i++)
            {
                stream.Write(BitConverter.GetBytes(packet_stack[i].Length), 0, 4);
                stream.Write(packet_stack[i], 0, packet_stack[i].Length);
            }
        }

        private void SendStateTCP(TcpClient client)
        {
            NetworkStream stream = client.GetStream();

            EN_PacketGameState state;
            state.packet_type = EN_TCP_PACKET_TYPE.GAME_STATE;
            state.packet_client_amount = clients.Count;
            //state.packet_clients = clients.ToArray();
            
        }
    }
#endif
}



/*
 * 
 * 
 * 
 * 
                // Setup an ID to replace the old -1 ID from the packet
                byte[] newID = new byte[4];
                newID = BitConverter.GetBytes(tcp_clients.Count);
                for (int i = 0; i < 4; i++)
                {
                    // Good ol-fashioned byte swap to insert the new ID
                    byte b = newID[i];
                    bytes[4 + i] = b;
                }
*/