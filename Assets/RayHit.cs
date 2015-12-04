using UnityEngine;

namespace Raytracing
{
    public class RayHit
    {
        public Vector3 normal;
        public Vector3 point;
        public Object hitObject;
        public float t = float.MaxValue;
        public Vector3 tMin;
        public Vector3 tMax;

        public RayHit()
        {

        }
    }
}
