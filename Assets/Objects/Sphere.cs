using System;
using UnityEngine;

namespace Raytracing
{
    public class Sphere: Object
    {
        public Vector3 centerPosition;
        public float radius;

        public Sphere(): this(Vector3.zero, 1)
        {

        }

        public Sphere(Vector3 centerPosition, float radius): base()
        {
            this.centerPosition = centerPosition;
            this.radius = radius;
        }

        public override bool Hit(Ray ray, ref RayHit hitInfo)
        {
            Vector3 L = centerPosition - ray.origin;
            float C = Vector3.Dot(L, L) - radius * radius;

            if(C == 0)
            {
                return false;
            }

            float B = Vector3.Dot(ray.direction, L);
            float discriminant = B * B - C;

            if (discriminant <= Raytracer.Epsilon)
            {
                return false;
            }

            float root = Mathf.Sqrt(discriminant);

            float t1 = (B - root);
            float t2 = (B + root);

            float t = 0;
            if (t1 > 0)
            {
                t = t1;
            }
            else if (t2 > 0)
            {
                t = t2;
            }
            else
            {
                return false;
            }

            if (t > hitInfo.t)
            {
                return false;
            }

            Vector3 p = ray.origin + t * ray.direction;
            Vector3 normal = (p - centerPosition) / radius;

            ray.t = t;
            hitInfo.t = t;
            hitInfo.normal = normal;
            hitInfo.point = p;
            hitInfo.hitObject = this;

            return true;
        }

        public override void SetBoundingBox()
        {
            boundingBox = new SphereBBox(this);
        }
    }
}
