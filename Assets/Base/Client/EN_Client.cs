using System.Collections;
using UnityEngine;
using System.Net.Sockets;
using System.Net;

using ErnstNetworking.Server;
using ErnstNetworking.Client;
using ErnstNetworking.Protocol;



public class EN_Client : MonoBehaviour
{
    UdpClient client;
    IPEndPoint server;


    private void Start()
    {
        Application.runInBackground = true;

        client = new UdpClient();
        server = new IPEndPoint(IPAddress.Parse(EN_ServerSettings.HOSTNAME), EN_ServerSettings.PORT);
        //client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 2000);

        EN_Protocol.Connect(client, server, EN_ClientSettings.CLIENT_NAME);
        StartCoroutine(Send(EN_ClientSettings.SEND_INTERVAL));
        StartCoroutine(Recieve());
    }

    private void OnDestroy()
    {
        client.Close();
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
        /*if (type == EN_PACKET_TYPE.CONNECT_CONFIRMED)
        {
            // We connected and want to establish which client we are
            EN_PacketConnect packet = EN_Protocol.BytesToObject<EN_PacketConnect>(bytes);
            EN_Protocol.CLIENT_ID = packet.packet_data;
        }*/
    }

    private void SendPacket(EN_TransformData data)
    {

       // client.Send(ObjectToBytes(packet), )
    }
}
