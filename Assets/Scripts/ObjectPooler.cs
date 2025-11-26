using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance;
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
        public bool canExpand = true;
    }

    [Tooltip("Define object pools here")]
    public List<Pool> pools;
    private Dictionary<string, Queue<GameObject>> poolDictionary;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializePools();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void InitializePools()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectQueue = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++ )
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                obj.transform.SetParent(this.transform);
                objectQueue.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectQueue);
        }
    }
    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool with tag {tag} doesn't exist.");
            return null;
        }

        Queue<GameObject> objectQueue = poolDictionary[tag];
        GameObject objectToSpawn = null;

        if (objectQueue.Count > 0)
        {
            objectToSpawn = objectQueue.Dequeue();
        }
        else
        {
            Pool poolConfig = pools.Find(p => p.tag == tag);
            if (poolConfig != null && poolConfig.canExpand)
            {
                objectToSpawn = Instantiate(poolConfig.prefab);
                objectToSpawn.transform.SetParent(this.transform);
                Debug.Log($"Pool with tag {tag} expanded.");
            }
            else
            {
                Debug.LogWarning($"No available objects in pool with tag {tag} and expansion not allowed.");
                return null;
            }
        }

        if (objectToSpawn != null)
        {
            objectToSpawn.transform.position = position;
            objectToSpawn.transform.rotation = rotation;
            objectToSpawn.SetActive(true);
        }

        return objectToSpawn;
    }

    public void ReturnToPool(string tag, GameObject objectToReturn)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool with tag {tag} doesn't exist.");
            Destroy(objectToReturn);
            return;
        }
        objectToReturn.SetActive(false);
        poolDictionary[tag].Enqueue(objectToReturn);
    }
}
