using System.Collections;
using UnityEngine;
using System.Net.Sockets;
using System.Net;

using ErnstNetworking.Server;
using ErnstNetworking.Protocol;

public class EN_Client : MonoBehaviour
{
    UdpClient client;
    IPEndPoint target;

    int client_id = -1;

    private void Start()
    {
        Application.runInBackground = true;

        client = new UdpClient();
        target = new IPEndPoint(IPAddress.Parse(EN_ServerSettings.HOSTNAME), EN_ServerSettings.PORT);
        //client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 2000);

        EN_Protocol.Connect(client, target);
        StartCoroutine(Send());
        StartCoroutine(Recieve());
    }

    private void OnDestroy()
    {
        client.Close();
    }

    private IEnumerator Send()
    {
        while (true)
        {
            // Send text message every 1s
            EN_Protocol.SendText(client, "Hello!");
            yield return new WaitForSeconds(1.0f);
        }
    }

    private IEnumerator Recieve()
    {
        while (true)
        {
            if (client.Available > 0)
            {
                byte[] bytes = client.Receive(ref target);

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
        if (type == EN_PACKET_TYPE.CONNECT_CONFIRMED)
        {
            // We connected and want to establish which client we are
            EN_PacketConnect packet = EN_Protocol.BytesToObject<EN_PacketConnect>(bytes);
            client_id = packet.packet_data;
        }
    }
}
