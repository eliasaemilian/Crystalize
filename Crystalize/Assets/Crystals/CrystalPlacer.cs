using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalPlacer : MonoBehaviour
{
    SpawnCrystalsWithMouse crystalScript;
    private bool hasTouchedGround;
    private bool newStep;
    // Start is called before the first frame update
    void Start()
    {
       crystalScript = FindObjectOfType<SpawnCrystalsWithMouse>();   
    }

    // Update is called once per frame
    void Update()
    {
      
    }

    // On Trigger Enter - > Give Information to Crystal Skript

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("step at " +  transform.position);

        if (!newStep) newStep = true;
        // later: if other is ground where crystals can spawn
        if (newStep) crystalScript.CrystalCircle(transform.position, 20);
        // send spawn crystal with point
    }

    private void OnTriggerExit(Collider other)
    {
        if (newStep) newStep = false;
        Debug.Log("disabled");
    }
}
