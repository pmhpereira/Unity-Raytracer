using UnityEngine;
using UnityEditor;
using System.IO;
using System.Threading;
using System.Collections;
using System;

namespace Raytracing
{
    public class AntiAliasing
    {
        public const string None = "None";
        public const string Regular = "Regular";
        public const string Adaptive = "Adaptive";
        public const string Stochastic = "Stochastic";

        public static float PixelThreshold = 0.3f;

        public static readonly string[] Types = new string[]
        {
            None,
            Regular,
            Adaptive,
            Stochastic
        };
    }

    public class Shadow
    {
        public const string Hard = "Hard";
        public const string Soft = "Soft";

        public static readonly string[] Types = new string[]
        {
            Hard,
            Soft
        };
    }
    
    public class ThreadedRandom
    {
        private System.Random instance = new System.Random(Guid.NewGuid().GetHashCode());

        public double NextDouble()
        {
            lock(this)
            {
                return instance.NextDouble();
            }
        }
    }
    public class Raytracer : MonoBehaviour
    {
        public static readonly float Epsilon = 0.0001f;
        
        private Texture2D image;

        public int width, height;
        private int screenWidth, screenHeight;

        public int pixelSize;
        int factorWidth, factorHeight;

        public int antiAliasing;

        public int recursionDepth;

        public float refreshTime;

        private Color[] pixels;

        public static ThreadedRandom Random;
        public float startTime, endTime;
        public bool isRunning, isCompleted;

        public int rayCount;
        private int screenRayCount;
        private int maxScreenRayCount;

        private bool runThread;

        public bool overrideSettings;

        public Scene scene;

        public Accelerator accelerator;

        public int shadowType;
        public int shadowRays;
        public int acceleratorType;
        public int antiAliasingType;

        public static int gridMultiplier;

        public string filename;

        public bool hasLighting;

        public bool depthOfField;
        public int dofRays;
        public float dofAperture;
        private float dofAperture_inv;
        public float dofFocalDistance;

        private Thread raytracerThread;
        public int threadCount;

        public int xMin, xMax, yMin, yMax;

        void Start()
        {
            scene = new Scene();

            screenWidth = Screen.width;
            screenHeight = Screen.height;

            StartCoroutine(Refresh());
        }
        
        void BuildAccelerator()
        {
            string accelerationStructure = Accelerator.Types[acceleratorType];

            switch(accelerationStructure)
            {
                case Accelerator.None:
                default:
                    accelerator = new Accelerator(ref scene.objects);
                    break;
            }
        }

        public void StartRaytrace()
        {
            if (!overrideSettings)
            {
                if(scene.width == 0 && scene.height == 0)
                {
                    width = screenWidth;
                    height = screenHeight;
                }
                else
                {
                    width = scene.width;
                    height = scene.height;

                }

                xMin = 0;
                xMax = width;
                yMin = 0;
                yMax = height;

                threadCount = Environment.ProcessorCount;
            }
            else
            {
                xMin = Mathf.Max(0, xMin);
                yMin = Mathf.Max(0, yMin);

                if (xMax < 0)
                {
                    xMax = width - xMax;
                }

                if (yMax < 0)
                {
                    yMax = height - yMax;
                }
            }

            factorWidth = width / pixelSize;
            factorHeight = height / pixelSize;

            pixels = new Color[factorWidth * factorHeight];

            image = new Texture2D(factorWidth, factorHeight);
            image.filterMode = FilterMode.Point;

            startTime = Time.realtimeSinceStartup;
            isRunning = true;
            isCompleted = false;
            runThread = true;

            rayCount = 0;
            screenRayCount = 0;
            maxScreenRayCount = factorWidth * factorHeight;
            dofAperture_inv = 1 / dofAperture;

            BuildAccelerator();

            if (Shadow.Types[shadowType] == Shadow.Hard)
            {
                shadowRays = 1;
            }

            raytracerThread = new Thread(Raytrace);
            raytracerThread.Priority = System.Threading.ThreadPriority.Highest;
            raytracerThread.Start();
        }

        ManualResetEvent[] waitHandles = null;

        void Raytrace()
        {
            Random = new ThreadedRandom();

            if (threadCount > 1)
            {
                ThreadPool.SetMaxThreads(threadCount, threadCount);
                int pixel = 0;
                waitHandles = new ManualResetEvent[threadCount];
                for(int i = 0; i < waitHandles.Length; i++)
                {
                    waitHandles[i] = new ManualResetEvent(true);
                }

                while(pixel < pixels.Length)
                {
                    for(int i = 0; i < waitHandles.Length && pixel < pixels.Length; i++)
                    {
                        if (!runThread)
                        {
                            return;
                        }

                        if(waitHandles[i].WaitOne(0))
                        {
                            int pixelMin = pixel;
                            pixel += pixels.Length / (threadCount * threadCount);
                            int pixelMax = pixel;
                            waitHandles[i].Reset();

                            ThreadPool.QueueUserWorkItem(RaytraceWorker, new ThreadData(i, pixelMin, pixelMax, waitHandles[i]));
                        }
                    }
                }

                WaitHandle.WaitAll(waitHandles);
            }
            else
            {
                RaytraceWorker();
            }
        }

        struct ThreadData {
            public int id, pixelMin, pixelMax;
            public ManualResetEvent waitHandle;

            public ThreadData(int id, int pixelMin, int pixelMax, ManualResetEvent waitHandle)
            {
                this.id = id;
                this.pixelMin = pixelMin;
                this.pixelMax = pixelMax;
                this.waitHandle = waitHandle;
            }
        }

        void RaytraceWorker(object data = null)
        {
            int _xMin = xMin / pixelSize;
            int _yMin = Mathf.Max(0, yMin) / pixelSize;
            int _xMax = Mathf.Min(width, xMax) / pixelSize;
            int _yMax = Mathf.Min(height, yMax) / pixelSize;

            if (data != null)
            {
                ThreadData tData = (ThreadData)data;

                for (int p = tData.pixelMin; p < tData.pixelMax; p++)
                {
                    int x = p % factorWidth;
                    int y = p / factorWidth;

                    if(x >= _xMin && x <= _xMax && y >= _yMin && y <= _yMax)
                    {
                        TracePixel(p);
                    }
                }

                waitHandles[tData.id].Set();
            }
            else
            {

                for (int y = (int)_yMin; y < _yMax; y++)
                {
                    for (int x = (int)_xMin; x < _xMax; x++)
                    {
                        TracePixel(x + y * factorWidth);
                    }
                }
            }
        }

        void TracePixel(int pixel)
        {
            if (!runThread || pixel < 0 || pixel >= pixels.Length)
            {
                return;
            }

            int x = pixel % factorWidth;
            int y = pixel / factorWidth;

            pixels[pixel] = TraceRay(x, y);
            screenRayCount++;
        }

        Color TraceRaySimple(float x, float y)
        {
            Ray ray = ScreenPointToRay(x, y);

            if (depthOfField)
            {
                Color finalColor = new Color(0, 0, 0, 0);

                for (int i = 0; i < dofRays; i++)
                {
                    finalColor += TraceJitteredRay(ray);
                }

                return finalColor / dofRays;
            }
            else
            {
                Color pixel = TraceRay(ray);
                return pixel;
            }
        }

        Color TraceRayRegular(float x, float y)
        {
            float inv_antiAliasing = 1f / antiAliasing;
            float inv_antiAliasing2 = inv_antiAliasing * inv_antiAliasing;

            Color pixel = new Color(0, 0, 0, 0);
            for (int dy = 1; dy <= antiAliasing; dy++)
            {
                for (int dx = 1; dx <= antiAliasing; dx++)
                {
                    pixel += TraceRaySimple(x + dx * inv_antiAliasing, y + dy * inv_antiAliasing) * inv_antiAliasing2;
                }
            }

            return pixel;
        }

        Color TraceRayAdaptive(float x, float y, float step = 1)
        {
            Color sum = new Color(0, 0, 0, 0);
            Color[] corners = new Color[4];
            corners[0] = TraceRaySimple(x, y);
            corners[1] = TraceRaySimple(x + step, y);

            if (corners[0].Difference(corners[1]) <= AntiAliasing.PixelThreshold)
            {
                corners[2] = TraceRaySimple(x, y + step);

                if (corners[0].Difference(corners[2]) <= AntiAliasing.PixelThreshold)
                {
                    corners[3] = TraceRaySimple(x + step, y + step);

                    if (corners[0].Difference(corners[3]) <= AntiAliasing.PixelThreshold)
                    {
                        return (corners[0] + corners[1] + corners[2] + corners[3]) / 4;
                    }
                }
            }

            float half = step / 2;
            if (half <= 1f / antiAliasing)
            {
                return corners[0];
            }

            return (TraceRayAdaptive(x, y, half)
                    + TraceRayAdaptive(x + half, y, half)
                    + TraceRayAdaptive(x, y + half, half)
                    + TraceRayAdaptive(x + half, y + half, half)) / 4;
        }

        Color TraceRayStochastic(float x, float y)
        {
            float inv_antiAliasing = 1f / antiAliasing;
            float inv_antiAliasing2 = inv_antiAliasing * inv_antiAliasing;

            Color pixel = new Color(0, 0, 0, 0);

            for (int dy = 1; dy <= antiAliasing; dy++)
            {
                for (int dx = 1; dx <= antiAliasing; dx++)
                {
                    float rx = (float)Random.NextDouble();
                    float ry = (float)Random.NextDouble();

                    pixel += TraceRaySimple(x + (dx + rx) * inv_antiAliasing, y + (dy + ry) * inv_antiAliasing) * inv_antiAliasing2;
                }
            }

            return pixel;
        }

        Color TraceRay(float x, float y)
        {
            switch(AntiAliasing.Types[antiAliasingType])
            {
                case AntiAliasing.Regular:
                    return TraceRayRegular(x, y);
                case AntiAliasing.Adaptive:
                    return TraceRayAdaptive(x, y);
                case AntiAliasing.Stochastic:
                    return TraceRayStochastic(x, y);
                case AntiAliasing.None:
                default:
                    return TraceRaySimple(x, y);
            }
        }

        Color TraceRay(Ray ray, int depth = 0, float indexOfRefraction = 1, float normalDirection = 1)
        {
            if (!runThread)
            {
                return Color.black;
            }
            
            RayHit hitInfo = new RayHit();

            bool hit = Raycast(ray, ref hitInfo);

            Color diffuseColor = new Color(0, 0, 0, 0);
            Color specularColor = new Color(0, 0, 0, 0);
            Color reflectedColor = Color.black;
            Color refractedColor = Color.black;

            if (hit)
            {
                if(!hasLighting)
                {
                    diffuseColor += hitInfo.hitObject.material.color * hitInfo.hitObject.material.kd;
                }
                else
                {
                    int lightsHit = scene.lights.Count;

                    foreach (Light light in scene.lights)
                    {
                        Vector3 lightDirection = (light.position - hitInfo.point).normalized;
                        float cosine = Vector3.Dot(hitInfo.normal, lightDirection);

                        for (int i = 0; i < shadowRays; i++)
                        {
                            Vector3 randomLightPosition = light.position;

                            if(shadowRays > 1)
                            {
                                randomLightPosition = light.GetRandomPoint(-lightDirection);
                                lightDirection = (randomLightPosition - hitInfo.point).normalized;
                            }

                            if(cosine > 0)
                            {
                                Ray lightRay = new Ray(hitInfo.point, lightDirection);

                                RayHit shadowInfo = new RayHit();

                                if (!Raycast(lightRay, ref shadowInfo))
                                {
                                    lightsHit--;

                                    diffuseColor += hitInfo.hitObject.material.color *  light.color * cosine * hitInfo.hitObject.material.kd / shadowRays;

                                    Vector3 reflected = hitInfo.normal * 2 * Vector3.Dot(lightDirection, hitInfo.normal) - lightDirection;
                                    float cosine2 = Vector3.Dot(reflected, -ray.direction);

                                    if(cosine2 > 0)
                                    {
                                        float amount = Mathf.Pow(cosine2, hitInfo.hitObject.material.shine);
                                        specularColor += light.color * hitInfo.hitObject.material.ks * amount / shadowRays;
                                    }
                                }
                            }
                        }
                    }
                }

                if (depth < recursionDepth)
                {
                    int nextDepth = depth + 1;

                    float refractiveness = hitInfo.hitObject.material.t;
                    float reflectiveness = hitInfo.hitObject.material.ks;

                    if (reflectiveness > 0)
                    {
                        Vector3 reflectionDirection = (hitInfo.normal * 2 * Vector3.Dot(-ray.direction, hitInfo.normal) + ray.direction).normalized;
                        Ray reflectedRay = new Ray(hitInfo.point, reflectionDirection);
                        reflectedColor = TraceRay(reflectedRay, nextDepth, indexOfRefraction, normalDirection) * reflectiveness;
                    }

                    if(refractiveness > 0)
                    {
                        float newIndexOfRefraction = 1;

                        if (normalDirection == 1)
                        {
                            newIndexOfRefraction = hitInfo.hitObject.material.indexOfRefraction;
                        }

                        float n = indexOfRefraction / newIndexOfRefraction;

                        float cosI = -Vector3.Dot(hitInfo.normal * normalDirection, ray.direction);
                        float sin = n * n * (1 - cosI * cosI);

                        if(sin > 0)
                        {
                            screenRayCount++;

                            float cosT = Mathf.Sqrt(1 - sin);

                            Vector3 refractionDirection = (ray.direction * n + hitInfo.normal * (n * cosI - cosT)).normalized;
                            Ray refractedRay = new Ray(hitInfo.point, refractionDirection);
                            refractedColor = TraceRay(refractedRay, nextDepth, newIndexOfRefraction, -normalDirection) * refractiveness;
                        }
                    }
                }

                return diffuseColor + specularColor + reflectedColor + refractedColor;
            }
            else
            {
                return scene.background;
            }
        }

        Ray ScreenPointToRay(float x, float y)
        {
            Camera camera = scene.camera;

            Vector3 z = camera.eye - camera.at;
            float df = z.magnitude;

            float h = 2 * df * Mathf.Tan((Mathf.Deg2Rad * camera.fov) / 2);
            float w = ((float)width / height) * h;

            Vector3 ze = z.normalized;
            Vector3 xe = Vector3.Cross(ze, camera.up).normalized;
            Vector3 ye = Vector3.Cross(xe, ze);

            Vector3 d = (-df * ze + h * (y / (factorHeight - 1) - 0.5f) * ye + w * (x / (factorWidth - 1) - 0.5f) * xe).normalized;

            Ray ray = new Ray(camera.eye, d);
            return ray;
        }

        Color TraceJitteredRay(Ray ray)
        {
            // stochastic jittered ray
            float du = (float)Random.NextDouble();
            float dv = (float)Random.NextDouble();

            Vector3 n = (scene.camera.eye - scene.camera.at).normalized;
            Vector3 u = Vector3.Cross(n, scene.camera.up).normalized;
            Vector3 v = Vector3.Cross(u, n).normalized;

            //new camera position
            Vector3 newEye = ray.origin - u * (dofAperture_inv / 2) - v * (dofAperture_inv / 2) + u * du * dofAperture_inv + v * dv * dofAperture_inv;

            //new ray dir
            Vector3 AimedPoint = ray.origin + ray.direction * dofFocalDistance;
            Vector3 newDir = AimedPoint - newEye;

            ray.origin = newEye;
            ray.direction = newDir.normalized;

            return TraceRay(ray);
        }

        bool Raycast(Ray ray, ref RayHit hitInfo)
        {
            lock(this)
            {
                rayCount++;
            }

            return accelerator.Hit(ray, ref hitInfo);
        }

        IEnumerator Refresh()
        {
            if(pixels != null)
            {
                image.SetPixels(pixels);
                image.Apply();
            }

            yield return new WaitForSeconds(refreshTime);
            yield return StartCoroutine(Refresh());
        }

        void Update()
        {
            if(raytracerThread != null && !raytracerThread.IsAlive && isRunning)
            {
                StopRayTracer();
            }
        }

        void OnGUI()
        {
            if (raytracerThread != null)
            {
                GUI.DrawTexture(new Rect(0, 0, width, height), image);
            }
        }

        public void LoadScene()
        {
            string[] formats = new string[] { "Scene files", "ply,nff" };

            string path = EditorUtility.OpenFilePanelWithFilters("Load Scene", Application.dataPath + "/Scenes", formats);

            if (!string.IsNullOrEmpty(path))
            {
                scene = new Scene();
                scene.Load(path);
                filename = Path.GetFileName(path);
            }
        }

        public void SaveImage()
        {
            string defaultFilename = filename.Split('.')[0];
            defaultFilename += "&depth=" + recursionDepth;
            defaultFilename += "&resolution=" + factorWidth + "x" + factorHeight;
            defaultFilename += "&aliasing=" + antiAliasing;

            string path = EditorUtility.SaveFilePanelInProject("Save Image", defaultFilename, "png", "", Application.dataPath + "/Renderings");

            if (!string.IsNullOrEmpty(path))
            {
                File.WriteAllBytes(path, image.EncodeToPNG());
            }
        }

        public float GetFinishedPercentage()
        {
            return (float)screenRayCount / maxScreenRayCount;
        }

        public void StopRayTracer()
        {
            endTime = Time.realtimeSinceStartup;
            runThread = false;

            isRunning = false;
            isCompleted = true;
        }

        void OnDestroy()
        {
            StopRayTracer();
        }
    }

    [CustomEditor(typeof(Raytracer))]
    public class RaytracerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            Raytracer raytracer = (Raytracer)target;

            raytracer.refreshTime = EditorGUILayout.Slider("Refresh Time", raytracer.refreshTime, 0, 1);

            GUI.enabled = !raytracer.isRunning;
            int[] pixelSizeValues = new int[] { 1, 2, 4, 8, 16 };
            
            EditorGUILayout.Space();
            raytracer.hasLighting = EditorGUILayout.Toggle("Lights", raytracer.hasLighting);
            if(raytracer.hasLighting)
            {
                raytracer.shadowType = EditorGUILayout.Popup("Shadows", raytracer.shadowType, Shadow.Types);
                if (Shadow.Types[raytracer.shadowType] == Shadow.Soft)
                {
                    raytracer.shadowRays = SnappingIntSlider("Shadow Rays", raytracer.shadowRays, new int[] { 16, 32, 64, 128, 256, 512 });
                }
            }

            EditorGUILayout.Space();
            raytracer.recursionDepth = EditorGUILayout.IntSlider("Recursion Depth", raytracer.recursionDepth, 0, 4);

            EditorGUILayout.Space();
            raytracer.antiAliasingType = EditorGUILayout.Popup("Anti-Aliasing", raytracer.antiAliasingType, AntiAliasing.Types);

            if(AntiAliasing.Types[raytracer.antiAliasingType] == AntiAliasing.Regular || AntiAliasing.Types[raytracer.antiAliasingType] == AntiAliasing.Stochastic)
            {
                raytracer.antiAliasing = SnappingIntSlider("AA Multiplier", raytracer.antiAliasing, new int[] { 2, 4, 8, 16 });
            }
            else if(AntiAliasing.Types[raytracer.antiAliasingType] == AntiAliasing.Adaptive)
            {
                raytracer.antiAliasing = SnappingIntSlider("AA Multiplier", raytracer.antiAliasing, new int[] { 4, 8, 16 });
                AntiAliasing.PixelThreshold = EditorGUILayout.Slider("Pixel Threshold", AntiAliasing.PixelThreshold, 0, 3);
            }

            EditorGUILayout.Space();
            raytracer.depthOfField = EditorGUILayout.Toggle("Depth of Field", raytracer.depthOfField);
            if(raytracer.depthOfField)
            {
                raytracer.dofFocalDistance = EditorGUILayout.FloatField("DoF Focal Distance", raytracer.dofFocalDistance);
                raytracer.dofAperture = EditorGUILayout.Slider("DoF Aperture", raytracer.dofAperture, 1, 3);
                raytracer.dofRays = SnappingIntSlider("DoF Rays", raytracer.dofRays, new int[] { 4, 8, 16, 32, 64 });
            }

            EditorGUILayout.Space();
            raytracer.acceleratorType = EditorGUILayout.Popup("Acceleration Structure", raytracer.acceleratorType, Accelerator.Types);

            EditorGUILayout.Space();
            raytracer.overrideSettings = EditorGUILayout.Toggle("Override Settings", raytracer.overrideSettings);
            if(raytracer.overrideSettings)
            {
                raytracer.threadCount = EditorGUILayout.IntSlider("Threads", raytracer.threadCount, 1, Environment.ProcessorCount);
                raytracer.pixelSize = SnappingIntSlider("Pixel Size", raytracer.pixelSize, new int[] { 1, 2, 4, 8, 16 });

                EditorGUILayout.Space();
                Vector2 screenResolution = EditorGUILayout.Vector2Field("Screen Resolution", new Vector2(raytracer.width, raytracer.height));
                raytracer.width = (int) screenResolution.x;
                raytracer.height = (int) screenResolution.y;

                EditorGUILayout.Space();
                Vector2 lowerLeft = EditorGUILayout.Vector2Field("Lower Left", new Vector2(raytracer.xMin, raytracer.yMin));
                raytracer.xMin = (int)Mathf.Max(0, Mathf.Min(lowerLeft.x, screenResolution.x));
                raytracer.yMin = (int)Mathf.Max(0, Mathf.Min(lowerLeft.y, screenResolution.y));

                Vector2 upperRight = EditorGUILayout.Vector2Field("Upper Right", new Vector2(raytracer.xMax, raytracer.yMax));
                raytracer.xMax = (int)Mathf.Min(screenResolution.x, Mathf.Max(lowerLeft.x, upperRight.x));
                raytracer.yMax = (int)Mathf.Min(screenResolution.y, Mathf.Max(lowerLeft.y, upperRight.y));
            }

            EditorGUILayout.Space();
            if (Application.isPlaying)
            {
                GUI.enabled = !raytracer.isRunning;

                string loadButtonText = "Load scene";
                if(!string.IsNullOrEmpty(raytracer.filename))
                {
                    loadButtonText += " (" + raytracer.filename + ")";
                }
                if (GUILayout.Button(loadButtonText))
                {
                    raytracer.LoadScene();
                }

                GUI.enabled = raytracer.scene.objects.Count > 0;
                if (!raytracer.isRunning)
                {
                    if (GUILayout.Button("Raytrace"))
                    {
                        raytracer.StartRaytrace();
                    }
                }
                else
                {
                    if (GUILayout.Button("Stop raytrace"))
                    {
                        raytracer.StopRayTracer();
                    }
                }

                GUI.enabled = true;
                string statsString = "";
                float time = 0;
                if (raytracer.isRunning)
                {
                    time = Time.realtimeSinceStartup - raytracer.startTime;
                }
                else
                {
                    time = raytracer.endTime - raytracer.startTime;
                }

                statsString += "Time: " + time.ToString("n2") + "s";

                if (raytracer.isRunning)
                {
                    statsString += " ";
                    statsString += "(" + (raytracer.GetFinishedPercentage() * 100).ToString("n2") + "%)";
                }

                statsString += "\n";
                statsString += "Objects: " + raytracer.scene.objects.Count;
                statsString += "\n";
                statsString += "Rays: " + raytracer.rayCount;

                EditorGUILayout.HelpBox(statsString, MessageType.Info);

                GUI.enabled = raytracer.isCompleted;
                if (GUILayout.Button("Save image"))
                {
                    raytracer.SaveImage();
                }

                Repaint();
            }
        }

        int SnappingIntSlider(string label, int value, int[] values)
        {
            return EditorGUILayout.IntSlider(label, values.CloserTo(value), values[0], values[values.Length - 1]);
        }
    }
}