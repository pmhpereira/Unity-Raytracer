using UnityEngine;

namespace Raytracing
{
    public class BoundingBox
    {
        public Vector3 vertexMin, vertexMax;

        public BoundingBox(): this(Vector3.zero, Vector3.one)
        {

        }

        public BoundingBox(Vector3 vertexMin, Vector3 vertexMax)
        {
            this.vertexMin = vertexMin;
            this.vertexMax = vertexMax;
        }

        public bool Hit(Ray ray, ref RayHit hitInfo)
        {
            float tprox = -1000, tdist = 1000;
            float Vo, Vd, Vmin, Vmax;
            float tmin = 0, tmax = 0;

            Vector3 t_min = new Vector3();
            Vector3 t_max = new Vector3();

            for (int i = 0; i < 3; i++)
            {
                Vo = ray.origin[i];
                Vd = ray.direction[i];
                Vmin = vertexMin[i];
                Vmax = vertexMax[i];

                if (Vd == 0 && Vo < Vmin && Vo > Vmax)
                    return false;

                tmin = (Vmin - Vo) / Vd;
                tmax = (Vmax - Vo) / Vd;

                if (tmin > tmax)
                {
                    float temp = tmin;
                    tmin = tmax;
                    tmax = temp;
                }

                if (tmin > tprox)
                {
                    tprox = tmin;
                }

                if (tmax < tdist)
                {
                    tdist = tmax;
                }

                if (tprox > tdist || tdist < 0)
                    return false;

                t_min[i]= tmin;
                t_max[i]= tmax;
            }

            hitInfo.tMin = t_min;
            hitInfo.tMax = t_max;

            ray.t = tprox;

            /**/
            hitInfo.normal = Vector3.up;
            hitInfo.point = ray.origin + ray.t * ray.direction;
            hitInfo.hitObject = new Sphere();
            /**/

            return true;
        }

        public bool HasVertex(ref Vector3 vertex)
        {
            for (int i = 0; i < 3; i++)
            {
                if (vertex[i] < vertexMin[i] || vertex[i] > vertexMax[i])
                    return false;
            }

            return true;
        }
    }
}
