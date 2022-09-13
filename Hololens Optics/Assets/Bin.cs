using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bin : MonoBehaviour
{
    public static Bin instance;

    public HashSet<Collider> colliders = new HashSet<Collider>();

    private void Awake()
    {
        #region Singleton
        if (!instance) instance = this;
        #endregion
    }

    private void OnTriggerEnter(Collider other)
    {
        colliders.Add(other);
    }

    private void OnTriggerExit(Collider other)
    {
        colliders.Remove(other);
    }
}
