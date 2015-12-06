using System.Collections.Generic;

namespace Raytracing
{
    public class Accelerator
    {
        public const string None = "None";
        public const string UniformGrid = "Uniform Grid";

        public static readonly string[]  Types = new string[]
        {
            None,
            UniformGrid
        };

        protected List<Object> objects;

        public Accelerator(ref List<Object> objects)
        {
            this.objects = objects;
            SetupAccelerator();
        }

        public virtual bool Hit(Ray ray, ref RayHit hitInfo)
        {
            bool hit = false;

            foreach(Object obj in objects)
            {
                hit |= obj.Hit(ray, ref hitInfo);
            }

            return hit;
        }

        public virtual void SetupAccelerator()
        {

        }
    }
}
