using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lens : MonoBehaviour
{
    [System.Serializable]
    public struct RefractionEquation
    {
        // refractive index = m * (1 / wavelength) + c
        //
        // find m and c using real life measurements and know that
        // refractive index is inversely proportional to wavelength:
        //    - Use excel to get line of best fit from graph of y-axis "refractive index" by x-axis "1 / wavelength"
        //    - Display equation and take m and c values
        //
        // wavelength is typically in micro meters

        public float m;
        public float c;

        // used to define a fixed refraction index across all wavelengths
        public bool isFixed;
        public float refractionIndex;

        public float index(float invWavelength)
        {
            if (isFixed) return refractionIndex;
            return m * invWavelength + c;
        }
    }

    public enum Type
    {
        Surface,
        CustomSurface,
        ConvexLens,
        ConcaveLens
    }

    public static RefractionEquation refractiveIndexofAir = new RefractionEquation(){ m = 0.0277f, c = 0.9726f, refractionIndex = 1.000293f };
    
    public RefractionEquation refractiveIndex; //Crown glass m = 0.0163f, c = 1.4835f, refractionIndex = 1.523f
    
    public Type type;

    //TODO:: Change this if a better solution can be found
    public SphereCollider front;
    public SphereCollider back;
    public float bound;

    Vector3 frontFocalPoint;
    Vector3 backFocalPoint;

    //Baked properties
    float r1; //radius of first sphere
    float r2; //radius of second sphere
    Vector3 dir;
    Vector3 normDir;
    Vector3 p1;
    Vector3 p2;
    Vector3 center;

    public void Update()
    {
        if (type != Type.Surface && type != Type.CustomSurface) CalculateFocalPoints();
    }

    private void Bake()
    {
        r1 = front.radius * Mathf.Max(front.transform.localScale.x, front.transform.localScale.y, front.transform.localScale.z); //radius of first sphere
        r2 = back.radius * Mathf.Max(back.transform.localScale.x, back.transform.localScale.y, back.transform.localScale.z); //radius of second sphere

        dir = back.transform.position - front.transform.position;
        normDir = dir.normalized;
        p1 = front.transform.position + normDir * r1;
        p2 = back.transform.position - normDir * r2;
        center = p1 + (p2 - p1) / 2f;
    }

    public void CalculateFocalPoints() //This doesnt work properly
    {
        Bake();
        float d = (p2 - p1).magnitude;

        const float w = 0.64f;
        float r = refractiveIndex.index(w);
        float ra = refractiveIndexofAir.index(w);
        float rf = (r - ra) / ra;
        float f = rf * (1f / r2 - 1f / r1 + d * rf / (r * r1 * r2));
        frontFocalPoint = center - f * normDir;
        f = rf * (1f / r1 - 1f / r2 + d * rf / (r * r1 * r2));
        backFocalPoint = center - f * normDir;
    }

    public bool verifyHit(RayHit hit, ref bool backface)
    {
        switch (type)
        {
            case Type.ConcaveLens:
                {
                    Bake();
                    return (center - hit.point).sqrMagnitude < bound;
                }
            case Type.ConvexLens:
                {
                    Bake();
                    float sqrdistance = dir.sqrMagnitude;
                    float distance = dir.magnitude;
                    //refer to https://www.quora.com/Two-spheres-with-radii-of-6-and-4-have-centers-8m-apart-Whats-the-radius-of-the-circle-at-which-the-spheres-intersect
                    float x = (r1 * r1 - r2 * r2 + sqrdistance) / (2f * distance);
                    float y = r1 * r1 - x * x; //radius of intersection circle squared

                    return (center - hit.point).sqrMagnitude < y;
                }
            case Type.CustomSurface:
                backface = hit.backface;
                return true;
            case Type.Surface: 
                return true;
            default:
                return true;
        }
    }

    public void OnDrawGizmos()
    {
        Gizmos.DrawSphere(frontFocalPoint, 0.1f);
        Gizmos.DrawSphere(backFocalPoint, 0.1f);

        Gizmos.DrawSphere(p1, 0.1f);
        Gizmos.DrawSphere(p2, 0.1f);
        Gizmos.DrawSphere(center, 0.1f);
    }
}
