using InteractionTK.HandTracking;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using System.Net;
using VirtualRealityTK;
using NetworkToolkit;
using FieldsToolkit;

namespace InteractionTK.Menus
{
    public class MTKFieldsConsole : MonoBehaviour
    {
        public GameObject center;

        public GameObject magnet;
        public GameObject positive;
        public GameObject negative;

        public MTKButton spawnMagnet;
        public MTKButton spawnPositive;
        public MTKButton spawnNegative;
        public MTKButton delete;
        public MTKButton toggle;
        public MTKButton toggleITK;
        public MTKButton toggleSimulate;

        private List<GameObject> objects = new List<GameObject>();
        private List<FTKPole> poles = new List<FTKPole>();
        private List<ITKInteractable> grab = new List<ITKInteractable>();

        private bool render = false;
        private bool itk = true;
        private bool simulate = false;
        private float spawnDist = 0.4f;
        private void Start()
        {
            spawnMagnet.OnClick.AddListener(() =>
            {
                GameObject o = Instantiate(magnet);
                o.transform.position = VRTK.master.main.transform.position + VRTK.master.main.transform.rotation * Vector3.forward * spawnDist;
                poles.AddRange(o.GetComponentsInChildren<FTKPole>());
                grab.Add(o.GetComponent<ITKInteractable>());
                objects.Add(o);
            });
            spawnPositive.OnClick.AddListener(() =>
            {
                GameObject o = Instantiate(positive);
                o.transform.position = VRTK.master.main.transform.position + VRTK.master.main.transform.rotation * Vector3.forward * spawnDist;
                poles.AddRange(o.GetComponentsInChildren<FTKPole>());
                grab.Add(o.GetComponent<ITKInteractable>());
                objects.Add(o);
            });
            spawnNegative.OnClick.AddListener(() =>
            {
                GameObject o = Instantiate(negative);
                o.transform.position = VRTK.master.main.transform.position + VRTK.master.main.transform.rotation * Vector3.forward * spawnDist;
                poles.AddRange(o.GetComponentsInChildren<FTKPole>());
                grab.Add(o.GetComponent<ITKInteractable>());
                objects.Add(o);
            });

            toggle.OnClick.AddListener(() =>
            {
                render = !render;
                if (render) toggle.tmp.text = "Disable Field Lines";
                else toggle.tmp.text = "Render Field Lines";
            });

            toggleITK.OnClick.AddListener(() =>
            {
                itk = !itk;
                if (itk) toggleITK.tmp.text = "Disable ITK";
                else toggleITK.tmp.text = "Enable ITK";
            });

            toggleSimulate.OnClick.AddListener(() =>
            {
                simulate = !simulate;
                if (simulate) toggleSimulate.tmp.text = "Disable Simulation";
                else toggleSimulate.tmp.text = "Enable Simulation";
            });

            delete.OnClick.AddListener(() =>
            {
                for (int i = 0; i < objects.Count; ++i) Destroy(objects[i]);
                objects.Clear();
                poles.Clear();
                grab.Clear();
            });
        }

        private float distance = 0.5f;
        private Vector3 dir = Vector3.forward;
        private bool update = true;
        private void FixedUpdate()
        {
            for (int i = 0; i < poles.Count; ++i) { poles[i].render = render; poles[i].simulate = simulate; }
            for (int i = 0; i < grab.Count; ++i) { grab[i].pinch = itk; grab[i].grasp = itk; }

            if (!update) return;
            if (dir == Vector3.zero) dir = Vector3.forward;
            transform.position = VRTK.master.main.transform.position + dir.normalized * distance;
            transform.rotation = Quaternion.LookRotation(center.transform.position - VRTK.master.main.transform.position);
        }

        public void OnMove(ITKInteractable interactable, ITKHandController controller)
        {
            update = false;

            dir = transform.position - VRTK.master.main.transform.position;
            if (dir == Vector3.zero) dir = Vector3.forward;
            distance = Mathf.Clamp(dir.magnitude, 0.05f, 1f);

            transform.position = VRTK.master.main.transform.position + dir.normalized * distance;
            transform.rotation = Quaternion.LookRotation(center.transform.position - VRTK.master.main.transform.position);
        }

        public void OnMove(ITKPinchInteractable interactable, ITKPinchController controller)
        {
            update = false;

            dir = transform.position - VRTK.master.main.transform.position;
            if (dir == Vector3.zero) dir = Vector3.forward;
            distance = Mathf.Clamp(dir.magnitude, 0.1f, 1f);

            transform.position = VRTK.master.main.transform.position + dir.normalized * distance;
            transform.rotation = Quaternion.LookRotation(center.transform.position - VRTK.master.main.transform.position);
        }

        public void OnStopMove(ITKInteractable interactable, ITKHandController controller)
        {
            update = true;
        }

        public void OnStopMove(ITKPinchInteractable interactable, ITKPinchController controller)
        {
            update = true;
        }
    }
}