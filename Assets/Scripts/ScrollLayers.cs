using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScrollLayers : MonoBehaviour {

    public ScrollRect scrollView;
    public GameObject scrollContent;
    public GameObject scrollItemLayer;
   
 GameObject layerObject;
    
     
    string[] layernames;
    public TextMeshProUGUI objectName;
    public GameObject objectToggle;
    int count;
   
	// Use this for initialization
	void Start () {
        //layernames = layerObject.GetComponentsInChildren<GameObject>().name;

        //generateLayer(layerObject);
        
        
        

    }
    public void generateLayer(GameObject currentObject)
    {
       
        Toggle toggle = objectToggle.GetComponent<Toggle>();
        toggle.onValueChanged.RemoveAllListeners();
        toggle.onValueChanged.AddListener(delegate { ToggleValueChanged(currentObject); });


        foreach (Transform g in scrollContent.transform)
        {
            Destroy(g.gameObject);
        }
        objectName.text = currentObject.name;
        count = currentObject.GetComponent<Transform>().childCount;


        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {

                generateLayerItem(i,currentObject);
            }
        }

        
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void generateLayerItem(int i, GameObject currentObject)
    {
        GameObject scrollItemLayerObj = Instantiate(scrollItemLayer);
        scrollItemLayerObj.transform.SetParent(scrollContent.transform, false);
        scrollItemLayerObj.GetComponentInChildren<TextMeshProUGUI>().text = currentObject.GetComponent<Transform>().GetChild(i).name;
        GameObject go = currentObject.GetComponent<Transform>().GetChild(i).gameObject;

        Toggle toggle = scrollItemLayerObj.GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(delegate { ToggleValueChanged(go); });



    }

    void ToggleValueChanged(GameObject gameObject)
    {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
        }
       

    }

    public void DisableGameObjects(GameObject gameObject)
    {
        gameObject.SetActive(false);
    }

   

   
}
