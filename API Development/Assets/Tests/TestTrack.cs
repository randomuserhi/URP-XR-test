using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using VirtualRealityTK;

public class TestTrack : MonoBehaviour
{
    ConfigurableJoint j;
    public GameObject target;

    // Start is called before the first frame update
    void Start()
    {
        j = GetComponent<ConfigurableJoint>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        j.connectedAnchor = target.transform.position;
        j.targetRotation = Quaternion.Inverse(target.transform.rotation);
    }
}
