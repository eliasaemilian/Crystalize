using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Pool
{
    public string Tag;
    public GameObject Prefab;
    public int Size => (int)(NumberOfObjects * Uniqueness);

    public int NumberOfObjects; // total Number in Pool
    [Range(0, 1)]
    public float Uniqueness; // 1 = all unique, 0 generates only 1 



    // Start is called before the first frame update

}
