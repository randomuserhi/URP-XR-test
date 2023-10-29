using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Detection : MonoBehaviour
{
    private MeshRenderer mat;

    private void Start()
    {
        mat = GetComponent<MeshRenderer>();
    }

    public void OnDetection()
    {
        mat.material.color = Color.green;
    }

    public void OnLoss()
    {
        mat.material.color = Color.red;
    }
}
