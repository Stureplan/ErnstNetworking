using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EN_NetworkObject : MonoBehaviour 
{
    // Global ID count
    public static uint NetworkIDs = 0;

    // Current Network ID
    public uint network_id;

    private void Start()
    {
        network_id = NetworkIDs;
    }
}
