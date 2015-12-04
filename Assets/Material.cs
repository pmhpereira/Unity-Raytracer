using UnityEngine;

namespace Raytracing
{
    public class Material
    {
        public Color color;
        public float kd, ks, shine, t, indexOfRefraction;

        public Material(): this(Color.white)
        {

        }

        public Material(Color color): this(color, 1, 0, 100000, 0, 1)
        {

        }

        public Material(Color color, float kd, float ks, float shine, float t, float indexOfRefraction)
        {
            this.color = color;
            this.kd = kd;
            this.ks = ks;
            this.shine = shine;
            this.t = t;
            this.indexOfRefraction = indexOfRefraction;
        }
    }
}
