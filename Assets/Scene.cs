using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Raytracing
{
    public class Scene
    {
        public List<Object> objects;
        public List<Light> lights;

        public int width, height;

        public Camera camera;

        public Color background = Color.black;

        public Scene()
        {
            objects = new List<Object>();
        }

        public void Load(string path)
        {
            string extension = Path.GetExtension(path);

            objects = new List<Object>();

            switch (extension)
            {
                case ".ply":
                    PlyLoader ply = new PlyLoader(path);
                    objects = new List<Object>(ply.GetPolygons());
                    camera = new Camera(UnityEngine.Camera.main);
                    break;
                case ".nff":
                    NffLoader nff = new NffLoader(path);
                    objects = new List<Object>(nff.GetObjects());
                    lights = new List<Light>(nff.GetLights());

                    background = nff.GetBackgroundColor();
                    camera = nff.GetCamera();

                    width = nff.width;
                    height = nff.height;
                    break;
                default:
                    break;
            }
        }
    }
}
