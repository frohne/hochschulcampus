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

    private Vector3 lastRot;
    public GameObject networkRole;
	
	// Update is called once per frame
	void Update ()
    {
        if(selectedObject != null)
        {
            Vector3 rot = selectedObject.transform.rotation.eulerAngles;

            //if someone else has change the rotation of my selected Object 
            //=> adapt Slider Values
            if (rot != lastRot)
            {
                setSelectedObject(selectedObject);
            }

            selectedObject.transform.rotation = Quaternion.Euler(sliderX.value, sliderY.value, sliderZ.value);
            lastRot = selectedObject.transform.rotation.eulerAngles;

            if (rot != selectedObject.transform.rotation.eulerAngles)
            {
                //Tell Server to Update Rotation
                networkRole.GetComponent<NetworkController>().rotationUpdate(selectedObject);
            }
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
        networkRole.GetComponent<NetworkController>().rotationUpdate(selectedObject);
    }
}
