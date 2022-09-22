using InteractionTK.HandTracking;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace InteractionTK.Menus
{
    public class MTKConsoleWindow : MonoBehaviour
    {
        public GameObject center;

        public MTKButton teleport;
        public MTKButton connectDisconnect;
        public MTKButton host;

        public TextMeshProUGUI console;

        public MTKKeypadWindow keypad;

        private void Start()
        {
            keypad.gameObject.SetActive(false);
            keypad.OnClose.AddListener(() => keypad.gameObject.SetActive(false));

            connectDisconnect.OnClick.AddListener(Connect);
        }

        private void Connect()
        {
            keypad.gameObject.SetActive(true);
            keypad.transform.localPosition = center.transform.localPosition + new Vector3(0, 0.2f, -0.1f);
            keypad.transform.localRotation = Quaternion.identity;
        }

        private float distance = 0.5f;
        private Vector3 dir = Vector3.forward;
        private bool update = true;
        private void FixedUpdate()
        {
            if (!update) return;
            if (dir == Vector3.zero) dir = Vector3.forward;
            transform.position = Camera.main.transform.position + dir.normalized * distance;
            transform.rotation = Quaternion.LookRotation(center.transform.position - Camera.main.transform.position);
        }

        public void OnMove(ITKInteractable interactable, ITKHandController controller)
        {
            update = false;

            dir = transform.position - Camera.main.transform.position;
            if (dir == Vector3.zero) dir = Vector3.forward;
            distance = Mathf.Clamp(dir.magnitude, 0.05f, 1f);

            transform.position = Camera.main.transform.position + dir.normalized * distance;
            transform.rotation = Quaternion.LookRotation(center.transform.position - Camera.main.transform.position);
        }

        public void OnMove(ITKPinchInteractable interactable, ITKPinchController controller)
        {
            update = false;

            dir = transform.position - Camera.main.transform.position;
            if (dir == Vector3.zero) dir = Vector3.forward;
            distance = Mathf.Clamp(dir.magnitude, 0.1f, 1f);

            transform.position = Camera.main.transform.position + dir.normalized * distance;
            transform.rotation = Quaternion.LookRotation(center.transform.position - Camera.main.transform.position);
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