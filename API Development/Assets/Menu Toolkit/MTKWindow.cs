using InteractionTK.HandTracking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InteractionTK.Menus
{
    public class MTKWindow : MonoBehaviour
    {
        public ITKInteractable interaction;
        public ITKPinchInteractable pinchInteraction;

        public MTKButton Close;

        public virtual void Start()
        {
            Close.OnClick.AddListener(CloseWindow);
        }

        public virtual void CloseWindow()
        {
            Destroy(gameObject);
        }
    }
}
