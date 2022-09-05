using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct InteractPoint
{
    public int joint;
    public float distance;
    public Vector3 point;
}

public class Interactable : MonoBehaviour
{
    public struct GrabPoint
    {
        public int joint;
        public Vector3 point;

        public bool setOffset;
        public Vector3 offset;
        public Quaternion rotOffset;

        public Vector3 handOffset;
        public Vector3 objectOffset;
    }

    private bool setOffset = true;
    private float offset = 0;
    public Vector3 position
    {
        get
        {
            HandTracker.Hand[] grabs = grabPoints.Keys.ToArray();
            if (grabs.Length == 0) return transform.position;

            if (grabs.Length == 2)
            {
                GrabPoint g0 = grabPoints[grabs[0]];
                GrabPoint g1 = grabPoints[grabs[1]];

                g0.offset = Quaternion.Inverse(grabs[0].rawTrackers[g0.joint].transform.rotation) * (transform.position - grabs[0].rawTrackers[g0.joint].transform.position);
                g1.offset = Quaternion.Inverse(grabs[1].rawTrackers[g1.joint].transform.rotation) * (transform.position - grabs[1].rawTrackers[g1.joint].transform.position);

                grabPoints[grabs[0]] = g0;
                grabPoints[grabs[1]] = g1;

                Vector3 p0 = grabs[0].rawTrackers[g0.joint].transform.position + g0.handOffset;
                Vector3 p1 = grabs[1].rawTrackers[g1.joint].transform.position + g1.handOffset;

                if (setOffset)
                {
                    setOffset = false;
                    offset = (transform.position - p0).magnitude / (p1 - p0).magnitude;
                }

                return p0 + (p1 - p0) * offset;
            }

            return grabs[0].rawTrackers[grabPoints[grabs[0]].joint].transform.position + grabs[0].rawTrackers[grabPoints[grabs[0]].joint].transform.rotation * grabPoints[grabs[0]].offset;
        }
    }

    private bool setRot = true;
    Quaternion rotOffset = Quaternion.identity;
    public Quaternion rotation
    {
        get
        {
            HandTracker.Hand[] grabs = grabPoints.Keys.ToArray();
            if (grabs.Length == 0) return transform.rotation;

            if (grabs.Length == 2)
            {
                GrabPoint g0 = grabPoints[grabs[0]];
                GrabPoint g1 = grabPoints[grabs[1]];

                // Comment out for snap back
                g0.rotOffset = Quaternion.Inverse(transform.rotation) * grabs[0].rawTrackers[g0.joint].transform.rotation;
                g1.rotOffset = Quaternion.Inverse(transform.rotation) * grabs[1].rawTrackers[g1.joint].transform.rotation;

                grabPoints[grabs[0]] = g0;
                grabPoints[grabs[1]] = g1;

                if (setRot)
                {
                    setRot = false;
                    Vector3 rel0 = grabs[0].rawTrackers[grabPoints[grabs[0]].joint].transform.position + g0.handOffset;
                    Vector3 rel1 = grabs[1].rawTrackers[grabPoints[grabs[1]].joint].transform.position + g1.handOffset;
                    rotOffset = Quaternion.Inverse(transform.rotation) * Quaternion.LookRotation(rel1 - rel0, Vector3.up);
                }

                Vector3 r0 = grabs[0].rawTrackers[grabPoints[grabs[0]].joint].transform.position + g0.handOffset;
                Vector3 r1 = grabs[1].rawTrackers[grabPoints[grabs[1]].joint].transform.position + g1.handOffset;
                return Quaternion.LookRotation(
                    r1 - r0, Vector3.up) * Quaternion.Inverse(rotOffset);
            }

            return grabs[0].rawTrackers[grabPoints[grabs[0]].joint].transform.rotation * Quaternion.Inverse(grabPoints[grabs[0]].rotOffset);
        }
    }

    public Collider[] colliders;
    public Dictionary<HandTracker.Hand, InteractPoint> hands = new Dictionary<HandTracker.Hand, InteractPoint>();
    public Dictionary<HandTracker.Hand, GrabPoint> grabPoints = new Dictionary<HandTracker.Hand, GrabPoint>();
    public Dictionary<HandTracker.Hand, bool> handStates = new Dictionary<HandTracker.Hand, bool>();
    public List<HandTracker.Hand> disposeHands = new List<HandTracker.Hand>();
    public bool isGrabbing = false;

    void Start()
    {
        HandTracker.objects.Add(this);    
    }

    //TODO:: PROBLEM setOffset isnt triggered when an object is held by 2 hands, then 1 lets go and then is regrabbed by the second hand
    //           FIX:: Make grabpoint only get added to when isGrabbing is true then on count change trigger setOffset 

    private void FixedUpdate()
    {
#if UNITY_EDITOR
        if (GetComponent<MeshRenderer>() != null)
        {
            if (hands.Count > 0)
            {
                GetComponent<MeshRenderer>().material.color = Color.red;
            }
            else
            {
                GetComponent<MeshRenderer>().material.color = Color.white;
            }
        }
#endif
        isGrabbing = false;

        HandTracker.Hand[] list = hands.Keys.ToArray();
        for (int i = 0; i < list.Length; i++)
        {
            HandTracker.Hand h = list[i];
            GrabPoint g = new GrabPoint()
            {
                joint = hands[h].joint,
                point = hands[h].point,
                setOffset = true
            };

            if (!handStates.ContainsKey(h))
                handStates.Add(h, h.isGrabbing);

            if (handStates[h] && !h.isGrabbing) handStates[h] = false;

            if (h.isGrabbing && !handStates[h])
            {
                if (!grabPoints.ContainsKey(h))
                    grabPoints.Add(h, g);

                isGrabbing = true;

                if (grabPoints[h].setOffset)
                {
                    GrabPoint temp = grabPoints[h];

                    temp.setOffset = false;
                    temp.offset = Quaternion.Inverse(h.rawTrackers[temp.joint].transform.rotation) * (transform.position - h.rawTrackers[temp.joint].transform.position);
                    temp.rotOffset = Quaternion.Inverse(transform.rotation) * h.rawTrackers[temp.joint].transform.rotation;
                    temp.handOffset = temp.point - h.rawTrackers[temp.joint].transform.position;
                    temp.objectOffset = temp.point - transform.position;

                    grabPoints[h] = temp;
                }
            }
            else grabPoints.Remove(h);
        }

        if (grabPoints.Count == 1)
        {
            if (!setRot)
            {
                //Uncomment for let go

                /*disposeHands.AddRange(hands.Keys);
                for (int i = 0; i < list.Length; i++)
                {
                    if (list[i].current == this) list[i].current = null;
                }*/
                setRot = true;
                //isGrabbing = false;
            }

            if (!setOffset)
            {
                setOffset = true;
            }
        }

        for (int i = 0; i < disposeHands.Count; i++)
        {
            HandTracker.Hand h = disposeHands[i];

            grabPoints.Remove(h);
            handStates.Remove(h);
            hands.Remove(h);
        }
        disposeHands.Clear();
    }

    private void OnDestroy()
    {
        HandTracker.objects.Remove(this);
    }
}
