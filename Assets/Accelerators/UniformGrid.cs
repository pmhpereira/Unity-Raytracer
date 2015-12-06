using System.Collections.Generic;
using UnityEngine;

namespace Raytracing
{
    public class UniformGrid : Accelerator
    {
        int nx, ny, nz;
        List<Object>[] grid;
        BoundingBox boundingBox;

        List<Object> boundlessObjects;

        public UniformGrid(ref List<Object> objects): base(ref objects)
        {

        }

        private int GetGridIndex(int x, int y, int z)
        {
            return x + y * nx + z * nx * ny;
        }

        public override void SetupAccelerator()
        {
            Vector3 vertexMin = Vector3.one * Raytracer.Epsilon + Vector3.one;
            Vector3 vertexMax = -Vector3.one * Raytracer.Epsilon - Vector3.one;

            foreach (Object obj in objects)
            {
                obj.SetBoundingBox();

                if(obj.boundingBox != null)
                {
                    vertexMin.x = Mathf.Min(vertexMin.x, obj.boundingBox.vertexMin.x);
                    vertexMin.y = Mathf.Min(vertexMin.y, obj.boundingBox.vertexMin.y);
                    vertexMin.z = Mathf.Min(vertexMin.z, obj.boundingBox.vertexMin.z);

                    vertexMax.x = Mathf.Max(vertexMax.x, obj.boundingBox.vertexMax.x);
                    vertexMax.y = Mathf.Max(vertexMax.y, obj.boundingBox.vertexMax.y);
                    vertexMax.z = Mathf.Max(vertexMax.z, obj.boundingBox.vertexMax.z);
                }
            }

            vertexMin -= Vector3.one * Raytracer.Epsilon;
            vertexMax += Vector3.one * Raytracer.Epsilon;

            boundingBox = new BoundingBox(vertexMin, vertexMax);

            float m = Raytracer.gridMultiplier;
            Vector3 w = vertexMax - vertexMin;
            float s = Mathf.Pow(w.x * w.y * w.z / objects.Count, 1f / 3);
            nx = (int)(m * w.x / s) + 1;
            ny = (int)(m * w.x / s) + 1;
            nz = (int)(m * w.x / s) + 1;

            grid = new List<Object>[nx * ny * nz];

            for (int i = 0; i < nx * ny * nz; i++)
            {
                grid[i] = new List<Object>();
            }

            boundlessObjects = new List<Object>();

            foreach (Object obj in objects)
            {
                if(obj.boundingBox != null)
                {
                    Vector3 bbMinDiff = obj.boundingBox.vertexMin - vertexMin;
                    Vector3 bbMaxDiff = obj.boundingBox.vertexMax - vertexMin;

                    int ixMin = (int) Mathf.Clamp(bbMinDiff.x * nx / w.x, 0, nx -1);
                    int iyMin = (int) Mathf.Clamp(bbMinDiff.y * ny / w.y, 0, ny - 1);
                    int izMin = (int) Mathf.Clamp(bbMinDiff.z * nz / w.z, 0, nz - 1);
                    int ixMax = (int) Mathf.Clamp(bbMaxDiff.x * nx / w.x, 0, nx - 1);
                    int iyMax = (int) Mathf.Clamp(bbMaxDiff.y * ny / w.y, 0, ny - 1);
                    int izMax = (int) Mathf.Clamp(bbMaxDiff.z * nz / w.z, 0, nz - 1);

                    for(int iz = izMin; iz <= izMax; iz++)
                    {
                        for (int iy = iyMin; iy <= iyMax; iy++)
                        {
                            for (int ix = ixMin; ix <= ixMax; ix++)
                            {
                                int index = GetGridIndex(ix, iy, iz);
                                grid[index].Add(obj);
                            }
                        }
                    }
                }
                else
                {
                    boundlessObjects.Add(obj);
                }
            }
        }

        public override bool Hit(Ray ray, ref RayHit hitInfo)
        {
            bool hit = false;

            if (boundingBox.Hit(ray, ref hitInfo))
            {
                Vector3 w = boundingBox.vertexMax - boundingBox.vertexMin;

                Vector3 bbDiff;
                if(boundingBox.HasVertex(ref ray.origin))
                {
                    bbDiff = ray.origin;
                }
                else
                {
                    bbDiff = (ray.origin + ray.t * ray.direction);
                }

                bbDiff -= boundingBox.vertexMin;

                int ix = (int)Mathf.Clamp(bbDiff.x * nx / w.x, 0, nx - 1);
                int iy = (int)Mathf.Clamp(bbDiff.y * ny / w.y, 0, ny - 1);
                int iz = (int)Mathf.Clamp(bbDiff.z * nz / w.z, 0, nz - 1);

                float dtx = (hitInfo.tMax.x - hitInfo.tMin.x) / nx;
                float dty = (hitInfo.tMax.y - hitInfo.tMin.y) / ny;
                float dtz = (hitInfo.tMax.z - hitInfo.tMin.z) / nz;

                float tx_next, ty_next, tz_next;
                int ix_step, ix_stop;
                int iy_step, iy_stop;
                int iz_step, iz_stop;

                if(ray.direction.x > 0)
                {
                    tx_next = hitInfo.tMin.x + (ix + 1) * dtx;
                    ix_step = 1;
                    ix_stop = nx;
                }
                else if(ray.direction.x < 0)
                {
                    tx_next = hitInfo.tMin.x + (nx - ix) * dtx;
                    ix_step = -1;
                    ix_stop = -1;
                }
                else
                {
                    tx_next = float.MaxValue;
                    ix_step = -1;
                    ix_stop = -1;
                }

                if (ray.direction.y > 0)
                {
                    ty_next = hitInfo.tMin.y + (iy + 1) * dty;
                    iy_step = 1;
                    iy_stop = ny;
                }
                else if (ray.direction.y < 0)
                {
                    ty_next = hitInfo.tMin.y + (ny - iy) * dty;
                    iy_step = -1;
                    iy_stop = -1;
                }
                else
                {
                    ty_next = float.MaxValue;
                    iy_step = -1;
                    iy_stop = -1;
                }

                if (ray.direction.z > 0)
                {
                    tz_next = hitInfo.tMin.z + (iz + 1) * dtz;
                    iz_step = 1;
                    iz_stop = nz;
                }
                else if (ray.direction.z < 0)
                {
                    tz_next = hitInfo.tMin.z + (nz - iz) * dtz;
                    iz_step = -1;
                    iz_stop = -1;
                }
                else
                {
                    tz_next = float.MaxValue;
                    iz_step = -1;
                    iz_stop = -1;
                }

                while (true)
                {
                    int index = GetGridIndex(ix, iy, iz);
                    List<Object> cell = grid[index];

                    if(tx_next < ty_next && tx_next < tz_next)
                    {
                        foreach (Object obj in cell)
                        {
                            if (obj.Hit(ray, ref hitInfo))
                            {
                                hit = true;
                            }
                        }

                        ix += ix_step;

                        if ((hit && hitInfo.t < tx_next) || ix == ix_stop)
                        {
                            goto exit_loop;
                        }

                        tx_next += dtx;
                    }
                    else if (ty_next < tz_next)
                    {
                        foreach (Object obj in cell)
                        {
                            if (obj.Hit(ray, ref hitInfo))
                            {
                                hit = true;
                            }
                        }

                        if (hit && hitInfo.t < ty_next)
                        {
                            goto exit_loop;
                        }

                        iy += iy_step;

                        if ((hit && hitInfo.t < ty_next) || iy == iy_stop)
                        {
                            goto exit_loop;
                        }

                        ty_next += dty;
                    }
                    else
                    {
                        foreach (Object obj in cell)
                        {
                            if (obj.Hit(ray, ref hitInfo))
                            {
                                hit = true;
                            }
                        }

                        iz += iz_step;

                        if ((hit && hitInfo.t < tz_next) || iz == iz_stop)
                        {
                            goto exit_loop;
                        }

                        tz_next += dtz;
                    }
                }
            }

            exit_loop:
            foreach (Object obj in boundlessObjects)
            {
                if(obj.Hit(ray, ref hitInfo))
                {
                    hit = true;
                }
            }

            return hit;
        }
    }
}
