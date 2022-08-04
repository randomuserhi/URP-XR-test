using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SimMagnet
{
    public Vector3 north
    {
        get => rotation * (Vector3.up * distance) + position;
    }
    public Vector3 south
    {
        get => rotation * (Vector3.up * -distance) + position;
    }

    [System.NonSerialized]
    public Vector3 fn;
    [System.NonSerialized]
    public Vector3 fs;

    public float distance;

    [System.NonSerialized]
    public float prevMagneticForce;
    public float magneticForce;

    [System.NonSerialized]
    public Vector3 reset;
    [System.NonSerialized]
    public Vector3 position;
    [System.NonSerialized]
    public Vector3 targetPosition;
    [System.NonSerialized]
    public Quaternion rotation;
    [System.NonSerialized]
    public Quaternion targetRotation;

    [System.NonSerialized]
    public Vector3 prevPosition;
    [System.NonSerialized]
    public Quaternion prevRotation;

    public SimMagnet(float distance, float magneticForce = 1)
    {
        this.magneticForce = magneticForce;
        prevMagneticForce = this.magneticForce;
        this.distance = distance;
        position = Vector3.zero;
        rotation = Quaternion.identity;
        targetPosition = Vector3.zero;
        targetRotation = Quaternion.identity;
        prevPosition = Vector3.zero;
        prevRotation = Quaternion.identity;
        reset = Vector3.zero;

        fn = Vector3.zero;
        fs = Vector3.zero;
    }
}

public struct Particle
{
    public LineRenderer renderer;

    public Vector3 forceDir;
    public Vector3 position;

    public int parent;
    public bool reachedOrigin;
    public Vector3 end;
    public Vector3 prevEnd;

    public float dir;

    public Vector3 reset;
    public bool hasReset;
    public List<Vector3> points;
}

public class FilingSimulation
{
    public Particle[] filings;
    public SimMagnet[] magnets;

    private int numArcs;
    private int numRings;

    public FilingSimulation(SimMagnet[] magnets, int numArcs, int numRings)
    {
        for (int i = 0; i < magnets.Length; i++)
        {
            magnets[i].position = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));
            magnets[i].rotation = Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));
        }

        this.numArcs = numArcs;
        this.numRings = numRings;

        this.magnets = magnets;

        filings = new Particle[numRings * numArcs * magnets.Length * 2]; //Remove * 2 to simulate only north lines
        int idx = 0;
        for (int i = 0; i < magnets.Length; i++)
        {
            for (int j = 0; j < numRings; j++)
            {
                for (int k = 0; k < numArcs; k++)
                {
                    filings[idx] = new Particle();

                    float arcOffset = 0.01f * j;
                    Vector3 SpawnPosition = magnets[i].rotation * (magnets[i].north + Vector3.up * 0.01f + Quaternion.AngleAxis(k * 360 / numArcs, Vector3.up) * Vector3.forward * (0.01f + arcOffset));

                    filings[idx].dir = 1;
                    filings[idx].reachedOrigin = false;
                    filings[idx].parent = i;

                    filings[idx].reset = SpawnPosition;
                    filings[idx].position = SpawnPosition;

                    filings[idx].hasReset = false;
                    filings[idx].points = new List<Vector3>();

                    idx++;
                }
            }
        }
        //Remove below for loop to simulate only north lines
        for (int i = 0; i < magnets.Length; i++)
        {
            for (int j = 0; j < numRings; j++)
            {
                for (int k = 0; k < numArcs; k++)
                {
                    filings[idx] = new Particle();

                    float arcOffset = 0.01f * j;
                    Vector3 SpawnPosition = magnets[i].rotation * (magnets[i].south + Vector3.down * 0.01f + Quaternion.AngleAxis(k * 360 / numArcs, Vector3.up) * Vector3.forward * (0.01f + arcOffset));

                    filings[idx].dir = -1;
                    filings[idx].parent = i;

                    filings[idx].reset = SpawnPosition;
                    filings[idx].position = SpawnPosition;

                    filings[idx].hasReset = false;
                    filings[idx].points = new List<Vector3>();

                    idx++;
                }
            }
        }
    }

    private int iteration = 0;
    private int completed = 0;
    private bool hasSettled = false;
    private const int iterPerFrame = 200; //50 //35;
    public void Render(float step, int maxIter = 500)
    {
        if (iteration == 0)
        {
            int idx = 0;
            for (int i = 0; i < magnets.Length; i++)
            {
                ref SimMagnet m = ref magnets[i];
                for (int j = 0; j < numRings; j++)
                {
                    for (int k = 0; k < numArcs; k++)
                    {
                        //int idx = i * numRings * numArcs + j * numArcs + k;
                        ref Particle f = ref filings[idx];

                        float arcOffset = 0.01f * j;
                        Vector3 SpawnPosition = m.position + m.rotation * (Vector3.up * (m.distance + 0.01f) + Quaternion.AngleAxis(k * 360 / numArcs, Vector3.up) * Vector3.forward * (0.01f + arcOffset));

                        f.reset = SpawnPosition;
                        f.position = SpawnPosition;
                        f.reachedOrigin = false;

                        f.hasReset = false;
                        f.points.Clear();
                        f.points.Add(m.north);

                        idx++;
                    }
                }
            }
            //Remove below for loop to simulate only north lines
            for (int i = 0; i < magnets.Length; i++)
            {
                ref SimMagnet m = ref magnets[i];
                for (int j = 0; j < numRings; j++)
                {
                    for (int k = 0; k < numArcs; k++)
                    {
                        //int idx = i * numRings * numArcs + j * numArcs + k;
                        ref Particle f = ref filings[idx];

                        float arcOffset = 0.01f * j;
                        Vector3 SpawnPosition = m.position + m.rotation * (Vector3.down * (m.distance + 0.01f) + Quaternion.AngleAxis(k * 360 / numArcs, Vector3.up) * Vector3.forward * (0.01f + arcOffset));

                        f.reset = SpawnPosition;
                        f.position = SpawnPosition;

                        f.hasReset = false;
                        f.points.Clear();
                        f.points.Add(m.south);

                        idx++;
                    }
                }
            }
            completed = 0;
        }

        int subIterations = 0;
        while (subIterations < iterPerFrame && completed < filings.Length && iteration < maxIter)
        {
            Solve(step);
            Step(step, ref completed);
            iteration++;
            subIterations++;
        }

        if (completed >= filings.Length || iteration >= maxIter) // COMMENT OUT TO SHOW PARTIAL SOLUTIONS (probably make it an option to show partial solutions)
        {
            iteration = 0;

#if UNITY_EDITOR
            //Debug.Log("Completed in " + iteration + " iterations. (" + completed + " / " + filings.Length + ")");
#endif

            hasSettled = true;
            int count = numRings * numArcs * magnets.Length;
            for (int i = 0; i < count; i++)
            {
                ref Particle f = ref filings[i];

                f.prevEnd = f.end;
                f.end = f.points[f.points.Count - 2];
                if (f.prevEnd != f.end)
                    hasSettled = false;

                if (hasSettled) // COMMENT OUT IF YOU DONT WANT TO WAIT FOR SOLUTION TO SETTLE => essentially show partial / corrupted solutions (probably make this also an option)
                {
                    f.renderer.positionCount = f.points.Count - 1;
                    f.renderer.SetPositions(f.points.ToArray());

                    ref Particle fs = ref filings[i + count];

                    if (!f.reachedOrigin)
                    {
                        fs.renderer.positionCount = fs.points.Count - 1;
                        fs.renderer.SetPositions(fs.points.ToArray());
                    }
                    else
                    {
                        fs.renderer.positionCount = 0;
                    }
                }
            }
        }
    }

    public void Update(float step, int maxIter = 500, bool change = false)
    {
        for (int i = 0; i < magnets.Length; i++)
        {
            ref SimMagnet m = ref magnets[i];

            if (m.prevPosition != m.targetPosition || m.prevRotation != m.targetRotation || m.prevMagneticForce != m.magneticForce)
            {
                change = true;
            }

            if (hasSettled)
            {
                m.position = m.targetPosition;
                m.rotation = m.targetRotation;
            }

            m.prevMagneticForce = m.magneticForce;
            m.prevRotation = m.rotation;
            m.prevPosition = m.position;
        }

        if (!hasSettled || change || completed < filings.Length)
        {
            Render(step, maxIter);
        }
    }

    private void Step(float step, ref int completed)
    {
        for (int i = 0; i < filings.Length; i++)
        {
            ref Particle f = ref filings[i];
            if (f.hasReset) continue;

            float closest = -1;
            for (int j = 0; j < magnets.Length; j++)
            {
                Vector3 dest = f.dir > 0 ? magnets[j].south : magnets[j].north;
                float dist = (f.position - dest).sqrMagnitude;
                if (j == 0 || dist < closest) closest = dist;

                if (dist < 0.01f * 0.01f)
                {
                    //f.position = f.reset;
                    if (j == f.parent) f.reachedOrigin = true;
                    f.hasReset = true;
                    completed++;

                    f.points.Add(f.dir > 0 ? magnets[j].south : magnets[j].north);

                    break;
                }
            }

            f.position += f.dir * f.forceDir * Mathf.Min(step, Mathf.Sqrt(closest));
            f.points.Add(f.position);
        }
    }

    /*public void Step(float step)
    {
        for (int i = 0; i < filings.Length; i++)
        {
            ref Particle f = ref filings[i];
            f.position += f.rotation * Vector3.up * step;

            for (int j = 0; j < magnets.Length; j++)
            {
                if ((f.position - magnets[j].south).sqrMagnitude < 0.3f * 0.3f)
                {
                    f.position = f.reset;   
                }
            }
        }
    }*/

    private Vector3 CalculateForce(Vector3 m1, Vector3 m2, float f1, float f2)
    {
        Vector3 r = m2 - m1;
        float dist = r.magnitude;
        if (dist == 0) r = Vector3.up;
        const float p = 0.05f;
        float p0 = p * f1 * f2;
        float p1 = 4 * Mathf.PI * dist;
        float f = p0 / p1;
        if (dist == 0) f = 0;

        return f * r.normalized;
    }

    private void Solve(float step)
    {
        for (int i = 0; i < filings.Length; i++)
        {
            ref Particle f = ref filings[i];
            if (f.hasReset) continue;

            Vector3 forceN = Vector3.zero;

            for (int k = 0; k < magnets.Length; k++)
            {
                ref SimMagnet m = ref magnets[k];

                forceN -= CalculateForce(f.position, m.north, 1, m.magneticForce) * m.magneticForce;
                forceN += CalculateForce(f.position, m.south, 1, m.magneticForce) * m.magneticForce;
            }

            f.forceDir = forceN.normalized;
        }
    }

    /*public void Solve(int iterations, float timeStep)
    {
        const float maxForce = 100f;

        for (int i = 0; i < filings.Length; i++)
        {
            ref Particle f = ref filings[i];

            //f.rotation = Quaternion.identity;

            for (int j = 0; j < iterations; j++)
            {
                Vector3 forceN = Vector3.zero;
                Vector3 forceS = Vector3.zero;

                for (int k = 0; k < magnets.Length; k++)
                {
                    ref Particle m = ref magnets[k];

                    forceN -= CalculateForce(f.north, m.north, 1, m.magneticForce) * m.magneticForce;
                    forceN += CalculateForce(f.north, m.south, 1, m.magneticForce) * m.magneticForce;

                    forceS -= CalculateForce(f.south, m.south, 1, m.magneticForce) * m.magneticForce;
                    forceS += CalculateForce(f.south, m.north, 1, m.magneticForce) * m.magneticForce;
                }

                forceN = (forceN.sqrMagnitude > maxForce * maxForce) ? forceN.normalized * maxForce : forceN;
                forceS = (forceS.sqrMagnitude > maxForce * maxForce) ? forceS.normalized * maxForce : forceS;

                f.fn = forceN.normalized;
                f.fs = forceS.normalized;

                if (Vector3.Dot(forceN, f.south - f.north) > 0)
                {
                    Vector3 axis = new Vector3(
                        forceN.y * forceS.z - forceS.y * forceN.z,
                        - (forceN.x * forceS.z - forceS.x * forceN.z),
                        forceN.x * forceS.y - forceS.x * forceN.y
                        );

                    f.rotation *= Quaternion.AngleAxis(90, axis);
                }

                // Calculate torque
                Vector3 torque = Vector3.Cross(f.north - f.position, forceN) + Vector3.Cross(f.south - f.position, forceS);

                Debug.Log(torque);

                // Apply torque (integrate quaternion)
                Quaternion q = new Quaternion(torque.x * timeStep, torque.y * timeStep, torque.z * timeStep, 0);
                q *= f.rotation;

                f.rotation.x += q.x * 0.5f;
                f.rotation.y += q.y * 0.5f;
                f.rotation.z += q.z * 0.5f;
                f.rotation.w += q.w * 0.5f;

                f.rotation = f.rotation.normalized;
            }
        }
    }*/
}
