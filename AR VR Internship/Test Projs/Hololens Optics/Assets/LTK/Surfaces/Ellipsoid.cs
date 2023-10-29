using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LightTK
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "Ellipsoid", menuName = "LTK/Create Ellipsoid", order = 1)]
    public class Ellipsoid : AbstractSurface
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

        /*public Vector3 squash
        {
            get { return new Vector3(curve.j, curve.k, curve.l); }
            set { curve.j = value.x; curve.k = value.y; curve.l = value.z; }
        }*/
        public float squash
        {
            get { return surface.surface.l; }
            set { surface.surface.l = value; }
        }

        public Ellipsoid(float minimum = float.NegativeInfinity, float maximum = float.PositiveInfinity, float offset = 0)
        {
            surface = new Surface()
            {
                surface = Equation.Ellipsoid,
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