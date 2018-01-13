using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "EN_NetworkPrefabs", menuName = "ErnstNetworking/Create Network Prefab List", order = 52)]
public class EN_NetworkPrefabs : ScriptableObject
{
    public GameObject[] networkPrefabs;

    public static void BuildPrefabList()
    {
        if (Application.isEditor)
        {
#if UNITY_EDITOR
            // Find game path asset
            string[] assets = AssetDatabase.FindAssets("t:EN_NetworkPrefabs");
            if (assets == null || assets.Length < 1)
            {
                return;
            }

            // Find game path through asset
            string asset = AssetDatabase.GUIDToAssetPath(assets[0]);
            EN_NetworkPrefabs prefabs = AssetDatabase.LoadAssetAtPath<EN_NetworkPrefabs>(asset);
            prefabs.networkPrefabs = Resources.LoadAll<GameObject>("NetworkPrefabs");



            AssetDatabase.Refresh();
            EditorUtility.SetDirty(prefabs);
            AssetDatabase.SaveAssets();
#endif
        }
        else
        {
            EN_NetworkPrefabs prefabs_build = Resources.Load<EN_NetworkPrefabs>("NetworkDatabase/EN_NetworkPrefabs");
            prefabs_build.networkPrefabs = Resources.LoadAll<GameObject>("NetworkPrefabs");

            EN_Client.ConsoleMessage(prefabs_build.networkPrefabs.Length.ToString());

            for (int i = 0; i < prefabs_build.networkPrefabs.Length; i++)
            {
                Instantiate(prefabs_build.networkPrefabs[i], Vector3.zero + (Vector3.right * i * 1.0f), Quaternion.identity);
            }
        }

    }
}
