using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FilingManager : MonoBehaviour
{
    public static List<Magnet> magnets = new List<Magnet>();
    public Vector3Int filingCount = new Vector3Int(8, 8, 8);
    public GameObject filingPrefab;
    public static GameObject[] filings;

    public static float scale = 0.125f;

    public GameObject followTarget;

    void Start()
    {
        filings = new GameObject[filingCount.x * filingCount.y * filingCount.z];
        //filings = new GameObject[filingCount.x, filingCount.y, filingCount.z];
        for (int i = 0; i < filingCount.z; i++)
        {
            for (int j = 0; j < filingCount.y; j++)
            {
                for (int k = 0; k < filingCount.x; k++)
                {
                    Vector3 pos = new Vector3((k - filingCount.x / 2) * scale, (j - filingCount.y / 2) * scale, (i - filingCount.z / 2) * scale);
                    filings[i * filingCount.x * filingCount.y + j * filingCount.x + k] = Instantiate(filingPrefab, pos, Quaternion.identity, transform);
                    //filings[i,j,k] = Instantiate(filingPrefab, new Vector3((k - filingCount.x / 2) * scale, (j - filingCount.y / 2) * scale, (i - filingCount.z / 2) * scale), Quaternion.identity, transform);
                    //filings[i * filingCount.x * filingCount.y + j * filingCount.x + k].GetComponent<Filing>().magnet = magnet;
                    Filing f = filings[i * filingCount.x * filingCount.y + j * filingCount.x + k].GetComponent<Filing>();
                    f.relPos = pos;
                    f.parent = followTarget;
                }
            }
        }
        
    }


    //private void FixedUpdate()
    //{
    //    int mCount = magnets.Count;
    //    for (int i = 0; i < mCount; i++)
    //    {
    //        Magnet m1 = magnets[i];
    //        if (m1.rb == null)
    //            continue;

    //        Rigidbody rb = m1.rb;
    //        Vector3 f1 = Vector3.zero;
    //        Vector3 f2 = Vector3.zero;

    //        for (int j = 0; j < mCount; j++)
    //        {
    //            if (i == j)
    //                continue;

    //            Magnet m2 = magnets[j];

    //            if (m1.magnetForce < 0.5f || m2.magnetForce < 5.0f)
    //                continue;

    //            if (m1.transform.parent == m2.transform.parent)
    //                continue;

    //            Vector3 f = CalculateForce(m1, m2);

    //            f1 += f * m1.magnetForce * m2.magnetForce;
    //        }

    //        f1  = (f1.magnitude > maxF)?f1.normalized * maxF : f1;

    //        rb.AddForceAtPosition(f1, m1.transform.position);
    //    }
    //}
}
