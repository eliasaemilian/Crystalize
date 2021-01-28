using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotaDebug : MonoBehaviour
{
    Vector3 local;
    Vector3 localForward;
    // Start is called before the first frame update
    void Start()
    {
        

    }

    // Update is called once per frame
    void Update()
    {
        local = transform.parent.InverseTransformDirection(transform.up);
       // localForward = transform.parent.InverseTransformDirection(transform.forward);
        //localForward = transform.worldToLocalMatrix.MultiplyVector(transform.forward);

       // Debug.DrawLine(transform.localPosition, localForward, Color.red, .5f);
      //  Debug.DrawLine(transform.localPosition, local, Color.cyan, .5f);
    }
}
