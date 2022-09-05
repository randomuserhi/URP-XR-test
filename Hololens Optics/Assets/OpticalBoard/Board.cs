using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//TODO:: refactor to use half cell size and full cell size properly

public class Board : MonoBehaviour
{
    public GameObject cell;

    public GameObject pool;
    public GameObject container;

    public Interactable left;
    public Interactable right;
    public Interactable top;
    public Interactable bottom;

    private Vector3 topBottomOffset;
    private Vector3 rightLeftOffset;

    public BoxCollider box;
    private Interactable self;

    private List<Cell> activeCells = new List<Cell>();
    private Stack<Cell> objectPool = new Stack<Cell>();
    public Dictionary<Vector2Int, CellIdentity> grid = new Dictionary<Vector2Int, CellIdentity>();
    public Dictionary<Vector2Int, Cell> cells = new Dictionary<Vector2Int, Cell>();

    private Vector3 topLeft = new Vector3(-1, 0, 1);
    private Vector3 bottomRight = new Vector3(1, 0, -1);

    private const float halfCellSize = 0.03f; // 0.06 / 2

    public void Start()
    {
        self = GetComponent<Interactable>();
        GenerateGrid();
    }

    float grabTimer = 0;
    public void FixedUpdate()
    {
        if (self != null)
        {
            if (self.isGrabbing)
            {
                if (grabTimer <= 0)
                {
                    transform.position = self.position;
                    Quaternion rot = transform.rotation;
                    Quaternion other = self.rotation;
                    Vector3 euler = rot.eulerAngles;
                    euler.y = other.eulerAngles.y;
                    rot.eulerAngles = euler;
                    transform.rotation = rot;
                }
                else grabTimer -= Time.fixedDeltaTime;
            }
            else grabTimer = 1f;
        }

        float globalOffset = 4f * halfCellSize;

        float topBound = halfCellSize * (topLeft.z + 1);
        float bottomBound = halfCellSize * (bottomRight.z - 1);
        if (top.isGrabbing)
        {
            top.transform.localPosition = Vector3.Project(transform.InverseTransformPoint(top.position), Vector3.forward) + topBottomOffset;
            if (top.transform.localPosition.z < bottomBound + globalOffset) top.transform.localPosition = new Vector3(0, 0, bottomBound + globalOffset) + topBottomOffset;
        }
        else
            top.transform.localPosition = new Vector3(0, 0, halfCellSize * Mathf.Round(topLeft.z + 0.5f)) + topBottomOffset;

        if (bottom.isGrabbing)
        {
            bottom.transform.localPosition = Vector3.Project(transform.InverseTransformPoint(bottom.position), Vector3.forward) + topBottomOffset;
            if (bottom.transform.localPosition.z > topBound - globalOffset) bottom.transform.localPosition = new Vector3(0, 0, topBound - globalOffset) + topBottomOffset;
        }
        else
            bottom.transform.localPosition = new Vector3(0, 0, halfCellSize * Mathf.Round(bottomRight.z - 0.5f)) + topBottomOffset;

        float leftBound = halfCellSize * (topLeft.x - 1);
        float rightBound = halfCellSize * (bottomRight.x + 1);
        if (left.isGrabbing)
        {
            left.transform.localPosition = Vector3.Project(transform.InverseTransformPoint(left.position), Vector3.right) + rightLeftOffset;
            if (left.transform.localPosition.x > rightBound - globalOffset) left.transform.localPosition = new Vector3(rightBound - globalOffset, 0, 0) + rightLeftOffset;
        }
        else
            left.transform.localPosition = new Vector3(halfCellSize * Mathf.Round(topLeft.x - 0.5f), 0, 0) + rightLeftOffset;

        if (right.isGrabbing)
        {
            right.transform.localPosition = Vector3.Project(transform.InverseTransformPoint(right.position), Vector3.right) + rightLeftOffset;
            if (right.transform.localPosition.x < leftBound + globalOffset) right.transform.localPosition = new Vector3(leftBound + globalOffset, 0, 0) + rightLeftOffset;
        }
        else
            right.transform.localPosition = new Vector3(halfCellSize * Mathf.Round(bottomRight.x + 0.5f), 0, 0) + rightLeftOffset;

        topLeft = new Vector3(left.transform.localPosition.x / halfCellSize + 1, 0, top.transform.localPosition.z / halfCellSize - 1);
        bottomRight = new Vector3(right.transform.localPosition.x / halfCellSize - 1, 0, bottom.transform.localPosition.z / halfCellSize + 1);

        GenerateGrid();
    }

    private float width
    {
        get
        {
            return Mathf.Abs(bottomRight.x - topLeft.x);
        }
    }
    private float height
    {
        get
        {
            return Mathf.Abs(topLeft.z - bottomRight.z);
        }
    }

    public Vector2Int PositionToGrid(Vector3 pos)
    {
        return new Vector2Int(Mathf.RoundToInt(pos.x / halfCellSize / 2f), Mathf.RoundToInt(pos.z / halfCellSize / 2f));
    }

    public void GenerateGrid()
    {
        box.center = (topLeft + bottomRight) * halfCellSize / 2f;
        box.size = new Vector3((width - 1) * halfCellSize, 0.01f, (height - 1) * halfCellSize);

        topBottomOffset = new Vector3((topLeft.x + bottomRight.x) / 2f * halfCellSize, 0, 0);
        rightLeftOffset = new Vector3(0, 0, (topLeft.z + bottomRight.z) / 2f * halfCellSize);

        top.transform.localScale = new Vector3(0.015f, halfCellSize * (width / 2f), 0.015f);
        bottom.transform.localScale = new Vector3(0.015f, halfCellSize * (width / 2f), 0.015f);
        left.transform.localScale = new Vector3(0.015f, halfCellSize * (height / 2f), 0.015f);
        right.transform.localScale = new Vector3(0.015f, halfCellSize * (height / 2f), 0.015f);

        int w = Mathf.Clamp(Mathf.FloorToInt(width / 2f), 1, int.MaxValue);
        int h = Mathf.Clamp(Mathf.FloorToInt(height / 2f), 1, int.MaxValue);
        int volume = w * h;

        //First create list of cells
        while (activeCells.Count < volume)
        {
            if (objectPool.Count > 0)
            {
                Cell c = objectPool.Pop();
                c.gameObject.SetActive(true);
                activeCells.Add(c);
            }
            else
            {
                GameObject g = Instantiate(cell, container.transform);
                activeCells.Add(g.GetComponent<Cell>());
            }
        }
        while (activeCells.Count > volume)
        {
            int last = activeCells.Count - 1;
            activeCells[last].gameObject.SetActive(false);
            if (activeCells[last].identity != null) //TODO:: move this code to cellIdentity
            {
                activeCells[last].identity.parent = null;
                activeCells[last].identity.transform.parent = null;
                activeCells[last].identity.owner = null;
                activeCells[last].identity.destroy = true;
                activeCells[last].identity = null;
            }
            objectPool.Push(activeCells[last]);
            activeCells.RemoveAt(last);
        }
        for (int i = 0; i < activeCells.Count; i++)
        {
            if (activeCells[i].identity != null) //TODO:: move this code to cellIdentity
            {
                activeCells[i].identity.parent = null;
                activeCells[i].identity.transform.parent = null;
                activeCells[i].identity.owner = null;
                activeCells[i].identity.destroy = true;
                activeCells[i].identity = null;
            }
        }
        cells.Clear();
        //Debug.Log(activeCells.Count + ", " + h + ", " + w);
        //Vector3 gridOffset = box.center - new Vector3((w - 1) * cellSize, 0, (h - 1) * cellSize);
        for (int i = 0, y = Mathf.CeilToInt((bottomRight.z + 1f) / 2f); i < h; i++, y++)
        {
            for (int j = 0, x = Mathf.CeilToInt((topLeft.x + 1f) / 2f); j < w; j++, x++)
            {
                int idx = i * w + j;
                
                Vector2Int pos = new Vector2Int(x, y);
                cells.Add(pos, activeCells[idx]);
                if (grid.ContainsKey(pos))
                {
                    activeCells[idx].identity = grid[pos];
                    activeCells[idx].identity.parent = activeCells[idx].transform;
                    activeCells[idx].identity.owner = activeCells[idx];
                    activeCells[idx].identity.destroy = false;
                }
                activeCells[idx].gridPos = pos;
                activeCells[idx].transform.localPosition = new Vector3(pos.x * 2 * halfCellSize, 0, pos.y * 2 * halfCellSize);// + gridOffset;
            }
        }
    }
}
