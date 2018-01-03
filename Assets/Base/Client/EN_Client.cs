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

    private void Start()
    {
        Application.runInBackground = true;

        //TODO: Move Send/Translate UDP/TCP to EN_Protocol
        // It shouldn't bother the Unity code, really.
    }

    private void OnDestroy()
    {
        if (connected)
        {
            udp_client.Close();
        }
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


    private void SendTCP(byte[] bytes)
    {
        // client.Send(ObjectToBytes(packet), )
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            EN_Protocol.SendText(stream, "Hello!");
        }
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
            }
            yield return null;
        }
    }
    private void TranslateUDP(EN_UDP_PACKET_TYPE type, byte[] bytes)
    {
        if (type == EN_UDP_PACKET_TYPE.TRANSFORM)
        {

        }
    }

    private void TranslateTCP(EN_TCP_PACKET_TYPE type, byte[] bytes)
    {
        if (type == EN_TCP_PACKET_TYPE.CONNECT)
        {
            //Debug.Log("CONNECT");
            // Someone connected and we want to establish who it is
            EN_PacketConnect packet = EN_Protocol.BytesToObject<EN_PacketConnect>(bytes);

            byte[] message = new byte[bytes.Length - 4 - 16];
            Buffer.BlockCopy(bytes, 4 + 16, message, 0, message.Length);
            string name = EN_Protocol.BytesToString(message);
            if (packet.packet_client_guid.Equals(EN_ClientSettings.CLIENT_GUID) == true)
            {
                name += " (you)";
            }

            AddClient(packet.packet_client_guid, name);
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
        udp_client = new UdpClient();
        tcp_client = new TcpClient();


        server = new IPEndPoint(IPAddress.Parse(EN_ServerSettings.HOSTNAME), EN_ServerSettings.PORT);
        clients = new List<EN_ClientInfo>();

        EN_ClientSettings.CLIENT_NAME = text_name.text;
        EN_ClientSettings.CLIENT_GUID = Guid.NewGuid();

        EN_Protocol.Connect(udp_client, server);
        EN_Protocol.Connect(tcp_client, server, EN_ClientSettings.CLIENT_NAME, EN_ClientSettings.CLIENT_GUID);

        stream = tcp_client.GetStream();

        StartCoroutine(SendUDP(EN_ClientSettings.SEND_INTERVAL));
        StartCoroutine(ReceiveUDP());
        StartCoroutine(ReceiveTCP());

        connected = true;
    }
}
