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
    private Dictionary<Guid, string> clients;

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
            int bytes_available = tcp_client.Available;
            if (bytes_available > 0)
            {
                byte[] bytes = new byte[bytes_available];
                int bytes_read = stream.Read(bytes, 0, bytes_available);

                Debug.Log(bytes_read + " bytes recieved. (TCP)");

                EN_TCP_PACKET_TYPE type = EN_Protocol.BytesToTCPType(bytes);
                TranslateTCP(type, bytes);
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
            // We connected and want to establish which client we are
            EN_PacketConnect packet = EN_Protocol.BytesToObject<EN_PacketConnect>(bytes);

            byte[] message = new byte[bytes.Length - 4 - 4 - 16];
            Buffer.BlockCopy(bytes, 4 + 4 + 16, message, 0, message.Length);
            string name = EN_Protocol.BytesToString(message);
            if (packet.packet_client_guid.Equals(EN_ClientSettings.CLIENT_GUID) == true)
            {
                name += " (you)";
            }

            AddClient(packet.packet_client_guid, packet.packet_client_id, name);
        }
    }


    private void AddClient(Guid guid, int id, string n)
    {
        clients.Add(guid,n + "\tID: " +id.ToString());

        string s = "";
        foreach(KeyValuePair<Guid, string> item in clients)
        {
            s += item.Value;
            s += '\n';
        }

        text_clients.text = s;
    }

    public void ConnectClient()
    {
        udp_client = new UdpClient();
        tcp_client = new TcpClient();


        server = new IPEndPoint(IPAddress.Parse(EN_ServerSettings.HOSTNAME), EN_ServerSettings.PORT);
        clients = new Dictionary<Guid, string>();

        EN_ClientSettings.CLIENT_NAME = text_name.text;
        EN_ClientSettings.CLIENT_GUID = Guid.NewGuid();

        EN_Protocol.Connect(udp_client, server, EN_ClientSettings.CLIENT_NAME, EN_ClientSettings.CLIENT_GUID);
        EN_Protocol.Connect(tcp_client, server, EN_ClientSettings.CLIENT_NAME, EN_ClientSettings.CLIENT_GUID);

        stream = tcp_client.GetStream();

        StartCoroutine(SendUDP(EN_ClientSettings.SEND_INTERVAL));
        StartCoroutine(ReceiveUDP());
        StartCoroutine(ReceiveTCP());

        connected = true;
    }
}
