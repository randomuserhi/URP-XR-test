using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //GetComponent<Rigidbody>().velocity = new Vector3(1, 0 ,1);
    }

    public GameObject target;

    // Update is called once per frame
    void FixedUpdate()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.velocity -= new Vector3(0, 9, 0) * Time.fixedDeltaTime;

        Vector3 goal0 = Vector3.zero;
        Vector3 goal1 = Vector3.zero;
        Vector3 vel0 = Vector3.zero / Time.fixedDeltaTime;
        Vector3 vel1 = Vector3.zero / Time.fixedDeltaTime;

        float percentage = 0f;

        Vector3 offset = transform.position - transform.rotation * new Vector3(0f, -0.6f, 0f);
        RaycastHit contact;
        if (Physics.Raycast(new Ray(offset, transform.position - offset), out contact))
        {
            Vector3 tangent;
            Vector3 bitangent;
            contact.normal.FormOrthogonalBasis(out tangent, out bitangent);

            Vector3 velocity = rb.GetPointVelocity(contact.point);
            vel0 = (target.transform.position + target.transform.rotation * new Vector3(0f, 0.5f, 0) - contact.point) / Time.fixedDeltaTime;
            vel0 -= Vector3.Project(vel0, contact.normal);
            vel0 += Vector3.Project(velocity, contact.normal);

            Vector3 offsetPoint = contact.point + contact.normal * 0.01f;
            Debug.DrawRay(offsetPoint, Vector3.Project(velocity, tangent), Color.red);
            Debug.DrawRay(offsetPoint, Vector3.Project(velocity, bitangent), Color.red);
            Debug.DrawRay(offsetPoint, velocity - Vector3.Project(velocity, contact.normal), Color.cyan);

            Vector3 current = velocity - vel0;
            goal0 = current * percentage;
        }

        /*offset = transform.position - transform.rotation * new Vector3(1f, -0.25f, 0f);
        if (Physics.Raycast(new Ray(offset, transform.position - offset), out contact))
        {
            Vector3 tangent;
            Vector3 bitangent;
            contact.normal.FormOrthogonalBasis(out tangent, out bitangent);

            vel1 = (target.transform.position + target.transform.rotation * new Vector3(-0.5f, 0, 0) - contact.point) / Time.fixedDeltaTime;

            Vector3 velocity = rb.GetPointVelocity(contact.point);
            Vector3 offsetPoint = contact.point + contact.normal * 0.01f;
            Debug.DrawRay(offsetPoint, Vector3.Project(velocity, tangent), Color.red);
            Debug.DrawRay(offsetPoint, Vector3.Project(velocity, bitangent), Color.red);
            Debug.DrawRay(offsetPoint, velocity - Vector3.Project(velocity, contact.normal), Color.cyan);

            Vector3 current = velocity - vel1;
            goal1 = current * percentage;
        }*/

        for (int i = 0; i < 10; i++)
        {
            float damping = 1f;

            offset = transform.position - transform.rotation * new Vector3(0f, -0.6f, 0f);
            if (Physics.Raycast(new Ray(offset, transform.position - offset), out contact))
            {
                Vector3 tangent;
                Vector3 bitangent;
                contact.normal.FormOrthogonalBasis(out tangent, out bitangent);

                Vector3 velocity = rb.GetPointVelocity(contact.point);

                Vector3 current = velocity - vel0;
                Vector3 acceleration = goal0 - current;
                acceleration *= damping;

                //rb.AddForceAtPosition(contact.point, -(velocity - Vector3.Project(velocity, contact.normal)), ForceMode.VelocityChange);
                rb.velocity += acceleration;
                rb.angularVelocity += Vector3.Cross(contact.point - rb.worldCenterOfMass, acceleration);
            }

            /*offset = transform.position - transform.rotation * new Vector3(1f, -0.25f, 0f);
            if (Physics.Raycast(new Ray(offset, transform.position - offset), out contact))
            {
                Vector3 tangent;
                Vector3 bitangent;
                contact.normal.FormOrthogonalBasis(out tangent, out bitangent);

                Vector3 velocity = rb.GetPointVelocity(contact.point);

                Vector3 current = velocity - vel1;
                Vector3 acceleration = goal1 - current;

                //rb.AddForceAtPosition(contact.point, -(velocity - Vector3.Project(velocity, contact.normal)), ForceMode.VelocityChange);
                rb.velocity += acceleration *= damping;
                rb.angularVelocity += Vector3.Cross(contact.point - rb.worldCenterOfMass, acceleration);
            }*/
        }

        offset = transform.position - transform.rotation * new Vector3(0f, -0.6f, 0f);
        if (Physics.Raycast(new Ray(offset, transform.position - offset), out contact))
        {
            Vector3 tangent;
            Vector3 bitangent;
            contact.normal.FormOrthogonalBasis(out tangent, out bitangent);

            Vector3 velocity = rb.GetPointVelocity(contact.point);

            Debug.DrawRay(contact.point, Vector3.Project(velocity, tangent), Color.blue);
            Debug.DrawRay(contact.point, Vector3.Project(velocity, bitangent), Color.blue);
            Debug.DrawRay(contact.point, Vector3.Project(velocity, contact.normal), Color.blue);
            Debug.DrawRay(contact.point, velocity, Color.green);
        }

        /*offset = transform.position - transform.rotation * new Vector3(1f, -0.25f, 0f);
        if (Physics.Raycast(new Ray(offset, transform.position - offset), out contact))
        {
            Vector3 tangent;
            Vector3 bitangent;
            contact.normal.FormOrthogonalBasis(out tangent, out bitangent);

            Vector3 velocity = rb.GetPointVelocity(contact.point);

            Debug.DrawRay(contact.point, Vector3.Project(velocity, tangent), Color.blue);
            Debug.DrawRay(contact.point, Vector3.Project(velocity, bitangent), Color.blue);
            Debug.DrawRay(contact.point, velocity - Vector3.Project(velocity, contact.normal), Color.green);
        }*/

        //rb.velocity += change;
        //rb.angularVelocity += change;
    }
}
