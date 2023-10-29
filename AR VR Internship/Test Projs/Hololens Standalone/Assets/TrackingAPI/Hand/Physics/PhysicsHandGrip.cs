using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

//TODO:: IMPLEMENT SLIP AND FRICTION (move anchor along surface based on velocity)

public class PhysicsHandGrip : MonoBehaviour
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
        insertion.Insert(5, new PlayerLoopSystem()
        {
            updateDelegate = Grip,
            type = typeof(CustomLoop)
        });
        insertion.Insert(12, new PlayerLoopSystem()
        {
            updateDelegate = PostGrip,
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

    private static float stickyTimer = 0f;
    struct jointInfo
    {
        public Vector3 velocity;
        public float timeTillBreak;
        public bool broken;
    }

    private static List<PhysicsHandGrip> objects = new List<PhysicsHandGrip>();
    private static void Grip()
    {
        if (objects == null) return;
        for (int q = 0; q < objects.Count; q++)
        {
            PhysicsHandGrip o = objects[q];
            if (o == null) return;

            List<ConfigurableJoint> joints = o.velocityInfo.Keys.ToList();
            for (int i = joints.Count; i < o.contacts.Count; i++)
            {
                ContactPoint p = o.contacts[i];
                Rigidbody b = o.bodies[i];
                if (b != null)
                {
                    Vector3 r = b.velocity - o.rb.velocity;
                    if (Vector3.Dot(r, p.normal) > 0)
                    {
                        ConfigurableJoint j = o.gameObject.AddComponent<ConfigurableJoint>();
                        j.xMotion = ConfigurableJointMotion.Free;
                        j.yMotion = ConfigurableJointMotion.Free;
                        j.zMotion = ConfigurableJointMotion.Free;
                        j.autoConfigureConnectedAnchor = false;

                        j.connectedBody = b;
                        j.anchor = o.transform.InverseTransformPoint(p.point);
                        j.connectedAnchor = b.transform.InverseTransformPoint(p.point);
                        j.breakForce = 1f;

                        joints.Add(j);
                    }
                }
            }

            for (int i = 0; i < joints.Count; i++)
            {
                ConfigurableJoint j = joints[i];
                Vector3 vel = j.GetComponent<Rigidbody>().velocity;
                if (!o.velocityInfo.ContainsKey(j)) o.velocityInfo.Add(j, new jointInfo() { velocity = vel, timeTillBreak = stickyTimer, broken = false });
                else
                {
                    jointInfo info = o.velocityInfo[j];
                    info.velocity = vel;
                    o.velocityInfo[j] = info;
                }
            }

            o.contacts.Clear();
            o.bodies.Clear();
        }
    }

    private static void PostGrip()
    {
        if (objects == null) return;
        for (int q = 0; q < objects.Count; q++)
        {
            PhysicsHandGrip o = objects[q];
            if (o == null) return;

            ConfigurableJoint[] joints = o.velocityInfo.Keys.ToArray();
            for (int i = 0; i < joints.Length; i++)
            {
                ConfigurableJoint j = joints[i];
                if (j == null)
                {
                    o.velocityInfo.Remove(j);
                    continue;
                }
                jointInfo info = o.velocityInfo[j];

                Rigidbody b = j.connectedBody;
                Vector3 origin = j.transform.position;
                Vector3 point = Physics.ClosestPoint(origin, b.GetComponent<Collider>(), b.transform.position, b.transform.rotation);
                if (Physics.Raycast(new Ray(origin, point - origin), out RaycastHit hit, float.PositiveInfinity, LayerMask.GetMask("Default")))
                {
                    Vector3 r = Vector3.Project(b.velocity, hit.normal) - Vector3.Project(info.velocity, hit.normal);

                    if (Vector3.Dot(r, hit.normal) < 0)
                    {
                        if (!o.velocityInfo[j].broken) info.broken = true;
                        else
                        {
                            info.timeTillBreak -= Time.fixedDeltaTime;
                        }
                    }
                    else
                    {
                        info.timeTillBreak = stickyTimer;
                        info.broken = false;
                    }
                }
                if (info.timeTillBreak <= 0)
                {
                    j.xMotion = ConfigurableJointMotion.Locked;
                    j.yMotion = ConfigurableJointMotion.Locked;
                    j.zMotion = ConfigurableJointMotion.Locked;
                    j.breakForce = 0;
                    o.velocityInfo.Remove(j);
                }
                else
                {
                    j.xMotion = ConfigurableJointMotion.Locked;
                    j.yMotion = ConfigurableJointMotion.Locked;
                    j.zMotion = ConfigurableJointMotion.Locked;
                    o.velocityInfo[j] = info;
                }
            }
        }
    }

    private Rigidbody rb;
    private Vector3 velocity;
    private ContactPoint[] contactPoints;
    private List<ContactPoint> contacts = new List<ContactPoint>();
    private List<Rigidbody> bodies = new List<Rigidbody>();
    private Dictionary<ConfigurableJoint, jointInfo> velocityInfo = new Dictionary<ConfigurableJoint, jointInfo>();

    private void Start()
    {
        objects.Add(this);
        rb = GetComponent<Rigidbody>();
    }

    private void Round(ref Vector3 a, float threshold = 0.001f)
    {
        a.x = Mathf.Sign(a.x) * (threshold > Mathf.Abs(a.x) ? 0 : Mathf.Abs(a.x));
        a.y = Mathf.Sign(a.y) * (threshold > Mathf.Abs(a.y) ? 0 : Mathf.Abs(a.y));
        a.z = Mathf.Sign(a.z) * (threshold > Mathf.Abs(a.z) ? 0 : Mathf.Abs(a.z));
    }

    private void FixedUpdate()
    {
        velocity = rb.velocity;
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < contacts.Count; i++)
        {
            Gizmos.DrawSphere(contacts[i].point, 0.01f);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!collision.rigidbody) return;
        if (collision.gameObject.GetComponent<PhysicsHandGrip>() != null) return;
        int contactPointCount = collision.contactCount;
        if (contactPoints == null || contactPoints.Length < contactPointCount)
        {
            contactPoints = new ContactPoint[contactPointCount];
            
        }
        collision.GetContacts(contactPoints);

        for (int i = 0; i < contactPointCount; i++)
        {
            contacts.Add(contactPoints[i]);
            bodies.Add(collision.rigidbody);
        }
    }
}
