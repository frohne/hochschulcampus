using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
 * Scripts wich selects an object by raycasting and commits it to translation, rotation and scaling Script
 * 
 * Lennart
 */
public class ObjectSelection : MonoBehaviour
{

    private GameObject selectedObj = null;
    private GameObject boundingBox = null;
    public Material boundingBoxMaterial = null;
    public GameObject objectUI = null;
    public GameObject translationUI = null;
    public GameObject rotationUI = null;
    public GameObject scalingUI = null;

    // Update is called once per frame
    void Update ()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began && !translationUI.activeSelf && !rotationUI.activeSelf && !scalingUI.activeSelf)
        {
            selectedObj = null;
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                selectedObj = hit.transform.gameObject;

                if(selectedObj != null && selectedObj != boundingBox)
                {
                    Destroy(boundingBox);
                    boundingBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
                
                    boundingBox.transform.position = selectedObj.transform.position;
                    boundingBox.transform.rotation = selectedObj.transform.rotation;
                    boundingBox.transform.SetParent(selectedObj.transform);
                    /**/
                    boundingBox.transform.localScale = new Vector3(1.1F, 1.1F, 1.1F);
                
                    Renderer rend = boundingBox.GetComponent<Renderer>();
                    rend.material = boundingBoxMaterial;
                    /**/
                    translationUI.GetComponent<ObjectTranslation>().setSelectedObject(selectedObj);
                    rotationUI.GetComponent<ObjectRotation>().setSelectedObject(selectedObj);
                    scalingUI.GetComponent<ObjectScaling>().setSelectedObject(selectedObj);
                }
                
            }
        }

        if(selectedObj != null)
        {
            objectUI.SetActive(true);
        }
    }

    public void deselectObject()
    {
        selectedObj = null;
        Destroy(boundingBox);
        boundingBox = null;
    }
}
