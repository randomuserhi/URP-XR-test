using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellIdentity : MonoBehaviour
{
    [HideInInspector]
    public Transform parent;
    [HideInInspector]
    public Cell owner;

    [HideInInspector]
    public bool destroy = false;
    [HideInInspector]
    public float destroyTimer = 0;

    [HideInInspector]
    public Interactable interactable;

    [HideInInspector]
    public CellIdentity linkedIdentity;

    public int gridSnapAngle = 30;
    public int sliderSnapAngle = 90;

    private bool setPos = false;
    private bool useBoard = true;
    [HideInInspector]
    public Vector2Int gridPos;

    private Canvas label;
    private Collider c;

    Quaternion rotOffset = Quaternion.identity;

    private void OnEnable()
    {
        interactable = GetComponent<Interactable>();
    }

    public void Start()
    {
        label = GetComponentInChildren<Canvas>();
        c = GetComponent<Collider>();
    }

    private void FixedUpdate()
    {
        //if (!interaction && !destroy) interaction = GetComponent<Interactable>();

        //if (parent) transform.parent = parent;

        label.enabled = interactable.isHovering;
        if (label.enabled) { label.transform.LookAt(label.transform.position + Camera.main.transform.rotation * Vector3.left, Camera.main.transform.rotation * Vector3.up); label.transform.eulerAngles = new Vector3(0f, label.transform.eulerAngles.y, 0f); }

        if (interactable.isGrabbing)
        {
            if (Board.instance.grid.ContainsKey(gridPos) && Board.instance.grid[gridPos] == this || Board.instance.reverseGrid.ContainsKey(this) && Board.instance.reverseGrid[this] == gridPos)
            {
                Board.instance.grid.Remove(gridPos);
                Board.instance.reverseGrid.Remove(this);
            }

            if (Board.instance.sliderCells.Contains(owner))
            {
                useBoard = true;
                owner.identity = null;
                parent = transform;
                owner = null;
            }

            destroy = false;
            if (linkedIdentity) linkedIdentity.destroy = false;
            parent = transform;

            transform.position = interactable.position;
            transform.rotation = interactable.rotation;

            //IMPORTANT
            if (linkedIdentity)
            {
                if (Board.instance.reverseGrid.ContainsKey(linkedIdentity))
                {
                    transform.parent = null;
                    linkedIdentity.transform.parent = null;
                    transform.parent = linkedIdentity.transform;
                }
            }

            setPos = true;

            if (Board.instance != null) //TODO:: assign Board.instance based on closest Board.instance in world
            {
                Vector3 localPos = Board.instance.transform.InverseTransformPoint(transform.position);
                gridPos = Board.instance.PositionToGrid(localPos);
            }
        }
        else
        {
            foreach (Cell c in Board.instance.sliderCells)
            {
                if (c.enabled && !owner && !linkedIdentity && (c.col.bounds.center - transform.position).sqrMagnitude < 0.1f * 0.1f)
                {
                    if (!c.identity)
                    {
                        useBoard = false;
                        parent = c.transform;
                        owner = c;
                        owner.identity = this;
                        destroy = false;
                    }
                    rotOffset = Quaternion.Inverse(transform.rotation) * c.transform.rotation;
                }
            }
        }
        if (!interactable.isGrabbing && setPos && useBoard)
        {
            setPos = false;
            if (Board.instance.cells.ContainsKey(gridPos) && !Board.instance.grid.ContainsKey(gridPos))
            {
                Board.instance.grid.Add(gridPos, this);
                Board.instance.reverseGrid.Add(this, gridPos);

                rotOffset = Quaternion.Inverse(transform.rotation) * Board.instance.transform.rotation;
            }
            else
            {
                if (linkedIdentity)
                {
                    // IMPORTANT
                    bool isParent = !transform.parent;
                    if (!isParent)
                    {
                        transform.parent = null;
                        linkedIdentity.transform.parent = null;
                        transform.parent = linkedIdentity.transform;

                        transform.localPosition = Vector3.up * 0.08f;
                        transform.localRotation = Quaternion.identity;
                        //label.transform.position = linkedIdentity.transform.TransformPoint(linkedIdentity.label.transform.position);
                        //Vector3 l = label.transform.position;
                        //label.transform.position = linkedIdentity.label.transform.localPosition;

                        //enabled = false;
                    }
                }

                if (Bin.instance.colliders.Contains(c))
                {
                    Bin.instance.colliders.Remove(c);
                    InvokeDestroy();
                }
            }
        }

        if (parent == null && destroy)
        {
            if (destroyTimer <= 0) InvokeDestroy();
            else destroyTimer -= Time.fixedDeltaTime;
        }

        if (!destroy) destroyTimer = 2f;
    }

    public void CellUpdate(Vector3 localSnapPoint, Cell c)
    {
        transform.position = localSnapPoint;
        int snapAngle = useBoard ? gridSnapAngle : sliderSnapAngle;
        rotOffset.eulerAngles = snapAngle * (rotOffset.eulerAngles / snapAngle).RoundToInt();
        Quaternion multiplier = useBoard ? Board.instance.transform.rotation : c.transform.rotation;
        Quaternion rot = multiplier * Quaternion.Inverse(rotOffset);
        float xRot = useBoard ? 0f : 180f;
        rot.eulerAngles = new Vector3(xRot, rot.eulerAngles.y - xRot, 0);
        transform.rotation = rot;
    }

    private bool isDead = false;

    public void InvokeDestroy()
    {
        Destroy();
        if (linkedIdentity) linkedIdentity.Destroy();
    }

    private void Destroy()
    {
        if (isDead) return;
        isDead = true;

        if (TryGetComponent(out SliderSpawner spawner))
            if (spawner.lensSlider) Destroy(spawner.lensSlider.gameObject);

        if (Board.instance != null)
        {
            Board.instance.grid.Remove(gridPos);
            Board.instance.reverseGrid.Remove(this);
        }

        interactable.hands.Clear();
        interactable.isGrabbing = false;
        interactable.enabled = false;

        foreach (Transform T in transform)
        {
            if (!T.TryGetComponent(out Rigidbody rb))
                rb = T.gameObject.AddComponent<Rigidbody>();
            rb.AddForceAtPosition(new Vector3(Random.Range(-1f, 1f), 1f, Random.Range(-1f, 1f)), new Vector3(0, 20, 0), ForceMode.Impulse);
            rb.angularDrag = 0;
        }

        Destroy(gameObject, 2f);
    }
}
