using UnityEngine;

namespace Raytracing
{
    public class Camera
    {
        public Vector3 eye, at, up;
        public float fov; // in degrees

        public Camera(Vector3 eye): this(eye, Vector3.forward)
        {
        }

        public Camera(Vector3 eye, Vector3 at) : this(eye, at, Vector3.up)
        {
        }

        public Camera(Vector3 eye, Vector3 at, Vector3 up) : this(eye, at, up, 60)
        {
        }

        public Camera(Vector3 eye, Vector3 at, Vector3 up, float fov)
        {
            this.eye = eye;
            this.at = at;
            this.up = up;
            this.fov = fov;
        }

        public Camera(UnityEngine.Camera camera)
        {
            this.eye = camera.transform.position;
            this.at = camera.transform.forward;
            this.up = camera.transform.up;
            this.fov = camera.fieldOfView;
        }
    }
}
