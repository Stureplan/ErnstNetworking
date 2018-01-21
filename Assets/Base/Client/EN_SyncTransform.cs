using UnityEngine;

using ErnstNetworking.Protocol;

public class EN_SyncTransform : MonoBehaviour 
{
    [Header("Network Traffic Framerate (Higher = Less traffic)")]
    public int syncFrameRate = 1;

    private int frame = 0;
    private int instanceID = -1;

    private Vector3 lastpos = Vector3.zero;
    private Vector3 velocity = Vector3.zero;

    private void Start()
    {
        instanceID = gameObject.GetInstanceID();

        Animation anim = GetComponent<Animation>();
        anim.enabled = true;
        anim.Play("CubeTestTransform");

    }

    private void Update()
    {
        frame++;
        if (frame > 999)
        {
            frame = 0;
        }

        if (frame % syncFrameRate == 0)
        {
            if (velocity.magnitude>0.01f)
            {
                //SendUDP();
            }

            SendUDP();
            lastpos = transform.position;
        }

    }

    private void SendUDP()
    {
        Vector3 pos = transform.position;
        Vector3 rot = transform.rotation.eulerAngles;
        Vector3 vel = (transform.position - lastpos).normalized*Vector3.Distance(transform.position, lastpos);
        velocity = vel;
        Debug.Log(vel);

        EN_PacketTransform data;
        data.packet_type = EN_UDP_PACKET_TYPE.TRANSFORM;
        data.packet_network_id = instanceID;
        data.tX = pos.x; data.tY = pos.y; data.tZ = pos.z;
        data.rX = rot.x; data.rY = rot.y; data.rZ = rot.z;
        data.vX = vel.x; data.vY = vel.y; data.vZ = vel.z;

        EN_Client.Contact().SendUDP(data);
    }
}
