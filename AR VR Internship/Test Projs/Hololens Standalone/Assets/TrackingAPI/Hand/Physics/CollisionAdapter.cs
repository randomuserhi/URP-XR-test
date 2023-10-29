using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

using System.Text;

public class CollisionAdapter : MonoBehaviour
{
    /*public struct CustomLoop { }

    [RuntimeInitializeOnLoadMethod]
    private static void Initialize()
    {
        Physics.queriesHitTriggers = false;

        PlayerLoopSystem def = PlayerLoop.GetDefaultPlayerLoop();
        ref PlayerLoopSystem fixedUpdateSystem = ref FindSubSystem<FixedUpdate>(ref def);
        List<PlayerLoopSystem> insertion = fixedUpdateSystem.subSystemList.ToList();
        insertion.Insert(5, new PlayerLoopSystem()
        {
            updateDelegate = () => { Custom(); },
            type = typeof(CustomLoop)
        });
        insertion.Insert(12, new PlayerLoopSystem()
        {
            updateDelegate = () => { Custom(false); },
            type = typeof(CustomLoop)
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

    private static List<CollisionAdapter> objects = new List<CollisionAdapter>();

    //TODO:: CLEANUP A LOT OF DUPLICATE CODE HERE
    private static void Custom(bool applyForce = true)
    {
        for (int i = 0; i < objects.Count; i++)
        {
            RaycastHit hit;

            CollisionAdapter ca = objects[i];
            if (ca == null) return;

            BindInfo[] c = ca.colliders.Values.ToArray();
            Debug.Log(c.Length);
            for (int j = 0; j < c.Length; j++)
            {
                BindInfo b = c[j];

                if (b.rb) b.velocity = b.rb.velocity;
                else
                {
                    b.velocity = b.transform.position - b.prevPosition;
                    b.prevPosition = b.transform.position;
                }

                if (b.rb) b.angularVelocity = b.rb.angularVelocity;

                Vector3 penetrationDir;
                float penetrationDist;
                if (Physics.ComputePenetration(
                    ca.c, ca.transform.position, ca.transform.rotation,
                    b.c, b.c.transform.position, b.c.transform.rotation,
                    out penetrationDir, out penetrationDist))
                {
                    ca.transform.position += penetrationDir * (penetrationDist);
                    b.penetrationDir = -penetrationDir;
                }

                if (b.penetrationDir != Vector3.zero && Physics.Raycast(ca.transform.position, b.penetrationDir, out hit))
                {
                    if (hit.collider == b.c)
                    {
                        b.point = hit.point;
                        b.normal = hit.normal;
                        if (Physics.Raycast(ca.transform.position, -b.normal, out hit))
                        {
                            b.contactDist = hit.distance;
                        }
                    }
                }

                if (b.normal != Vector3.zero && Physics.Raycast(ca.transform.position, -b.normal, out hit))
                {
                    if (hit.collider == b.c)
                    {
                        b.normal = hit.normal;
                        b.point = hit.point;
                    }

                    if (hit.distance > b.contactDist)
                    {
                        ca.colliders.Remove(b.c);
                    }
                }
                else ca.colliders.Remove(b.c);

                Vector3 velocityAtPoint = b.velocity + b.angularVelocity * (b.point - b.c.transform.position).magnitude;

                Vector3 projection = Vector3.Project(ca.velocity, b.normal);

                float selfDot = Vector3.Dot(ca.velocity, b.normal);
                float otherDot = Vector3.Dot(velocityAtPoint, b.normal);

                float selfDir = selfDot < 0 ? -1 : 1;
                float otherDir = otherDot < 0 ? -1 : 1;

                Vector3 other = Vector3.Project(velocityAtPoint, b.normal);

                float seperation = projection.magnitude * selfDir - other.magnitude * otherDir;
                if ((selfDir < 0 && otherDir < 0) || seperation > 0)
                {
                    ca.colliders.Remove(b.c);
                }

                if (selfDir < 0) ca.velocity -= projection;

                //dunno if acceleration forcemode is better than velocitychange
                if (b.rb && applyForce)
                {
                    b.rb.AddForceAtPosition(projection, b.point, ForceMode.VelocityChange);

                    //Grip force
                    if (seperation < 0)
                    {
                        b.rb.AddForceAtPosition(ca.velocity, b.point, ForceMode.VelocityChange);
                    }
                }
            }

            if (ca.velocity != Vector3.zero && Physics.Raycast(ca.transform.position, ca.velocity, out hit, ca.velocity.magnitude * Time.fixedDeltaTime))
            {
                if (ca.c != hit.collider)
                {
                    ca.transform.position = hit.point;
                    Collider other = hit.collider;
                    BindInfo b;
                    if (!ca.colliders.ContainsKey(other))
                    {
                        b = new BindInfo()
                        {
                            c = other,
                            rb = other.attachedRigidbody,
                            transform = other.transform,
                            prevPosition = other.transform.position,
                            prevRotation = other.transform.rotation,
                            velocity = other.attachedRigidbody ? other.attachedRigidbody.velocity : Vector3.zero,
                            angularVelocity = other.attachedRigidbody ? other.attachedRigidbody.angularVelocity : Vector3.zero
                        };
                        ca.colliders.Add(other, b);
                    }
                    else b = ca.colliders[other];

                    Vector3 penetrationDir;
                    float penetrationDist;
                    if (Physics.ComputePenetration(
                        ca.c, ca.transform.position, ca.transform.rotation,
                        b.c, b.c.transform.position, b.c.transform.rotation,
                        out penetrationDir, out penetrationDist))
                    {
                        ca.transform.position += penetrationDir * (penetrationDist);
                        b.penetrationDir = -penetrationDir;
                    }

                    if (b.penetrationDir != Vector3.zero && Physics.Raycast(ca.transform.position, b.penetrationDir, out hit))
                    {
                        if (hit.collider == b.c)
                        {
                            b.normal = hit.normal;
                            b.point = hit.point;
                            if (Physics.Raycast(ca.transform.position, -b.normal, out hit))
                            {
                                b.contactDist = hit.distance;
                            }
                        }
                    }
                }
            }
            else if (applyForce)
            {
                ca.transform.position += ca.velocity * Time.fixedDeltaTime;
            }
        }
    }*/

    /*
     * Algorithm involves:
     * 1) on collision, bind to surface. {Complete}
     * 2) only allow movement parallel to that surface. {Complete}
     * 3) if another surface is run into, use that surface. {Complete}
     * 4) unbind from surface once there is no force opposite to surface normal.
     */

    public Collider c;
    public Vector3 velocity = Vector3.zero;

    public class BindInfo
    {
        public Vector3 point;
        public Vector3 normal;
        public Vector3 penetrationDir;
        public float contactDist;

        public Collider c { get; set; }
        public Rigidbody rb { get; set; }
        public Vector3 velocity { get; set; }
        public Vector3 angularVelocity { get; set; }
        public Transform transform { get; set; }
        public Vector3 prevPosition { get; set; }
        public Quaternion prevRotation { get; set; }
    }
    public Dictionary<Collider, BindInfo> colliders = new Dictionary<Collider, BindInfo>();

    private void Start()
    {
        c = GetComponent<Collider>();
        //objects.Add(this);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!colliders.ContainsKey(other))
        {
            BindInfo b = new BindInfo()
            {
                c = other,
                rb = other.attachedRigidbody,
                transform = other.transform,
                prevPosition = other.transform.position,
                prevRotation = other.transform.rotation,
                velocity = other.attachedRigidbody ? other.attachedRigidbody.velocity : Vector3.zero,
                angularVelocity = other.attachedRigidbody ? other.attachedRigidbody.angularVelocity : Vector3.zero
            };
            colliders.Add(other, b);
        }
    }

        //DEPRECATED:
        /*private struct CollisionPoint
        {
            public Vector3 point { get; set; }
            public Vector3 normal { get; set; }
            public Collider collider { get; set; }
        }

        private class subscription
        {
            public Rigidbody rb;

            public Vector3 penetrationDir;

            public Vector3 velocity;
            public Vector3 angularVelocity;

            public Vector3 position;
            public Vector3 prevPosition;

            public Vector3 rotation;
            public Vector3 prevRotation;
        }

        private Collider c;

        private Dictionary<Collider, subscription> others = new Dictionary<Collider, subscription>();
        private List<CollisionPoint> contactPoints = new List<CollisionPoint>();

        public Vector3 velocity = Vector3.zero;

        // Start is called before the first frame update
        private void Start()
        {
            c = GetComponent<Collider>();
            c.isTrigger = true;
        }

        private void SolvePenetration(subscription s, Collider o)
        {
            Vector3 penetrationDir;
            float penetrationDist;
            if (Physics.ComputePenetration(
                c, transform.position, transform.rotation,
                o, o.transform.position, o.transform.rotation,
                out penetrationDir, out penetrationDist))
            {
                transform.position += penetrationDir * (penetrationDist);
                s.penetrationDir = -penetrationDir;
            }
        }

        private void FixedUpdate()
        {
            Collider[] others = this.others.Keys.ToArray();
            for (int i = 0; i < others.Length; i++)
            {
                Collider o = others[i];

                if (!this.others.ContainsKey(o)) continue;

                subscription s = this.others[o];

                s.velocity = s.position - s.prevPosition;
                s.prevPosition = s.position;

                //TODO:: update s.angularVelocity, s.rotation, s.prevRotation properly
                if (s.rb) s.angularVelocity = s.rb.angularVelocity;

                SolvePenetration(s, o);

                RaycastHit hit;
                if (s.penetrationDir != Vector3.zero && Physics.Raycast(transform.position, s.penetrationDir, out hit))
                {
                    CollisionPoint point = new CollisionPoint()
                    {
                        point = hit.point,
                        normal = hit.normal,
                        collider = hit.collider
                    };
                    contactPoints.Add(point);
                }

                for (int j = 0; j < contactPoints.Count; j++)
                {
                    CollisionPoint cp = contactPoints[j];

                    Vector3 velocityAtPoint = s.velocity + s.angularVelocity * (cp.point - cp.collider.transform.position).magnitude;

                    Vector3 projection = Vector3.Project(velocity, cp.normal);

                    float selfDot = Vector3.Dot(velocity, cp.normal);
                    float otherDot = Vector3.Dot(velocityAtPoint, cp.normal);

                    float selfDir = selfDot < 0 ? -1 : 1;
                    float otherDir = otherDot <= 0 ? -1 : 1;

                    Vector3 other = Vector3.Project(velocityAtPoint, cp.normal);

                    float seperation = projection.magnitude * selfDir - other.magnitude * otherDir;
                    float movement = projection.magnitude * selfDir + other.magnitude * otherDir;

                    if (selfDot < 0)
                    {
                        if (seperation < 0)
                        {
                            if (selfDir != otherDir && movement < 0 && Physics.Raycast(transform.position, velocity, out hit, velocity.magnitude) && hit.collider == cp.collider)
                            {
                                transform.position = hit.point;
                                SolvePenetration(s, o);
                                Debug.Log(hit.normal);
                                velocity -= Vector3.Project(velocity, hit.normal);
                            }
                            else
                            {
                                SolvePenetration(s, o);
                                if (Physics.Raycast(transform.position, s.penetrationDir, out hit, velocity.magnitude) && hit.collider == cp.collider)
                                {
                                    Debug.Log(hit.normal);
                                    velocity -= Vector3.Project(velocity, hit.normal);
                                }
                            }
                        }
                        else
                        {
                            this.others.Remove(cp.collider);
                        }
                    }
                    else
                    {
                        if (seperation < 0)
                        {
                            SolvePenetration(s, o);
                            if (Physics.Raycast(transform.position, s.penetrationDir, out hit) && hit.collider == cp.collider)
                            {
                                Debug.Log(hit.normal);
                                velocity -= Vector3.Project(velocity, hit.normal);
                            }
                        }
                        else
                        {
                            this.others.Remove(cp.collider);
                        }
                    }
                }
            }
            contactPoints.Clear();

            transform.position += velocity * Time.fixedDeltaTime;
        }

        private void OnTriggerStay(Collider other)
        {
            if (!others.ContainsKey(other))
                others.Add(other, new subscription()
                {
                    rb = other.attachedRigidbody,
                    velocity = Vector3.zero,
                    position = other.transform.position,
                    prevPosition = other.transform.position,
                    rotation = other.transform.position,
                    prevRotation = other.transform.position
                });
        }*/
    }
