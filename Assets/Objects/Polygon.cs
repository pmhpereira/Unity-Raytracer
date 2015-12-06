using UnityEngine;
using System;

namespace Raytracing
{
    public class Polygon : Object
    {
        public Vector3[] vertices;
        public int vertexCount;

        private Vector3 normal;
        private float D;
        private int i0 = -1, i1, i2;

        public Polygon(): this(0)
        {

        }

        public Polygon(int vertexCount) : this(vertexCount, new Vector3[vertexCount])
        {
            this.vertexCount = 0; // vertices is empty
        }

        public Polygon(int vertexCount, Vector3[] vertices) : base()
        {
            this.vertexCount = vertexCount;
            this.vertices = vertices;
        }

        public void AddVertex(Vector3 vertex)
        {
            if(vertexCount < vertices.Length)
            {
                vertices[vertexCount++] = vertex;
            }
        }

        public override bool Hit(Ray ray, ref RayHit hitInfo)
        {
            bool hit = false;

            for (int i = 0; i <= vertices.Length - 3; i++)
            {
                Vector3[] triangle = new Vector3[3];
                triangle[0] = vertices[i + 0];
                triangle[1] = vertices[i + 1];
                triangle[2] = vertices[i + 2];

                if (HitTriangle(ray, triangle, ref hitInfo))
                {
                    hit = true;
                }
            }

            return hit;
        }

        private void SetupVariables()
        {
            normal = Vector3.Cross(vertices[1] - vertices[0], vertices[2] - vertices[0]).normalized;
            D = Vector3.Dot(-vertices[0], normal);

            float maxNormal = Mathf.Max(Mathf.Abs(normal.x), Mathf.Max(Mathf.Abs(normal.y), Mathf.Abs(normal.z)));

            if (maxNormal == Mathf.Abs(normal.x))
            {
                i0 = 0;
                i1 = 1;
                i2 = 2;
            }
            else if (maxNormal == Mathf.Abs(normal.y))
            {
                i0 = 1;
                i1 = 2;
                i2 = 0;
            }
            else //if (maxNormal == Mathf.Abs(normal.z))
            {
                i0 = 2;
                i1 = 0;
                i2 = 1;
            }
        }

        private bool HitTriangle(Ray ray, Vector3[] triangle, ref RayHit hitInfo)
        {
            if(i0 == -1)
            {
                SetupVariables();
            }

            float dot = Vector3.Dot(normal, ray.direction);

            if (dot == 0)
            {
                return false;
            }

            float t = -(D + Vector3.Dot(normal, ray.origin)) / dot;

            if (t < Raytracer.Epsilon || t > hitInfo.t)
                return false;

            float a = 0, b = 0;

            Vector3[] v = triangle;

            Vector3 p = ray.origin + ray.direction * t;
            float u0 = p[i1] - v[0][i1];
            float v0 = p[i2] - v[0][i2];
            float u1 = v[1][i1] - v[0][i1];
            float u2 = v[2][i1] - v[0][i1];
            float v1 = v[1][i2] - v[0][i2];
            float v2 = v[2][i2] - v[0][i2];

            if (u1 == 0)
            {
                b = u0 / u2;

                if (b >= 0 && b <= 1)
                {
                    a = (v0 - b * v2) / v1;
                }
            }
            else
            {
                b = (v0 * u1 - u0 * v1) / (v2 * u1 - u2 * v1);

                if (b >= 0 && b <= 1)
                {
                    a = (u0 - b * u2) / u1;
                }
            }

            if (a < 0 || b < 0 || a + b > 1)
                return false;

            ray.t = t;
            hitInfo.t = t;
            hitInfo.normal = normal;
            hitInfo.point = p;
            hitInfo.hitObject = this;

            return true;
        }

        public override void SetBoundingBox()
        {
            boundingBox = new PolygonBBox(this);
        }
    }
}
