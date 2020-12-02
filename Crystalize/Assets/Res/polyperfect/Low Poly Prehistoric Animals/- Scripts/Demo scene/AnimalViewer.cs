using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
public class AnimalViewer : MonoBehaviour
{
    GameObject activeAnimal;
    public GameObject Button;
    public Transform actionPanel;
    public Transform animalButtons;

    [HideInInspector] public List<Button> buttons;

    public Camera mainCamera;
    [Tooltip("Change the min and max value to set the FOV to what you want")]
    public float FOVMin, FOVMax;

    void Start()
    {
        activeAnimal = transform.GetChild(0).gameObject;

        for (int i = 0; i < transform.childCount; i++)
        {
            var animalButton = Instantiate(Button, animalButtons);
            var button = animalButton.GetComponent<Button>();
            button.ID = i;
            button.animalChoose = true;
            button.animalViewer = this;
            button.GetComponentInChildren<Text>().text = transform.GetChild(i).name;

            if (transform.GetChild(i).gameObject != activeAnimal)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }

            else
            {
                transform.GetChild(i).gameObject.SetActive(true);
            }
        }

        SortUI();
    }

    public void SwapAnimal(int animalCount)
    {
        for (int e = 0; e < transform.childCount; e++)
        {
            if (e == animalCount)
            {
                activeAnimal = transform.GetChild(e).gameObject;
            }
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).gameObject != activeAnimal)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }

            else
            {
                transform.GetChild(i).gameObject.SetActive(true);
            }
        }

        SortUI();
        SortCamera();

    }

    public void SetAnimals(string animationName)
    {
        var anim = activeAnimal.GetComponent<Animator>();

        if (animationName == "")
        {
            return;
        }

        else
        {
            if (anim.GetBool(animationName))
            {
                // Get the value of the animation paramater e.g. true or false
                var value = anim.GetBool(animationName);

                //Set the value to the inverse of what it was
                anim.SetBool(animationName, !value);
            }

            else
            {
                //If its not a bool then set a trigger to happen
                anim.SetTrigger(animationName);
            }
        }
    }

    public void ResetAnimals()
    {
        var anim = activeAnimal.GetComponent<Animator>();

        foreach (var items in anim.parameters)
        {
            anim.ResetTrigger(items.name);
        }
    }


    void SortUI()
    {
        foreach (var item in buttons)
        {
            Destroy(item.gameObject);
        }

        buttons.Clear();

        var paramaters = activeAnimal.GetComponent<Animator>().parameters;

        foreach (var item in paramaters)
        {
            var newButton = Instantiate(Button, actionPanel);

            newButton.GetComponentInChildren<Text>().text = item.name;
            var button = newButton.GetComponent<Button>();
            button.animationName = item.name;
            button.animalViewer = this;
            buttons.Add(button);
        }
    }

    void SortCamera()
    {
        var skinnedMeshRenderer = activeAnimal.GetComponentInChildren<SkinnedMeshRenderer>();

        var center = skinnedMeshRenderer.bounds.center;

        //Get the closest point from the camera to a point on the bounds
        var distance = skinnedMeshRenderer.bounds.SqrDistance(mainCamera.transform.position);
        
        SetCameraFOV(center,distance);
    }

    void SetCameraFOV(Vector3 center, float distance)
    {
        mainCamera.transform.LookAt(center);
        
        //Remap the distance value between two new values for the Field of View based on the meshes bounds
        mainCamera.fieldOfView = Remap(distance, 70f, 0, FOVMin, FOVMax);
    }

    public float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}
