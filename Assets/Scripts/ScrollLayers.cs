using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScrollLayers : MonoBehaviour {

    public ScrollRect scrollView;
    public GameObject scrollContent;
    public GameObject scrollItemLayer;
    public GameObject layerObject;
     
    string[] layernames;
    public TextMeshProUGUI objectName;
    int count;
   
	// Use this for initialization
	void Start () {
        //layernames = layerObject.GetComponentsInChildren<GameObject>().name;
        objectName.text = layerObject.name;
        count = layerObject.GetComponent<Transform>().childCount;
       
        
     

        for(int i = 0; i < count; i++)
        {

            generateLayerItem(i);
        }
           
        
        print(layerObject.GetComponent<Transform>().GetChild(0).name);
        print(layerObject.GetComponent<Transform>().GetChild(1).name);
        
        

    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void generateLayerItem(int i)
    {
        GameObject scrollItemLayerObj = Instantiate(scrollItemLayer);
        scrollItemLayerObj.transform.SetParent(scrollContent.transform, false);
        scrollItemLayerObj.GetComponentInChildren<TextMeshProUGUI>().text = layerObject.GetComponent<Transform>().GetChild(i).name;
        GameObject go = layerObject.GetComponent<Transform>().GetChild(i).gameObject;

        Toggle toggle = scrollItemLayerObj.GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(delegate { ToggleValueChanged(toggle,go); });



    }

    void ToggleValueChanged(Toggle toggle, GameObject gameObject)
    {
        if (gameObject.active)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
        }
       

    }

   
}
