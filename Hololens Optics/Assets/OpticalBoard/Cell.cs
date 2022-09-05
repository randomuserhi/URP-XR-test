using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public Vector2Int gridPos;
    public CellIdentity identity;
    public GameObject snapPoint;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        identity?.CellUpdate(snapPoint.transform.position);
    }
}
