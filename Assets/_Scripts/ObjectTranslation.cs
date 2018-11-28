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

    private Vector3 lastpos;
	
	// Update is called once per frame
	void Update ()
    {

        if (selectedObject != null)
        {
            Vector3 pos = selectedObject.transform.position;

            //Check if someoneelse has modifyed the selected Object
            if(pos != lastpos)
            {
                //Adapt Slider
                setSelectedObject(selectedObject);
            }

            selectedObject.transform.position = new Vector3(sliderX.value, sliderY.value, sliderZ.value);
            lastpos = selectedObject.transform.position;

            //You have modifyed the selected Object
            if(pos != lastpos)
            {
                //dings.EditSyncObject(selectedObject);
            }
        }
	}

    public void setSelectedObject(GameObject obj)
    {
        //0.5 => Wertebereich
        selectedObject = obj;
        sliderX.value = selectedObject.transform.position.x;
        sliderX.minValue = selectedObject.transform.position.x - 0.5f;
        sliderX.maxValue = selectedObject.transform.position.x + 0.5f;

        sliderY.value = selectedObject.transform.position.y;
        sliderY.minValue = selectedObject.transform.position.y - 0.5f;
        sliderY.maxValue = selectedObject.transform.position.y + 0.5f;

        sliderZ.value = selectedObject.transform.position.z;
        sliderZ.minValue = selectedObject.transform.position.z - 0.5f;
        sliderZ.maxValue = selectedObject.transform.position.z + 0.5f;
    }

    public void resetPosition()
    {
        selectedObject.transform.position = selectedObject.GetComponent<RootSettings>().getRootPosition();
        setSelectedObject(selectedObject);
    }
}
