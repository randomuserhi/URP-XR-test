using Microsoft.MixedReality.Toolkit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TMPro;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;
using UnityEngine.XR.ARFoundation;
using static Microsoft.MixedReality.Toolkit.Experimental.UI.KeyboardKeyFunc;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.Image;

//TODO:: see what distances can be converted to sqrdDistance

public struct Matrix3x3
{
    public float I00;
    public float I01;
    public float I02;
    public float I10;
    public float I11;
    public float I12;
    public float I20;
    public float I21;
    public float I22;

    public Vector3 Row0
    {
        get { return new Vector3(I00, I01, I02); }
        set { I00 = value.x; I01 = value.y; I02 = value.z; }
    }
    public Vector3 Row1
    {
        get { return new Vector3(I10, I11, I12); }
        set { I10 = value.x; I11 = value.y; I12 = value.z; }
    }
    public Vector3 Row2
    {
        get { return new Vector3(I20, I21, I22); }
        set { I20 = value.x; I21 = value.y; I22 = value.z; }
    }
    public Vector3 Col0
    {
        get { return new Vector3(I00, I10, I20); }
        set { I00 = value.x; I10 = value.y; I20 = value.z; }
    }
    public Vector3 Col1
    {
        get { return new Vector3(I01, I11, I21); }
        set { I01 = value.x; I11 = value.y; I21 = value.z; }
    }
    public Vector3 Col2
    {
        get { return new Vector3(I02, I12, I22); }
        set { I02 = value.x; I12 = value.y; I22 = value.z; }
    }

    public static Matrix3x3 FromRows(Vector3 row0, Vector3 row1, Vector3 row2)
    {
        return
          new Matrix3x3
          (
            row0.x, row0.y, row0.z,
            row1.x, row1.y, row1.z,
            row2.x, row2.y, row2.z
          );
    }

    public static Matrix3x3 FromCols(Vector3 col0, Vector3 col1, Vector3 col2)
    {
        return
          new Matrix3x3
          (
            col0.x, col1.x, col2.x,
            col0.y, col1.y, col2.y,
            col0.z, col1.z, col2.z
          );
    }

    public Matrix3x3(float i00, float i01, float i02, float i10, float i11, float i12, float i20, float i21, float i22)
    {
        I00 = i00; I01 = i01; I02 = i02;
        I10 = i10; I11 = i11; I12 = i12;
        I20 = i20; I21 = i21; I22 = i22;
    }

    public Matrix3x3 Inverted { get { return Inverse(this); } }
    public Matrix3x3 Transposed { get { return Transpose(this); } }

    private static readonly Matrix3x3 kIdentity =
      new Matrix3x3
      (
        1.0f, 0.0f, 0.0f,
        0.0f, 1.0f, 0.0f,
        0.0f, 0.0f, 1.0f
      );
    public static Matrix3x3 Identity { get { return kIdentity; } }

    // Cross(A, B) = Skew(A) B = Skew(-B) A = Cross(-B, A)
    public static Matrix3x3 Skew(Vector3 v)
    {
        return
          new Matrix3x3
          (
              0.0f, -v.z, v.y,
             v.z, 0.0f, -v.x,
            -v.y, v.x, 0.0f
          );
    }

    public static Matrix3x3 operator +(Matrix3x3 a, Matrix3x3 b)
    {
        return FromRows(a.Row0 + b.Row0, a.Row1 + b.Row1, a.Row2 + b.Row2);
    }

    public static Matrix3x3 operator -(Matrix3x3 a, Matrix3x3 b)
    {
        return FromRows(a.Row0 - b.Row0, a.Row1 - b.Row1, a.Row2 - b.Row2);
    }

    public static Matrix3x3 operator *(float s, Matrix3x3 m)
    {
        return FromRows(s * m.Row0, s * m.Row1, s * m.Row2);
    }

    public static Matrix3x3 operator *(Matrix3x3 m, float s)
    {
        return s * m;
    }

    public static Vector3 operator *(Matrix3x3 m, Vector3 v)
    {
        return
          new Vector3
          (
            Vector3.Dot(m.Row0, v),
            Vector3.Dot(m.Row1, v),
            Vector3.Dot(m.Row2, v)
          );
    }

    public static Vector3 operator *(Vector3 v, Matrix3x3 m)
    {
        return
          new Vector3
          (
            Vector3.Dot(v, m.Col0),
            Vector3.Dot(v, m.Col1),
            Vector3.Dot(v, m.Col2)
          );
    }

    public static Matrix3x3 operator *(Matrix3x3 a, Matrix3x3 b)
    {
        return
          FromCols
          (
            a * b.Col0,
            a * b.Col1,
            a * b.Col2
          );
    }

    public static float Mul(Vector3 a, Matrix3x3 m, Vector3 b)
    {
        return Vector3.Dot(a * m, b);
    }

    public static Matrix3x3 Inverse(Matrix3x3 m)
    {
        // too lazy to optimize
        // help, compiler
        float det =
            m.I00 * m.I11 * m.I22
          + m.I01 * m.I12 * m.I20
          + m.I10 * m.I21 * m.I02
          - m.I02 * m.I11 * m.I20
          - m.I01 * m.I10 * m.I22
          - m.I12 * m.I21 * m.I00;

        // I trust that this inertia tensor is well-constructed
        float detInv = 1.0f / det;

        return
          new Matrix3x3
          (
            (m.I11 * m.I22 - m.I21 * m.I12) * detInv,
            (m.I12 * m.I20 - m.I10 * m.I22) * detInv,
            (m.I10 * m.I21 - m.I20 * m.I11) * detInv,
            (m.I02 * m.I21 - m.I01 * m.I22) * detInv,
            (m.I00 * m.I22 - m.I02 * m.I20) * detInv,
            (m.I20 * m.I01 - m.I00 * m.I21) * detInv,
            (m.I01 * m.I12 - m.I02 * m.I11) * detInv,
            (m.I10 * m.I02 - m.I00 * m.I12) * detInv,
            (m.I00 * m.I11 - m.I10 * m.I01) * detInv
          );
    }

    public static Matrix3x3 Transpose(Matrix3x3 m)
    {
        return
          new Matrix3x3
          (
            m.I00, m.I10, m.I20,
            m.I01, m.I11, m.I21,
            m.I02, m.I12, m.I22
          );
    }

    public static Matrix3x3 From4x4(Matrix4x4 m)
    {
        return
            new Matrix3x3
            (
                m.m00, m.m01, m.m02,
                m.m10, m.m11, m.m12,
                m.m20, m.m21, m.m22
            );
    }
}

public static class VectorExtensions
{
    public static bool IsValidVector(this Vector3 vector)
    {
        if (!float.IsNaN(vector.x) && !float.IsNaN(vector.y) && !float.IsNaN(vector.z) && !float.IsInfinity(vector.x) && !float.IsInfinity(vector.y))
        {
            return !float.IsInfinity(vector.z);
        }

        return false;
    }

    private static float Sqrt3Inv = 1.0f / Mathf.Sqrt(3.0f);
    // Returns a vector orthogonal to given vector.
    // If the given vector is a unit vector, the returned vector will also be a unit vector.
    public static Vector3 FindOrthogonal(this Vector3 v)
    {
        if (Mathf.Abs(v.x) >= Sqrt3Inv)
            return Vector3.Normalize(new Vector3(v.y, -v.x, 0.0f));
        else
            return Vector3.Normalize(new Vector3(0.0f, v.z, -v.y));
    }

    // Yields two extra vectors that form an orthogonal basis with the given vector.
    // If the given vector is a unit vector, the returned vectors will also be unit vectors.
    public static void FormOrthogonalBasis(this Vector3 v, out Vector3 a, out Vector3 b)
    {
        a = FindOrthogonal(v);
        b = Vector3.Cross(a, v);
    }
}

public class SecondOrderSolver
{
    public float f = 8;
    public float z = 1;
    public float r = 0;

    private Vector3 xp = Vector3.zero;
    public Vector3 yd = Vector3.zero;
    public Vector3 ydd = Vector3.zero;
    private float _w, _z, _d, k1, k2, k3;

    private Vector3 xd;

    public SecondOrderSolver()
    {
        Initialize();
    }

    public void Initialize()
    {
        _w = 2 * Mathf.PI * f;
        _z = z;
        _d = _w * Mathf.Sqrt(Mathf.Abs(z * z - 1));

        k1 = z / (Mathf.PI * f);
        k2 = 1f / ((2f * Mathf.PI * f) * (2f * Mathf.PI * f));
        k3 = r * z / (2f * Mathf.PI * f);
    }

    public Vector3 SolveVel(Vector3 y, Vector3 x)
    {
        float T = Time.fixedDeltaTime;
        xd = (x - xp) / T;
        xp = x;

        float k1Stable, k2Stable;
        if (_w * T < _z) //Clamp k2 for stability and prevent jitter
        {
            k1Stable = k1;
            k2Stable = Mathf.Max(k2, T * T / 2 + T * k1 / 2, T * k1);
        }
        else
        {
            float t1 = Mathf.Exp(-_z * _w * T);
            float alpha = 2 * t1 * (_z <= 1 ? Mathf.Cos(T * _d) : math.cosh(T * _d));
            float beta = t1 * t1;
            float t2 = T / (1 + beta - alpha);
            k1Stable = (1 - beta) * t2;
            k2Stable = T * t2;
        }

        yd += T * (x + k3 * xd - y - k1 * yd) / k2Stable;
        return yd;
    }
}

//TODO:: WRITE A BETTER FUNCTION FOR GET NORMAL AT POINT AND USE THAT AFTER GETTUBG CLOSEST POINT WITH COMPUTEPENETRATION

public class TestParticle : MonoBehaviour
{
    public struct CustomLoop { }
    public struct PostCustomLoop { }

    [RuntimeInitializeOnLoadMethod]
    private static void Initialize()
    {
        Physics.queriesHitTriggers = false;

        PlayerLoopSystem def = PlayerLoop.GetDefaultPlayerLoop();
        ref PlayerLoopSystem fixedUpdateSystem = ref FindSubSystem<FixedUpdate>(ref def);
        List<PlayerLoopSystem> insertion = fixedUpdateSystem.subSystemList.ToList();
        insertion.Insert(6, new PlayerLoopSystem()
        {
            updateDelegate = () => { Custom(); },
            type = typeof(CustomLoop)
        });
        insertion.Insert(5, new PlayerLoopSystem()
        {
            updateDelegate = () => { PostCustom(); },
            type = typeof(PostCustomLoop)
        });
        fixedUpdateSystem.subSystemList = insertion.ToArray();

        PlayerLoop.SetPlayerLoop(def);

        StringBuilder sb = new StringBuilder();
        RecursivePlayerLoopPrint(def, sb, 0);
        Debug.Log(sb.ToString());
    }

    private static ref PlayerLoopSystem FindSubSystem<T>(ref PlayerLoopSystem def)
    {
        if (def.type == typeof(T))
        {
            return ref def;
        }
        if (def.subSystemList != null)
        {
            for (int i = 0; i < def.subSystemList.Length; i++)
            {
                ref PlayerLoopSystem system = ref FindSubSystem<T>(ref def.subSystemList[i]);
                if (system.type == typeof(T))
                {
                    return ref system;
                }
            }
        }
        return ref def;
    }

    private static void RecursivePlayerLoopPrint(PlayerLoopSystem def, StringBuilder sb, int depth)
    {
        if (depth == 0)
        {
            sb.AppendLine("ROOT NODE");
        }
        else if (def.type != null)
        {
            for (int i = 0; i < depth; i++)
            {
                sb.Append("\t");
            }
            sb.AppendLine(def.type.Name);
        }
        if (def.subSystemList != null)
        {
            depth++;
            foreach (var s in def.subSystemList)
            {
                RecursivePlayerLoopPrint(s, sb, depth);
            }
            depth--;
        }
    }

    private static List<TestParticle> objects = new List<TestParticle>();

    private static void Custom()
    {
        for (int i = 0; i < objects.Count; i++)
        {
            if (objects[i].enabled)
            {
                objects[i].CustomUpdate(Time.fixedDeltaTime, 1f / Time.fixedDeltaTime);
            }
        }
    }

    private static void PostCustom()
    {
        for (int i = 0; i < objects.Count; i++)
        {
            if (objects[i].enabled)
            {
                objects[i].PostCustomUpdate(Time.fixedDeltaTime, 1f / Time.fixedDeltaTime);
            }
        }
    }

    void Start()
    {
        objects.Add(this);

        prevPosition = transform.position;
    }

    private void OnDestroy()
    {
        objects.Remove(this);
    }

    public GameObject target;
    //public GameObject prev;
    //public GameObject debug;
    private SecondOrderSolver solver = new SecondOrderSolver();
    private Vector3 prevPosition;
    // Update is called once per frame
    void FixedUpdate()
    {
        //velocity.y -= 9f * Time.fixedDeltaTime;

        //velocity = solver.SolveVel(transform.position, target.transform.position);

        velocity = target.transform.position - transform.position;
        velocity /= Time.fixedDeltaTime;

        //velocity = transform.position - prevPosition;
        //prevPosition = transform.position;
    }

    public Vector3 velocity;
    private SphereCollider sphereCollider;
    private struct CollisionInfo
    {
        public ClosestPoint contact;

        public CollisionInfo(ClosestPoint closestPoint)
        {
            contact = closestPoint;
        }
    }
    private class Friction
    {
        public Jacobian normal;
        public RaycastHit contact;
        public Rigidbody rb;
        private Vector3 goal;
        private Vector3 velocity;
        public Friction(Jacobian normal, Vector3 velocity)
        {
            this.normal = normal;
            contact = normal.contact;
            rb = normal.rb;

            Vector3 currentNormalVel = Vector3.Project(rb.velocity, contact.normal);
            Vector3 targetNormalVel = Vector3.Project(velocity, contact.normal);
            velocity -= targetNormalVel;
            //velocity += currentNormalVel;
            this.velocity = velocity;
            Vector3 v = rb.GetPointVelocity(contact.point);
            Vector3 current = v - velocity;
            goal = current * 0;

            Vector3 tangent;
            Vector3 bitangent;
            contact.normal.FormOrthogonalBasis(out tangent, out bitangent);
            Vector3 offsetPoint = contact.point + contact.normal * -0.01f;
            Debug.DrawRay(offsetPoint, Vector3.Project(goal, tangent), Color.yellow);
            Debug.DrawRay(offsetPoint, Vector3.Project(goal, bitangent), Color.yellow);
            Debug.DrawRay(offsetPoint, Vector3.Project(goal, contact.normal), Color.yellow);
            Debug.DrawRay(offsetPoint, v, Color.black);
        }

        public void DebugStart()
        {
            Vector3 v = rb.GetPointVelocity(contact.point);

            Vector3 tangent;
            Vector3 bitangent;
            contact.normal.FormOrthogonalBasis(out tangent, out bitangent);
            Vector3 offsetPoint = contact.point + contact.normal * 0.01f;
            Debug.DrawRay(offsetPoint, Vector3.Project(v, tangent), Color.red);
            Debug.DrawRay(offsetPoint, Vector3.Project(v, bitangent), Color.red);
            Debug.DrawRay(offsetPoint, Vector3.Project(v, contact.normal), Color.red);
            Debug.DrawRay(offsetPoint, v, Color.cyan);
        }

        public void Solve()
        {
            Vector3 v = rb.GetPointVelocity(contact.point);
            Vector3 current = v - velocity;
            Vector3 acceleration = goal - current;

            rb.velocity += acceleration;
            rb.angularVelocity += Vector3.Cross(contact.point - rb.worldCenterOfMass, acceleration);
        }

        public void DebugRays()
        {
            Vector3 v = rb.GetPointVelocity(contact.point);
            Vector3 tangent;
            Vector3 bitangent;
            contact.normal.FormOrthogonalBasis(out tangent, out bitangent);
            Debug.DrawRay(contact.point, Vector3.Project(v, tangent), Color.green);
            Debug.DrawRay(contact.point, Vector3.Project(v, bitangent), Color.green);
            Debug.DrawRay(contact.point, Vector3.Project(v, contact.normal), Color.green);
            Debug.DrawRay(contact.point, v, Color.blue);
        }
    }
    private class Jacobian
    {
        public RaycastHit contact;
        public Rigidbody rb;
        private Vector3 v;
        private Vector3 a;
        private Matrix3x3 InverseInertiaTensorWorld;

        private float bias;
        private float effectiveMass;
        public float totalLambda;

        public Jacobian(RaycastHit contact, Rigidbody rb, Vector3 point, float dt)
        {
            this.contact = contact;
            this.rb = rb;
            v = -contact.normal;
            a = -Vector3.Cross(contact.point - rb.worldCenterOfMass, contact.normal);

            float beta = 0.1f; //magic value
            float penetration = Vector3.Dot(contact.point - point, contact.normal);
            bias = -(beta / dt) * penetration; //TODO:: implement restitution

            // Inertia Tensor Matrix can be decomposed in M = transpose(R)*D*R
            // M is the original matrix
            // R is a rotation matrix, stored in the rigidbody as a quaternion in inertiaTensorRotation
            // D is a diagonal matrix, stored in the rigidbody as a vector3 in inertiaTensor
            // D are the eigenvalues and R are the eigenvectors
            // Inertia Tensor Matrix is a 3x3 Matrix, so it will appear in the first 3x3 positions of the 4x4 Unity Matrix used here
            Matrix4x4 R = Matrix4x4.Rotate(rb.inertiaTensorRotation); //rotation matrix created
            Matrix4x4 S = Matrix4x4.Scale(rb.inertiaTensor); // diagonal matrix created
            Matrix4x4 InertiaTensorLocal4x4 = R * S * R.transpose; // R is orthogonal, so R.transpose == R.inverse
            Matrix4x4 rotationMatrix = Matrix4x4.Rotate(rb.transform.rotation);
            Matrix4x4 inertiaTensorWorld4x4 = rotationMatrix * InertiaTensorLocal4x4 * rotationMatrix.transpose;
            Matrix3x3 InertiaTensorWorld = Matrix3x3.From4x4(inertiaTensorWorld4x4);
            InverseInertiaTensorWorld = InertiaTensorWorld.Inverted;

            float k = 1f / contact.rigidbody.mass + Vector3.Dot(a, InverseInertiaTensorWorld * a);
            effectiveMass = 1.0f / k;
            totalLambda = 0.0f;
        }

        public void Resolve(float dt)
        {
            float jv = Vector3.Dot(v, rb.velocity) + Vector3.Dot(a, rb.angularVelocity);
            float lambda = effectiveMass * (-(jv + bias));

            float oldTotalLambda = totalLambda;
            totalLambda = Mathf.Max(0.0f, totalLambda + lambda);
            lambda = totalLambda - oldTotalLambda;

            float invMass = 1f / rb.mass;
            rb.velocity += invMass * v * lambda;
            rb.angularVelocity += InverseInertiaTensorWorld * a * lambda;
        }
    }
    private Dictionary<Collider, CollisionInfo> contacts = new Dictionary<Collider, CollisionInfo>();
    private List<Jacobian> jacobians = new List<Jacobian>();
    void CustomUpdate(float dt, float invDeltaTime)
    {
        const float radius = 0.05f;

        // Initialize sphere collider for bound checking in collision detection
        if (sphereCollider == null)
        {
            sphereCollider = gameObject.AddComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
            sphereCollider.radius = radius;
        }

        Vector3 step = velocity * dt;
        Vector3 position = transform.position;
        prevPosition = position;

        //debug.transform.position = unobstructedPosition;

        // Handle being in a collision
        Collider[] colliders;

        CollisionResolution resolution;
        if (SolveCollision(position, step, sphereCollider, out resolution))
        {
            position = resolution.point;

            colliders = resolution.contacts.Keys.ToArray();
            for (int i = 0; i < colliders.Length; i++)
            {
                Collider collider = colliders[i];
                if (!contacts.ContainsKey(collider))
                {
                    contacts.Add(collider, resolution.contacts[collider]);
                }
            }
        }
        else
        {
            //Debug.Log("No Collisions");
        }

        //prev.transform.position = position;

        // Handle continuous collision - TODO:: MAKE WORK
        /*colliders = contacts.Keys.ToArray();
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider collider = colliders[i];
            CollisionInfo collision = contacts[collider];

            Vector3 newSurfacePoint = collider.transform.TransformPoint(collision.contact.localPoint);
            Vector3 newSurfaceNormal = collider.transform.TransformDirection(collision.contact.localNormal);
            Vector3 oldVel = newSurfacePoint - collision.oldContactPoint;
            Vector3 seperation = step - oldVel;
            //Debug.DrawRay(collision.oldContactPoint, Vector3.back, Color.white);
            //Debug.DrawRay(collision.oldContactPoint, oldVel, Color.green);
            //Debug.DrawRay(collision.oldContactPoint, step, Color.red);
            //Debug.DrawRay(collision.oldContactPoint, seperation, Color.black);
            //Debug.DrawRay(collision.oldContactPoint, collision.contact.info.normal, Color.blue);
            //Debug.DrawRay(newSurfacePoint, newSurfaceNormal, Color.magenta);

            Debug.DrawRay(collision.oldContactPoint, seperation, Color.black);
            Debug.DrawRay(newSurfacePoint, newSurfaceNormal, Color.magenta);

            //Debug.Log("moving towards each other: " + (Vector3.Dot(seperation, newSurfaceNormal) < 0));
            bool movingTowards = Vector3.Dot(oldVel, collision.contact.info.normal) > 0;
            if (movingTowards)
            {
                //Debug.Log("collider into point: " + (Vector3.Dot(oldVel, newSurfaceNormal) > 0));
                if (Vector3.Dot(oldVel, newSurfaceNormal) > 0) // Other into point
                {
                    position = newSurfacePoint;

                    ClosestPoint temp;
                    if (TargetedRayCast(new Ray(position, -newSurfaceNormal), sphereCollider.radius, sphereCollider, collider, out temp))
                    {
                        contacts[collider] = new CollisionInfo(temp);
                    }
                    else Debug.Log("Shouldn't happen");
                }
                //else if (TargetedRayCast(new Ray(collision.oldContactPoint, step), step.magnitude, sphereCollider, collider, out temp)) // point into other
                //{
                //    position = temp.info.point;
                //
                //    contacts[collider] = new CollisionInfo(temp);
                //}
            }
            //else contacts.Remove(collider);
            
            if (!movingTowards && (resolution.count == 0 || !resolution.contacts.ContainsKey(collider))) contacts.Remove(collider);
        }*/

        if (resolution.count > 0)
        {
            // Solve velocity of visual point
            colliders = resolution.contacts.Keys.ToArray();
            for (int i = 0; i < colliders.Length; i++)
            {
                Collider collider = colliders[i];
                CollisionInfo collision = resolution.contacts[collider];
                RaycastHit contact = collision.contact.info;

                if (Vector3.Dot(step, contact.normal) < 0)
                {
                    Vector3 normalVel = Vector3.Project(step, contact.normal);
                    step -= normalVel;
                }
            }
        }

        velocity = step * invDeltaTime;

        // Move point according to physics
        ClosestPoint hit;
        if (resolution.count != 0)
        {
            colliders = resolution.contacts.Keys.ToArray();
            if (RayCast(new Ray(position, step), step.magnitude, sphereCollider, colliders, out hit))
                position = hit.info.point;
            else
                position += step;
        }
        else
        {
            if (RayCast(new Ray(position, step), step.magnitude, sphereCollider, out hit))
                position = hit.info.point;
            else
                position += step;
        }

        //debug.transform.position = position;
        transform.position = position;

        // Physics
        colliders = Physics.OverlapSphere(transform.position, sphereCollider.radius);
        jacobians.Clear();
        for (int i = 0; i < colliders.Length; i++)
        {
            ClosestPoint point;
            if (GetClosestPoint(transform.position, sphereCollider, colliders[i], out point))
            {
                RaycastHit contact = point.info;

                if (contact.rigidbody != null)
                {
                    Vector3 target = this.target.transform.position; //Change target at somepoint, shouldnt directly reference target gameobject
                    jacobians.Add(new Jacobian(contact, contact.rigidbody, target, dt));
                }
            }
        }

        // Solve impulses sequentially
        for (int iter = 0; iter < 2; iter++)
        {
            for (int i = 0; i < jacobians.Count; i++)
            {
                jacobians[i].Resolve(dt);
            }
        }
    }

    private void PostCustomUpdate(float dt, float invDeltaTime)
    {
        // Apply friction
        Friction[] frictions = new Friction[jacobians.Count];
        Vector3 vel = (transform.position - prevPosition) / dt; // Change this later to not reference target gameobject
        for (int i = 0; i < jacobians.Count; i++)
        {
            frictions[i] = new Friction(jacobians[i], vel);
            frictions[i].DebugStart();
        }
        for (int iter = 0; iter < 10; iter++)
        {
            for (int i = 0; i < frictions.Length; i++)
            {
                frictions[i].Solve();
            }
        }
        for (int i = 0; i < frictions.Length; i++)
        {
            frictions[i].DebugRays();
        }
    }

    private struct CollisionResolution
    {
        public Vector3 point;
        public Vector3 normal;
        public int count;
        public Dictionary<Collider, CollisionInfo> contacts;
    }
    private static void SolveCollisionRecursive(List<CollisionResolution> branches, Vector3 point, Vector3 combinedNormals, HashSet<Collider> visited, SphereCollider sphereCollider, int depth = 0)
    {
        //Debug.Log("Solve : " + depth);
        Collider[] colliders = Physics.OverlapSphere(point, sphereCollider.radius);
        Dictionary<Collider, CollisionInfo> contacts = new Dictionary<Collider, CollisionInfo>();
        bool end = true;
        //Debug.Log("Colliders : " + colliders.Length);
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider collider = colliders[i];
            ClosestPoint closest;
            if (GetClosestPoint(point, sphereCollider, collider, out closest))
            {
                //Debug.Log("Hit : " + collider.name);

                CollisionInfo c = new CollisionInfo(closest);
                if (!contacts.ContainsKey(collider))
                {
                    contacts.Add(collider, c);
                }

                if (!visited.Contains(collider))
                {
                    end = false;

                    HashSet<Collider> v = new HashSet<Collider>(visited);
                    v.Add(collider);
                    SolveCollisionRecursive(branches, closest.info.point, combinedNormals + closest.info.normal, v, sphereCollider, depth + 1);
                }
            }
        }
        if (depth != 0 && end)
        {
            bool inside = false;
            Vector3 normal = combinedNormals.normalized;
            //Debug.DrawRay(point, normal * 4f, Color.gray);
            //Debug.DrawRay(point + normal * sphereCollider.radius, Vector3.right, Color.black);
            for (int i = 0; i < colliders.Length; i++) // THIS LOOP CAN BE MOVED INTO THE ABOVE ONE
            {
                Collider collider = colliders[i];
                // Offset the point off the surface so that its not inside of it to verify if it truly is inside
                // (point is just on the border, so would count as being inside)
                inside = GetClosestPoint(point + normal * sphereCollider.radius, sphereCollider, collider, out _); 
                if (inside) break;
            }
            if (!inside)
            {
                branches.Add(new CollisionResolution()
                {
                    point = point,
                    normal = normal,
                    contacts = contacts,
                    count = contacts.Count
                });
            }
            //else Debug.Log("Invalid point");
        }
    }
    private static bool SolveCollision(Vector3 point, Vector3 velocity, SphereCollider sphereCollider, out CollisionResolution resolution)
    {
        List<CollisionResolution> branches = new List<CollisionResolution>();
        SolveCollisionRecursive(branches, point, Vector3.zero, new HashSet<Collider>(), sphereCollider);
        if (branches.Count == 0)
        {
            resolution = new CollisionResolution() { point = point, normal = Vector3.zero, count = 0 };
            return false;
        }
        resolution = branches.OrderByDescending((v) => { return Vector3.Distance(v.point, point); }).First();
        return true;
    }

    private static bool IsPointWithin(Collider c, Vector3 point, bool useRigidbody = false)
    {
        Vector3 closest = c.ClosestPoint(point);
        Vector3 origin = c.transform.position + (c.transform.rotation * c.bounds.center);
        Vector3 originToContact = closest - origin;
        Vector3 pointToContact = closest - point;

        Rigidbody r = c.attachedRigidbody;
        if (useRigidbody && (r != null))
        {
            // The benefit of this is the use of the center of mass for a more accurate physics origin; we multiply by rotation to convert it from it's local-space to a world offset.
            originToContact = closest - (r.position + (r.rotation * r.centerOfMass));
        }

        return (Vector3.Angle(originToContact, pointToContact) < 90);
    }

    private static bool RayCast(Ray ray, float maxDistance, SphereCollider sphereCollider, Collider[] ignore, out ClosestPoint closestPoint)
    {
        closestPoint = new ClosestPoint();
        float closestDistance = float.PositiveInfinity;

        // Already inside
        Collider[] colliders = Physics.OverlapSphere(ray.origin, sphereCollider.radius);
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider collider = colliders[i];
            if (ignore.Contains(collider)) continue;
            ClosestPoint closest;
            if (TargetedRayCast(ray, maxDistance, sphereCollider, collider, out closest))
            {
                float dist = Vector3.Distance(closest.info.point, ray.origin);
                if (dist < closestDistance)
                {
                    closestPoint = closest;
                    closestDistance = dist;
                }
            }
        }
        if (closestDistance != float.PositiveInfinity) return true;

        // Going to be inside
        RaycastHit[] hits;
        hits = Physics.RaycastAll(ray, maxDistance);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].distance < closestDistance)
            {
                closestPoint = new ClosestPoint(hits[i], false);
                closestDistance = hits[i].distance;
            }
        }

        return hits.Length > 0;
    }
    private static bool RayCast(Ray ray, float maxDistance, SphereCollider sphereCollider, out ClosestPoint closestPoint)
    {
        closestPoint = new ClosestPoint();
        float closestDistance = float.PositiveInfinity;

        // Already inside
        Collider[] colliders = Physics.OverlapSphere(ray.origin, sphereCollider.radius);
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider collider = colliders[i];
            ClosestPoint closest;
            if (TargetedRayCast(ray, maxDistance, sphereCollider, collider, out closest))
            {
                float dist = Vector3.Distance(closest.info.point, ray.origin);
                if (dist < closestDistance)
                {
                    closestPoint = closest;
                    closestDistance = dist;
                }
            }
        }
        if (closestDistance != float.PositiveInfinity) return true;

        // Going to be inside
        RaycastHit[] hits;
        hits = Physics.RaycastAll(ray, maxDistance);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].distance < closestDistance)
            {
                closestPoint = new ClosestPoint(hits[i], false);
                closestDistance = hits[i].distance;
            }
        }

        return hits.Length > 0;
    }
    private static bool TargetedRayCast(Ray ray, float maxDistance, SphereCollider sphereCollider, Collider collider, out ClosestPoint closestPoint)
    {
        RaycastHit[] hits;

        // Already inside
        // Add a very small offset to prevent Physics.ComputePenetration from making
        // penDir NaN as if the point results in a penDist of 0, penDir will be NaN
        // NOTE :: the offset must be away from the surface, if it is parallel to the surface
        // the offset won't affect Physics.ComputePenetration at all, thats why we check various offsets
        Vector3[] offsets = new Vector3[] { Vector3.up, Vector3.left, Vector3.forward };
        for (int o = 0; o < offsets.Length; o++)
        {
            Vector3 penDir;
            float penDist;
            Vector3 penOrigin = ray.origin + offsets[o] * epsilon * 100f;
            if (Physics.ComputePenetration(sphereCollider, penOrigin, Quaternion.identity, collider, collider.transform.position, collider.transform.rotation, out penDir, out penDist))
            {
                Vector3 point = penOrigin + penDir * penDist;
                hits = Physics.RaycastAll(new Ray(point, -penDir), sphereCollider.radius * 2f);
                for (int i = 0; i < hits.Length; i++)
                {
                    Vector3 difference = ZeroRound(ray.origin - hits[i].point);
                    if (hits[i].collider == collider && Vector3.Dot(difference, hits[i].normal) <= 0)
                    {
                        closestPoint = new ClosestPoint(hits[i], true);
                        return true;
                    }
                }
            }
        }

        // Going to be inside
        hits = Physics.RaycastAll(ray, maxDistance);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider == collider)
            {
                closestPoint = new ClosestPoint(hits[i], false);
                return true;
            }
        }

        closestPoint = new ClosestPoint();
        return false;
    }
    private static bool TargetedRayCast(Ray ray, SphereCollider sphereCollider, Collider collider, out ClosestPoint closestPoint)
    {
        RaycastHit[] hits;

        // Already inside
        // Add a very small offset to prevent Physics.ComputePenetration from making
        // penDir NaN as if the point results in a penDist of 0, penDir will be NaN
        // NOTE :: the offset must be away from the surface, if it is parallel to the surface
        // the offset won't affect Physics.ComputePenetration at all, thats why we check various offsets
        Vector3[] offsets = new Vector3[] { Vector3.up, Vector3.left, Vector3.forward };
        for (int o = 0; o < offsets.Length; o++)
        {
            Vector3 penDir;
            float penDist;
            Vector3 penOrigin = ray.origin + offsets[o] * epsilon * 100f;
            if (Physics.ComputePenetration(sphereCollider, penOrigin, Quaternion.identity, collider, collider.transform.position, collider.transform.rotation, out penDir, out penDist))
            {
                Vector3 point = penOrigin + penDir * penDist;
                hits = Physics.RaycastAll(new Ray(point, -penDir), sphereCollider.radius * 2f);
                for (int i = 0; i < hits.Length; i++)
                {
                    Vector3 difference = ZeroRound(ray.origin - hits[i].point);
                    if (hits[i].collider == collider && Vector3.Dot(difference, hits[i].normal) <= 0)
                    {
                        closestPoint = new ClosestPoint(hits[i], true);
                        return true;
                    }
                }
            }
        }

        // Going to be inside
        hits = Physics.RaycastAll(ray);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider == collider)
            {
                closestPoint = new ClosestPoint(hits[i], true);
                return true;
            }
        }

        closestPoint = new ClosestPoint();
        return false;
    }

    private static float _epsilon = float.NaN;
    private static float epsilon { get
        {
            if (float.IsNaN(_epsilon))
            {
                _epsilon = 1.0f;
                float x = 0.0f;
                do
                {
                    _epsilon /= 2.0f;
                    x = 1.0f + _epsilon;
                }
                while (x > 1.0f);
            }
            return _epsilon;
        }
    }
    private static Vector3 ZeroRound(Vector3 v)
    {
        return new Vector3(
            Mathf.Abs(v.x * v.x) <= epsilon ? 0 : v.x,
            Mathf.Abs(v.y * v.y) <= epsilon ? 0 : v.y,
            Mathf.Abs(v.z * v.z) <= epsilon ? 0 : v.z);
    }
    private static Vector3 RealZeroRound(Vector3 v)
    {
        return new Vector3(
            Mathf.Abs(v.x) <= epsilon ? 0 : v.x,
            Mathf.Abs(v.y) <= epsilon ? 0 : v.y,
            Mathf.Abs(v.z) <= epsilon ? 0 : v.z);
    }

    private enum ClosestPointQuery
    {
        All,
        Inside,
        Outside
    }
    private struct ClosestPoint
    {
        public bool inside;
        public RaycastHit info;
        public Vector3 localPoint;
        public Vector3 localNormal;

        public ClosestPoint(RaycastHit hit, bool inside)
        {
            this.inside = inside;
            info = hit;
            localPoint = hit.collider.transform.InverseTransformPoint(hit.point);
            localNormal = hit.collider.transform.InverseTransformDirection(hit.normal);
        }
    }
    private static bool GetClosestPoint(Vector3 origin, SphereCollider sphereCollider, RaycastHit[] colliders, out ClosestPoint closestPoint, ClosestPointQuery query = ClosestPointQuery.Inside)
    {
        ClosestPoint closest = new ClosestPoint();
        float closestDist = float.PositiveInfinity;
        for (int i = 0; i < colliders.Length; i++)
        {
            ClosestPoint point;
            if (GetClosestPoint(origin, sphereCollider, colliders[i].collider, out point, query))
            {
                float distance = Vector3.Distance(point.info.point, origin);
                if (distance < closestDist)
                {
                    closest = point;
                    closestDist = distance;
                }
            }
        }
        closestPoint = closest;

        if (closestDist != float.PositiveInfinity) return true;
        else return false;
    }
    private static bool GetClosestPoint(Vector3 origin, SphereCollider sphereCollider, Collider[] colliders, out ClosestPoint closestPoint, ClosestPointQuery query = ClosestPointQuery.Inside)
    {
        ClosestPoint closest = new ClosestPoint();
        float closestDist = float.PositiveInfinity;
        for (int i = 0; i < colliders.Length; i++)
        {
            ClosestPoint point;
            if (GetClosestPoint(origin, sphereCollider, colliders[i], out point, query))
            {
                float distance = Vector3.Distance(point.info.point, origin);
                if (distance < closestDist)
                {
                    closest = point;
                    closestDist = distance;
                }
            }
        }
        closestPoint = closest;

        if (closestDist != float.PositiveInfinity) return true;
        else return false;
    }
    private static bool GetClosestPoint(Vector3 origin, SphereCollider sphereCollider, Collider collider, out ClosestPoint closestPoint, ClosestPointQuery query = ClosestPointQuery.Inside)
    {
        if (query == ClosestPointQuery.All || query == ClosestPointQuery.Outside)
        {
            if (collider is MeshCollider && !((MeshCollider)collider).convex)
            {
                Debug.LogWarning("MeshCollider cannot be non-convex...");
                // NOT SUPPORTED YET
            }
            Vector3 nearpoint = Physics.ClosestPoint(origin, collider, collider.transform.position, collider.transform.rotation);
            if (nearpoint != origin)
            {
                Vector3 dir = nearpoint - origin;
                RaycastHit[] hits = Physics.RaycastAll(new Ray(origin, dir), dir.magnitude * 2);
                for (int i = 0; i < hits.Length; i++)
                {
                    if (hits[i].collider == collider)
                    {
                        closestPoint = new ClosestPoint(hits[i], false);
                        return true;
                    }
                }
            }
        }

        // Add a very small offset to prevent Physics.ComputePenetration from making
        // penDir NaN as if the point results in a penDist of 0, penDir will be NaN
        // NOTE :: the offset must be away from the surface, if it is parallel to the surface
        // the offset won't affect Physics.ComputePenetration at all, thats why we check various offsets
        Vector3[] offsets = new Vector3[] { Vector3.up, Vector3.left, Vector3.forward };
        for (int o = 0; o < offsets.Length; o++)
        {
            //Debug.Log("-- Attempt " + o + " --");

            Vector3 penDir;
            float penDist;
            Vector3 penOrigin = origin + offsets[o] * epsilon * 100f;
            /*Debug.Log("origin: " + origin.x + ", " + origin.y + ", " + origin.z);
            Debug.DrawRay(origin, Vector3.forward, Color.magenta);
            Debug.Log("penOrigin: " + penOrigin.x + ", " + penOrigin.y + ", " + penOrigin.z);
            Debug.DrawRay(penOrigin, Vector3.back, Color.black);*/

            if (Physics.ComputePenetration(sphereCollider, penOrigin, Quaternion.identity, collider, collider.transform.position, collider.transform.rotation, out penDir, out penDist))
            {
                /*Debug.Log("penDir: " + penDir.x + ", " + penDir.y + ", " + penDir.z);
                Debug.Log("penDist: " + penDist);*/

                Vector3 point = penOrigin + penDir * penDist;
                /*Debug.DrawRay(point, penDir.normalized, Color.green);
                Debug.Log("point: " + point.x + ", " + point.y + ", " + point.z);
                Debug.DrawRay(point, Vector3.back, Color.red);*/
                RaycastHit[] hits = Physics.RaycastAll(new Ray(point, -penDir), sphereCollider.radius * 2f);
                //Debug.Log("hits: " + hits.Length);
                for (int i = 0; i < hits.Length; i++)
                {
                    Vector3 difference = ZeroRound(origin - hits[i].point);
                    if (hits[i].collider == collider)
                    {
                        if ((query == ClosestPointQuery.All || query == ClosestPointQuery.Inside) && Vector3.Dot(difference, hits[i].normal) <= 0)
                        {
                            closestPoint = new ClosestPoint(hits[i], true);
                            return true;
                        }
                        else if ((query == ClosestPointQuery.All || query == ClosestPointQuery.Outside) && Vector3.Dot(difference, hits[i].normal) > 0)
                        {
                            closestPoint = new ClosestPoint(hits[i], false);
                            return true;
                        }
                    }
                    //else Debug.Log("Invalid point");
                }
            }
        }

        closestPoint = new ClosestPoint();
        return false;
    }

    /*private static bool PointInCollider(Vector3 point, Collider collider,)
    {

    }*/

    /*private struct State
    {
        public Vector3 point;
        public Vector3 direction;
    }
    private State prevState;
    void CustomUpdate()
    {
        prev.transform.position = transform.position;

        // Collision check

        // TODO:: cleanup code for collision checking
        // TODO:: continuous collision and handle planes (really thin boxes) where compute penetration doesnt work

        HashSet<Collider> collidersHandled = new HashSet<Collider>();
        List<RaycastHit> collisions = new List<RaycastHit>();
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider collider = colliders[i];
            if (collider == sphereCollider) continue;
            Vector3 penDir;
            float penDist;
            if (Physics.ComputePenetration(sphereCollider, transform.position, transform.rotation, collider, collider.transform.position, collider.transform.rotation, out penDir, out penDist))
            {
                Vector3 point = transform.position + penDir * penDist;
                State state = new State()
                {
                    point = point,
                    direction = transform.position - point
                };
                if (state.point.IsValidVector())
                    prevState = state;

                debug.transform.position = prevState.point;
                RaycastHit[] all = Physics.RaycastAll(new Ray(prevState.point - prevState.direction, prevState.direction));
                for (int j = 0; j < all.Length; j++)
                {
                    RaycastHit hit = all[j];
                    Vector3 normal = Vector3.Project(velocity, hit.normal);
                    Vector3 ovelocity = hit.rigidbody ? hit.rigidbody.GetPointVelocity(hit.point) : Vector3.zero;
                    Vector3 onormal = Vector3.Project(ovelocity, hit.normal);
                    float seperation = Mathf.Sign(Vector3.Dot(hit.normal, velocity)) * normal.magnitude - Mathf.Sign(Vector3.Dot(hit.normal, ovelocity)) * onormal.magnitude;
                    if (hit.collider == collider && (transform.position - hit.point).sqrMagnitude < velocity.sqrMagnitude && seperation < 0)
                    {
                        if (!collidersHandled.Contains(collider))
                        {
                            collidersHandled.Add(collider);
                            collisions.Add(hit);
                        }
                        
                        break;
                    }
                }
            }
        }
        //RaycastHit[] hits = Physics.RaycastAll(new Ray(transform.position - velocity.normalized * radius, velocity));
        //for ()

        for (int i = 0; i < collisions.Count; i++)
        {
            RaycastHit hit = collisions[i];

            velocity -= Vector3.Project(velocity, hit.normal);
            transform.position = hit.point;
        }

        transform.position += velocity * Time.fixedDeltaTime;
    }*/
}
