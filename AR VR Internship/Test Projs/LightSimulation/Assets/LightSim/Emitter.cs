using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;

/*public struct LightRayHit
{
    public Vector3 point;
    public Vector3 normal;
    public Lens l;
    public bool backface;

    public static implicit operator LightRayHit(RaycastHit hit)
    {
        return new LightRayHit() { point = hit.point, normal = hit.normal, l = hit.transform.GetComponentInParent<Lens>() };
    }
}*/

/*public struct Light
{
    public LightRayHit hit;
    public Vector3 pos;
    public Vector3 dir;
    public Vector3 prevDir;
    public Vector3 norm;

    public float invWavelength; //in micrometers
    
    public float currentRefractiveIndex;
    public List<Lens> mediums;

    public void Init(float invWavelength)
    {
        this.invWavelength = invWavelength;
        currentRefractiveIndex = Lens.refractiveIndexofAir.index(invWavelength);
        mediums = new List<Lens>();
    }

    public void Reset()
    {
        mediums.Clear();
        currentRefractiveIndex = Lens.refractiveIndexofAir.index(invWavelength);
    }

    public bool Update()
    {
        const float far = 1000f;

        Parabola[] curves = Object.FindObjectsOfType<Parabola>();
        List<LightRayHit> hits = new List<LightRayHit>();
        LightRayHit[] buffer = new LightRayHit[2];
        for (int i = 0; i < curves.Length; i++)
        {
            Parabola p = curves[i];
            if (!p.gameObject.activeSelf || !p.enabled) continue;
            int count = p.Solve(pos, dir, buffer);
            for (int j = 0; j < count; j++) hits.Add(buffer[j]);
        }

        bool backface = false;
        bool success = false;
        RaycastHit[] unity = Physics.RaycastAll(pos, dir, float.PositiveInfinity, 1 << 3);
        for (int i = 0; i < unity.Length; i++) hits.Add(unity[i]);
        LightRayHit[] forward = hits.ToArray();
        Lens l = null;
        for (int i = 0; i < forward.Length; i++)
        {
            ref LightRayHit f = ref forward[i];
            if (Vector3.Dot(f.point - pos, dir) <= 0) continue;
            l = f.l;
            if (!success || (f.point != pos && (f.point - pos).sqrMagnitude < (hit.point - pos).sqrMagnitude))
            {
                bool temp = false;
                if (l.verifyHit(f, ref temp))
                {
                    hit = f;
                    success = true;
                    backface = temp;
                }
            }
        }
        RaycastHit[] back = Physics.RaycastAll(pos + dir * far, - dir, float.PositiveInfinity, 1 << 3);
        if (!success && back.Length == 0) return false;
        for (int i = 0; i < back.Length; i++)
        {
            ref RaycastHit b = ref back[i];
            if (Vector3.Dot(b.point - pos, dir) <= 0) continue;
            if (!success || (b.point != pos && (b.point - pos).sqrMagnitude < (hit.point - pos).sqrMagnitude))
            {
                l = b.transform.GetComponentInParent<Lens>();
                bool temp = true;
                if (l.verifyHit(b, ref temp))
                {
                    hit = b;
                    success = true;
                    backface = temp;
                }
            }
        }
        if (!success) return false;
        if (Vector3.Dot(hit.normal, dir) > 0) hit.normal = -hit.normal;

        prevDir = dir;

        float refractiveIndex = Lens.refractiveIndexofAir.index(invWavelength);
        if (backface) mediums.Remove(l);
        else mediums.Add(l);
        if (mediums.Count > 0) refractiveIndex = mediums[mediums.Count - 1].refractiveIndex.index(invWavelength);

        float ratio = currentRefractiveIndex / refractiveIndex;
        float cosI = -Vector3.Dot(hit.normal, dir);
        float sinT2 = ratio * ratio * (1f - cosI * cosI);
        if (sinT2 > 1.0f) // Total internal reflection
        {
            Vector3 perp = Vector3.Cross(dir, hit.normal);
            Vector3 p = Vector3.Cross(hit.normal, perp);
            Vector3 projection = Vector3.Project(dir, p);
            dir = -dir + projection * 2;

            if (backface) mediums.Add(l);
            else mediums.Remove(l);
            refractiveIndex = currentRefractiveIndex;
        }
        else
        {
            float cosT = Mathf.Sqrt(1f - sinT2);
            dir = ratio * dir + (ratio * cosI - cosT) * hit.normal;
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
    public float current;

    public float wavelength = 0.64f;

    private void Start()
    {
        l.Init(1f / wavelength);
        l.dir = transform.forward;
        l.pos = transform.position;

        Physics.queriesHitBackfaces = true;
    }

    private void Update()
    {
        l.Init(1f / wavelength);
        l.pos = transform.position;
        l.dir = transform.forward;
        l.Reset();

        for (int count = 0; count < 10; count++)
        {
            l.Update();
            /*if (!l.Update())
            {
                l.Init(1f / wavelength);
                l.pos = transform.position;
                l.dir = transform.forward;
                l.Reset();
            }
            current = l.currentRefractiveIndex;
        }
    }

    private void OnDrawGizmos()
    {
        Debug.DrawLine(l.pos, l.pos + l.dir*10f);
        Debug.DrawLine(l.pos, l.pos + l.norm, Color.red);
        Debug.DrawLine(l.pos, l.pos - l.norm*1.5f, Color.green);
        Debug.DrawLine(l.pos, l.pos - l.prevDir, Color.magenta);
    }
}*/
