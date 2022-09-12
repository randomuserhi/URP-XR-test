using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTrack : MonoBehaviour
{
    Rigidbody rb;
    public GameObject target;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.velocity = (target.transform.position - transform.position) / Time.fixedDeltaTime;
        rb.maxAngularVelocity = float.MaxValue;

        Quaternion rotation = target.transform.rotation * Quaternion.Inverse(transform.rotation);
        Vector3 rot;
        float speed;
        rotation.ToAngleAxis(out speed, out rot);
        rb.angularVelocity = rot * speed * Mathf.Deg2Rad / Time.fixedDeltaTime;
    }
}
