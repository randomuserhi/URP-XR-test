using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigJointTest : MonoBehaviour
{
    ConfigurableJoint joint;
    Rigidbody rb;
    public Rigidbody B;
    public Rigidbody C;
    public Rigidbody D;

    public GameObject target;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        joint = GetComponent<ConfigurableJoint>();
    }

    public float sensitivity = 1;
    public float damping = 1;

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.mass = 100;
        rb.velocity = (target.transform.position - transform.position) / Time.fixedDeltaTime;
        rb.maxAngularVelocity = float.MaxValue;
        B.maxAngularVelocity = float.MaxValue;
        C.maxAngularVelocity = float.MaxValue;
        D.maxAngularVelocity = float.MaxValue;
        Quaternion rotation = target.transform.rotation * Quaternion.Inverse(transform.rotation);
        Vector3 rot;
        float speed;
        rotation.ToAngleAxis(out speed, out rot);
        rb.angularVelocity = rot * speed * Mathf.Deg2Rad / Time.fixedDeltaTime;
    }
}
