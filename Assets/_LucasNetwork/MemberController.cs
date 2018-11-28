using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SyncObject
{
    public string name;
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale;
}

public class MemberController : NetworkBehaviour {
	
	[SyncVar]
	public bool Far = false;
    public List<SyncObject> SyncObjectList = new List<SyncObject>();
	
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

        UpdateSyncObjects();

    }
	
	void OnDestroy(){
		
	}

    //Add an Object to SyncObjectList
    public void AddObjectToSyncObjectList(GameObject go)
    {
        SyncObject sync = new SyncObject();
        sync.name = go.name;
        sync.position = go.transform.position;
        sync.rotation = go.transform.rotation.eulerAngles;
        sync.scale = go.transform.localScale;

        SyncObjectList.Add(sync);
    }

    public void EditSyncObject(GameObject go)
    {
        SyncObject sync = SyncObjectList.Find(x => x.name == go.name);
        sync.position = go.transform.position;
        sync.rotation = go.transform.rotation.eulerAngles;
        sync.scale = go.transform.localScale;
    }

    //Update Pos, Rot & Scale of every SyncObject to Clients
    public void UpdateSyncObjects()
    {
        foreach(SyncObject sync in SyncObjectList)
        {
            GameObject go = GameObject.Find(sync.name).gameObject;
            go.transform.position = sync.position;
            go.transform.rotation = Quaternion.Euler(sync.rotation);
            go.transform.localScale = sync.scale;
        }
    }
}
