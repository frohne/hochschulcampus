using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * Script wich handles the scaling sliders and scales the selected object
 * 
 * Lennart
 */

public class ObjectScaling : MonoBehaviour
{
    private GameObject selectedObject = null;
    public Slider sliderX = null;
    private Vector3 startScale;


    // Update is called once per frame
    void Update ()
    {
        if (selectedObject != null)
        {
            selectedObject.transform.localScale = startScale * sliderX.value;
        }
    }

    public void setSelectedObject(GameObject obj)
    {
        selectedObject = obj;
        startScale = selectedObject.transform.localScale;
        sliderX.value = 1;
        sliderX.maxValue =  4;
        
    }

    public void resetScale()
    {
        selectedObject.transform.localScale = selectedObject.GetComponent<RootSettings>().getRootScaling();
        setSelectedObject(selectedObject);
    }
}
