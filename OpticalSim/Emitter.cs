using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;

public struct Light
{
    public RaycastHit hit;
    public Vector3 pos;
    public Vector3 dir;
    public Vector3 prevDir;
    public Vector3 norm;
    public float currentRefractiveIndex;
    public List<Lens> mediums;

    public void Init()
    {
        currentRefractiveIndex = Lens.refractiveIndexofAir;
        mediums = new List<Lens>();
    }

    public void Reset()
    {
        mediums.Clear();
        currentRefractiveIndex = Lens.refractiveIndexofAir;
    }

    public bool Update()
    {
        Ray ray = new Ray(pos, dir);
        if (!Physics.Raycast(ray, out hit, 100f, 1 << 3))
        {
            return false;
        }
        Lens l = hit.transform.GetComponentInParent<Lens>();
        bool exiting = mediums.Contains(l);
        if (!exiting)
        if (Vector3.Dot(ray.direction, hit.normal) < 0) hit.normal = exiting ? -hit.normal : hit.normal;
        else hit.normal = exiting ? hit.normal : -hit.normal;
        else
        if (Vector3.Dot(ray.direction, hit.normal) < 0) hit.normal = exiting ? hit.normal : -hit.normal;
        else hit.normal = exiting ? -hit.normal : hit.normal;
        float refractiveIndex = Lens.refractiveIndexofAir;
        if (exiting) mediums.Remove(l); else mediums.Add(l);
        if (mediums.Count > 0) refractiveIndex = mediums[mediums.Count - 1].refractiveIndex;
        //float ratio = refractiveIndex / currentRefractiveIndex;
        //if (ratio < 1f)
        //float criticalAngle = Mathf.Asin(ratio);
        float I = Mathf.Acos(Vector3.Dot(ray.direction, -hit.normal) / (ray.direction.magnitude * hit.normal.magnitude));
        float R = Mathf.Rad2Deg * Mathf.Asin((currentRefractiveIndex * Mathf.Sin(I)) / refractiveIndex);
        Quaternion rot = Quaternion.RotateTowards(Quaternion.LookRotation(ray.direction, Vector3.up), Quaternion.LookRotation(-hit.normal, Vector3.up), -R);
        prevDir = dir;
        float criticalAngle = Mathf.Rad2Deg * Mathf.Asin(refractiveIndex / currentRefractiveIndex);
        if ((!float.IsNaN(R) && R < criticalAngle) || float.IsNaN(criticalAngle))
        {
            if (currentRefractiveIndex <= refractiveIndex)
                rot = Quaternion.RotateTowards(Quaternion.LookRotation(-hit.normal, Vector3.up), Quaternion.LookRotation(ray.direction, Vector3.up), R);
            dir = (rot * ray.direction).normalized;
        }
        else
        {
            Vector3 perp = Vector3.Cross(ray.direction, hit.normal);
            Vector3 p = Vector3.Cross(hit.normal, perp);
            Vector3 projection = Vector3.Project(ray.direction, p);
            dir = -ray.direction + projection * 2;

            if (exiting) mediums.Add(l);
            else mediums.Remove(l);
            refractiveIndex = currentRefractiveIndex;
        }
        pos = hit.point + dir.normalized * 0.001f;
        norm = hit.normal;
        currentRefractiveIndex = refractiveIndex;
        return true;
    }
}

public class Emitter : MonoBehaviour
{
    public Light l;

    private void Start()
    {
        l.Init();

        Physics.queriesHitBackfaces = true;
    }

    private void Update()
    {
        l.pos = transform.position;
        l.dir = transform.forward;
        l.Reset();

        int count = 0;
        for (;count < 10; count++)
        {
            if (!l.Update()) break;
        }

        Debug.Log("Num bounces: " + count);
    }

    private void OnDrawGizmos()
    {
        Debug.DrawLine(l.pos, l.pos + l.dir*1.5f);
        Debug.DrawLine(l.pos, l.pos + l.norm, Color.red);
        Debug.DrawLine(l.pos, l.pos - l.norm*1.5f, Color.green);
        Debug.DrawLine(l.pos, l.pos - l.prevDir, Color.magenta);
    }
}
