using InteractionTK.Menus;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Events;
using VirtualRealityTK;

namespace FieldsToolkit
{
    public class FieldsToolkit : MonoBehaviour
    {
        internal static FieldsToolkit instance;

        public FTK.Settings settings = FTK.Settings.defaultSettings;

        public void Start()
        {
            if (instance != null)
            {
                Debug.LogWarning("An instance of FTK already exists.");
                Destroy(this);
                return;
            }

            instance = this;
        }

        public void FixedUpdate()
        {
        }

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            FieldsToolkit instance = FindObjectOfType<FieldsToolkit>();
            if (instance == null)
            {
                Debug.LogWarning("FTK was not found, creating a new object.");
                GameObject o = new GameObject("Fields Toolkit");
                FieldsToolkit.instance = o.AddComponent<FieldsToolkit>();
            }
        }
    }
}
