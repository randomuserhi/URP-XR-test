using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lens : MonoBehaviour
{
    public const float refractiveIndexofAir = 1.000293f;
    public float refractiveIndex = 1.523f;

    public Type type = Type.plane;
    public float focalLength = 2f;

    public enum Type
    {
        plane = 0,
        convex = 1,
        concave = 2
    }
}
