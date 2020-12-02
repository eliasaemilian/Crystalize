using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HologramGlitchEffect : MonoBehaviour
{
    public float GlitchChance = .1f;

    private Renderer _renderer;
    private WaitForSeconds glitchLoopTime = new WaitForSeconds(.1f);
    private WaitForSeconds glitchDuration = new WaitForSeconds(.1f);

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();    
    }

    // Start is called before the first frame update
    IEnumerator Start()
    {
        while (true)
        {
            float glitchTest = Random.Range(0f, 1f);

            if (glitchTest <= GlitchChance)
            {
                StartCoroutine(Glitch());
            }
            yield return glitchLoopTime;
        }
    }

    IEnumerator Glitch()
    {
        glitchDuration = new WaitForSeconds(Random.Range(.05f, 0.25f));
        _renderer.material.SetFloat("_Amount", 1f);
        _renderer.material.SetFloat("_CutoutThresh", .29f);
        _renderer.material.SetFloat("_Amplitude", Random.Range(100, 250));
        _renderer.material.SetFloat("_Speed", Random.Range(1, 10));
        yield return glitchDuration;
        _renderer.material.SetFloat("_Amount", 0f);
        _renderer.material.SetFloat("_CutoutThresh", 0f);
    }
}
