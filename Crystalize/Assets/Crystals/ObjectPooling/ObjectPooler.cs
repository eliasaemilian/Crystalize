using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    public List<Pool> Pools; // Pools to be spawned, eventually move to ScrObj

    public static ObjectPooler Instance;
    public GameObject EmptyPoolObject;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        for (int i = 0; i < Pools.Count; i++)
        {
            Queue<GameObject> pool = new Queue<GameObject>();
            int debug = 0, size;
            if (Pools[i].Size <= 0) size = 1;
            else size = Pools[i].Size;
            // Generate Unique Objects in Pool
            for (int j = 0; j < size; j++)
            {
                GameObject obj = Instantiate(Pools[i].Prefab);

                IPooledObject pooled = obj.GetComponent<IPooledObject>();
                if (pooled != null) pooled.OnInstantiation();

                obj.SetActive(false);
                pool.Enqueue(obj);
                debug++;
            }
           // Debug.Log("Spawned " + debug + " Objects");

            debug = 0;
            // Generate Duplicates in Pool
            List<GameObject> dupes = new List<GameObject>();
            int duplicates = Pools[i].NumberOfObjects - size;
            for (int j = 0; j <  duplicates; j++)
            {
                GameObject obj = pool.Dequeue();
                GameObject dupe = Instantiate(EmptyPoolObject);
                dupe.GetComponent<MeshFilter>().mesh = obj.GetComponent<MeshFilter>().mesh;
                dupe.GetComponent<MeshRenderer>().material = obj.GetComponent<MeshRenderer>().material;
                dupe.name = "Dupe " + obj.gameObject.name;
                dupe.SetActive(false);
                pool.Enqueue(obj);
                dupes.Add(dupe);
               // pool.Enqueue(dupe);
                
                debug++;
            }



            // Randomize Order in Queue for duplicates to avoid pattern
            while (dupes.Count > 0)
            {
                int r = Random.Range(0, dupes.Count);
                pool.Enqueue(dupes[r]);
                dupes.RemoveAt(r);
            }
            poolDictionary.Add(Pools[i].Tag, pool);

        }

    }


    public GameObject SpawnFromPool (string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " does not exist");
            return null;

        }

       GameObject obj = poolDictionary[tag].Dequeue();

        obj.transform.position = position;
        obj.transform.rotation = rotation;

        IPooledObject pooled = obj.GetComponent<IPooledObject>();
        if (pooled != null) pooled.OnObjectSpawn();
        obj.SetActive(true);

        poolDictionary[tag].Enqueue(obj);

        return obj;
    }

    public void ReturnToPool (string tag, GameObject obj)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " does not exist");
            return;
        }

      //  Debug.Log(obj.name + " returned to pool");

        IPooledObject pooled = obj.GetComponent<IPooledObject>();
        if (pooled != null) pooled.OnObjectDespawn();
        poolDictionary[tag].Enqueue(obj);

    }


}

