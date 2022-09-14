using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public Vector2Int gridPos;
    public CellIdentity identity;
    public GameObject snapPoint;

    public bool onBoard = true;

    public Collider col;

    private void Start()
    {
        if (!onBoard)
        {
            col = transform.parent.GetComponent<Collider>();
            Board.instance.sliderCells.Add(this);
        }
    }

    void FixedUpdate()
    {
        if (identity != null) identity.CellUpdate(snapPoint.transform.position, this);
    }

    private void OnDestroy()
    {
        Board.instance.sliderCells.Remove(this);
        if (identity) identity.InvokeDestroy();
    }
}
