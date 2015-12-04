using UnityEngine;

namespace Raytracing
{
    public class Ray
    {
        public Vector3 origin;
        public Vector3 direction;
        public float t;

        public Ray(Vector3 origin, Vector3 direction)
        {
            this.origin = origin + direction * Raytracer.Epsilon;
            this.direction = Vector3.Normalize(direction);
            this.t = float.MaxValue;
        }
    }
}
