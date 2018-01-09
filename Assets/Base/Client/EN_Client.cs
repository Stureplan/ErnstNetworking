using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using System;
using System.Net;
using System.Net.Sockets;

using ErnstNetworking.Server;
using ErnstNetworking.Client;
using ErnstNetworking.Protocol;



public class EN_Client : MonoBehaviour
{
    // This client (PC)
    private UdpClient udp_client;
    private TcpClient tcp_client;

    // Target connection
    private IPEndPoint server;
    private bool connected = false;

    // Network stream
    private NetworkStream stream;

    // Client list (connected players)
    private List<EN_ClientInfo> clients;

    // UI Stuff
    public Text text_clients;
    public Text text_name;

    public Text udp_in;
    public Text tcp_in;

    // Network Tracking
    private uint udpBytesIn = 0;
    private uint tcpBytesIn = 0;


    public static EN_Client Client;

    public static EN_Client Contact()
    {
        if (Client == null)
        {
            Client = FindObjectOfType<EN_Client>();
        }

        return Client;
    }


    private void Start()
    {
        Application.runInBackground = true;
        Client = this;

        //TODO: Move Send/Translate UDP/TCP to EN_Protocol
        // It shouldn't bother the Unity code, really.
    }

    private void OnDestroy()
    {
        udp_client.Close();
        tcp_client.Client.Disconnect(true);
        tcp_client.GetStream().Close();
        tcp_client.Close();
    }


    /*  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -
        Constantly send updates (translate/rotate etc)
        -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  */
    private IEnumerator SendUDP(float interval)
    {
        while (true)
        {
            // Send text message every <interval> seconds
            //EN_Protocol.SendText(udp_client, "Hello!");

            //TODO: Send transform instead of text
            yield return new WaitForSeconds(interval);
        }
    }

    public void SendUDP(object obj)
    {
        if (connected == false) { return; }

        Debug.Log("Sent");
        EN_Protocol.SendUDP(udp_client,server, obj);
    }

    private void SendTCP(byte[] bytes)
    {
        // client.Send(ObjectToBytes(packet), )
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            EN_Protocol.SendTCP(stream, new EN_PacketMessage("Hello!"));
        }

        udp_in.text = TrafficInUDP().ToString();
        tcp_in.text = TrafficInTCP().ToString();

        udpBytesIn = 0;
        tcpBytesIn = 0;
    }

    private uint TrafficInUDP()
    {
        uint bytes = udpBytesIn;
        return bytes;
    }

    private uint TrafficInTCP()
    {
        uint bytes = tcpBytesIn;
        return bytes;
    }


    private IEnumerator ReceiveUDP()
    {
        while (true)
        {
            if (udp_client.Available > 0)
            {
                byte[] bytes = udp_client.Receive(ref server);

                if (bytes.Length > 0)
                {
                    EN_UDP_PACKET_TYPE type = EN_Protocol.BytesToUDPType(bytes);
                    TranslateUDP(type, bytes);

                    udpBytesIn += (uint)bytes.Length;
                }
            }

            yield return null;
        }
    }

    private IEnumerator ReceiveTCP()
    {
        while(true)
        {
            if (tcp_client.Available > 0)
            {
                byte[] bytes_size = new byte[4];
                stream.Read(bytes_size, 0, 4);
                int bytesize = BitConverter.ToInt32(bytes_size, 0);

                byte[] bytes_data = new byte[bytesize];
                stream.Read(bytes_data, 0, bytesize);

                EN_TCP_PACKET_TYPE type = EN_Protocol.BytesToTCPType(bytes_data, 0);
                TranslateTCP(type, bytes_data);

                tcpBytesIn += (uint)bytes_size.Length + (uint)bytes_data.Length;
            }
            yield return null;
        }
    }
    private void TranslateUDP(EN_UDP_PACKET_TYPE type, byte[] bytes)
    {
        if (type == EN_UDP_PACKET_TYPE.TRANSFORM)
        {
            Debug.Log(bytes.Length);
        }
    }

    private void TranslateTCP(EN_TCP_PACKET_TYPE type, byte[] bytes)
    {
        if (type == EN_TCP_PACKET_TYPE.CONNECT)
        {
            // Someone connected and we want to establish who it is
            EN_PacketConnect packet = EN_Protocol.BytesToObject<EN_PacketConnect>(bytes);

            if (packet.packet_client_guid.Equals(EN_ClientSettings.CLIENT_GUID) == true)
            {
                packet.packet_client_name += " (you)";
            }

            AddClient(packet.packet_client_guid, packet.packet_client_name);
        }
        if (type == EN_TCP_PACKET_TYPE.GAME_STATE)
        {
            EN_PacketGameState packet = EN_Protocol.BytesToObject<EN_PacketGameState>(bytes);
        }
    }


    private void AddClient(Guid guid, string n)
    {
        clients.Add(new EN_ClientInfo(guid,n));

        string s = "";
        for (int i = 0; i < clients.Count; i++)
        {
            s += clients[i].client_name + '\n';
        }

        text_clients.text = s;
    }

    public void ConnectClient()
    {
        udp_client = new UdpClient();// EN_ServerSettings.HOSTNAME, EN_ServerSettings.PORT);
        tcp_client = new TcpClient();// EN_ServerSettings.HOSTNAME, EN_ServerSettings.PORT);
        //tcp_client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);





        server = new IPEndPoint(IPAddress.Parse(EN_ServerSettings.HOSTNAME), EN_ServerSettings.PORT);
        clients = new List<EN_ClientInfo>();

        EN_ClientSettings.CLIENT_NAME = text_name.text;
        EN_ClientSettings.CLIENT_GUID = Guid.NewGuid();

        if (EN_Protocol.Connect(tcp_client, server, EN_ClientSettings.CLIENT_NAME, EN_ClientSettings.CLIENT_GUID) == false)
        {
            Debug.Log("Not connected (TCP)");
        }


        IPEndPoint ep = (IPEndPoint)tcp_client.Client.LocalEndPoint;
        int p = ep.Port;

        udp_client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        udp_client.Client.Bind(new IPEndPoint(IPAddress.Any, p));
        EN_Protocol.Connect(udp_client, server);



        stream = tcp_client.GetStream();

        StartCoroutine(SendUDP(EN_ClientSettings.SEND_INTERVAL));
        StartCoroutine(ReceiveUDP());
        StartCoroutine(ReceiveTCP());

        StartCoroutine(WaitForConnection(1.0f));
    }

    private IEnumerator WaitForConnection(float timer)
    {
        float t = 0.0f;
        while (t < timer)
        {
            t += Time.deltaTime;

            yield return null;
        }

        connected = true;
    }
}
