using UnityEngine;
using System;

namespace Raytracing
{
    public class Cylinder: Object
    {
        public Vector3 basePosition, apexPosition;
        public float baseRadius, apexRadius;

        public Cylinder(): this(Vector3.zero, 1, Vector3.up, 1)
        {

        }

        public Cylinder(Vector3 basePosition, float baseRadius, Vector3 apexPosition, float apexRadius): base()
        {
            SetBase(basePosition, baseRadius);
            SetApex(apexPosition, apexRadius);
        }

        public void SetBase(Vector3 position, float radius)
        {
            this.basePosition = position;
            this.baseRadius = radius;
        }

        public void SetApex(Vector3 position, float radius)
        {
            this.apexPosition = position;
            this.apexRadius = radius;
        }

        public override bool Hit(Ray ray, ref RayHit hitInfo)
        {
            // TODO: implement Cylinder.Hit()
            return false; 
        }

        public override void SetBoundingBox()
        {
            boundingBox = new CylinderBBox(this);
        }
    }
}
