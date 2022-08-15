using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace LightTK
{
    public abstract class AbstractSurface : ScriptableObject
    {
        public Vector3 eulerAngles;

        public Vector3 position
        {
            get
            {
                return surface.position;
            }
            set
            {
                surface.position = value;
            }
        }
        public Quaternion rotation
        {
            get
            {
                return surface.rotation;
            }
            set
            {
                surface.rotation = value;
            }
        }

        public RefractionSettings refractionSettings
        {
            get
            {
                return surface.settings.refractionSettings;
            }
            set
            {
                surface.settings.refractionSettings = value;
            }
        }
        public Surface surface;

        public AbstractSurface()
        {
            surface.settings = RefractionEquation.crownGlass;
        }

        public static implicit operator Surface(AbstractSurface surface)
        {
            return surface.surface;
        }
    }
}
