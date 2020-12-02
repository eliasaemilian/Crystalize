using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour {

    [HideInInspector]
    public string animationName;
    [HideInInspector]
    public int ID;

    public UnityEngine.UI.Button button;

    public AnimalViewer animalViewer;

    [HideInInspector]
    public bool animalChoose = false;

    void Start()
    {
        button = GetComponent<UnityEngine.UI.Button>();

        if(animalChoose)
        {
            button.onClick.AddListener(AnimalChoose);
        }

        else
        {
            button.onClick.AddListener(DoAnimation);
        }
    }

    void DoAnimation()
    {
        animalViewer.SetAnimals(animationName);
    }

    void AnimalChoose()
    {
        animalViewer.SwapAnimal(ID);
    }
}
