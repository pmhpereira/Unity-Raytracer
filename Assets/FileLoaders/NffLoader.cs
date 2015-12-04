using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Raytracing
{
    public class NffLoader
    {
        Color background = Color.black;
        public int width, height;

        Vector3 eye, at, up;
        float near, fov;

        Material currentMaterial;

        [Flags]
        enum NffSection
        {
            Unspecified = 1 << 0,
            V  = 1 << 1,
            C = 1 << 2,
            P = 1 << 3,
            PP = 1 << 4,
            End = 1 << 5
        }

        NffSection currentSection;

        int contextLinesRemaining;

        List<Object> objects;
        List<Light> lights;

        Object currentObject;

        public NffLoader(string path)
        {
            ParseNff(path);
        }

        void ParseNff(string path)
        {
            objects = new List<Object>();
            lights = new List<Light>();

            currentSection = NffSection.Unspecified;

            StreamReader reader = File.OpenText(path);
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                string[] values = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                ParseLine(values);
            }

            currentSection = NffSection.End;
        }

        void ParseLine(string[] values)
        {
            if (currentSection == NffSection.V)
            {
                ParseViewport(values);
            }
            else if (currentSection == NffSection.C)
            {
                ParseConeOrCylinder(values);
            }
            else if (currentSection == NffSection.P)
            {
                ParsePolygon(values);
            }
            else if (currentSection == NffSection.PP)
            {
                ParsePolygonPatch(values);
            }

            if(contextLinesRemaining > 0)
            {
                contextLinesRemaining--;

                if (contextLinesRemaining == 0)
                {
                    currentSection = NffSection.Unspecified;
                    if(currentObject != null)
                    {
                        objects.Add(currentObject);
                        currentObject = null;
                    }
                }
                else
                {
                    return;
                }
            }

            if(currentSection != NffSection.Unspecified)
            {
                return;
            }


            switch (values[0])
            {
                case "v":
                    currentSection = NffSection.V;
                    break;
                case "b":
                    ParseBackground(values);
                    break;
                case "l":
                    ParseLight(values);
                    break;
                case "f":
                    ParseShading(values);
                    break;
                case "pl":
                    ParsePlane(values);
                    break;
                case "c":
                    currentSection = NffSection.C;
                    contextLinesRemaining = 2;

                    currentObject = new Cylinder();
                    currentObject.SetMaterial(currentMaterial);
                    break;
                case "s":
                    ParseSphere(values);
                    break;
                case "p":
                    currentSection = NffSection.P;
                    contextLinesRemaining = int.Parse(values[1]);

                    currentObject = new Polygon(contextLinesRemaining);
                    currentObject.SetMaterial(currentMaterial);
                    break;
                case "pp":
                    currentSection = NffSection.PP;
                    contextLinesRemaining = int.Parse(values[1]);

                    currentObject = new PolygonPatch(contextLinesRemaining);
                    currentObject.SetMaterial(currentMaterial);
                    break;
                default:
                    break;
            }
        }

        void ParseViewport(string[] values)
        {
            switch (values[0])
            {
                case "from":
                    eye = new Vector3(float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3]));
                    break;
                case "at":
                    at = new Vector3(float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3]));
                    break;
                case "up":
                    up = new Vector3(float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3]));
                    break;
                case "angle":
                    fov = float.Parse(values[1]);
                    break;
                case "hither":
                    near = float.Parse(values[1]);
                    break;
                case "resolution":
                    width = int.Parse(values[1]);
                    height = int.Parse(values[2]);
                    break;
                default:
                    currentSection = NffSection.Unspecified;
                    break;
            }
        }

        void ParseBackground(string[] values)
        {
            background = new Color(float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3]), 1);
        }

        void ParseLight(string[] values)
        {
            Vector3 position = new Vector3(float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3]));
            Color color = Color.white;

            if (values.Length == 7) // has color info
            {
                color = new Color(float.Parse(values[4]), float.Parse(values[5]), float.Parse(values[6]), 1);
            }

            lights.Add(new Light(position, color));
        }

        void ParseShading(string[] values)
        {
            Color color = new Color(float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3]), 1);
            float kd = float.Parse(values[4]);
            float ks = float.Parse(values[5]);
            float shine = float.Parse(values[6]);
            float t = float.Parse(values[7]);
            float indexOfRefraction = float.Parse(values[8]);

            currentMaterial = new Material(color, kd, ks, shine, t, indexOfRefraction);
        }

        void ParsePlane(string[] values)
        {
            Vector3[] vertices = new Vector3[3];
            vertices[0] = new Vector3(float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3]));
            vertices[1] = new Vector3(float.Parse(values[4]), float.Parse(values[5]), float.Parse(values[6]));
            vertices[2] = new Vector3(float.Parse(values[7]), float.Parse(values[8]), float.Parse(values[9]));

            Plane plane = new Plane(vertices);
            plane.SetMaterial(currentMaterial);
            objects.Add(plane);
        }

        void ParseConeOrCylinder(string[] values)
        {
            Vector3 position = new Vector3(float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3]));
            float radius = float.Parse(values[4]);

            if(contextLinesRemaining == 2)
            {
                ((Cylinder)currentObject).SetBase(position, radius);
            }
            else if(contextLinesRemaining == 1)
            {
                ((Cylinder)currentObject).SetApex(position, radius);
            }
        }

        void ParseSphere(string[] values)
        {
            Vector3 position = new Vector3(float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3]));
            float radius = float.Parse(values[4]);

            Sphere sphere = new Sphere(position, radius);
            sphere.SetMaterial(currentMaterial);
            objects.Add(sphere);
        }

        void ParsePolygon(string[] values)
        {
            Vector3 vertex = new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
            ((Polygon)currentObject).AddVertex(vertex);
        }

        void ParsePolygonPatch(string[] values)
        {
            Vector3 vertex = new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
            Vector3 normal = new Vector3(float.Parse(values[3]), float.Parse(values[4]), float.Parse(values[5]));
            ((PolygonPatch)currentObject).AddVertex(vertex, normal);
        }

        void ParseComment(string[] values)
        {
            throw new NotImplementedException();
        }

        public Object[] GetObjects()
        {
            return objects.ToArray();
        }

        public Light[] GetLights()
        {
            return lights.ToArray();
        }

        public Camera GetCamera()
        {
            return new Camera(eye, at, up, fov);
        }

        public Color GetBackgroundColor()
        {
            return background;
        }
    }
}
