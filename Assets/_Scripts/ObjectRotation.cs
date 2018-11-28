using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * Script wich handles the rotation sliders and rotates the selected object
 * 
 * Lennart
 */

public class ObjectRotation : MonoBehaviour
{

    private GameObject selectedObject = null;
    public Slider sliderX = null;
    public Slider sliderY = null;
    public Slider sliderZ = null;

    private Vector3 lastrot;

    // Update is called once per frame
    void Update ()
    {
        if(selectedObject != null)
        {
            Vector3 rot = selectedObject.transform.rotation.eulerAngles;

            //Check if someoneelse has modifyed the selected Object
            if (rot != lastrot)
            {
                //Adapt Slider
                setSelectedObject(selectedObject);
            }

            selectedObject.transform.rotation = Quaternion.Euler(sliderX.value, sliderY.value, sliderZ.value);

        }
    }

    public void setSelectedObject(GameObject obj)
    {
        selectedObject = obj;
        
        sliderX.value = selectedObject.transform.rotation.eulerAngles.x;

        sliderY.value = selectedObject.transform.rotation.eulerAngles.y;

        sliderZ.value = selectedObject.transform.rotation.eulerAngles.z;
    }

    public void resetRotation()
    {
        selectedObject.transform.rotation = Quaternion.Euler(selectedObject.GetComponent<RootSettings>().getRootScaling());
        setSelectedObject(selectedObject);
    }
}
