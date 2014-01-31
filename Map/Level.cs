using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Threading;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using RacingGame.Collision;
using RacingGame.Graphics;

namespace RacingGame.Map
{
    struct SpawnPoint
    {
        public Vector3 pos;
        public Vector3 dir;

        public SpawnPoint(Vector3 position, Vector3 direction)
        {
            pos = position;
            dir = direction;
        }
    }

    class Level : Node
    {
        static private String levelFile;
        static public Thread loaderThread;

        static private String modelFile;
        static private Model level;
        static private ModelMesh levelMesh;
        static private ModelMesh sceneryMesh;
        static private ModelMesh lightsMesh;
        static private Texture levelTexture;
        static private Texture sceneryTexture;
        static private Texture lightsTexture;
        static private CollisionObject levelCollisionObject;

        static public List<Light> lights { get; private set; }
        static public List<Vector3> powerups { get; private set; }
        static public List<SpawnPoint> startingPoints { get; private set; }
        static public List<Vector3> boosts { get; private set; }
        static public List<BoundingBox> checkPoints { get; private set; }

        class LightTypeInfo
        {
            public int index;
            public LightType type;
            public Vector3 color;
            public float range;
            public float aspect;
            public float fovY;
        };

        static private List<LightTypeInfo> lightTypes = new List<LightTypeInfo>();

        public static void startLoad(String xmlLevel)
        {
            levelFile = xmlLevel;

            parseXML(levelFile);

            level = RacingGame.contentManager.Load<Model>(modelFile);
            levelMesh = findMesh("Track");
            levelTexture = ((BasicEffect)levelMesh.MeshParts[0].Effect).Texture;

            sceneryMesh = findMesh("Scenery");
            sceneryTexture = ((BasicEffect)sceneryMesh.MeshParts[0].Effect).Texture;
            //levelMesh = findMesh("Collision");

            lightsMesh = findMesh("SceneryLamps");
            lightsTexture = ((BasicEffect)lightsMesh.MeshParts[0].Effect).Texture;

            List<Triangle> spawnPoints = parseMesh(findMesh("StartPos"));
            startingPoints = new List<SpawnPoint>();
            foreach (Triangle t in spawnPoints)
            {
                startingPoints.Add(new SpawnPoint(average(t), t.normal));
            }

            powerups = average(parseMesh(findMesh("Power")));
            boosts = average(parseMesh(findMesh("Boost")));

            checkPoints = new List<BoundingBox>();
            for (int i = 0; i < 9; ++i)
            {
                ModelMesh m = findMesh(i.ToString() + "_CheckPoint");

                if (m == null)
                {
                    break;
                }

                checkPoints.Add(bbox(parseMesh(m)));
            }

            lights = new List<Light>();
            for (int i = 0; i < 9; ++i)
            {
                ModelMesh m = findMesh(i.ToString() + "_Light");

                if (m == null)
                {
                    break;
                }
                foreach (Triangle t in parseMesh(m))
                {
                    LightTypeInfo lti = getLTI(i);
                    switch (lti.type)
                    {
                        case LightType.Spot:
                            SpotLight light = new SpotLight();
                            light.diffuse = lti.color;
                            light.setDirection(t.normal, Vector3.Left);
                            light.setProjection(lti.fovY, lti.aspect, lti.range);
                            light.position = average(t);
                            lights.Add(light);
                            break;
                    };
                }
            }

            loaderThread = new Thread(new ThreadStart(load));
            loaderThread.Start();
        }

        public void addCollisionObject()
        {
            CollisionManager.getInstance().addMapObject(levelCollisionObject);
        }

        static void load()
        {

            CollisionManager.getInstance().clear();
            TriangleMesh triMesh = new TriangleMesh();
            //triMesh.loadModelMesh(RacingGame.contentManager.Load<Model>("Models/Tiles/Track_02_Tile_01").Meshes[1]);
            triMesh.loadModelMesh(findMesh("Collision"));
            levelCollisionObject = new KDTreeObject(triMesh);
            levelCollisionObject.position = Vector3.Zero;
            levelCollisionObject.rotation = Quaternion.Identity;
            levelCollisionObject.uniformScale = 1.0f;
            levelCollisionObject.updateTransform();
            CollisionManager.getInstance().addMapObject(levelCollisionObject);
         }


        public override void render()
        {
            if (loaderThread.IsAlive)
                return;

            GraphicsManager.getInstance().setWorldMatrix(Matrix.Identity);

            renderMesh(levelMesh, levelTexture);

            renderMesh(sceneryMesh, sceneryTexture);

            renderMesh(lightsMesh, lightsTexture);
        }

        private void renderMesh(ModelMesh mesh, Texture texture)
        {
            GraphicsDevice device = GraphicsManager.getDevice();

            GraphicsManager.getInstance().setTexture(texture);
            GraphicsManager.getInstance().updateShader();
            device.Indices = mesh.IndexBuffer;

            foreach (ModelMeshPart meshPart in mesh.MeshParts)
            {
                device.Vertices[0].SetSource(mesh.VertexBuffer, meshPart.StreamOffset,
                    meshPart.VertexStride);

                device.VertexDeclaration = meshPart.VertexDeclaration;

                device.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                    meshPart.BaseVertex, 0, meshPart.NumVertices, meshPart.StartIndex,
                    meshPart.PrimitiveCount);
            }
        }

        public override RenderQueues getRenderQueue()
        {
            return RenderQueues.Normal;
        }

        private static LightTypeInfo getLTI(int index)
        {
            foreach (LightTypeInfo lti in lightTypes)
            {
                if (lti.index == index)
                {
                    return lti;
                }
            }

            return null;
        }

        private static ModelMesh findMesh(String name)
        {
            foreach (ModelMesh m in level.Meshes)
            {
                if (m.Name.EndsWith(name + "Object"))
                {
                    return m;
                }
            }

            return null;
        }

        private static void parseXML(String fileName)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load("Content\\Levels\\" + fileName);

            XmlNode root = xml.FirstChild;
            while (root != null && root.NodeType != XmlNodeType.Element && root.Name != "SLERP3D")
            {
                root = root.NextSibling;
            }

            XmlNode child = root.FirstChild;

            while (child != null)
            {
                if (child.NodeType == XmlNodeType.Element)
                {
                    if (child.Name == "LevelFile")
                    {
                        modelFile = "Models\\Tiles\\" + child.InnerText;
                    }
                    else if (child.Name == "LightType")
                    {
                        LightTypeInfo ltInf = new LightTypeInfo();
                        ltInf.index = Convert.ToInt32(child.Attributes["index"].Value);

                        XmlNode ltiNode = child.FirstChild;

                        while (ltiNode != null)
                        {
                            if (ltiNode.Name == "Type")
                            {
                                String type = ltiNode.InnerText;

                                switch (type)
                                {
                                    case "Spot":
                                        ltInf.type = LightType.Spot;
                                        break;
                                    default:
                                        ltInf.type = LightType.None;
                                        break;
                                }
                            } 
                            else if (ltiNode.Name == "Color")
                            {
                                ltInf.color = new Vector3((float)Convert.ToDouble(ltiNode.Attributes["x"].Value),
                                                          (float)Convert.ToDouble(ltiNode.Attributes["y"].Value),
                                                          (float)Convert.ToDouble(ltiNode.Attributes["z"].Value));
                            } 
                            else if (ltiNode.Name == "Range")
                            {
                                ltInf.range = (float)Convert.ToDouble(ltiNode.InnerText);
                            } 
                            else if (ltiNode.Name == "Aspect")
                            {
                                ltInf.aspect = (float)Convert.ToDouble(ltiNode.InnerText);
                            }
                            else if(ltiNode.Name == "FoV")
                            {
                                ltInf.fovY = (float)Convert.ToDouble(ltiNode.InnerText);
                            }

                            lightTypes.Add(ltInf);

                            ltiNode = ltiNode.NextSibling;
                        }
                    }
                }

                child = child.NextSibling;
            }

            /*
            while (xmlReader.Read())
            {
                if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "LevelFile")
                {
                    modelFile = "Models\\Tiles\\" + xmlReader.ReadElementContentAsString();
                }
                else if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "LightType")
                {
                    xmlReader.GetAttribute(
                }
            }*/
        }

        private static BoundingBox bbox(List<Triangle> triangles)
        {
            BoundingBox bb = new BoundingBox(new Vector3(float.MaxValue), new Vector3(float.MinValue));

            foreach (Triangle t in triangles)
            {
                bb.Min = Vector3.Min(t.vertex1, bb.Min);
                bb.Min = Vector3.Min(t.vertex2, bb.Min);
                bb.Min = Vector3.Min(t.vertex2, bb.Min);

                bb.Max = Vector3.Max(t.vertex1, bb.Max);
                bb.Max = Vector3.Max(t.vertex2, bb.Max);
                bb.Max = Vector3.Max(t.vertex2, bb.Max);
            }

            return bb;
        }

        private static List<Vector3> average(List<Triangle> triangles)
        {
            List<Vector3> avg = new List<Vector3>();
            avg.Capacity = triangles.Count();

            foreach (Triangle t in triangles)
            {
                avg.Add(average(t));
            }

            return avg;
        }

        private static Vector3 average(Triangle t)
        {
            Vector3 avg = t.vertex1 + t.vertex2 + t.vertex3;
            avg /= 3.0f;

            return avg;           
        }

        private static List<Triangle> parseMesh(ModelMesh m)
        {
            if (m == null)
            {
                return new List<Triangle>();
            }

            int[] indices;

            if (m.IndexBuffer.IndexElementSize == IndexElementSize.SixteenBits)
            {
                indices = new int[m.IndexBuffer.SizeInBytes / sizeof(short)];
                short[] tmpIndices = new short[indices.Length];

                m.IndexBuffer.GetData(tmpIndices);

                for (int i = 0; i < indices.Length; ++i)
                {
                    indices[i] = tmpIndices[i];
                }
            }
            else
            {
                indices = new int[m.IndexBuffer.SizeInBytes / sizeof(int)];
                m.IndexBuffer.GetData(indices);
            }

            float[] verts = new float[m.VertexBuffer.SizeInBytes / sizeof(float)];
            m.VertexBuffer.GetData(verts);

            int stride = m.MeshParts[0].VertexStride;
            int offs = 0;

            foreach (VertexElement v in m.MeshParts[0].VertexDeclaration.GetVertexElements())
            {
                if (v.VertexElementUsage == VertexElementUsage.Position && v.UsageIndex == 0)
                {
                    offs = v.Offset;
                    break;
                }
            }

            int numVerts = m.VertexBuffer.SizeInBytes / stride;
            Vector3[] positions = new Vector3[numVerts];

            for (int i = 0; i < numVerts; ++i)
            {
                positions[i].X = verts[i * (stride / 4) + offs];
                positions[i].Y = verts[i * (stride / 4) + offs + 1];
                positions[i].Z = verts[i * (stride / 4) + offs + 2];
            }

            List<Triangle> triangles = new List<Triangle>();
            triangles.Capacity = indices.Length / 3;

            for (int i = 0; i < indices.Length / 3; ++i)
            {
                triangles.Add(new Triangle(positions[indices[i * 3]], positions[indices[i * 3 + 1]], positions[indices[i * 3 + 2]]));
            }

            return triangles;
        }

        /*Scene scene = null;
        private int xDim = 0; 
        private int zDim = 0;
        private int tileSize = 0;
        private SimpleTile[] tiles = null;

        public Level(String xmlFile, Scene scene) 
        {
            this.scene = scene;

            Collision.CollisionManager.getInstance().clear();

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            settings.IgnoreWhitespace = true;
            settings.IgnoreComments = true;
            XmlReader reader = XmlReader.Create(".\\Content\\Levels\\" + xmlFile, settings);

            // Reads tile size
            reader.ReadToFollowing("TileSize");
            reader.Read();                          //HAVE to skip for some reason, in couple of places...
            tileSize = Int32.Parse(reader.Value);

            // X - dim
            reader.ReadToFollowing("XDim");
            reader.Read();                                    
            xDim = Int32.Parse(reader.Value);
            
            // Z - dim
            reader.ReadToFollowing("ZDim");
            reader.Read();
            zDim = Int32.Parse(reader.Value);

            // Fill tile array
            tiles = new SimpleTile[xDim * zDim];

            for (int z = 0; z < zDim; z++)
            {
                for (int x = 0; x < xDim; x++)
                {
                    String file = null;
                    int orientation = 0; 
                    
                    reader.ReadToFollowing("TileFile");
                    reader.Read();
                    file = reader.Value;
                    reader.ReadToFollowing("TileRot");
                    reader.Read();
                    orientation = Int32.Parse(reader.Value);

                    scene.addNode(new SimpleTile(file, 0, 0, orientation, tileSize, x == 0));
                }
            }
            reader.Close();
        }*/
        
    }
}
