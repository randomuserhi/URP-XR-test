using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

[System.Serializable]
public class SecondOrderSolver
{
    [Range(0f, 10f)]
    public float f = 8;
    [Range(0f, 2f)]
    public float z = 1;
    [Range(-5f, 5f)]
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

//TODO:: maybe rewrite enums to not require cast => https://stackoverflow.com/questions/981776/using-an-enum-as-an-array-index-in-c-sharp
public class PhysicsHand : MonoBehaviour
{
    //TODO:: probably change some names because ConfigJoints and Joints are a bit confusing, maybe rename ConfigJoints, Joints and Joints, trackers
    //       also rename the properties
    private const int numJoints = 25;
    public enum Joints
    {
        // wrist
        wrist,

        // palm
        thumbMetacarpalJoint,
        indexMetacarpal,
        middleMetacarpal,
        ringMetacarpal,
        pinkyMetacarpal,

        thumbProximalJoint,
        indexKnuckle,
        middleKnuckle,
        ringKnuckle,
        pinkyKnuckle,

        // fingers
        thumbTip,
        thumbDistalJoint,

        indexTip,
        indexDistalJoint,
        indexMiddleJoint,

        middleTip,
        middleDistalJoint,
        middleMiddleJoint,

        ringTip,
        ringDistalJoint,
        ringMiddleJoint,

        pinkyTip,
        pinkyDistalJoint,
        pinkyMiddleJoint
    }

    private const int numConnectingBodies = 20;
    public enum Bodies //TODO:: rename these to be nicer and make more sense
    {
        // wrist
        wrist,

        // palm
        thumbMetacarpal_ProximalJoint, //3
        indexMetacarpal_Knuckle,
        middleMetacarpal_Knuckle,
        ringMetacarpal_Knuckle,
        pinkyMetacarpal_Knuckle,

        // fingers
        thumbDistal_Tip,
        thumbProximal_DistalJoint,

        indexDistal_Tip,
        indexMiddle_DistalJoint,
        indexKnuckle_MiddleJoint,

        middleDistal_Tip,
        middleMiddle_DistalJoint,
        middleKnuckle_MiddleJoint,

        ringDistal_Tip,
        ringMiddle_DistalJoint,
        ringKnuckle_MiddleJoint,

        pinkyDistal_Tip,
        pinkyMiddle_DistalJoint,
        pinkyKnuckle_MiddleJoint
    }

    private const int numConfigJoints = 22;
    public enum ConfigJoints
    {
        thumbMetacarpal_ProximalJoint_Knuckle, // 3
        indexMetacarpal_Knuckle_Knuckle,
        middleMetacarpal_Knuckle_Knuckle,
        ringMetacarpal_Knuckle_Knuckle,
        _thumbMetacarpal_ProximalJoint_Knuckle, // 3
        _indexMetacarpal_Knuckle_Knuckle,
        _middleMetacarpal_Knuckle_Knuckle,
        _ringMetacarpal_Knuckle_Knuckle,

        // fingers
        thumbDistal_Tip,
        thumbProximal_DistalJoint,

        indexDistal_Tip,
        indexMiddle_DistalJoint,
        indexKnuckle_MiddleJoint,

        middleDistal_Tip,
        middleMiddle_DistalJoint,
        middleKnuckle_MiddleJoint,

        ringDistal_Tip,
        ringMiddle_DistalJoint,
        ringKnuckle_MiddleJoint,

        pinkyDistal_Tip,
        pinkyMiddle_DistalJoint,
        pinkyKnuckle_MiddleJoint
    }

    public Transform[] trackers;
    private Rigidbody[] points;
    private SecondOrderSolver[] solvers;
    private CapsuleCollider[][] colliders;
    private Rigidbody[][] bodies;
    private ConfigurableJoint[][] joints;

    public bool DebugMesh = true;
    public GameObject DebugObj;

    // Creates a capsule collider along y-axis
    private (CapsuleCollider, Rigidbody, Rigidbody, Rigidbody) CreateCollider(float length = 0.07f, float radius = 0.006f, bool endCap0 = true, bool endCap1 = true)
    {
        GameObject o = new GameObject();
        o.layer = LayerMask.NameToLayer("Hands");
        o.transform.parent = transform;
        o.AddComponent<PhysicsHandGrip>();

        if (DebugMesh && DebugObj)
        {
            GameObject mesh = Instantiate(DebugObj);
            mesh.transform.parent = o.transform;
            mesh.transform.localScale = new Vector3(radius * 2f, length / 2f, radius * 2f);
        }

        CapsuleCollider c = o.AddComponent<CapsuleCollider>();
        c.height = length;
        c.radius = radius;

        Rigidbody rigidbody = o.AddComponent<Rigidbody>();
        rigidbody.mass = 1;
        rigidbody.useGravity = false;

        Rigidbody p1 = null;
        if (endCap0)
        {
            GameObject o1 = new GameObject();
            o1.layer = LayerMask.NameToLayer("Hands");
            o1.transform.parent = o.transform;
            p1 = o1.AddComponent<Rigidbody>();
            p1.mass = 1;
            p1.useGravity = false;
            CreateJoint(o1, rigidbody, Vector3.zero, new Vector3(0, -length / 2f + radius, 0));
        }

        Rigidbody p2 = null;
        if (endCap1)
        {
            GameObject o2 = new GameObject();
            o2.layer = LayerMask.NameToLayer("Hands");
            o2.transform.parent = o.transform;
            p2 = o2.AddComponent<Rigidbody>();
            p2.mass = 1;
            p2.useGravity = false;
            CreateJoint(o2, rigidbody, Vector3.zero, new Vector3(0, length / 2f - radius, 0));
        }
        
        return (c, rigidbody, p1, p2);
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

    //TODO:: Convert code to abuse the fact everything are lists now
    //TODO:: Joint constraints
    private void Start()
    {
        solvers = new SecondOrderSolver[numJoints];
        for (int i = 0; i < solvers.Length; i++)
            solvers[i] = new SecondOrderSolver();

        trackers = new Transform[numJoints];
        points = new Rigidbody[numJoints];
        colliders = new CapsuleCollider[numConnectingBodies][];
        bodies = new Rigidbody[numConnectingBodies][];
        joints = new ConfigurableJoint[numConfigJoints][];

        #region Palms

        const int thumbInbetweens = 4;
        colliders[(int)Bodies.thumbMetacarpal_ProximalJoint] = new CapsuleCollider[thumbInbetweens];
        bodies[(int)Bodies.thumbMetacarpal_ProximalJoint] = new Rigidbody[thumbInbetweens];
        joints[(int)ConfigJoints.thumbMetacarpal_ProximalJoint_Knuckle] = new ConfigurableJoint[thumbInbetweens];
        joints[(int)ConfigJoints._thumbMetacarpal_ProximalJoint_Knuckle] = new ConfigurableJoint[thumbInbetweens];

        (CapsuleCollider, Rigidbody, Rigidbody, Rigidbody) tuple = CreateCollider(0.05f);
        colliders[(int)Bodies.thumbMetacarpal_ProximalJoint][0] = tuple.Item1;
        bodies[(int)Bodies.thumbMetacarpal_ProximalJoint][0] = tuple.Item2;
        points[(int)Joints.thumbMetacarpalJoint] = tuple.Item3;
        points[(int)Joints.thumbProximalJoint] = tuple.Item4;

        for (int i = 1; i < thumbInbetweens; i++)
        {
            tuple = CreateCollider(0.05f, endCap0: false, endCap1: false);
            colliders[(int)Bodies.thumbMetacarpal_ProximalJoint][i] = tuple.Item1;
            bodies[(int)Bodies.thumbMetacarpal_ProximalJoint][i] = tuple.Item2;

            joints[(int)ConfigJoints.thumbMetacarpal_ProximalJoint_Knuckle][i - 1] =
                CreateJoint(colliders[(int)Bodies.thumbMetacarpal_ProximalJoint][i - 1].gameObject, bodies[(int)Bodies.thumbMetacarpal_ProximalJoint][i],
                            new Vector3(0.006f, 0.025f), new Vector3(-0.006f, 0.025f));
            joints[(int)ConfigJoints._thumbMetacarpal_ProximalJoint_Knuckle][i - 1] =
                CreateJoint(colliders[(int)Bodies.thumbMetacarpal_ProximalJoint][i - 1].gameObject, bodies[(int)Bodies.thumbMetacarpal_ProximalJoint][i],
                            new Vector3(0.002f, -0.025f), new Vector3(-0.002f, -0.025f));
        }

        const int indexInbetweens = 2;
        colliders[(int)Bodies.indexMetacarpal_Knuckle] = new CapsuleCollider[indexInbetweens];
        bodies[(int)Bodies.indexMetacarpal_Knuckle] = new Rigidbody[indexInbetweens];
        joints[(int)ConfigJoints.indexMetacarpal_Knuckle_Knuckle] = new ConfigurableJoint[indexInbetweens];
        joints[(int)ConfigJoints._indexMetacarpal_Knuckle_Knuckle] = new ConfigurableJoint[indexInbetweens];

        tuple = CreateCollider();
        colliders[(int)Bodies.indexMetacarpal_Knuckle][0] = tuple.Item1;
        bodies[(int)Bodies.indexMetacarpal_Knuckle][0] = tuple.Item2;
        points[(int)Joints.indexMetacarpal] = tuple.Item3;
        points[(int)Joints.indexKnuckle] = tuple.Item4;

        joints[(int)ConfigJoints.thumbMetacarpal_ProximalJoint_Knuckle][thumbInbetweens - 1] =
                CreateJoint(colliders[(int)Bodies.thumbMetacarpal_ProximalJoint][thumbInbetweens - 1].gameObject, bodies[(int)Bodies.indexMetacarpal_Knuckle][0],
                            new Vector3(0.005f, 0.025f), new Vector3(-0.005f, 0.025f));
        joints[(int)ConfigJoints._thumbMetacarpal_ProximalJoint_Knuckle][thumbInbetweens - 1] =
            CreateJoint(colliders[(int)Bodies.thumbMetacarpal_ProximalJoint][thumbInbetweens - 1].gameObject, bodies[(int)Bodies.indexMetacarpal_Knuckle][0],
                        new Vector3(0.001f, -0.025f), new Vector3(-0.001f, -0.025f));

        for (int i = 1; i < indexInbetweens; i++)
        {
            tuple = CreateCollider(endCap0: false, endCap1: false);
            colliders[(int)Bodies.indexMetacarpal_Knuckle][i] = tuple.Item1;
            bodies[(int)Bodies.indexMetacarpal_Knuckle][i] = tuple.Item2;

            joints[(int)ConfigJoints.indexMetacarpal_Knuckle_Knuckle][i - 1] =
                CreateJoint(colliders[(int)Bodies.indexMetacarpal_Knuckle][i - 1].gameObject, bodies[(int)Bodies.indexMetacarpal_Knuckle][i],
                            new Vector3(0.005f, 0.025f), new Vector3(-0.005f, 0.025f),
                            ConfigurableJointMotion.Locked,
                            ConfigurableJointMotion.Locked,
                            ConfigurableJointMotion.Free);
            joints[(int)ConfigJoints._indexMetacarpal_Knuckle_Knuckle][i - 1] =
                CreateJoint(colliders[(int)Bodies.indexMetacarpal_Knuckle][i - 1].gameObject, bodies[(int)Bodies.indexMetacarpal_Knuckle][i],
                            new Vector3(0.001f, -0.025f), new Vector3(-0.003f, -0.025f),
                            ConfigurableJointMotion.Locked,
                            ConfigurableJointMotion.Locked,
                            ConfigurableJointMotion.Free);
        }

        const int middleInbetweens = 2;
        colliders[(int)Bodies.middleMetacarpal_Knuckle] = new CapsuleCollider[middleInbetweens];
        bodies[(int)Bodies.middleMetacarpal_Knuckle] = new Rigidbody[middleInbetweens];
        joints[(int)ConfigJoints.middleMetacarpal_Knuckle_Knuckle] = new ConfigurableJoint[middleInbetweens];
        joints[(int)ConfigJoints._middleMetacarpal_Knuckle_Knuckle] = new ConfigurableJoint[middleInbetweens];

        tuple = CreateCollider();
        colliders[(int)Bodies.middleMetacarpal_Knuckle][0] = tuple.Item1;
        bodies[(int)Bodies.middleMetacarpal_Knuckle][0] = tuple.Item2;
        points[(int)Joints.middleMetacarpal] = tuple.Item3;
        points[(int)Joints.middleKnuckle] = tuple.Item4;

        joints[(int)ConfigJoints.indexMetacarpal_Knuckle_Knuckle][indexInbetweens - 1] =
                CreateJoint(colliders[(int)Bodies.indexMetacarpal_Knuckle][indexInbetweens - 1].gameObject, bodies[(int)Bodies.middleMetacarpal_Knuckle][0],
                            new Vector3(0.005f, 0.025f), new Vector3(-0.005f, 0.025f),
                            ConfigurableJointMotion.Locked,
                            ConfigurableJointMotion.Locked,
                            ConfigurableJointMotion.Free);
        joints[(int)ConfigJoints._indexMetacarpal_Knuckle_Knuckle][indexInbetweens - 1] =
            CreateJoint(colliders[(int)Bodies.indexMetacarpal_Knuckle][indexInbetweens - 1].gameObject, bodies[(int)Bodies.middleMetacarpal_Knuckle][0],
                        new Vector3(0.003f, -0.025f), new Vector3(-0.003f, -0.025f),
                        ConfigurableJointMotion.Locked,
                        ConfigurableJointMotion.Locked,
                        ConfigurableJointMotion.Free);

        for (int i = 1; i < middleInbetweens; i++)
        {
            tuple = CreateCollider(endCap0: false, endCap1: false);
            colliders[(int)Bodies.middleMetacarpal_Knuckle][i] = tuple.Item1;
            bodies[(int)Bodies.middleMetacarpal_Knuckle][i] = tuple.Item2;

            joints[(int)ConfigJoints.middleMetacarpal_Knuckle_Knuckle][i - 1] =
                CreateJoint(colliders[(int)Bodies.middleMetacarpal_Knuckle][i - 1].gameObject, bodies[(int)Bodies.middleMetacarpal_Knuckle][i],
                            new Vector3(0.005f, 0.025f), new Vector3(-0.005f, 0.025f),
                            ConfigurableJointMotion.Locked,
                            ConfigurableJointMotion.Locked,
                            ConfigurableJointMotion.Free);
            joints[(int)ConfigJoints._middleMetacarpal_Knuckle_Knuckle][i - 1] =
                CreateJoint(colliders[(int)Bodies.middleMetacarpal_Knuckle][i - 1].gameObject, bodies[(int)Bodies.middleMetacarpal_Knuckle][i],
                            new Vector3(0.001f, -0.025f), new Vector3(-0.003f, -0.025f),
                            ConfigurableJointMotion.Locked,
                            ConfigurableJointMotion.Locked,
                            ConfigurableJointMotion.Free);
        }

        const int ringInbetweens = 2;
        colliders[(int)Bodies.ringMetacarpal_Knuckle] = new CapsuleCollider[ringInbetweens];
        bodies[(int)Bodies.ringMetacarpal_Knuckle] = new Rigidbody[ringInbetweens];
        joints[(int)ConfigJoints.ringMetacarpal_Knuckle_Knuckle] = new ConfigurableJoint[ringInbetweens];
        joints[(int)ConfigJoints._ringMetacarpal_Knuckle_Knuckle] = new ConfigurableJoint[ringInbetweens];

        tuple = CreateCollider();
        colliders[(int)Bodies.ringMetacarpal_Knuckle][0] = tuple.Item1;
        bodies[(int)Bodies.ringMetacarpal_Knuckle][0] = tuple.Item2;
        points[(int)Joints.ringMetacarpal] = tuple.Item3;
        points[(int)Joints.ringKnuckle] = tuple.Item4;

        joints[(int)ConfigJoints.middleMetacarpal_Knuckle_Knuckle][middleInbetweens - 1] =
                CreateJoint(colliders[(int)Bodies.middleMetacarpal_Knuckle][middleInbetweens - 1].gameObject, bodies[(int)Bodies.ringMetacarpal_Knuckle][0],
                            new Vector3(0.005f, 0.025f), new Vector3(-0.005f, 0.025f),
                            ConfigurableJointMotion.Locked,
                            ConfigurableJointMotion.Locked,
                            ConfigurableJointMotion.Free);
        joints[(int)ConfigJoints._middleMetacarpal_Knuckle_Knuckle][middleInbetweens - 1] =
            CreateJoint(colliders[(int)Bodies.middleMetacarpal_Knuckle][middleInbetweens - 1].gameObject, bodies[(int)Bodies.ringMetacarpal_Knuckle][0],
                        new Vector3(0.003f, -0.025f), new Vector3(-0.003f, -0.025f),
                        ConfigurableJointMotion.Locked,
                        ConfigurableJointMotion.Locked,
                        ConfigurableJointMotion.Free);

        for (int i = 1; i < ringInbetweens; i++)
        {
            tuple = CreateCollider(endCap0: false, endCap1: false);
            colliders[(int)Bodies.ringMetacarpal_Knuckle][i] = tuple.Item1;
            bodies[(int)Bodies.ringMetacarpal_Knuckle][i] = tuple.Item2;

            joints[(int)ConfigJoints.ringMetacarpal_Knuckle_Knuckle][i - 1] =
                CreateJoint(colliders[(int)Bodies.ringMetacarpal_Knuckle][i - 1].gameObject, bodies[(int)Bodies.ringMetacarpal_Knuckle][i],
                            new Vector3(0.005f, 0.025f), new Vector3(-0.005f, 0.025f),
                            ConfigurableJointMotion.Locked,
                            ConfigurableJointMotion.Locked,
                            ConfigurableJointMotion.Free);
            joints[(int)ConfigJoints._ringMetacarpal_Knuckle_Knuckle][i - 1] =
                CreateJoint(colliders[(int)Bodies.ringMetacarpal_Knuckle][i - 1].gameObject, bodies[(int)Bodies.ringMetacarpal_Knuckle][i],
                            new Vector3(0.001f, -0.025f), new Vector3(-0.003f, -0.025f),
                            ConfigurableJointMotion.Locked,
                            ConfigurableJointMotion.Locked,
                            ConfigurableJointMotion.Free);
        }

        colliders[(int)Bodies.pinkyMetacarpal_Knuckle] = new CapsuleCollider[1];
        bodies[(int)Bodies.pinkyMetacarpal_Knuckle] = new Rigidbody[1];

        tuple = CreateCollider();
        colliders[(int)Bodies.pinkyMetacarpal_Knuckle][0] = tuple.Item1;
        bodies[(int)Bodies.pinkyMetacarpal_Knuckle][0] = tuple.Item2;
        points[(int)Joints.pinkyMetacarpal] = tuple.Item3;
        points[(int)Joints.pinkyKnuckle] = tuple.Item4;

        joints[(int)ConfigJoints.ringMetacarpal_Knuckle_Knuckle][0] =
                CreateJoint(colliders[(int)Bodies.ringMetacarpal_Knuckle][ringInbetweens - 1].gameObject, bodies[(int)Bodies.pinkyMetacarpal_Knuckle][0],
                            new Vector3(0.005f, 0.025f), new Vector3(-0.005f, 0.025f),
                            ConfigurableJointMotion.Locked,
                            ConfigurableJointMotion.Locked,
                            ConfigurableJointMotion.Free);
        joints[(int)ConfigJoints._ringMetacarpal_Knuckle_Knuckle][0] =
            CreateJoint(colliders[(int)Bodies.ringMetacarpal_Knuckle][ringInbetweens - 1].gameObject, bodies[(int)Bodies.pinkyMetacarpal_Knuckle][0],
                        new Vector3(0.003f, -0.025f), new Vector3(-0.003f, -0.025f),
                        ConfigurableJointMotion.Locked,
                        ConfigurableJointMotion.Locked,
                        ConfigurableJointMotion.Free);

        #endregion

        #region Fingers
        #region Thumb

        colliders[(int)Bodies.thumbProximal_DistalJoint] = new CapsuleCollider[1];
        bodies[(int)Bodies.thumbProximal_DistalJoint] = new Rigidbody[1];
        joints[(int)ConfigJoints.thumbProximal_DistalJoint] = new ConfigurableJoint[1];

        tuple = CreateCollider(0.035f, endCap0: false);
        colliders[(int)Bodies.thumbProximal_DistalJoint][0] = tuple.Item1;
        bodies[(int)Bodies.thumbProximal_DistalJoint][0] = tuple.Item2;
        points[(int)Joints.thumbDistalJoint] = tuple.Item4;

        joints[(int)ConfigJoints.thumbProximal_DistalJoint][0] =
                CreateJoint(colliders[(int)Bodies.thumbProximal_DistalJoint][0].gameObject, bodies[(int)Bodies.thumbMetacarpal_ProximalJoint][0],
                            new Vector3(0f, -0.015f), new Vector3(0f, 0.025f));

        colliders[(int)Bodies.thumbDistal_Tip] = new CapsuleCollider[1];
        bodies[(int)Bodies.thumbDistal_Tip] = new Rigidbody[1];
        joints[(int)ConfigJoints.thumbDistal_Tip] = new ConfigurableJoint[1];
        joints[(int)ConfigJoints.thumbDistal_Tip] = new ConfigurableJoint[1];

        tuple = CreateCollider(0.02f, endCap0: false);
        colliders[(int)Bodies.thumbDistal_Tip][0] = tuple.Item1;
        bodies[(int)Bodies.thumbDistal_Tip][0] = tuple.Item2;
        points[(int)Joints.thumbTip] = tuple.Item4;

        joints[(int)ConfigJoints.thumbDistal_Tip][0] =
                CreateJoint(colliders[(int)Bodies.thumbDistal_Tip][0].gameObject, bodies[(int)Bodies.thumbProximal_DistalJoint][0],
                            new Vector3(0f, -0.01f), new Vector3(0f, 0.015f));

        #endregion

        //Rewrite to abuse the fact that index, middle and ring indices are +5 away from each other, so can be condensed into for loop
        #region Index

        colliders[(int)Bodies.indexKnuckle_MiddleJoint] = new CapsuleCollider[1];
        bodies[(int)Bodies.indexKnuckle_MiddleJoint] = new Rigidbody[1];
        joints[(int)ConfigJoints.indexKnuckle_MiddleJoint] = new ConfigurableJoint[1];

        tuple = CreateCollider(0.05f, endCap0: false);
        colliders[(int)Bodies.indexKnuckle_MiddleJoint][0] = tuple.Item1;
        bodies[(int)Bodies.indexKnuckle_MiddleJoint][0] = tuple.Item2;
        points[(int)Joints.indexMiddleJoint] = tuple.Item4;

        joints[(int)ConfigJoints.indexKnuckle_MiddleJoint][0] =
                CreateJoint(colliders[(int)Bodies.indexKnuckle_MiddleJoint][0].gameObject, bodies[(int)Bodies.indexMetacarpal_Knuckle][0],
                            new Vector3(0f, -0.025f), new Vector3(0f, 0.025f));

        colliders[(int)Bodies.indexMiddle_DistalJoint] = new CapsuleCollider[1];
        bodies[(int)Bodies.indexMiddle_DistalJoint] = new Rigidbody[1];
        joints[(int)ConfigJoints.indexMiddle_DistalJoint] = new ConfigurableJoint[1];

        tuple = CreateCollider(0.03f, endCap0: false);
        colliders[(int)Bodies.indexMiddle_DistalJoint][0] = tuple.Item1;
        bodies[(int)Bodies.indexMiddle_DistalJoint][0] = tuple.Item2;
        points[(int)Joints.indexDistalJoint] = tuple.Item4;

        joints[(int)ConfigJoints.indexMiddle_DistalJoint][0] =
                CreateJoint(colliders[(int)Bodies.indexMiddle_DistalJoint][0].gameObject, bodies[(int)Bodies.indexKnuckle_MiddleJoint][0],
                            new Vector3(0f, -0.015f), new Vector3(0f, 0.025f));

        colliders[(int)Bodies.indexDistal_Tip] = new CapsuleCollider[1];
        bodies[(int)Bodies.indexDistal_Tip] = new Rigidbody[1];
        joints[(int)ConfigJoints.indexDistal_Tip] = new ConfigurableJoint[1];

        tuple = CreateCollider(0.02f, endCap0: false);
        colliders[(int)Bodies.indexDistal_Tip][0] = tuple.Item1;
        bodies[(int)Bodies.indexDistal_Tip][0] = tuple.Item2;
        points[(int)Joints.indexTip] = tuple.Item4;

        joints[(int)ConfigJoints.indexDistal_Tip][0] =
                CreateJoint(colliders[(int)Bodies.indexDistal_Tip][0].gameObject, bodies[(int)Bodies.indexMiddle_DistalJoint][0],
                            new Vector3(0f, -0.005f), new Vector3(0f, 0.015f));

        #endregion

        #region Middle

        colliders[(int)Bodies.middleKnuckle_MiddleJoint] = new CapsuleCollider[1];
        bodies[(int)Bodies.middleKnuckle_MiddleJoint] = new Rigidbody[1];
        joints[(int)ConfigJoints.middleKnuckle_MiddleJoint] = new ConfigurableJoint[1];

        tuple = CreateCollider(0.05f, endCap0: false);
        colliders[(int)Bodies.middleKnuckle_MiddleJoint][0] = tuple.Item1;
        bodies[(int)Bodies.middleKnuckle_MiddleJoint][0] = tuple.Item2;
        points[(int)Joints.middleMiddleJoint] = tuple.Item4;

        joints[(int)ConfigJoints.middleKnuckle_MiddleJoint][0] =
                CreateJoint(colliders[(int)Bodies.middleKnuckle_MiddleJoint][0].gameObject, bodies[(int)Bodies.middleMetacarpal_Knuckle][0],
                            new Vector3(0f, -0.025f), new Vector3(0f, 0.025f));

        colliders[(int)Bodies.middleMiddle_DistalJoint] = new CapsuleCollider[1];
        bodies[(int)Bodies.middleMiddle_DistalJoint] = new Rigidbody[1];
        joints[(int)ConfigJoints.middleMiddle_DistalJoint] = new ConfigurableJoint[1];

        tuple = CreateCollider(0.03f, endCap0: false);
        colliders[(int)Bodies.middleMiddle_DistalJoint][0] = tuple.Item1;
        bodies[(int)Bodies.middleMiddle_DistalJoint][0] = tuple.Item2;
        points[(int)Joints.middleDistalJoint] = tuple.Item4;

        joints[(int)ConfigJoints.middleMiddle_DistalJoint][0] =
                CreateJoint(colliders[(int)Bodies.middleMiddle_DistalJoint][0].gameObject, bodies[(int)Bodies.middleKnuckle_MiddleJoint][0],
                            new Vector3(0f, -0.015f), new Vector3(0f, 0.025f));

        colliders[(int)Bodies.middleDistal_Tip] = new CapsuleCollider[1];
        bodies[(int)Bodies.middleDistal_Tip] = new Rigidbody[1];
        joints[(int)ConfigJoints.middleDistal_Tip] = new ConfigurableJoint[1];

        tuple = CreateCollider(0.02f, endCap0: false);
        colliders[(int)Bodies.middleDistal_Tip][0] = tuple.Item1;
        bodies[(int)Bodies.middleDistal_Tip][0] = tuple.Item2;
        points[(int)Joints.middleTip] = tuple.Item4;

        joints[(int)ConfigJoints.middleDistal_Tip][0] =
                CreateJoint(colliders[(int)Bodies.middleDistal_Tip][0].gameObject, bodies[(int)Bodies.middleMiddle_DistalJoint][0],
                            new Vector3(0f, -0.005f), new Vector3(0f, 0.015f));

        #endregion

        #region Ring

        colliders[(int)Bodies.ringKnuckle_MiddleJoint] = new CapsuleCollider[1];
        bodies[(int)Bodies.ringKnuckle_MiddleJoint] = new Rigidbody[1];
        joints[(int)ConfigJoints.ringKnuckle_MiddleJoint] = new ConfigurableJoint[1];

        tuple = CreateCollider(0.05f, endCap0: false);
        colliders[(int)Bodies.ringKnuckle_MiddleJoint][0] = tuple.Item1;
        bodies[(int)Bodies.ringKnuckle_MiddleJoint][0] = tuple.Item2;
        points[(int)Joints.ringMiddleJoint] = tuple.Item4;

        joints[(int)ConfigJoints.ringKnuckle_MiddleJoint][0] =
                CreateJoint(colliders[(int)Bodies.ringKnuckle_MiddleJoint][0].gameObject, bodies[(int)Bodies.ringMetacarpal_Knuckle][0],
                            new Vector3(0f, -0.025f), new Vector3(0f, 0.025f));

        colliders[(int)Bodies.ringMiddle_DistalJoint] = new CapsuleCollider[1];
        bodies[(int)Bodies.ringMiddle_DistalJoint] = new Rigidbody[1];
        joints[(int)ConfigJoints.ringMiddle_DistalJoint] = new ConfigurableJoint[1];

        tuple = CreateCollider(0.03f, endCap0: false);
        colliders[(int)Bodies.ringMiddle_DistalJoint][0] = tuple.Item1;
        bodies[(int)Bodies.ringMiddle_DistalJoint][0] = tuple.Item2;
        points[(int)Joints.ringDistalJoint] = tuple.Item4;

        joints[(int)ConfigJoints.ringMiddle_DistalJoint][0] =
                CreateJoint(colliders[(int)Bodies.ringMiddle_DistalJoint][0].gameObject, bodies[(int)Bodies.ringKnuckle_MiddleJoint][0],
                            new Vector3(0f, -0.015f), new Vector3(0f, 0.025f));

        colliders[(int)Bodies.ringDistal_Tip] = new CapsuleCollider[1];
        bodies[(int)Bodies.ringDistal_Tip] = new Rigidbody[1];
        joints[(int)ConfigJoints.ringDistal_Tip] = new ConfigurableJoint[1];

        tuple = CreateCollider(0.02f, endCap0: false);
        colliders[(int)Bodies.ringDistal_Tip][0] = tuple.Item1;
        bodies[(int)Bodies.ringDistal_Tip][0] = tuple.Item2;
        points[(int)Joints.ringTip] = tuple.Item4;

        joints[(int)ConfigJoints.ringDistal_Tip][0] =
                CreateJoint(colliders[(int)Bodies.ringDistal_Tip][0].gameObject, bodies[(int)Bodies.ringMiddle_DistalJoint][0],
                            new Vector3(0f, -0.005f), new Vector3(0f, 0.015f));

        #endregion

        #region Pinky

        colliders[(int)Bodies.pinkyKnuckle_MiddleJoint] = new CapsuleCollider[1];
        bodies[(int)Bodies.pinkyKnuckle_MiddleJoint] = new Rigidbody[1];
        joints[(int)ConfigJoints.pinkyKnuckle_MiddleJoint] = new ConfigurableJoint[1];

        tuple = CreateCollider(0.03f, endCap0: false);
        colliders[(int)Bodies.pinkyKnuckle_MiddleJoint][0] = tuple.Item1;
        bodies[(int)Bodies.pinkyKnuckle_MiddleJoint][0] = tuple.Item2;
        points[(int)Joints.pinkyMiddleJoint] = tuple.Item4;

        joints[(int)ConfigJoints.pinkyKnuckle_MiddleJoint][0] =
                CreateJoint(colliders[(int)Bodies.pinkyKnuckle_MiddleJoint][0].gameObject, bodies[(int)Bodies.pinkyMetacarpal_Knuckle][0],
                            new Vector3(0f, -0.015f), new Vector3(0f, 0.025f));

        colliders[(int)Bodies.pinkyMiddle_DistalJoint] = new CapsuleCollider[1];
        bodies[(int)Bodies.pinkyMiddle_DistalJoint] = new Rigidbody[1];
        joints[(int)ConfigJoints.pinkyMiddle_DistalJoint] = new ConfigurableJoint[1];

        tuple = CreateCollider(0.02f, endCap0: false);
        colliders[(int)Bodies.pinkyMiddle_DistalJoint][0] = tuple.Item1;
        bodies[(int)Bodies.pinkyMiddle_DistalJoint][0] = tuple.Item2;
        points[(int)Joints.pinkyDistalJoint] = tuple.Item4;

        joints[(int)ConfigJoints.pinkyMiddle_DistalJoint][0] =
                CreateJoint(colliders[(int)Bodies.pinkyMiddle_DistalJoint][0].gameObject, bodies[(int)Bodies.pinkyKnuckle_MiddleJoint][0],
                            new Vector3(0f, -0.01f), new Vector3(0f, 0.015f));

        colliders[(int)Bodies.pinkyDistal_Tip] = new CapsuleCollider[1];
        bodies[(int)Bodies.pinkyDistal_Tip] = new Rigidbody[1];
        joints[(int)ConfigJoints.pinkyDistal_Tip] = new ConfigurableJoint[1];

        tuple = CreateCollider(0.02f, endCap0: false);
        colliders[(int)Bodies.pinkyDistal_Tip][0] = tuple.Item1;
        bodies[(int)Bodies.pinkyDistal_Tip][0] = tuple.Item2;
        points[(int)Joints.pinkyTip] = tuple.Item4;

        joints[(int)ConfigJoints.pinkyDistal_Tip][0] =
                CreateJoint(colliders[(int)Bodies.pinkyDistal_Tip][0].gameObject, bodies[(int)Bodies.pinkyMiddle_DistalJoint][0],
                            new Vector3(0f, -0.005f), new Vector3(0f, 0.015f));

        #endregion
        #endregion
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < solvers.Length; i++)
        {
            if (points[i] && trackers[i])
                points[i].velocity = solvers[i].SolveVel(points[i].position, trackers[i].position);
        }
    }
}
