using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LightTK;
using static LightTK.LTK;

[DisallowMultipleComponent]
public class LTKScene : MonoBehaviour
{
    private static LTKScene scene;
    
    [HideInInspector]
    public List<LTKCollider> colliders;

    [HideInInspector]
    public List<LightRay> rays;

    // Start is called before the first frame update
    void Awake()
    {
        if (scene == null) scene = this;
        else Debug.LogError("Multiple instances of LTKScene exist.");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }
}
