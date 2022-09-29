using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LightTK;
using static LightTK.LTK;

public class LTKCollider : MonoBehaviour
{
    public AbstractSurface surface;
    public Surface _surface;
    public bool enableCollision = true;

    //TODO:: Allow altering of variables and surface settings
    //       - Scriptable object is supposed to be used to determine the equation and bounds (Remove ability to set surface type there)
    //       - LTKCollider is used to determine surface type, refraction settings etc...

    private void FixedUpdate()
    {
        SurfaceSettings s = _surface.settings;
        _surface = surface; //TODO:: this copy is a bit unnecessary to do every frame, but might be easiest to implement like so
        _surface.settings = s;
        _surface.position = transform.rotation * surface.position + transform.position; // Offset (surface.position) is relative so needs to be rotated
        _surface.rotation = transform.rotation * surface.rotation;
    }
}
