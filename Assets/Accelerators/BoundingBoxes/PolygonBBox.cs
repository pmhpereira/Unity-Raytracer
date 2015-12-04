using UnityEngine;

namespace Raytracing
{
    public class PolygonBBox : BoundingBox
    {
        public PolygonBBox(Polygon obj)
        {
            vertexMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            vertexMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            foreach (Vector3 vertex in obj.vertices)
            {
                for(int i = 0; i < 3; i++)
                {
                    if (vertex[i] < vertexMin[i])
                    {
                        vertexMin[i] = vertex[i];
                    }
                    else if (vertex[i] > vertexMax[i])
                    {
                        vertexMax[i] = vertex[i];
                    }
                }
            }
        }
    }
}
