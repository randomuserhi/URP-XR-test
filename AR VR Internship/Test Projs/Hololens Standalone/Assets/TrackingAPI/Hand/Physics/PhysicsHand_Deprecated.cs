using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO:: REWRITE a lot of this to make it smarter. For example using an array instead of structures for t, p, s, c, rb and j.
//       The same access can still be achieved using an enum for joint names.
//       Arrays will also allow for loops for better code.
public class PhysicsHand_Deprecated : MonoBehaviour
{
    [System.Serializable]
    public struct Trackers
    {
        // wrist
        public Transform wrist;

        // palm
        public Transform thumbMetacarpalJoint;
        public Transform indexMetacarpal;
        public Transform middleMetacarpal;
        public Transform ringMetacarpal;
        public Transform pinkyMetacarpal;

        public Transform thumbProximalJoint;
        public Transform indexKnuckle;
        public Transform middleKnuckle;
        public Transform ringKnuckle;
        public Transform pinkyKnuckle;

        // fingers
        public Transform thumbTip;
        public Transform thumbDistalJoint;
        
        public Transform indexTip;
        public Transform indexDistalJoint;
        public Transform indexMiddleJoint;

        public Transform middleTip;
        public Transform middleDistalJoint;
        public Transform middleMiddleJoint;

        public Transform ringTip;
        public Transform ringDistalJoint;
        public Transform ringMiddleJoint;

        public Transform pinkyTip;
        public Transform pinkyDistalJoint;
        public Transform pinkyMiddleJoint;
    }
    public struct Solvers
    {
        // wrist
        public SecondOrderSolver wrist;

        // palm
        public SecondOrderSolver thumbMetacarpalJoint;
        public SecondOrderSolver indexMetacarpal;
        public SecondOrderSolver middleMetacarpal;
        public SecondOrderSolver ringMetacarpal;
        public SecondOrderSolver pinkyMetacarpal;

        public SecondOrderSolver thumbProximalJoint;
        public SecondOrderSolver indexKnuckle;
        public SecondOrderSolver middleKnuckle;
        public SecondOrderSolver ringKnuckle;
        public SecondOrderSolver pinkyKnuckle;

        // fingers
        public SecondOrderSolver thumbTip;
        public SecondOrderSolver thumbDistalJoint;

        public SecondOrderSolver indexTip;
        public SecondOrderSolver indexDistalJoint;
        public SecondOrderSolver indexMiddleJoint;

        public SecondOrderSolver middleTip;
        public SecondOrderSolver middleDistalJoint;
        public SecondOrderSolver middleMiddleJoint;

        public SecondOrderSolver ringTip;
        public SecondOrderSolver ringDistalJoint;
        public SecondOrderSolver ringMiddleJoint;

        public SecondOrderSolver pinkyTip;
        public SecondOrderSolver pinkyDistalJoint;
        public SecondOrderSolver pinkyMiddleJoint;
    }
    public struct Points
    {
        // wrist
        public Rigidbody wrist;

        // palm
        public Rigidbody thumbMetacarpalJoint;
        public Rigidbody indexMetacarpal;
        public Rigidbody middleMetacarpal;
        public Rigidbody ringMetacarpal;
        public Rigidbody pinkyMetacarpal;

        public Rigidbody thumbProximalJoint;
        public Rigidbody indexKnuckle;
        public Rigidbody middleKnuckle;
        public Rigidbody ringKnuckle;
        public Rigidbody pinkyKnuckle;

        // fingers
        public Rigidbody thumbTip;
        public Rigidbody thumbDistalJoint;

        public Rigidbody indexTip;
        public Rigidbody indexDistalJoint;
        public Rigidbody indexMiddleJoint;

        public Rigidbody middleTip;
        public Rigidbody middleDistalJoint;
        public Rigidbody middleMiddleJoint;

        public Rigidbody ringTip;
        public Rigidbody ringDistalJoint;
        public Rigidbody ringMiddleJoint;

        public Rigidbody pinkyTip;
        public Rigidbody pinkyDistalJoint;
        public Rigidbody pinkyMiddleJoint;
    }
    private struct Colliders
    {
        // wrist
        public CapsuleCollider wrist;

        // palm
        public CapsuleCollider[] thumbMetacarpal_ProximalJoint; //3
        public CapsuleCollider[] indexMetacarpal_Knuckle;
        public CapsuleCollider[] middleMetacarpal_Knuckle; 
        public CapsuleCollider[] ringMetacarpal_Knuckle; 
        public CapsuleCollider pinkyMetacarpal_Knuckle;

        // fingers
        public CapsuleCollider thumbDistal_Tip;
        public CapsuleCollider thumbProximal_DistalJoint;

        public CapsuleCollider indexDistal_Tip;
        public CapsuleCollider indexMiddle_DistalJoint;
        public CapsuleCollider indexKnuckle_MiddleJoint;

        public CapsuleCollider middleDistal_Tip;
        public CapsuleCollider middleMiddle_DistalJoint;
        public CapsuleCollider middleKnuckle_MiddleJoint;

        public CapsuleCollider ringDistal_Tip;
        public CapsuleCollider ringMiddle_DistalJoint;
        public CapsuleCollider ringKnuckle_MiddleJoint;

        public CapsuleCollider pinkyDistal_Tip;
        public CapsuleCollider pinkyMiddle_DistalJoint;
        public CapsuleCollider pinkyKnuckle_MiddleJoint;
    }
    private struct Bodies
    {
        // wrist
        public Rigidbody wrist;

        // palm
        public Rigidbody[] thumbMetacarpal_ProximalJoint; //3
        public Rigidbody[] indexMetacarpal_Knuckle; 
        public Rigidbody[] middleMetacarpal_Knuckle;
        public Rigidbody[] ringMetacarpal_Knuckle; 
        public Rigidbody pinkyMetacarpal_Knuckle;

        // fingers
        public Rigidbody thumbDistal_Tip;
        public Rigidbody thumbProximal_DistalJoint;

        public Rigidbody indexDistal_Tip;
        public Rigidbody indexMiddle_DistalJoint;
        public Rigidbody indexKnuckle_MiddleJoint;

        public Rigidbody middleDistal_Tip;
        public Rigidbody middleMiddle_DistalJoint;
        public Rigidbody middleKnuckle_MiddleJoint;

        public Rigidbody ringDistal_Tip;
        public Rigidbody ringMiddle_DistalJoint;
        public Rigidbody ringKnuckle_MiddleJoint;

        public Rigidbody pinkyDistal_Tip;
        public Rigidbody pinkyMiddle_DistalJoint;
        public Rigidbody pinkyKnuckle_MiddleJoint;
    }
    private struct Joints
    {
        // palm
        public ConfigurableJoint[] thumbMetacarpal_ProximalJoint_Knuckle; // 3
        public ConfigurableJoint[] indexMetacarpal_Knuckle_Knuckle;
        public ConfigurableJoint[] middleMetacarpal_Knuckle_Knuckle;
        public ConfigurableJoint[] ringMetacarpal_Knuckle_Knuckle;
        public ConfigurableJoint[] _thumbMetacarpal_ProximalJoint_Knuckle; // 3
        public ConfigurableJoint[] _indexMetacarpal_Knuckle_Knuckle; 
        public ConfigurableJoint[] _middleMetacarpal_Knuckle_Knuckle; 
        public ConfigurableJoint[] _ringMetacarpal_Knuckle_Knuckle; 

        // fingers
        public ConfigurableJoint thumbDistal_Tip;
        public ConfigurableJoint thumbProximal_DistalJoint;

        public ConfigurableJoint indexDistal_Tip;
        public ConfigurableJoint indexMiddle_DistalJoint;
        public ConfigurableJoint indexKnuckle_MiddleJoint;

        public ConfigurableJoint middleDistal_Tip;
        public ConfigurableJoint middleMiddle_DistalJoint;
        public ConfigurableJoint middleKnuckle_MiddleJoint;

        public ConfigurableJoint ringDistal_Tip;
        public ConfigurableJoint ringMiddle_DistalJoint;
        public ConfigurableJoint ringKnuckle_MiddleJoint;

        public ConfigurableJoint pinkyDistal_Tip;
        public ConfigurableJoint pinkyMiddle_DistalJoint;
        public ConfigurableJoint pinkyKnuckle_MiddleJoint;
    }

    public Trackers t;
    private Points p;
    private Solvers s;
    private Colliders c;
    private Bodies rb;
    private Joints j;

    // Creates a capsule collider along y-axis
    private (CapsuleCollider, Rigidbody, Rigidbody, Rigidbody) CreateCollider(float length = 0.06f, float radius = 0.005f, bool endCaps = true)
    {
        GameObject o = new GameObject();
        o.transform.parent = transform;

        CapsuleCollider c = o.AddComponent<CapsuleCollider>();
        c.height = length;
        c.radius = radius;

        Rigidbody rigidbody = o.AddComponent<Rigidbody>();
        rigidbody.mass = 0;
        rigidbody.useGravity = false;

        if (endCaps)
        {
            GameObject o1 = new GameObject();
            o1.transform.parent = transform;
            Rigidbody p1 = o1.AddComponent<Rigidbody>();
            p1.mass = 0;
            p1.useGravity = false;
            CreateJoint(o1, rigidbody, Vector3.zero, new Vector3(0, -length / 2f + radius, 0));

            GameObject o2 = new GameObject();
            o2.transform.parent = transform;
            Rigidbody p2 = o2.AddComponent<Rigidbody>();
            p2.mass = 0;
            p2.useGravity = false;
            CreateJoint(o2, rigidbody, Vector3.zero, new Vector3(0, length / 2f - radius, 0));

            return (c, rigidbody, p1, p2);
        }
        else return (c, rigidbody, null, null);
    }

    private ConfigurableJoint CreateJoint(GameObject obj, Rigidbody other, Vector3 anchor0, Vector3 anchor1,
        ConfigurableJointMotion rotX = ConfigurableJointMotion.Free,
        ConfigurableJointMotion rotY = ConfigurableJointMotion.Free,
        ConfigurableJointMotion rotZ = ConfigurableJointMotion.Free)
    {
        ConfigurableJoint j = obj.AddComponent<ConfigurableJoint>();
        j.connectedBody = other;
        j.autoConfigureConnectedAnchor = false;
        j.anchor = anchor0;
        j.connectedAnchor = anchor1;

        j.xMotion = ConfigurableJointMotion.Locked;
        j.yMotion = ConfigurableJointMotion.Locked;
        j.zMotion = ConfigurableJointMotion.Locked;

        j.angularXMotion = rotX;
        j.angularYMotion = rotY;
        j.angularZMotion = rotZ;

        j.enableCollision = false;
        return j;
    }

    private void Start()
    {
        s.thumbMetacarpalJoint = new SecondOrderSolver();
        s.indexMetacarpal = new SecondOrderSolver();
        s.middleMetacarpal = new SecondOrderSolver();
        s.ringMetacarpal = new SecondOrderSolver();
        s.pinkyMetacarpal = new SecondOrderSolver();
        s.thumbProximalJoint = new SecondOrderSolver();
        s.indexKnuckle = new SecondOrderSolver();
        s.middleKnuckle = new SecondOrderSolver();
        s.ringKnuckle = new SecondOrderSolver();
        s.pinkyKnuckle = new SecondOrderSolver();
        s.thumbTip = new SecondOrderSolver();
        s.thumbDistalJoint = new SecondOrderSolver();
        s.indexTip = new SecondOrderSolver();
        s.indexDistalJoint = new SecondOrderSolver();
        s.indexMiddleJoint = new SecondOrderSolver();
        s.middleTip = new SecondOrderSolver();
        s.middleDistalJoint = new SecondOrderSolver();
        s.middleMiddleJoint = new SecondOrderSolver();
        s.ringTip = new SecondOrderSolver();
        s.ringDistalJoint = new SecondOrderSolver();
        s.ringMiddleJoint = new SecondOrderSolver();
        s.pinkyTip = new SecondOrderSolver();
        s.pinkyDistalJoint = new SecondOrderSolver();
        s.pinkyMiddleJoint = new SecondOrderSolver();

        c.thumbMetacarpal_ProximalJoint = new CapsuleCollider[3];
        rb.thumbMetacarpal_ProximalJoint = new Rigidbody[3];
        j.thumbMetacarpal_ProximalJoint_Knuckle = new ConfigurableJoint[3];
        j._thumbMetacarpal_ProximalJoint_Knuckle = new ConfigurableJoint[3];

        (CapsuleCollider, Rigidbody, Rigidbody, Rigidbody) tuple = CreateCollider();
        c.thumbMetacarpal_ProximalJoint[0] = tuple.Item1;
        rb.thumbMetacarpal_ProximalJoint[0] = tuple.Item2;
        p.thumbMetacarpalJoint = tuple.Item3;
        p.thumbProximalJoint = tuple.Item4;

        for (int i = 1; i < 3; i++)
        {
            tuple = CreateCollider(0.05f, endCaps: false);
            c.thumbMetacarpal_ProximalJoint[i] = tuple.Item1;
            rb.thumbMetacarpal_ProximalJoint[i] = tuple.Item2;

            j.thumbMetacarpal_ProximalJoint_Knuckle[i - 1] =
                CreateJoint(c.thumbMetacarpal_ProximalJoint[i - 1].gameObject, rb.thumbMetacarpal_ProximalJoint[i],
                            new Vector3(0.005f, 0.025f), new Vector3(-0.005f, 0.025f));
            j._thumbMetacarpal_ProximalJoint_Knuckle[i - 1] =
                CreateJoint(c.thumbMetacarpal_ProximalJoint[i - 1].gameObject, rb.thumbMetacarpal_ProximalJoint[i],
                            new Vector3(0.001f, -0.025f), new Vector3(-0.001f, -0.025f));
        }

        c.indexMetacarpal_Knuckle = new CapsuleCollider[1];
        rb.indexMetacarpal_Knuckle = new Rigidbody[1];
        j.indexMetacarpal_Knuckle_Knuckle = new ConfigurableJoint[1];
        j._indexMetacarpal_Knuckle_Knuckle = new ConfigurableJoint[1];

        tuple = CreateCollider();
        c.indexMetacarpal_Knuckle[0] = tuple.Item1;
        rb.indexMetacarpal_Knuckle[0] = tuple.Item2;
        p.indexMetacarpal = tuple.Item3;
        p.indexKnuckle = tuple.Item4;

        j.thumbMetacarpal_ProximalJoint_Knuckle[2] =
                CreateJoint(c.thumbMetacarpal_ProximalJoint[2].gameObject, rb.indexMetacarpal_Knuckle[0],
                            new Vector3(0.005f, 0.025f), new Vector3(-0.005f, 0.025f));
        j._thumbMetacarpal_ProximalJoint_Knuckle[2] =
            CreateJoint(c.thumbMetacarpal_ProximalJoint[2].gameObject, rb.indexMetacarpal_Knuckle[0],
                        new Vector3(0.001f, -0.025f), new Vector3(-0.001f, -0.025f));

        c.middleMetacarpal_Knuckle = new CapsuleCollider[1];
        rb.middleMetacarpal_Knuckle = new Rigidbody[1];
        j.middleMetacarpal_Knuckle_Knuckle = new ConfigurableJoint[1];
        j._middleMetacarpal_Knuckle_Knuckle = new ConfigurableJoint[1];

        tuple = CreateCollider();
        c.middleMetacarpal_Knuckle[0] = tuple.Item1;
        rb.middleMetacarpal_Knuckle[0] = tuple.Item2;
        p.middleMetacarpal = tuple.Item3;
        p.middleKnuckle = tuple.Item4;

        j.indexMetacarpal_Knuckle_Knuckle[0] =
                CreateJoint(c.indexMetacarpal_Knuckle[0].gameObject, rb.middleMetacarpal_Knuckle[0],
                            new Vector3(0.005f, 0.025f), new Vector3(-0.005f, 0.025f),
                            ConfigurableJointMotion.Locked,
                            ConfigurableJointMotion.Locked,
                            ConfigurableJointMotion.Free);
        j._indexMetacarpal_Knuckle_Knuckle[0] =
            CreateJoint(c.indexMetacarpal_Knuckle[0].gameObject, rb.middleMetacarpal_Knuckle[0],
                        new Vector3(0.003f, -0.025f), new Vector3(-0.003f, -0.025f),
                        ConfigurableJointMotion.Locked,
                        ConfigurableJointMotion.Locked,
                        ConfigurableJointMotion.Free);

        c.ringMetacarpal_Knuckle = new CapsuleCollider[1];
        rb.ringMetacarpal_Knuckle = new Rigidbody[1];
        j.ringMetacarpal_Knuckle_Knuckle = new ConfigurableJoint[1];
        j._ringMetacarpal_Knuckle_Knuckle = new ConfigurableJoint[1];

        tuple = CreateCollider();
        c.ringMetacarpal_Knuckle[0] = tuple.Item1;
        rb.ringMetacarpal_Knuckle[0] = tuple.Item2;
        p.ringMetacarpal = tuple.Item3;
        p.ringKnuckle = tuple.Item4;

        j.middleMetacarpal_Knuckle_Knuckle[0] =
                CreateJoint(c.middleMetacarpal_Knuckle[0].gameObject, rb.ringMetacarpal_Knuckle[0],
                            new Vector3(0.005f, 0.025f), new Vector3(-0.005f, 0.025f),
                            ConfigurableJointMotion.Locked,
                            ConfigurableJointMotion.Locked,
                            ConfigurableJointMotion.Free);
        j._middleMetacarpal_Knuckle_Knuckle[0] =
            CreateJoint(c.middleMetacarpal_Knuckle[0].gameObject, rb.ringMetacarpal_Knuckle[0],
                        new Vector3(0.003f, -0.025f), new Vector3(-0.003f, -0.025f),
                        ConfigurableJointMotion.Locked,
                        ConfigurableJointMotion.Locked,
                        ConfigurableJointMotion.Free);

        tuple = CreateCollider();
        c.pinkyMetacarpal_Knuckle = tuple.Item1;
        rb.pinkyMetacarpal_Knuckle = tuple.Item2;
        p.pinkyMetacarpal = tuple.Item3;
        p.pinkyKnuckle = tuple.Item4;

        j.ringMetacarpal_Knuckle_Knuckle[0] =
                CreateJoint(c.ringMetacarpal_Knuckle[0].gameObject, rb.pinkyMetacarpal_Knuckle,
                            new Vector3(0.005f, 0.025f), new Vector3(-0.005f, 0.025f),
                            ConfigurableJointMotion.Locked,
                            ConfigurableJointMotion.Locked,
                            ConfigurableJointMotion.Free);
        j._ringMetacarpal_Knuckle_Knuckle[0] =
            CreateJoint(c.ringMetacarpal_Knuckle[0].gameObject, rb.pinkyMetacarpal_Knuckle,
                        new Vector3(0.003f, -0.025f), new Vector3(-0.003f, -0.025f),
                        ConfigurableJointMotion.Locked,
                        ConfigurableJointMotion.Locked,
                        ConfigurableJointMotion.Free);

        List<Collider> PalmColliders = new List<Collider>();
        PalmColliders.AddRange(c.thumbMetacarpal_ProximalJoint);
        PalmColliders.AddRange(c.indexMetacarpal_Knuckle);
        PalmColliders.AddRange(c.middleMetacarpal_Knuckle);
        PalmColliders.AddRange(c.ringMetacarpal_Knuckle);
        PalmColliders.Add(c.pinkyMetacarpal_Knuckle);
        for (int i = 0; i < PalmColliders.Count; i++)
        {
            for (int j = 0; j < PalmColliders.Count; j++)
            {
                if (i != j)
                    Physics.IgnoreCollision(PalmColliders[i], PalmColliders[j]);
            }
        }
    }

    private void FixedUpdate()
    {
        p.thumbMetacarpalJoint.velocity = s.thumbMetacarpalJoint.SolveVel(p.thumbMetacarpalJoint.position, t.thumbMetacarpalJoint.position);
        p.thumbProximalJoint.velocity = s.thumbProximalJoint.SolveVel(p.thumbProximalJoint.position, t.thumbProximalJoint.position);

        p.indexMetacarpal.velocity = s.indexMetacarpal.SolveVel(p.indexMetacarpal.position, t.indexMetacarpal.position);
        p.indexKnuckle.velocity = s.indexKnuckle.SolveVel(p.indexKnuckle.position, t.indexKnuckle.position);

        p.middleMetacarpal.velocity = s.middleMetacarpal.SolveVel(p.middleMetacarpal.position, t.middleMetacarpal.position);
        p.middleKnuckle.velocity = s.middleKnuckle.SolveVel(p.middleKnuckle.position, t.middleKnuckle.position);

        p.ringMetacarpal.velocity = s.ringMetacarpal.SolveVel(p.ringMetacarpal.position, t.ringMetacarpal.position);
        p.ringKnuckle.velocity = s.ringKnuckle.SolveVel(p.ringKnuckle.position, t.ringKnuckle.position);

        p.pinkyMetacarpal.velocity = s.pinkyMetacarpal.SolveVel(p.pinkyMetacarpal.position, t.pinkyMetacarpal.position);
        p.pinkyKnuckle.velocity = s.pinkyKnuckle.SolveVel(p.pinkyKnuckle.position, t.pinkyKnuckle.position);
    }
}
