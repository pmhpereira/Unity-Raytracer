namespace Raytracing
{
    public abstract class Object
    {
        public Material material;
        public BoundingBox boundingBox;

        public Object()
        {
            material = new Material();
            boundingBox = null;
        }

        public void SetMaterial(Material material)
        {
            if(material == null)
            {
                material = new Material();
            }

            this.material = material;
        }

        public abstract bool Hit(Ray ray, ref RayHit hitInfo);

        public abstract void SetBoundingBox();

        public bool HitBoundingBox(Ray ray, ref RayHit hitInfo)
        {
            bool hit = true;

            if (boundingBox != null)
            {
                hit = boundingBox.Hit(ray, ref hitInfo);
            }

            return hit;
        }
    }
}
