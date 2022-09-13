using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarColour : MonoBehaviour
{
    Interactable interactable;
    MeshRenderer meshRenderer;

    private void Start()
    {
        interactable = GetComponent<Interactable>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void FixedUpdate() => meshRenderer.material.color = interactable.isHovering ? Color.red : Color.white;
}
