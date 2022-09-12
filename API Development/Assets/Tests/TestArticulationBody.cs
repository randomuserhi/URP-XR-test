using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestArticulationBody : MonoBehaviour
{
    public ArticulationBody A;
    public ArticulationBody B;
    public ArticulationBody C;
    public ArticulationBody D;

    // Start is called before the first frame update
    void Start()
    {
        A.solverIterations = 10;
        A.solverVelocityIterations = 10;

        B.zDrive = new ArticulationDrive()
        {
            lowerLimit = -90,
            upperLimit = 90,
            stiffness = 100000,
            damping = 10000,
            forceLimit = 5000,
            target = -45
        };

        C.zDrive = new ArticulationDrive()
        {
            lowerLimit = -90,
            upperLimit = 90,
            stiffness = 100000,
            damping = 10000,
            forceLimit = 5000,
            target = -90
        };

        D.zDrive = new ArticulationDrive()
        {
            lowerLimit = -90,
            upperLimit = 90,
            stiffness = 100000,
            damping = 10000,
            forceLimit = 5000,
            target = -90
        };
    }

    public GameObject target;

    // Update is called once per frame
    void FixedUpdate()
    {
        A.AddForce(-Physics.gravity * A.mass);
        B.AddForce(-Physics.gravity * B.mass);
        C.AddForce(-Physics.gravity * C.mass);
        D.AddForce(-Physics.gravity * D.mass);

        float strength = 5;

        A.velocity *= 0.05f;
        Vector3 delta = (target.transform.position - A.transform.position);
        Vector3 force = Vector3.ClampMagnitude((((delta / Time.fixedDeltaTime) / Time.fixedDeltaTime) * A.mass) * (1f - 0.05f), 1000f * strength);
        A.AddForce(force);
        Quaternion rotation = target.transform.rotation * Quaternion.Inverse(A.transform.rotation);
        Vector3 angularVelocity = Vector3.ClampMagnitude((new Vector3(
          Mathf.DeltaAngle(0, rotation.eulerAngles.x),
          Mathf.DeltaAngle(0, rotation.eulerAngles.y),
          Mathf.DeltaAngle(0, rotation.eulerAngles.z)) / Time.fixedDeltaTime) * Mathf.Deg2Rad, 45f);
        A.angularVelocity = angularVelocity;
        A.angularDamping = 50f;
    }
}
