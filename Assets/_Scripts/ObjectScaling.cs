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
    public Slider sliderY = null;
    public Slider sliderZ = null;


    // Update is called once per frame
    void Update ()
    {
        if (selectedObject != null)
        {
            selectedObject.transform.localScale = new Vector3(sliderX.value, sliderY.value, sliderZ.value);

        }
    }

    public void setSelectedObject(GameObject obj)
    {
        selectedObject = obj;
        sliderX.value = selectedObject.transform.localScale.x;
        sliderX.maxValue = selectedObject.transform.position.x + 0.5f;

        sliderY.value = selectedObject.transform.localScale.y;
        sliderY.maxValue = selectedObject.transform.position.y + 0.5f;

        sliderZ.value = selectedObject.transform.localScale.z;
        sliderZ.maxValue = selectedObject.transform.position.z + 0.5f;
    }

    public void resetScale()
    {
        selectedObject.transform.localScale = selectedObject.GetComponent<RootSettings>().getRootScaling();
        setSelectedObject(selectedObject);
    }
}
