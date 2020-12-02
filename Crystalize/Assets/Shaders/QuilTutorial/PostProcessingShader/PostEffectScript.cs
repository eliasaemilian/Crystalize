using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class PostEffectScript : MonoBehaviour
{
    public Material mat;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        // src is the fully rendered scene

        Graphics.Blit(source, destination, mat);
    }
}
