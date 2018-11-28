using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialBehaviour : MonoBehaviour {
	
	static bool far = false;
	public static bool Far{
		get {return far;}
		set {far = value;}
	}
	
	void Start () {
		
	}
	
	void Update () {
		
		if(far)
			GetComponent<MeshRenderer>().material.color = Color.red;
		else
			GetComponent<MeshRenderer>().material.color = Color.blue;
	}
	
	
}
