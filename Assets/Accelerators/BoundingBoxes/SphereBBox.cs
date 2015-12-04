using UnityEngine;

namespace Raytracing
{
    public class SphereBBox : BoundingBox
    {
        public SphereBBox(Sphere obj)
        {
            vertexMin = obj.centerPosition - Vector3.one * obj.radius;
            vertexMax = obj.centerPosition + Vector3.one * obj.radius;
        }
    }
}
