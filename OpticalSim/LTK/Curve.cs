using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace LightTK
{
    public abstract class Curve
    {
        public Vector3 position
        {
            get
            {
                return parameters.position;
            }
            set
            {
                parameters.position = value;
            }
        }
        public Quaternion rotation
        {
            get
            {
                return parameters.rotation;
            }
            set
            {
                parameters.rotation = value;
            }
        }

        public CurveParameter parameters;

        public Curve()
        {
            parameters.minimum = CurveParameter.minimumInfinity;
            parameters.maximum = CurveParameter.maximumInfinity;
            parameters.radial = 0f;
        }

        public static implicit operator CurveParameter(Curve c)
        {
            return c.parameters;
        }
    }

    public class Plane : Curve
    {
        public Vector2 minimum
        {
            get
            {
                return parameters.minimum;
            }
            set
            {
                parameters.minimum = minimum;
            }
        }
        public Vector2 maximum
        {
            get
            {
                return parameters.maximum;
            }
            set
            {
                parameters.minimum = maximum;
            }
        }

        public Plane(Vector2 minimum, Vector2 maximum)
        {
            parameters = CurveParameter.Plane;
            parameters.minimum = minimum;
            parameters.maximum = maximum;
            parameters.radial = 0f;
        }
    }

    public class Paraboloid : Curve
    {
        public float minimum
        {
            get
            {
                return parameters.minimum.z;
            }
            set
            {
                parameters.minimum.z = value;
            }
        }

        public float maximum
        {
            get
            {
                return parameters.maximum.z;
            }
            set
            {
                parameters.maximum.z = value;
            }
        }

        public float scale
        {
            get
            {
                return parameters.l;
            }
            set
            {
                parameters.l = value;
            }
        }

        public Paraboloid(float minimum, float maximum) : base()
        {
            parameters = CurveParameter.Paraboloid;
        }
    }

    public class Cylinder : Curve
    {
        public float minimum
        {
            get
            {
                return parameters.minimum.z;
            }
            set
            {
                parameters.minimum.z = value;
            }
        }

        public float maximum
        {
            get
            {
                return parameters.maximum.z;
            }
            set
            {
                parameters.maximum.z = value;
            }
        }

        public float radius
        {
            get
            {
                return parameters.r;
            }
            set
            {
                parameters.r = value;
            }
        }

        public Cylinder(float minimum, float maximum) : base()
        {
            parameters = CurveParameter.Cylinder;
            parameters.minimum.z = minimum;
            parameters.maximum.z = maximum;
        }
    }

    public class Elliptoid : Curve
    {
        public float minimum
        {
            get
            {
                return parameters.minimum.z;
            }
            set
            {
                parameters.minimum.z = value;
            }
        }

        public float maximum
        {
            get
            {
                return parameters.maximum.z;
            }
            set
            {
                parameters.maximum.z = value;
            }
        }

        public float radius
        {
            get
            {
                return parameters.r;
            }
            set
            {
                parameters.r = value;
            }
        }

        public float squash
        {
            get
            {
                return parameters.s;
            }
            set
            {
                parameters.s = value;
            }
        }

        public Elliptoid(float minimum, float maximum) : base()
        {
            parameters = CurveParameter.Elliptoid;
            parameters.minimum.z = minimum;
            parameters.maximum.z = maximum;
        }
    }

    public class Sphere : Curve
    {
        public float minimum
        {
            get
            {
                return parameters.minimum.z;
            }
            set
            {
                parameters.minimum.z = value;
            }
        }

        public float maximum
        {
            get
            {
                return parameters.maximum.z;
            }
            set
            {
                parameters.maximum.z = value;
            }
        }

        public float radius
        {
            get
            {
                return parameters.r;
            }
            set
            {
                parameters.r = value;
            }
        }

        public Sphere(float minimum, float maximum) : base()
        {
            parameters = CurveParameter.Sphere;
            parameters.minimum.z = minimum;
            parameters.maximum.z = maximum;
        }
    }

    public class Hyperboloid : Curve
    {
        public float minimum
        {
            get
            {
                return parameters.minimum.z;
            }
            set
            {
                parameters.minimum.z = value;
            }
        }

        public float maximum
        {
            get
            {
                return parameters.maximum.z;
            }
            set
            {
                parameters.maximum.z = value;
            }
        }

        public float squash
        {
            get
            {
                return parameters.r;
            }
            set
            {
                parameters.r = value;
            }
        }

        public Hyperboloid(float minimum, float maximum) : base()
        {
            parameters = CurveParameter.Hyperboloid;
            parameters.minimum.z = minimum;
            parameters.maximum.z = maximum;
        }
    }
}
