using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    private Interactable interactable;
    private Interactable temp;

    protected GameObject newObject;
    private CellIdentity[] newLensCellIdentities;


    void Start()
    {
        interactable = GetComponent<Interactable>();
    }

    void FixedUpdate()
    {
        if (interactable.isGrabbing && newObject == null) {
        newObject = Instantiate(gameObject, transform.position, transform.rotation);

            newLensCellIdentities = newObject.GetComponentsInChildren<CellIdentity>();
            newLensCellIdentities[0].enabled = true;
            newLensCellIdentities[0].interactable = interactable;
            for (int i = 0; i < newLensCellIdentities.Length; i++) newLensCellIdentities[i].enabled = true;
            newObject.GetComponent<Spawner>().enabled = false;
            LightRayEmitter.colliders = FindObjectsOfType<LTKCollider>();

            Spawned();
        }
        else if (!interactable.isGrabbing && newObject != null)
        {
            newLensCellIdentities[0].interactable = newObject.GetComponent<Interactable>();
            interactable.enabled = true;
            newObject = null;
        }
    }

    protected virtual void Spawned() { }
}
