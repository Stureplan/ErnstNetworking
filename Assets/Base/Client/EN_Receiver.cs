using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;

using ErnstNetworking.Server;

public class EN_Receiver : MonoBehaviour 
{
    UdpClient client;
    IPEndPoint source;

    private void Start()
    {
        Application.runInBackground = true;

        client = new UdpClient(EN_ServerSettings.PORT);
        source = new IPEndPoint(IPAddress.Any, 0);

        //client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 2000);

        StartCoroutine(Recieve());
    }


    private IEnumerator Recieve()
    {
        while(true)
        {
            //recieve
            //try
            {
                if (client.Available > 0)
                {
                    //byte[] bytes = client.Receive(ref source);
                    byte[] bytes = client.Receive(ref source);

                    if (bytes.Length > 0)
                    {
                        Debug.Log(System.Text.Encoding.ASCII.GetString(bytes));
                    }
                }
            }
            //catch(SocketException e)
            //{
            //    Debug.Log(e.ToString());
            //}

            yield return null;
        }
    }
}
