using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LightTK
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "Plane", menuName = "Scriptable Objects/Create Plane", order = 1)]
    public class Plane : AbstractSurface
    {
        public Vector3 minimum
        {
            get { return surface.minimum; }
            set { surface.minimum = new Vector3(value.x, value.y, value.z); }
        }

        public Vector3 maximum
        {
            get { return surface.maximum; }
            set { surface.maximum = new Vector3(value.x, value.y, value.z); }
        }

        public float offset
        {
            get { return -surface.surface.p; }
            set { surface.surface.p = -value; }
        }

        public Plane(float offset = 0)
        {
            Equation eq = new Equation()
            {
                o = 1f
            };

            surface = new Surface()
            {
                surface = eq,
                normals = eq,
                oNormals = eq,
                settings = RefractionEquation.crownGlass
            };
            minimum = Vector3.negativeInfinity;
            maximum = Vector3.positiveInfinity;
            this.offset = offset;
        }

        public Plane(Vector2 minimum, Vector2 maximum, float offset = 0)
        {
            Equation eq = new Equation()
            {
                o = 1f
            };

            surface = new Surface()
            {
                surface = eq,
                normals = eq,
                oNormals = eq,
                settings = RefractionEquation.crownGlass
            };
            this.minimum = minimum;
            this.maximum = maximum;
            this.offset = offset;
        }
    }
}