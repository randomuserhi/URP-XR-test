using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LightTK;

public class Default : MonoBehaviour
{
    LightRay[] raysY = new LightRay[10];
    LightRay[] raysX = new LightRay[10];
    AbstractSurface[] surfaces;
    Surface[] curves;

    float dist = 2f;

    //Two non-parallel lines which may or may not touch each other have a point on each line which are closest
    //to each other. This function finds those two points. If the lines are not parallel, the function 
    //outputs true, otherwise false.
    public static bool ClosestPointsOnTwoLines(out Vector3 closestPointLine1, out Vector3 closestPointLine2, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
    {
        closestPointLine1 = Vector3.zero;
        closestPointLine2 = Vector3.zero;

        float a = Vector3.Dot(lineVec1, lineVec1);
        float b = Vector3.Dot(lineVec1, lineVec2);
        float e = Vector3.Dot(lineVec2, lineVec2);

        float d = a * e - b * b;

        //lines are not parallel
        if (d != 0.0f)
        {

            Vector3 r = linePoint1 - linePoint2;
            float c = Vector3.Dot(lineVec1, r);
            float f = Vector3.Dot(lineVec2, r);

            float s = (b * f - c * e) / d;
            float t = (a * f - c * b) / d;

            closestPointLine1 = linePoint1 + lineVec1 * s;
            closestPointLine2 = linePoint2 + lineVec2 * t;

            return true;
        }

        else
        {
            return false;
        }
    }

    public void Start()
    {
        surfaces = new AbstractSurface[1]
        {
            new LightTK.Plane()
        };
        surfaces[0].refractionSettings = new RefractionSettings()
        {
            type = RefractionSettings.Type.Single,
            single = 1.5f
        };
        surfaces[0].surface.settings.type = SurfaceSettings.SurfaceType.IdealLens;
        surfaces[0].surface.settings.setFocalLength(0.5f);

        curves = LTK.BakeCurves(surfaces);

        for (int i = 0; i < raysY.Length; i++)
        {
            ref LightRay l = ref raysY[i];
            l.position = new Vector3(0, -dist / 2f + dist / (raysY.Length - 1f) * i, -2f);
            l.direction = transform.forward;
            l.wavelength = 0.64f;
            l.refractiveIndex = 1;
        }
        for (int i = 0; i < raysY.Length; i++)
        {
            ref LightRay l = ref raysY[i];
            LTK.SimulateRay(ref l, curves);
        }
        Vector3 p1;
        Vector3 p2;
        ClosestPointsOnTwoLines(out p1, out p2, raysY[0].position, raysY[0].direction, raysY[1].position, raysY[1].direction);
        for (int i = 0; i < raysX.Length; i++)
        {
            ref LightRay l = ref raysX[i];
            l.position = p1;
            l.direction = raysY[i].position - l.position;
            l.wavelength = 0.64f;
            l.refractiveIndex = 1f;
        }
    }

    bool skip = false;
    public void FixedUpdate()
    {
        if (!skip) skip = !skip;
        else
        {
            for (int i = 0; i < raysX.Length; i++)
            {
                ref LightRay l = ref raysX[i];
                LTK.SimulateRay(ref l, curves);
            }
        }
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < raysY.Length; i++)
        {
            ref LightRay l = ref raysY[i];
            Gizmos.DrawSphere(l.position, 0.1f);
            Debug.DrawLine(l.position, l.position + l.direction.normalized * 5f);
            Debug.DrawLine(l.position, l.position + l.normal, Color.red);
            Debug.DrawLine(l.position, l.position - l.normal * 1.5f, Color.green);
            Debug.DrawLine(l.position, l.position - l.prevDirection.normalized, Color.magenta);
        }
        for (int i = 0; i < raysX.Length; i++)
        {
            ref LightRay l = ref raysX[i];
            Gizmos.DrawSphere(l.position, 0.1f);
            Debug.DrawLine(l.position, l.position + l.direction.normalized * 5f, Color.gray);
            Debug.DrawLine(l.position, l.position + l.normal, Color.red);
            Debug.DrawLine(l.position, l.position - l.normal * 1.5f, Color.green);
            Debug.DrawLine(l.position, l.position - l.prevDirection.normalized, Color.magenta);
        }
    }
}
