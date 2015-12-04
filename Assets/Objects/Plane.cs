using System;
using UnityEngine;

namespace Raytracing
{
    public class Plane : Object
    {
        Vector3 normal;
        float D;

        public Plane(Vector3[] vertices) : base()
        {
            normal = Vector3.Cross(vertices[1] - vertices[0], vertices[2] - vertices[0]).normalized;
            D = -Vector3.Dot(normal, vertices[0]);
        }

        public override bool Hit(Ray ray, ref RayHit hitInfo)
        {
            float t = -(D + Vector3.Dot(ray.origin, normal)) / Vector3.Dot(ray.direction, normal);

            if (t < Raytracer.Epsilon || t > hitInfo.t)
            {
                return false;
            }

            Vector3 p = ray.origin + t * ray.direction;

            ray.t = t;
            hitInfo.t = t;
            hitInfo.normal = normal;
            hitInfo.point = p;
            hitInfo.hitObject = this;

            return true;
        }

        public override void SetBoundingBox()
        {
            boundingBox = null;
        }
    }
}
