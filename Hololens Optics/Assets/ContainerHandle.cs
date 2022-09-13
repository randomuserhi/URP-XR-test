using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerHandle : MonoBehaviour
{
    private Interactable interactable;

    private void Start()
    {
        interactable = GetComponent<Interactable>();
    }

    private void FixedUpdate()
    {
        if (interactable.isGrabbing)
        {
            transform.parent.position = interactable.position - transform.localPosition;
            transform.parent.rotation = Quaternion.Euler(0, interactable.rotation.eulerAngles.y, 0);
        }
    }
}
