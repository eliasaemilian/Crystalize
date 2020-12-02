using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DissolveEffectTrigger : MonoBehaviour
{
    public Material DissolveMat;
    public float Speed, Max;

    MeshRenderer renderer;

    [SerializeField] private float currentY;
    // Start is called before the first frame update
    void Start()
    {
        renderer = GetComponent<MeshRenderer>();   
    }

    // Update is called once per frame
    void Update()
    {
        if (currentY < Max)
        {
            renderer.sharedMaterial.SetFloat("_DissolveY", currentY);
            
            currentY += Time.deltaTime * Speed;
            Debug.Log("Dissolving " + name);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            TriggerEffect();
            Debug.Log("Dissolving Start");
        }
    }

    private void TriggerEffect()
    {
        currentY = 0;

    }
}
