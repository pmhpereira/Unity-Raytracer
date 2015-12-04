using UnityEngine;

namespace Raytracing
{
    public class CylinderBBox : BoundingBox
    {
        public CylinderBBox(Cylinder obj)
        {
            vertexMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            vertexMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            float maxRadius = Mathf.Max(obj.baseRadius, obj.apexRadius);

            for (int i = 0; i < 3; i++)
            {
                if (obj.basePosition[i] - obj.baseRadius < vertexMin[i])
                {
                    vertexMin[i] = obj.basePosition[i] - obj.baseRadius;
                }
                else if (obj.basePosition[i] + obj.baseRadius > vertexMax[i])
                {
                    vertexMax[i] = obj.basePosition[i] + obj.baseRadius;
                }

                if (obj.apexPosition[i] - obj.apexRadius < vertexMin[i])
                {
                    vertexMin[i] = obj.apexPosition[i] - obj.apexRadius;
                }
                else if (obj.apexPosition[i] + obj.apexRadius > vertexMax[i])
                {
                    vertexMax[i] = obj.apexPosition[i] + obj.apexRadius;
                }
            }
        }
    }
}
