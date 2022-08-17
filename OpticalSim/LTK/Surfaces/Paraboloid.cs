using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LightTK
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "Paraboloid", menuName = "Scriptable Objects/Create Paraboloid", order = 1)]
    public class Paraboloid : AbstractSurface
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

        /*public Vector2 scale
        {
            get { return new Vector2(curve.j, curve.k); }
            set { curve.j = value.x; curve.k = value.y; }
        }*/
        public float scale
        {
            get { return surface.surface.j; }
            set { surface.surface.j = value; surface.surface.k = value; }
        }

        public Paraboloid(float minimum = float.NegativeInfinity, float maximum = float.PositiveInfinity, float offset = 0)
        {
            Equation eq = new Equation()
            {
                j = 1f,
                k = 1f,
                o = -1f
            };

            base.surface = new Surface()
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