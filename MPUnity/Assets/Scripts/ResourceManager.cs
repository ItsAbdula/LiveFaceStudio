using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager resourceManager;

    private static Dictionary<string, TextAsset> loadedData = new Dictionary<string, TextAsset>();
    private static Dictionary<string, GameObject> loadedPrefabs = new Dictionary<string, GameObject>();

    private readonly string[] prefabNames = { "Sphere", "Cube" };
    private readonly string[] dataNames = { "haarcascades/haarcascade_frontalface_alt", "haarcascades/haarcascade_eye_tree_eyeglasses" };

    private void Awake()
    {
        loadData();
        loadPrefabs();
    }

    private void loadData()
    {
        foreach (var name in dataNames)
        {
            var go = Resources.Load<TextAsset>("Data/" + name);
            if (go == null)
            {
                Debug.LogWarning("Can't find : " + name);
                continue;
            }

            loadedData[name] = go;
        }
    }

    private void loadPrefabs()
    {
        foreach (var name in prefabNames)
        {
            var go = Resources.Load<GameObject>("Prefabs/" + name);
            if (go == null)
            {
                Debug.LogWarning("Can't find : " + name);
                continue;
            }

            loadedPrefabs[name] = go;
        }
    }

    public static string getData(string name)
    {
        return loadedData[name].text;
    }

    public static GameObject instantiatePrefab(string name)
    {
        return Instantiate(loadedPrefabs[name]);
    }
}