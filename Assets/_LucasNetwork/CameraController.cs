using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
	
	public static bool Far = false;
	
	public Camera ARCamera;
	
	void Start () {
		ARCamera.enabled = true;
	}
	
	void Update () {
		if(MemberController.IsServer){
			var distance = this.transform.position.magnitude;
			if (distance > 0.5){
				Far = true;
			}
			else{
				Far = false;
			}
		}
	}
}
