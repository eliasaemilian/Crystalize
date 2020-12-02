using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnMouseGlowEffect : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Plane p = new Plane(Vector3.up, Vector3.zero);
        Vector2 mousePos = Input.mousePosition;

        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        if (p.Raycast(ray, out float enterDist))
        {
            Vector3 worldMousePos = ray.GetPoint(enterDist);

            Shader.SetGlobalVector("_MousePos", worldMousePos);
        }
    }
}
