using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace LightTK
{
    public abstract class Surface
    {
        public Vector3 position
        {
            get
            {
                return curve.position;
            }
            set
            {
                curve.position = value;
            }
        }
        public Quaternion rotation
        {
            get
            {
                return curve.rotation;
            }
            set
            {
                curve.rotation = value;
            }
        }

        public RefractionSettings refractionSettings
        {
            get
            {
                return curve.refractionSettings;
            }
            set
            {
                curve.refractionSettings = value;
            }
        }

        public Curve curve;

        public Surface()
        {
            curve.refractionSettings = RefractionEquation.crownGlass;
        }

        public static implicit operator Curve(Surface c)
        {
            return c.curve;
        }
    }

    public class Plane : Surface
    {
        public Vector3 minimum
        {
            get { return curve.minimum; }
            set { curve.minimum = new Vector3(value.x, value.y, value.z); }
        }

        public Vector3 maximum
        {
            get { return curve.maximum; }
            set { curve.maximum = new Vector3(value.x, value.y, value.z); }
        }

        public float offset
        {
            get { return -curve.p; }
            set { curve.p = -value; }
        }

        public Plane(float offset = 0)
        {
            curve = new Curve()
            {
                o = 1f,
                refractionSettings = RefractionEquation.crownGlass
            };
            minimum = Vector3.negativeInfinity;
            maximum = Vector3.positiveInfinity;
            this.offset = offset;
        }

        public Plane(Vector2 minimum, Vector2 maximum, float offset = 0)
        {
            curve = new Curve()
            {
                o = 1f,
                refractionSettings = RefractionEquation.crownGlass
            };
            this.minimum = minimum;
            this.maximum = maximum;
            this.offset = offset;
        }
    }

    public class Cylinder : Surface
    {
        public float minimum
        {
            get { return curve.minimum.z; }
            set { curve.minimum.z = value; }
        }

        public float maximum
        {
            get { return curve.maximum.z; }
            set { curve.maximum.z = value; }
        }

        public Cylinder(float minimum = float.NegativeInfinity, float maximum = float.PositiveInfinity)
        {
            curve = new Curve()
            {
                j = 1f,
                k = 1f,
                p = -1f,
                minimum = Vector3.negativeInfinity,
                maximum = Vector3.positiveInfinity,
                refractionSettings = RefractionEquation.crownGlass
            };
            this.minimum = minimum;
            this.maximum = maximum;
        }
    }

    public class Sphere : Surface
    {
        public float minimum
        {
            get { return curve.minimum.z; }
            set { curve.minimum.z = value; }
        }

        public float maximum
        {
            get { return curve.maximum.z; }
            set { curve.maximum.z = value; }
        }

        public float offset
        {
            get { return curve.i; }
            set { curve.i = value; }
        }

        public float radius
        {
            get { return -curve.p; }
            set { curve.p = -value; }
        }

        public Sphere(float minimum = float.NegativeInfinity, float maximum = float.PositiveInfinity, float offset = 0)
        {
            curve = new Curve()
            {
                j = 1f,
                k = 1f,
                l = 1f,
                p = -1f,
                minimum = Vector3.negativeInfinity,
                maximum = Vector3.positiveInfinity,
                refractionSettings = RefractionEquation.crownGlass
            };
            this.minimum = minimum;
            this.maximum = maximum;
            this.offset = offset;
        }
    }

    public class Ellipsoid : Surface
    {
        public float minimum
        {
            get { return curve.minimum.z; }
            set { curve.minimum.z = value; }
        }

        public float maximum
        {
            get { return curve.maximum.z; }
            set { curve.maximum.z = value; }
        }

        public float offset
        {
            get { return curve.i; }
            set { curve.i = value; }
        }

        public float radius
        {
            get { return -curve.p; }
            set { curve.p = -value; }
        }

        /*public Vector3 squash
        {
            get { return new Vector3(curve.j, curve.k, curve.l); }
            set { curve.j = value.x; curve.k = value.y; curve.l = value.z; }
        }*/
        public float squash
        {
            get { return curve.l; }
            set { curve.l = value; }
        }

        public Ellipsoid(float minimum = float.NegativeInfinity, float maximum = float.PositiveInfinity, float offset = 0)
        {
            curve = new Curve()
            {
                j = 1f,
                k = 1f,
                l = 1f,
                p = -1f,
                minimum = Vector3.negativeInfinity,
                maximum = Vector3.positiveInfinity,
                refractionSettings = RefractionEquation.crownGlass
            };
            this.minimum = minimum;
            this.maximum = maximum;
            this.offset = offset;
        }
    }

    public class Hyperboloid : Surface
    {
        public float minimum
        {
            get { return curve.minimum.z; }
            set { curve.minimum.z = value; }
        }

        public float maximum
        {
            get { return curve.maximum.z; }
            set { curve.maximum.z = value; }
        }

        public float offset
        {
            get { return curve.i; }
            set { curve.i = value; }
        }

        public float seperation
        {
            get { return -curve.p; }
            set { curve.p = -value; }
        }

        /*public Vector3 squash
        {
            get { return new Vector3(curve.j, curve.k, curve.l); }
            set { curve.j = value.x; curve.k = value.y; curve.l = value.z; }
        }*/
        public float squash
        {
            get { return curve.l; }
            set { curve.l = value; }
        }

        public Hyperboloid(float minimum = float.NegativeInfinity, float maximum = float.PositiveInfinity, float offset = 0)
        {
            curve = new Curve()
            {
                j = 1f,
                k = 1f,
                l = -1f,
                p = 1f,
                minimum = Vector3.negativeInfinity,
                maximum = Vector3.positiveInfinity,
                refractionSettings = RefractionEquation.crownGlass
            };
            this.minimum = minimum;
            this.maximum = maximum;
            this.offset = offset;
        }
    }

    public class Paraboloid : Surface
    {
        public float minimum
        {
            get { return curve.minimum.z; }
            set { curve.minimum.z = value; }
        }

        public float maximum
        {
            get { return curve.maximum.z; }
            set { curve.maximum.z = value; }
        }

        public float offset
        {
            get { return curve.i; }
            set { curve.i = value; }
        }

        /*public Vector2 scale
        {
            get { return new Vector2(curve.j, curve.k); }
            set { curve.j = value.x; curve.k = value.y; }
        }*/
        public float scale
        {
            get { return curve.j; }
            set { curve.j = value; curve.k = value; }
        }

        public Paraboloid(float minimum = float.NegativeInfinity, float maximum = float.PositiveInfinity, float offset = 0)
        {
            curve = new Curve()
            {
                j = 1f,
                k = 1f, 
                o = -1f,
                minimum = Vector3.negativeInfinity,
                maximum = Vector3.positiveInfinity,
                refractionSettings = RefractionEquation.crownGlass
            };
            this.minimum = minimum;
            this.maximum = maximum;
            this.offset = offset;
        }
    }
}
