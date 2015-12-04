using System;
using System.IO;
using UnityEngine;

namespace Raytracing
{
    public class PlyLoader
    {
        [Flags]
        enum PlySection
        {
            Header = 1 << 0,
            Body = 1 << 1,
            Vertices = 1 << 2,
            Faces = 1 << 3,
            Edges = 1 << 4,
            Materials = 1 << 5,
            End = 1 << 6
        }

        PlySection currentSection;

        string format;
        string version;

        int verticesCount;
        int facesCount;
        int faceLength;
        int edgesCount;
        int materialsCount;

        Vector3[] vertices;
        int[] faces;
        Vector3[] edges;
        Material[] materials;
            
        bool hasVertices;
        bool hasFaces;
        bool hasEdges;
        bool hasMaterials;

        public PlyLoader(string path)
        {
            vertices = new Vector3[0];
            faces = new int[0];
            edges = new Vector3[0];
            materials = new Material[0];

            ParsePly(path);
        }

        void ParsePly(string path)
        {
            currentSection = PlySection.Header;

            StreamReader reader = File.OpenText(path);
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                string[] values = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if ((currentSection & PlySection.Header) != 0)
                {
                    ParseHeader(values);
                }
                else if ((currentSection & PlySection.Body) != 0)
                {
                    ParseBody(values);
                }
            }

            currentSection = PlySection.End;
        }

        void ParseHeader(string[] values)
        {
            switch (values[0])
            {
                case "ply":
                    break;
                case "format":
                    ParseFormat(values);
                    break;
                case "comment":
                    ParseComment(values);
                    break;
                case "element":
                    ParseElement(values);
                    break;
                case "property":
                    ParseProperty(values);
                    break;
                case "end_header":
                    currentSection = PlySection.Body;
                    break;
                default:
                    break;
            }
        }

        void ParseBody(string[] values)
        {
            if(currentSection == PlySection.Body)
            {
                currentSection = PlySection.Body | PlySection.Vertices;
            }

            if((currentSection & PlySection.Vertices) != 0)
            {
                if(verticesCount > 0)
                {
                    ParseBodyVertex(values);
                    verticesCount--;
                }

                if(verticesCount == 0)
                {
                    verticesCount = vertices.Length;
                    currentSection = PlySection.Body | PlySection.Faces;
                }
            }
            else if((currentSection & PlySection.Faces) != 0)
            {
                if(facesCount > 0)
                {
                    ParseBodyFace(values);
                    facesCount--;
                }

                if (facesCount == 0)
                {
                    facesCount = faces.Length / faceLength;
                    currentSection = PlySection.Body | PlySection.Edges;
                }
            }
            else if((currentSection & PlySection.Edges) != 0)
            {
                if(edgesCount > 0)
                {
                    ParseBodyEdge(values);
                    edgesCount--;
                }

                if(edgesCount == 0)
                {
                    edgesCount = edges.Length;
                    currentSection = PlySection.Body | PlySection.Materials;
                }
            }
            else if((currentSection & PlySection.Materials) != 0)
            {
                if(materialsCount > 0)
                {
                    ParseBodyMaterial(values);
                }

                if(materialsCount == 0)
                {
                    materialsCount = materials.Length;
                    currentSection = PlySection.End;
                }
            }
        }

        void ParseFormat(string[] values)
        {
            format = values[1];
            version = values[2];
            
            //Debug.Log(format + " " + version);
        }

        void ParseComment(string[] values)
        {
            //string comment = string.Join(" ", values.Slice(1, values.Length));

            //Debug.Log(comment);
        }

        void ParseElement(string[] values)
        {
            int count = int.Parse(values[2]);

            switch (values[1])
            {
                case "vertex":
                    currentSection = PlySection.Header | PlySection.Vertices;
                    verticesCount = count;
                    hasVertices= count > 0;
                    vertices = new Vector3[count];
                    break;
                case "face":
                    currentSection = PlySection.Header | PlySection.Faces;
                    facesCount = count;
                    hasFaces= count > 0;
                    faces = new int[count];
                    faceLength = 0;
                    break;
                case "edge":
                    currentSection = PlySection.Header | PlySection.Edges;
                    edgesCount = count;
                    hasEdges= count > 0;
                    edges = new Vector3[count];
                    break;
                case "material":
                    currentSection = PlySection.Header | PlySection.Materials;
                    materialsCount = count;
                    hasMaterials = count > 0;
                    materials = new Material[count];
                    break;
                default:
                    break;
            }
        }

        void ParseProperty(string[] values)
        {

        }

        void ParseBodyVertex(string[] values)
        {
            float x = float.Parse(values[0]);
            float y = float.Parse(values[1]);
            float z = float.Parse(values[2]);

            Vector3 vertex = new Vector3(x, y, z);
            vertices[vertices.Length - verticesCount] = vertex;
        }

        void ParseBodyFace(string[] values)
        {
            int n = int.Parse(values[0]);

            if(faceLength == 0)
            {
                faceLength = n;
                faces = new int[facesCount * faceLength];
            }

            for(int i = 1; i <= n; i++)
            {
                int index = int.Parse(values[i]);
                faces[faces.Length - facesCount * faceLength + i - 1] = index;
            }

        }

        void ParseBodyEdge(string[] values)
        {

        }

        void ParseBodyMaterial(string[] values)
        {

        }

        public Polygon[] GetPolygons()
        {
            Polygon[] mesh = new Polygon[facesCount];

            for(int f = 0; f < facesCount; f++)
            {
                Polygon polygon = new Polygon(faceLength);

                for(int i = 0; i < faceLength; i++)
                {
                    polygon.AddVertex(vertices[faces[f * faceLength + i]]);
                }

                mesh[f] = polygon;
            }

            return mesh;
        }
    }
}
