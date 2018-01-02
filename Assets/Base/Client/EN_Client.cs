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
    private UdpClient client;
    private IPEndPoint server;
    private bool connected = false;

    private List<string> clients;

    public Text text_clients;
    public Text text_name;

    private void Start()
    {
        Application.runInBackground = true;
    }

    private void OnDestroy()
    {
        if (connected)
        {
            client.Close();
        }
    }

    /*  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -
        Constantly send updates (translate/rotate etc)
        -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  */
    private IEnumerator Send(float interval)
    {
        while (true)
        {
            // Send text message every <interval> seconds
            EN_Protocol.SendText(client, "Hello!");
            yield return new WaitForSeconds(interval);
        }
    }

    private IEnumerator Recieve()
    {
        while (true)
        {
            if (client.Available > 0)
            {
                byte[] bytes = client.Receive(ref server);

                if (bytes.Length > 0)
                {
                    EN_PACKET_TYPE type = EN_Protocol.BytesToType(bytes);
                    TranslateMessage(type, bytes);
                }
            }

            yield return null;
        }
    }

    private void TranslateMessage(EN_PACKET_TYPE type, byte[] bytes)
    {
        if (type == EN_PACKET_TYPE.CONNECT)
        {
            // We connected and want to establish which client we are
            EN_PacketConnect packet = EN_Protocol.BytesToObject<EN_PacketConnect>(bytes);

            byte[] message = new byte[bytes.Length - 4 - 4 - 16];
            Buffer.BlockCopy(bytes, 4+4+16, message, 0, message.Length);
            string name = EN_Protocol.BytesToString(message);
            if (packet.packet_client_guid.Equals(EN_ClientSettings.CLIENT_GUID) == true)
            {
                name += " (you)";
            }

            AddClient(name, packet.packet_client_id);

            //TODO: Find out if WE are the client that should be updated -V
            //EN_Protocol.CLIENT_ID = packet.packet_client_id;
        }
    }

    private void SendPacket(EN_TransformData data)
    {

       // client.Send(ObjectToBytes(packet), )
    }

    private void AddClient(string n, int id)
    {
        clients.Add(n + "\tID: " +id.ToString());

        string s = "";
        for (int i = 0; i < clients.Count; i++)
        {
            s += clients[i];
            s += '\n';
        }

        text_clients.text = s;
    }

    public void ConnectClient()
    {
        client = new UdpClient();
        server = new IPEndPoint(IPAddress.Parse(EN_ServerSettings.HOSTNAME), EN_ServerSettings.PORT);
        //client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 2000);

        clients = new List<string>();

        EN_ClientSettings.CLIENT_NAME = text_name.text;
        EN_ClientSettings.CLIENT_GUID = Guid.NewGuid();

        EN_Protocol.Connect(client, server, EN_ClientSettings.CLIENT_NAME, EN_ClientSettings.CLIENT_GUID);
        StartCoroutine(Send(EN_ClientSettings.SEND_INTERVAL));
        StartCoroutine(Recieve());

        connected = true;
    }
}
