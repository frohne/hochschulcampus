using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserPosition : MonoBehaviour {

    public GameObject networkRole;
    private float lastPositionUpdate = 0.0f;
    private float positionUpdateRate = 0.5f;

    // Update is called once per frame
    void Update ()
    {
        if (Time.time - lastPositionUpdate > positionUpdateRate)
        {
            lastPositionUpdate = Time.time;

            networkRole.GetComponent<NetworkController>().laserUpdate(this.gameObject);
        }
	}
}
