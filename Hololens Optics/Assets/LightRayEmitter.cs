using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LightTK;
using static LightTK.LTK;

public class LightRayEmitter : MonoBehaviour
{
    public static LTKCollider[] colliders;
    public LightRay[] rays;
    private LineRenderer[] renderers;

    private Spawner spawner;

    [SerializeField]
    public GameObject rayPrefab;
    void Start()
    {
        colliders = FindObjectsOfType<LTKCollider>();
        rays = new LightRay[10];
        renderers = new LineRenderer[rays.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i] = Instantiate(rayPrefab).GetComponent<LineRenderer>();
        }
        spawner = transform.parent.GetComponentInParent<Spawner>();
    }

    void FixedUpdate()
    {
        if (!spawner.enabled)
            for (int j = 0; j < rays.Length; j++)
            {
                LightRay ray = new LightRay();
                rays[j] = ray;
                ray.position = transform.position + Quaternion.AngleAxis(360 * j / rays.Length, transform.forward) * Vector3.up * 0.01f;
                ray.direction = -transform.forward;

                int i = 1;
                renderers[j].positionCount = i;
                renderers[j].SetPosition(0, ray.position);
                for (int z = 0; z < 100 && SimulateRay(ray, colliders); z++, i++)
                {
                    renderers[j].positionCount = i + 1;
                    renderers[j].SetPosition(i, ray.position);
                }
                renderers[j].positionCount = i + 1;
                renderers[j].SetPosition(i, ray.position + ray.direction * 100f);
            }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null) Destroy(renderers[i].gameObject);
        }
    }
}
