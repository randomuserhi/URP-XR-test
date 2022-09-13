using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LensSlider : MonoBehaviour
{
    [HideInInspector]
    public Transform slideBar;

    [HideInInspector]
    public bool snappable = false;

    [HideInInspector]
    public CellIdentity cellA;
    [HideInInspector]
    public CellIdentity cellB;

    private Transform A;
    private Transform B;

    private Interactable interactable;
    private Renderer[] renderers;
    private Cell cell;

    private float slideAmount = 0f;

    private bool active;


    private void Start()
    {
        interactable = GetComponent<Interactable>();
        renderers = GetComponentsInChildren<Renderer>();
        cell = GetComponentInChildren<Cell>();

        slideBar.parent = null;
    }

    private void FixedUpdate()
    {
        if (!A) A = cellA.transform;
        if (!B) B = cellB.transform;

        if (A && B)
        {
            slideBar.localScale = new Vector3(slideBar.localScale.x, slideBar.localScale.y, 1f + (B.position - A.position).magnitude / 0.03f);
            slideBar.position = Vector3.Lerp(A.position, B.position, 0.5f);
            slideBar.LookAt(B.position, Vector3.up);

            if (interactable.isGrabbing) transform.position = interactable.position;

            /*transform.position = A.position + Vector3.Project(transform.position - A.position, B.position - A.position);
            if (Vector3.Dot(transform.position - A.position, B.position - A.position) < 0) transform.position = A.position;
            else if ((transform.position - A.position).sqrMagnitude > (B.position - A.position).sqrMagnitude) transform.position = B.position;
            */

            if (interactable.isGrabbing)
            {
                transform.position = interactable.position;
                //if (snappable)
                //{
                //    transform.position = 
                //    Vector3 pos = transform.position + Board.instance.transform.position;
                //    pos = pos.MRound(0.06f);
                //    transform.position = pos - Board.instance.transform.position;
                //}
                transform.rotation = interactable.rotation;


                slideAmount = Mathf.Clamp(Vector3.Project(transform.position - A.position, B.position - A.position).magnitude / (B.position - A.position).magnitude, 0f, 1f);
                if (snappable)
                    slideAmount = slideAmount.MRound(1 / ((B.position - A.position).magnitude / 0.06f));
                if (Vector3.Dot(transform.position - A.position, B.position - A.position) < 0) slideAmount = 0;
            }

            transform.position = Vector3.Lerp(A.position, B.position, slideAmount);
            transform.rotation = Quaternion.LookRotation(B.position - A.position, Vector3.up);
        }

        active = Board.instance.reverseGrid.ContainsKey(cellA) && Board.instance.reverseGrid.ContainsKey(cellB);

        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].enabled = active;
        }
        interactable.enabled = active;
        cell.enabled = active;
    }

    private void OnDestroy()
    {
        if (slideBar != null) Destroy(slideBar.gameObject);
    }
}
