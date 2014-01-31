using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RacingGame.Graphics;

namespace RacingGame.Map
{
    class SceneryObject
    {
        public Model model;
        public float velocity;
        public List<Vector3> wayPoints = new List<Vector3>();
        public float progress;
        public int currentNode;
        public Quaternion rotation = new Quaternion();
        public Texture texture;

        public Vector3 dir;
        public Vector3 pos;

        public float scale = 1.0f;
    }

    struct ParticleSpawn
    {
        public Vector3 pos;
        public Vector3 dir;
        public float time;
    }

    class SceneryNode : Node, Logic.IUpdateable
    {
        Model planet;
        float planetRotation = 0.0f;
        static bool doneThisFrame = false;

        ParticleSystemBase particleSys;

        List<SceneryObject> sceneryObjects = new List<SceneryObject>();
        List<ParticleSpawn> particleSpawns = new List<ParticleSpawn>();

        #region Effect stuff
        Effect sceneryEffect;
        EffectTechnique sceneryTech;
        EffectPass planetPass;
        EffectPass objectPass;

        EffectParameter decalTexture;
        EffectParameter lightDir;
        EffectParameter viewProj;
        EffectParameter world;
        EffectParameter cameraPos;
        #endregion


        public SceneryNode(BoundingSphere scene, int numShips)
        {
            sceneryEffect = RacingGame.contentManager.Load<Effect>("Shaders/scenery");
            sceneryTech = sceneryEffect.Techniques["Scenery"];
            planetPass = sceneryTech.Passes["planet"];
            objectPass = sceneryTech.Passes["object"];

            decalTexture = sceneryEffect.Parameters["DecalTexture"];
            lightDir = sceneryEffect.Parameters["LightDirection"];
            viewProj = sceneryEffect.Parameters["ViewProj"];
            world = sceneryEffect.Parameters["World"];
            cameraPos = sceneryEffect.Parameters["CameraPos"];

            string[] files = { "Models/Scenery/spaceship_1", "Models/Scenery/rock_1" };

            planet = RacingGame.contentManager.Load<Model>("Models/Scenery/planet");

            SceneryObject test = new SceneryObject()
            {
                currentNode = 0,
                model = RacingGame.contentManager.Load<Model>(files[0]),
                progress = 0.0f,
                velocity = 30.0f
            };
            test.texture = ((BasicEffect)test.model.Meshes[0].MeshParts[0].Effect).Texture;

            for (int i = 0; i < 20; ++i)
            {
                float angle = (2 * MathHelper.Pi) / 20 * i;
                float xpos = scene.Radius * (float)Math.Cos(angle);
                float zpos = scene.Radius * (float)Math.Sin(angle);

                test.wayPoints.Add(new Vector3(xpos, 20.0f, zpos));
            }

            Model rockModel = RacingGame.contentManager.Load<Model>(files[1]);
            Texture rockTexture = ((BasicEffect)rockModel.Meshes[0].MeshParts[0].Effect).Texture;
            for (int i = 0; i < 60; ++i)
            {
                
                SceneryObject rock = new SceneryObject()
                {
                    currentNode = 0,
                    model = rockModel,
                    progress = 0.0f,
                    velocity = 0.7f,
                    scale = (float)RacingGame.random.NextDouble() / 2.0f + 0.2f,
                    texture = rockTexture
                };

                

                rock.rotation = new Quaternion((float)RacingGame.random.NextDouble(), (float)RacingGame.random.NextDouble(),
                    (float)RacingGame.random.NextDouble(), (float)RacingGame.random.NextDouble());
                rock.rotation.Normalize();

                for (int k = 0; k < 20; ++k)
                {
                    float angle = (2 * MathHelper.Pi) / 20 * k - 1.6f;
                    float xpos = (scene.Radius + 70) * (float)Math.Cos(angle);
                    float zpos = (scene.Radius + 70) * (float)Math.Sin(angle);

                    rock.wayPoints.Add(new Vector3(xpos + RacingGame.random.Next(300), -20.0f + RacingGame.random.Next(70), zpos + RacingGame.random.Next(300)));
                }
                sceneryObjects.Add(rock);

            }

            SceneryObject teapot = new SceneryObject()
            {
                currentNode = 0,
                model = RacingGame.contentManager.Load<Model>("Models/Scenery/teapot"),
                progress = 0.0f,
                velocity = 0.7f,
                scale = 50.0f
            };

            teapot.texture = ((BasicEffect)teapot.model.Meshes[0].MeshParts[0].Effect).Texture;

            teapot.rotation = new Quaternion((float)RacingGame.random.NextDouble(), (float)RacingGame.random.NextDouble(),
                (float)RacingGame.random.NextDouble(), (float)RacingGame.random.NextDouble());
            teapot.rotation.Normalize();

            /*for (int k = 0; k < 20; ++k)
            {
                float angle = (2 * MathHelper.Pi) / 20 * k - 1.6f;
                float xpos = scene.Radius * (float)Math.Cos(angle);
                float zpos = scene.Radius * (float)Math.Sin(angle);

                teapot.wayPoints.Add(new Vector3(xpos + RacingGame.random.Next(300), -20.0f + RacingGame.random.Next(70), zpos + RacingGame.random.Next(300)));
            }*/
            for (int i = 0; i < 20; ++i)
            {
                float angle = (2 * MathHelper.Pi) / 20 * i;
                float xpos = scene.Radius * (float)Math.Cos(angle);
                float zpos = scene.Radius * (float)Math.Sin(angle);

                teapot.wayPoints.Add(new Vector3(xpos, 20.0f, zpos));
            }
            sceneryObjects.Add(teapot);


            sceneryObjects.Add(test);
        }

        public override void render()
        {
            if (!doneThisFrame)
            {
                doneThisFrame = true;
                PIXTools.PIXTools.BeginEvent("Scenery");
                Camera cam = GraphicsManager.getInstance().getCamera();

                Matrix view = GraphicsManager.getInstance().view;
                view.Translation = Vector3.Zero;
                viewProj.SetValue(view * GraphicsManager.getInstance().proj);
                cameraPos.SetValue(cam.getPosition());

                sceneryEffect.Begin();
                drawPlanet();
                drawSceneryObejcts();

                sceneryEffect.End();

                PIXTools.PIXTools.EndEvent();
            }
        }

        public override RenderQueues getRenderQueue()
        {
            return RenderQueues.Scenery;
        }

        private void spawnBeam(GameTime time)
        {
            Random rnd = RacingGame.random;
            int rndDir = rnd.Next(2);
            Vector3 dir = Vector3.Up * 150.0f;
            Vector3 pos = new Vector3((float)rnd.Next(1000) - 500.0f, -50.0f, (float)rnd.Next(1000) - 500.0f);

            if (rndDir == 0)
            {
                dir = -dir;
                pos.Y = -pos.Y;
            }
            
            ParticleSpawn pspawn = new ParticleSpawn()
            {
                dir = dir,
                pos = pos,
                time = (float)time.TotalGameTime.TotalSeconds
            };

            particleSpawns.Add(pspawn);
        }

        private void spawnFlash()
        {
            Random rnd = RacingGame.random;
            Vector3 pos = new Vector3((float)rnd.Next(1000) - 500.0f, -30.0f, (float)rnd.Next(1000) - 500.0f);
            Vector4 color = new Vector4((float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble(), 0.6f);
            for (int i = 0; i < 100; ++i)
            {
                  particleSys.AddParticle(new Particle(pos + Vector3.Up * 2.0f * i)
                {
                    Color = color,
                    DeltaSize = 0.0f,
                    initAlpha = 0.6f,
                    LifeTime = 1.0f,
                    Size = 10.0f,
                    Velocity = Vector3.Zero
                });
            }
        }

        private void spawnParticles(GameTime time)
        {
            float delta = (float)time.ElapsedGameTime.TotalSeconds;
            particleSys = States.Game.ParticleSystem;
            if (particleSys == null)
            {
                return;
            }

            Random rnd = RacingGame.random;
            if (Properties.Settings.Default.spawnMegaBeams)
            {
                if (rnd.Next(8) == 4)
                {
                    spawnFlash();
                }

                if (rnd.Next(100) == 42)
                {
                    spawnBeam(time);
                }
            }

            particleSys.AddParticle(new Particle(new Vector3(0.0f, -50.0f, 0.0f))
            {
                Color = new Vector4(0.3f, 1.0f, 0.8f, 0.6f),
                DeltaSize = 0.0f,
                initAlpha = 0.6f,
                LifeTime = 5.0f,
                Size = 10.0f,
                Velocity = new Vector3(0.0f, 50.0f, 0.0f)
            });

            for (int i = 0; i < particleSpawns.Count; ++i)
            {
                particleSys.AddParticle(new Particle(particleSpawns[i].pos)
                {
                    Color = new Vector4(0.9f, 0.2f, 0.2f, 0.6f),
                    DeltaSize = 0.0f,
                    initAlpha = 0.6f,
                    LifeTime = 5.0f,
                    Size = 10.0f,
                    Velocity = particleSpawns[i].dir
                });

                //particleSpawns[i].time = 0.0f;

                if (particleSpawns[i].time + 0.5f < (float)time.TotalGameTime.TotalSeconds)
                {
                    particleSpawns.RemoveAt(i);
                }
            }
        }

        public override bool update(GameTime time)
        {
            spawnParticles(time);

            doneThisFrame = false;
            planetRotation += 0.01f * (float)time.ElapsedGameTime.TotalSeconds;
            updateSceneryObjects(time);

            return false;
        }

        private void drawPlanet()
        {
            Matrix wrld = Matrix.CreateWorld(Vector3.Forward * 60.0f, Vector3.Transform(Vector3.Forward, Quaternion.CreateFromAxisAngle(Vector3.Up, planetRotation)), Vector3.Up);
            world.SetValue(wrld);

            GraphicsDevice device = GraphicsManager.getDevice();
            decalTexture.SetValue(((BasicEffect)planet.Meshes[0].MeshParts[0].Effect).Texture);

            planetPass.Begin();

            foreach (ModelMesh mesh in planet.Meshes)
            {
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

            planetPass.End();
        }

        private void drawSceneryObejcts()
        {
            GraphicsDevice device = GraphicsManager.getDevice();
            objectPass.Begin();

            foreach (SceneryObject i in sceneryObjects)
            {
                Matrix wrld = Matrix.CreateWorld(i.pos, Vector3.Transform(i.dir, i.rotation), Vector3.Up);
                wrld = wrld * Matrix.CreateScale(i.scale);
                world.SetValue(wrld);
                decalTexture.SetValue(i.texture);

                foreach (ModelMesh mesh in i.model.Meshes)
                {
                    device.Indices = mesh.IndexBuffer;

                    foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    {
                        

                        sceneryEffect.CommitChanges();

                        device.Vertices[0].SetSource(mesh.VertexBuffer, meshPart.StreamOffset,
                            meshPart.VertexStride);

                        device.VertexDeclaration = meshPart.VertexDeclaration;

                        device.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                            meshPart.BaseVertex, 0, meshPart.NumVertices, meshPart.StartIndex,
                            meshPart.PrimitiveCount);

                    }
                }
            }

            objectPass.End();
        }

        private void updateSceneryObjects(GameTime time)
        {
            for (int j = 0; j < sceneryObjects.Count; ++j )
            {
                SceneryObject i = sceneryObjects[j];

                Vector3 currentNode = i.wayPoints[i.currentNode];
                Vector3 nextNode, nextNextNode;

                if (i.currentNode + 1 == i.wayPoints.Count)
                {
                    nextNode = i.wayPoints[0];
                    nextNextNode = i.wayPoints[1];
                }
                else if (i.currentNode + 2 == i.wayPoints.Count)
                {
                    nextNode = i.wayPoints[i.currentNode + 1];
                    nextNextNode = i.wayPoints[0];
                }
                else
                {
                    nextNode = i.wayPoints[i.currentNode + 1];
                    nextNextNode = i.wayPoints[i.currentNode + 2];
                }

                Vector3 delta = nextNode - currentNode;

                Vector3 mainDir = Vector3.Normalize(delta);
                Vector3 nextDir = Vector3.Normalize(nextNextNode - nextNode);

                i.dir = Vector3.Lerp(mainDir, nextDir, i.progress);

                float fullLen = delta.Length();
                float prevProgress = i.progress;
                float goneLen = fullLen * i.progress;
                goneLen += i.velocity * (float)time.ElapsedGameTime.TotalSeconds;
                i.progress = goneLen / fullLen;

                if (i.progress > 1.0f)
                {
                    i.currentNode++;
                    if (i.currentNode >= i.wayPoints.Count)
                    {
                        i.currentNode = 0;
                    }
                    i.progress -= 1.0f;

                    currentNode = i.wayPoints[i.currentNode];
                }

                i.pos = currentNode + delta * i.progress;

            }

        }
        
    }
}
