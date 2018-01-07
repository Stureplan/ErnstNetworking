﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ErnstNetworking.Protocol;

public class EN_SyncTransform : MonoBehaviour 
{
    public int syncFrameRate = 1;

    private int frame = 0;

    private void Update()
    {
        frame++;
        if (frame > 999)
        {
            frame = 0;
        }

        if (frame % syncFrameRate == 0)
        {
            SendUDP();
        }
    }

    private void SendUDP()
    {
        Vector3 pos = transform.position;
        Vector3 rot = transform.rotation.eulerAngles;

        EN_PacketTransform data;
        data.packet_type = EN_UDP_PACKET_TYPE.TRANSFORM;
        data.packet_object = 1;
        data.tX = pos.x; data.tY = pos.y; data.tZ = pos.z;
        data.rX = rot.x; data.rY = rot.y; data.rZ = rot.z;
        
    }
}
