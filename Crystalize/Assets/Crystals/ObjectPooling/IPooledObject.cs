using UnityEngine;

public interface IPooledObject
{
    void OnInstantiation(); //Gets called once when object is generated for the pool
    void OnObjectSpawn(); //Gets called when object spawns from pool

    void OnObjectDespawn(); //Gets called when object returns to pool
}
