using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * Script wich handles the translation sliders and translates the selected object
 * 
 * Lennart
 */
public class ObjectTranslation : MonoBehaviour
{

    private GameObject selectedObject = null;
    public Slider sliderX = null;
    public Slider sliderY = null;
    public Slider sliderZ = null;

    public Vector3 lastPos;

    public GameObject networkRole;
    
    

	
	// Update is called once per frame
	void Update ()
    {
		if(selectedObject != null)
        {
            Vector3 pos = selectedObject.transform.position;

            //if someone else has change the position of my selected Object 
            //=> adapt Slider Values
            if(pos != lastPos)
            {
                setSelectedObject(selectedObject);
            }

            selectedObject.transform.position = new Vector3(sliderX.value, sliderY.value, sliderZ.value);
            lastPos = selectedObject.transform.position;

            if (pos != selectedObject.transform.position)
            {
                //Tell Server to Update Position
                networkRole.GetComponent<NetworkController>().positionUpdate(selectedObject);
            }
        }
	}

    public void setSelectedObject(GameObject obj)
    {
        //0.3 => Wertebereich
        selectedObject = obj;
        sliderX.value = selectedObject.transform.position.x;
        sliderX.minValue = selectedObject.transform.position.x - 0.3f;
        sliderX.maxValue = selectedObject.transform.position.x + 0.3f;

        sliderY.value = selectedObject.transform.position.y;
        sliderY.minValue = selectedObject.transform.position.y - 0.3f;
        sliderY.maxValue = selectedObject.transform.position.y + 0.3f;

        sliderZ.value = selectedObject.transform.position.z;
        sliderZ.minValue = selectedObject.transform.position.z - 0.3f;
        sliderZ.maxValue = selectedObject.transform.position.z + 0.3f;
    }

    public void resetPosition()
    {
        selectedObject.transform.position = selectedObject.GetComponent<RootSettings>().getRootPosition();
        setSelectedObject(selectedObject);
        networkRole.GetComponent<NetworkController>().positionUpdate(selectedObject);
    }
}
