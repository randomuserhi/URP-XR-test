using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    private Interactable interactable;

    void Start()
    {
        interactable = GetComponent<Interactable>();
    }

    GameObject newLens;
    void FixedUpdate()
    {
        if (newLens == null)
        {
            if (interactable.isGrabbing)
            {
                newLens = Instantiate(gameObject, transform.position, transform.rotation, transform.parent);
                newLens.GetComponent<CellIdentity>().enabled = true;
                newLens.GetComponent<Spawner>().enabled = false;
                LightRayEmitter.colliders = FindObjectsOfType<LTKCollider>();
            }
        }
        if (interactable.isGrabbing && newLens != null)
        {
            newLens.transform.position = interactable.position;
            newLens.transform.rotation = interactable.rotation;
        }
        else if (newLens != null) newLens = null;
    }
}
