using System.Collections;
using UnityEngine;
using System.Net.Sockets;
using System.Net;

using ErnstNetworking.Server;
using ErnstNetworking.Protocol;

public class EN_Sender : MonoBehaviour 
{
    UdpClient client;
    IPEndPoint target;

    private void Start()
    {
        Application.runInBackground = true;

        client = new UdpClient();
        target = new IPEndPoint(IPAddress.Parse(EN_ServerSettings.HOSTNAME), EN_ServerSettings.PORT);

        EN_Protocol.Connect(client, target);
        StartCoroutine(Send());
    }

    private void OnDestroy()
    {
        client.Close();
    }

    private IEnumerator Send()
    {
        while (true)
        {
            EN_Protocol.SendText(client, "Hello!");
            //byte[] bytes = System.Text.Encoding.ASCII.GetBytes("Hello, every 1 seconds");
            //client.Send(bytes, bytes.Length);

            yield return new WaitForSeconds(1.0f);
        }
    }
}
