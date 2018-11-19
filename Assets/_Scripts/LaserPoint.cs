using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserPoint : MonoBehaviour {

    Camera arCam ;
    private SpriteRenderer hitSprite;
    public SpriteRenderer hitSpriteAdd;
    private bool toggle;
    public SpriteRenderer inactiveLaser;

    // Use this for initialization
    void Start()
    {
        arCam = Camera.main;
        hitSprite = gameObject.GetComponent<SpriteRenderer>();
       // inactiveLaser = gameObject.GetComponent<SpriteRenderer>();
        hitSprite.enabled = false;
        hitSpriteAdd.enabled = false;
        toggle = false;
    }

    public void toggleLaser()
    {
        if (toggle)
        {
            toggle = false;
            hitSprite.enabled = false;
            hitSpriteAdd.enabled = false;
            inactiveLaser.enabled = false;
        }
        else
        {
            toggle = true;
            hitSprite.enabled = false;
            hitSpriteAdd.enabled = false;
            inactiveLaser.enabled = true;
            inactiveLaser.transform.position = arCam.transform.position + arCam.transform.forward.normalized * 1f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        if (toggle)
        {
            if (Physics.Raycast(arCam.transform.position, arCam.transform.forward, out hit))
            {
                if (hit.collider)
                {
                    Debug.Log("hit element");
                    //transform.position = hit.point;
                    transform.position = hit.point - arCam.transform.forward.normalized * 0.1f;
                    hitSpriteAdd.transform.position = transform.position;
                    transform.LookAt(arCam.transform);
                    hitSpriteAdd.transform.LookAt(arCam.transform);
                    hitSprite.enabled = true;
                    hitSpriteAdd.enabled = true;
                    inactiveLaser.enabled = false;

                }
                else
                {
                    hitSprite.enabled = false;
                    hitSpriteAdd.enabled = false;
                    inactiveLaser.enabled = true;
                    inactiveLaser.transform.position = arCam.transform.position + arCam.transform.forward.normalized * 1f;

                }
            }
            else
            {
                hitSprite.enabled = false;
                hitSpriteAdd.enabled = false;
                inactiveLaser.enabled = true;
                inactiveLaser.transform.position = arCam.transform.position + arCam.transform.forward.normalized * 1f;

            }

        }

    }
}
