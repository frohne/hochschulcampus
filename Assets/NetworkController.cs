using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkController : MonoBehaviour {

    public GameObject Client;
    public GameObject Server;
	
	public void positionUpdate (GameObject go)
    {
        if (Client.GetComponent<Client>().getIsStarted())
            Client.GetComponent<Client>().positionUpdate(go);
        else if (Server.GetComponent<Server>().getIsStarted())
            Server.GetComponent<Server>().positionUpdate(go);
    }

    public void rotationUpdate(GameObject go)
    {
        if (Client.GetComponent<Client>().getIsStarted())
            Client.GetComponent<Client>().rotationUpdate(go);
        else if (Server.GetComponent<Server>().getIsStarted())
            Server.GetComponent<Server>().rotationUpdate(go);
    }

    public void scalingUpdate(GameObject go)
    {
        if (Client.GetComponent<Client>().getIsStarted())
            Client.GetComponent<Client>().scalingUpdate(go);
        else if (Server.GetComponent<Server>().getIsStarted())
            Server.GetComponent<Server>().scalingUpdate(go);
    }
}
