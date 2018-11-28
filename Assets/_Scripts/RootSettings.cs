using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Script wich contains the root transform information for an object
 * 
 * Lennart
 */

public class RootSettings : MonoBehaviour {

    private Vector3 position;
    private Vector3 rotation;
    private Vector3 scaling;
	// Use this for initialization
	void Start ()
    {
        position = this.transform.position;
        rotation = this.transform.rotation.eulerAngles;
        scaling = this.transform.localScale;

        if(MemberController.IsServer)
        {
            //dings.AddObjectToSyncObjectList(this.gameObject)
        }


    }
	
	
    public Vector3 getRootPosition ()
    {
        return position;
    }

    public Vector3 getRootRotation()
    {
        return rotation;
    }

    public Vector3 getRootScaling()
    {
        return scaling;
    }
}
