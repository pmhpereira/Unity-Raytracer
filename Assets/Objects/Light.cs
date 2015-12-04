using System;
using UnityEngine;

namespace Raytracing
{
    public class Light
    {
        public Vector3 position;
        public Color color;
        public float radius;

        public Light(Vector3 position) : this(position, Color.white, 0.5f)
        {

        }

        public Light(Vector3 position, Color color): this(position, color, 0.5f)
        {

        }

        public Light(Vector3 position, Color color, float size)
        {
            this.position = position;
            this.color = color;
            this.radius = size;
        }

        public Vector3 GetRandomPoint(Vector3 normal)
        {
            float x0, x1, x2, x3;
            float x02, x12, x22, x32;

            float denom;

            do
            {
                x0 = (float)Raytracer.Random.NextDouble() * 2 - 1;
                x1 = (float)Raytracer.Random.NextDouble() * 2 - 1;
                x2 = (float)Raytracer.Random.NextDouble() * 2 - 1;
                x3 = (float)Raytracer.Random.NextDouble() * 2 - 1;

                x02 = x0 * x0;
                x12 = x1 * x1;
                x22 = x2 * x2;
                x32 = x3 * x3;

                denom = x02 + x12 + x22 + x32;
            }
            while (denom >= 1);

            float x = 2 * (x1 * x3 + x0 * x2) / denom;
            float y = 2 * (x2 * x3 - x0 * x1) / denom;
            float z = (x02 + x32 - x12 - x22) / denom;

            Vector3 point = new Vector3(x, y, z) * radius + position;
            return point;
        }
    }
} 