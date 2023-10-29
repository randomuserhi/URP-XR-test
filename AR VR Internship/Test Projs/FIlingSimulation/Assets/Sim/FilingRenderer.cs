using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FilingRenderer : MonoBehaviour
{
    FilingSimulation sim;

    [Range(1, 500)]
    public int maxIter = 500;

    [Range(0.001f, 0.1f)]
    public float step = 0.001f;

    public GameObject magnet;
    public GameObject particle;

    GameObject[] filings;
    GameObject[] magnets;

    [SerializeField]
    public SimMagnet[] simMagnets;

    // Start is called before the first frame update
    void Start()
    {
        sim = new FilingSimulation(simMagnets, 5, 2);

        filings = new GameObject[sim.filings.Length];

        magnets = new GameObject[sim.magnets.Length];

        for (int i = 0; i < sim.magnets.Length; i++)
        {
            magnets[i] = Instantiate(magnet);
            magnets[i].transform.position = sim.magnets[i].position;
            magnets[i].transform.rotation = sim.magnets[i].rotation;
        }

        for (int i = 0; i < sim.filings.Length; i++)
        {
            filings[i] = Instantiate(particle);
            sim.filings[i].renderer = filings[i].GetComponent<LineRenderer>();
        }

        sim.Render(step, maxIter);
        m_step = step;
        m_maxIter = maxIter;
    }

    private float m_step;
    private float m_maxIter;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (m_step != step || m_maxIter != maxIter)
        {
            sim.Update(step, maxIter, true);
        }
        else
        {
            sim.Update(step, maxIter, false);
        }

        //sim.Render(0.1f, maxIter);

        /*for (int i = 0; i < sim.filings.Length; i++)
        {
            filings[i].transform.position = sim.filings[i].position;
        }*/

        for (int i = 0; i < sim.magnets.Length; i++)
        {
            sim.magnets[i].targetPosition = magnets[i].transform.position;
            sim.magnets[i].targetRotation = magnets[i].transform.rotation;
        }
    }
}
