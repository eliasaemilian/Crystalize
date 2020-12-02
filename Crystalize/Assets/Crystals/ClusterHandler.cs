using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClusterHandler : MonoBehaviour, IPooledObject
{
  //  public float Lifetime = 5f;

    [SerializeField] private float counter;

    public Material DissolveMat;
    public float Speed, Max;

    private float spawnTimer;

    MeshRenderer _renderer;

    private float currentY;
    [SerializeField] bool reverse;

    private CrystalConfig config;
    private ObjectPooler pooler;

    public CrystalConfig defaultConfig;

    public bool ignoreLifetime;

    // Start is called before the first frame update
    void Start()
    {
        pooler = FindObjectOfType<ObjectPooler>();
        _renderer = GetComponent<MeshRenderer>();
        if (GetComponent<CrystalCluster>() != null)
        {
            config = GetComponent<CrystalCluster>().Config;
        }
        else config = defaultConfig;

       // _renderer.sharedMaterial.SetFloat("_StartingY", transform.position.y - 5f);
        _renderer.material.SetFloat("_StartingY", transform.position.y - 6f);
        _renderer.material.SetFloat("_DissolveSize", config.MaxLength + 3f);

    }

    void Update()
    {
        // Dissolve
        if (currentY < Max && !reverse)
        {
           // _renderer.sharedMaterial.SetFloat("_DissolveY", currentY);
            _renderer.material.SetFloat("_DissolveY", currentY);

            currentY += Time.deltaTime * Speed;
           // Debug.Log("Building " + name);
        }
        else if (currentY > _renderer.sharedMaterial.GetFloat("_StartingY") && reverse)
        {
           // _renderer.sharedMaterial.SetFloat("_DissolveY", currentY);
            _renderer.material.SetFloat("_DissolveY", currentY);

            currentY -= Time.deltaTime * Speed;
           // Debug.Log("Dissolving " + name);
        }



    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if (!reverse) counter += Time.deltaTime;
        else counter -= Time.deltaTime;

        if (counter > config.Lifetime && !reverse && !ignoreLifetime)
        {
            pooler.ReturnToPool("Crystal", gameObject);
            // Trigger Dissolve Effect, then return this to pool
            //TriggerEffect();
          //  Debug.Log("Dissolve Start");
           // Destroy(gameObject); //Dequeue
        }

        
        spawnTimer += Time.deltaTime;
      //  _renderer.sharedMaterial.SetFloat("_SpawnTime", spawnTimer);
        _renderer.material.SetFloat("_SpawnTime", spawnTimer);

    }



    public void OnObjectSpawn()
    {
        spawnTimer = 0;

        if (_renderer == null) _renderer = GetComponent<MeshRenderer>();
        _renderer.material.SetFloat("_SpawnTime", spawnTimer);

        counter = 0;
        reverse = false;
        currentY = 0;

    }

    public void OnObjectDespawn()
    {
        // Cluser Dissolve Effect here
        reverse = true;
        currentY = Max;
        counter = _renderer.sharedMaterial.GetFloat("_DissolveSize");
        // TriggerEffect();
        StartCoroutine(Despawn());
        //start Coroutine, when done proceed with dequeueing
    }

    IEnumerator Despawn()
    {
        //bool val = (counter > 0) ? true : false;
       // Debug.Log("Despawning");
        yield return new WaitUntil(() => counter <= 0);
       // Debug.Log("Finished Despawn");
        gameObject.SetActive(false);
    }

    public void OnInstantiation()
    {
        GetComponent<CrystalCluster>().OnInstantiate();
    }
}
