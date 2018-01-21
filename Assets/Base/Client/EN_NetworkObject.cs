using System.Collections.Generic;
using UnityEngine;

public class EN_NetworkObject : MonoBehaviour 
{
    public int network_id;

    private static Dictionary<int, GameObject> networkObjects = new Dictionary<int, GameObject>();
    public static void Add(int id, GameObject go)
    {
        networkObjects.Add(id, go);
    }
    public static GameObject Find(int id)
    {
        return networkObjects[id];
    }
}
