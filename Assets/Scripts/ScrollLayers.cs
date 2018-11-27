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
        generateLayerItem();
      
        print(layerObject.GetComponent<Transform>().GetChild(0).name);
        print(layerObject.GetComponent<Transform>().GetChild(1).name);
        
        

    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void generateLayerItem()
    {
        GameObject scrollItemLayerObj = Instantiate(scrollItemLayer);
        scrollItemLayerObj.transform.SetParent(scrollContent.transform, false);
        scrollItemLayerObj.GetComponentInChildren<TextMeshProUGUI>().text = layerObject.GetComponent<Transform>().GetChild(0).name;
    }

   
}
