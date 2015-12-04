using System.Collections.Generic;
using UnityEngine;

namespace Raytracing
{
    public class UniformGrid : Accelerator
    {
        public UniformGrid(ref List<Object> objects): base(ref objects)
        {

        }

        public override void SetupAccelerator()
        {

        }

        public override bool Hit(Ray ray, ref RayHit hitInfo)
        {
            return base.Hit(ray, ref hitInfo);
        }
    }
}
