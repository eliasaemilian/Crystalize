using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplacementController : MonoBehaviour
{
    [SerializeField] private float _displacementAmount;
    [SerializeField] private ParticleSystem _explosionParticles;

    MeshRenderer _meshR;

    // Start is called before the first frame update
    void Start()
    {
        _meshR = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        _displacementAmount = Mathf.Lerp(_displacementAmount, 0, Time.deltaTime);
        _meshR.material.SetFloat("_Amount", _displacementAmount);

        if (Input.GetButtonDown("Jump"))
        {
            _displacementAmount += 1f;
            _explosionParticles.Play();
        }
    }
}
