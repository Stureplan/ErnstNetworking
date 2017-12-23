using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;

using ErnstNetworkingServer.Settings;

public class NS_Sender : MonoBehaviour 
{
    UdpClient client;
    IPEndPoint target;

    private void Start()
    {
        Application.runInBackground = true;

        client = new UdpClient();
        target = new IPEndPoint(IPAddress.Parse(EN_ServerSettings.HOSTNAME), EN_ServerSettings.PORT);
        client.Connect(target);

        StartCoroutine(Send());
    }

    private IEnumerator Send()
    {
        while (true)
        {
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes("Hello, every 1 seconds");
            client.Send(bytes, bytes.Length);

            yield return new WaitForSeconds(1.0f);
        }
    }
}
