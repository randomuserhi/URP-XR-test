using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public Vector2Int gridPos;
    public CellIdentity identity;
    public GameObject snapPoint;

    void FixedUpdate()
    {
        if (identity != null) identity.CellUpdate(snapPoint.transform.position);
    }
}
