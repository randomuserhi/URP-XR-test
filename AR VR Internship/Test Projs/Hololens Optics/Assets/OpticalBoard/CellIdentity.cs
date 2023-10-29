using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellIdentity : MonoBehaviour
{
    public Board board;

    [System.NonSerialized]
    public Transform parent;
    [System.NonSerialized]
    public Cell owner;
    [System.NonSerialized]
    public bool occupied;
    [System.NonSerialized]
    public Vector2Int position;

    [System.NonSerialized]
    public bool destroy = false;
    [System.NonSerialized]
    public float destroyTimer = 0;

    private Interactable interaction;

    private bool setPos = false;
    public Vector2Int gridPos;

    public void Start()
    {
        interaction = GetComponent<Interactable>();
    }

    private void FixedUpdate()
    {
        transform.parent = parent;

        if (interaction.isGrabbing)
        {
            if (board.grid.ContainsKey(gridPos) && board.grid[gridPos] == this) board.grid.Remove(gridPos);

            destroy = false;
            parent = transform;

            transform.position = interaction.position;
            transform.rotation = interaction.rotation;

            setPos = true;

            if (board != null) //TODO:: assign board based on closest board in world
            {
                Vector3 localPos = board.transform.InverseTransformPoint(transform.position);
                gridPos = board.PositionToGrid(localPos);
            }
        }
        else if (setPos)
        {
            setPos = false;
            if (board.cells.ContainsKey(gridPos) && !board.grid.ContainsKey(gridPos)) board.grid.Add(gridPos, this);
        }

        if (parent == null && destroy)
        {
            if (destroyTimer <= 0)
            {
                Destroy();
                if (board != null) board.grid.Remove(position);
            }
            else destroyTimer -= Time.fixedDeltaTime;
        }

        if (!destroy) destroyTimer = 2f;
    }

    public void CellUpdate(Vector3 localSnapPoint)
    {
        transform.position = localSnapPoint;
        Quaternion rot = transform.rotation;
        rot.eulerAngles = new Vector3(0, rot.eulerAngles.y, 0);
        transform.rotation = rot;
    }

    private bool isDead = false;
    public void Destroy()
    {
        if (isDead) return;
        isDead = true;

        Destroy(interaction);

        foreach (Transform T in transform)
        {
            Rigidbody rb = T.gameObject.AddComponent<Rigidbody>();
            rb.AddForceAtPosition(new Vector3(Random.Range(-1f, 1f), 1f, Random.Range(-1f, 1f)), new Vector3(0, 20, 0), ForceMode.Impulse);
            rb.angularDrag = 0;
        }

        Destroy(gameObject, 2f);
    }
}
