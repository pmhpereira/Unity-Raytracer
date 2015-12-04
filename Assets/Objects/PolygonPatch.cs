using UnityEngine;
using System;

namespace Raytracing
{
    public class PolygonPatch : Polygon
    {
        Vector3[] normals;

        public PolygonPatch(): this(0)
        {

        }

        public PolygonPatch(int vertexCount) : this(vertexCount, new Vector3[vertexCount], new Vector3[vertexCount])
        {
            vertexCount = 0; // vertices is empty
        }

        public PolygonPatch(int vertexCount, Vector3[] vertices, Vector3[] normals) : base()
        {
            this.vertexCount = vertexCount;
            this.vertices = vertices;
            this.normals = normals;
        }

        public void AddVertex(Vector3 vertex, Vector3 normal)
        {
            if(vertexCount < vertices.Length)
            {
                vertices[vertexCount] = vertex;
                normals[vertexCount] = normal;

                vertexCount++;
            }
        }

        public override bool Hit(Ray ray, ref RayHit hitInfo)
        {
            // TODO: implement PolygonPatch.Hit()
            return false;
        }

        public override void SetBoundingBox()
        {
            boundingBox = new PolygonBBox(this);
        }
    }
}
