using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SliderSpawner : Spawner
{
    [HideInInspector]
    public LensSlider lensSlider;

    public GameObject lensCartPrefab;
    public bool snappable = false;

    protected override void Spawned()
    {
        GameObject lc = Instantiate(lensCartPrefab);
        Cell c = lc.GetComponentInChildren<Cell>();
        c.onBoard = false;
        lensSlider = lc.GetComponent<LensSlider>();
        lensSlider.snappable = snappable;
        lensSlider.cellA = newObject.GetComponent<CellIdentity>();
        lensSlider.cellB = newObject.GetComponentsInChildren<CellIdentity>().Where(c => c.transform != newObject.transform).First();
        newObject.GetComponent<SliderSpawner>().lensSlider = lensSlider;
    }
}
