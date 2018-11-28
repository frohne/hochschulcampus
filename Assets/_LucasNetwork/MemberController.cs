using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MemberController : NetworkBehaviour {
	
	[SyncVar]
	public bool Far = false;
	
	static bool _isServer;
	public static bool IsServer{
		get {return _isServer;}
	}
	
	void Start () {
		_isServer = isServer;
	}
	
	void Update () {
		if (_isServer){
			Far = CameraController.Far;
		}
		else {
			
		}
			
		MaterialBehaviour.Far = Far;
	}
	
	void OnDestroy(){
		
	}
}
