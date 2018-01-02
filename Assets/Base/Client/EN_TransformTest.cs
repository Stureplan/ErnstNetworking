using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ErnstNetworking.Protocol;

public class EN_TransformTest : MonoBehaviour 
{
    private void Start()
    {
        /*Vector3 pos = transform.position;
        Vector3 rot = transform.rotation.eulerAngles;

        EN_TransformData data;
        data.tX = pos.x; data.tY = pos.y; data.tZ = pos.z;
        data.rX = rot.x; data.rY = rot.y; data.rZ = rot.z;

        EN_PacketHeader header;
        header.packet_type = EN_PACKET_TYPE.TRANSFORM;
        header.packet_sender = EN_Protocol.CLIENT_ID;

        byte[] bytes = header.AllToBytes(EN_Protocol.ObjectToBytes(data));

        TransformBack(bytes);*/
    }

    private void TransformBack(byte[] data)
    {
        /*EN_PacketHeader header;
        header = EN_Protocol.BytesToObject<EN_PacketHeader>(data);
        */
        //int b = 1;
    }
}
