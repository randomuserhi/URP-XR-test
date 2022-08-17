using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LightTK
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "Sphere", menuName = "Scriptable Objects/Create Sphere", order = 1)]
    public class Sphere : AbstractSurface
    {
        public float minimum
        {
            get { return surface.minimum.z; }
            set { surface.minimum.z = value; }
        }

        public float maximum
        {
            get { return surface.maximum.z; }
            set { surface.maximum.z = value; }
        }

        public float offset
        {
            get { return surface.surface.i; }
            set { surface.surface.i = value; }
        }

        public float radius
        {
            get { return -surface.surface.p; }
            set { surface.surface.p = -value; }
        }

        public Sphere(float minimum = float.NegativeInfinity, float maximum = float.PositiveInfinity, float offset = 0)
        {
            Equation eq = new Equation()
            {
                j = 1f,
                k = 1f,
                l = 1f,
                p = -1f
            };

            surface = new Surface()
            {
                surface = eq,
                minimum = Vector3.negativeInfinity,
                maximum = Vector3.positiveInfinity,
                settings = RefractionEquation.crownGlass
            };
            this.minimum = minimum;
            this.maximum = maximum;
            this.offset = offset;
        }
    }
}
