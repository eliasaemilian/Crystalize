using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeHandler : MonoBehaviour
{
    MeshCollider _col;
    MeshRenderer _renderer;

    public float Speed, Max;

    private float spawnTimer;
    private float currentY;

    [SerializeField] bool reverse;
    [SerializeField] private float counter;


    public void OnSpawn()
    {
        if (_renderer == null) _renderer = GetComponent<MeshRenderer>();


        _col = GetComponent<MeshCollider>();
        _renderer.material.SetFloat("_StartingY", transform.position.y - 6f);
        _renderer.material.SetFloat("_DissolveSize", _col.bounds.max.y*2);

        spawnTimer = 0;

        _renderer.material.SetFloat("_SpawnTime", spawnTimer);

        counter = 0;
        reverse = false;
        currentY = 0;

        Debug.Log("Bridge Spawned");

        // For now:
        Speed = 6f;
        Max = 20f;
    }

    void Update()
    {
        if (_renderer == null) OnSpawn();

        // Dissolve
        if (currentY < Max && !reverse)
        {
            // _renderer.sharedMaterial.SetFloat("_DissolveY", currentY);
            _renderer.material.SetFloat("_DissolveY", currentY);

            currentY += Time.deltaTime * Speed;
            // Debug.Log("Building " + name);
        }
        else if (currentY > _renderer.material.GetFloat("_StartingY") && reverse)
        {
            // _renderer.sharedMaterial.SetFloat("_DissolveY", currentY);
            _renderer.material.SetFloat("_DissolveY", currentY);

            currentY -= Time.deltaTime * Speed;
            // Debug.Log("Dissolving " + name);
        }     

    }

    void FixedUpdate()
    {
        if (!reverse) counter += Time.deltaTime;
        else counter -= Time.deltaTime;


        spawnTimer += Time.deltaTime;
        _renderer.material.SetFloat("_SpawnTime", spawnTimer);

    }

    private void OnDestroy()
    {
       // OnObjectDespawn();
    }

    public void OnObjectDespawn()
    {
        // Cluser Dissolve Effect here
        reverse = true;
        currentY = Max;
        counter = _renderer.sharedMaterial.GetFloat("_DissolveSize");
        StartCoroutine(Despawn());
        //start Coroutine, when done proceed with dequeueing
    }

    IEnumerator Despawn()
    {
        //bool val = (counter > 0) ? true : false;
        Debug.Log("Despawning");
        yield return new WaitUntil(() => counter <= 0);
        Debug.Log("Finished Despawn");
        gameObject.SetActive(false);
    }

}
