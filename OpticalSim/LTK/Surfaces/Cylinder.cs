using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LightTK
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "Cylinder", menuName = "Scriptable Objects/Create Cylinder", order = 1)]
    public class Cylinder : AbstractSurface
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

        public Cylinder(float minimum = float.NegativeInfinity, float maximum = float.PositiveInfinity)
        {
            Equation eq = new Equation()
            {
                j = 1f,
                k = 1f,
                p = -1f
            };

            surface = new Surface()
            {
                surface = eq,
                normals = eq,
                oNormals = eq,
                minimum = Vector3.negativeInfinity,
                maximum = Vector3.positiveInfinity,
                settings = RefractionEquation.crownGlass
            };
            this.minimum = minimum;
            this.maximum = maximum;
        }
    }
}