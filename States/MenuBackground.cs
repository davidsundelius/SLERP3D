using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RacingGame.Graphics;
using RacingGame.Sys;

namespace RacingGame.States
{
    static class MenuBackground
    {
        private static Graphics.GraphicsManager graphicsManager = Graphics.GraphicsManager.getInstance();
        private static Scene scene = new Scene();
        private static List<Logic.IUpdateable> updatables = new List<Logic.IUpdateable>();
        private static Node vehicle1Model = new ModelNode(RacingGame.contentManager.Load<Model>("Models/Ships/box_01"));
        private static Node vehicle2Model = new ModelNode(RacingGame.contentManager.Load<Model>("Models/Ships/ship_02"));
        private static Node playerModel = new ModelNode(RacingGame.contentManager.Load<Model>("Models/Ships/ship_02"));
        private static Camera camera = new Camera(playerModel, new Vector3(-4f, 0f, +1.7f), new Vector3(-5f, -0.0f, 0f));

        private static ParticleSystemBase particleSys;
        private static Random rnd = new Random();

        private static LensFlare lensFlare;
        
        private static PointLight light = new PointLight();
        private static LightNode lightNode = new LightNode(light);
        private static Map.SceneryNode scenery;

        public static void initialize()
        {
            //scene.addNode(playerModel);

            vehicle2Model.uniformScale = 1.7f;
            vehicle1Model.uniformScale = 5f;

            vehicle1Model.rotation = Quaternion.CreateFromAxisAngle(new Vector3(0f, 1.0f, 0f), 1.5f);
            vehicle2Model.rotation = Quaternion.CreateFromAxisAngle(new Vector3(0f, 1.0f, 0f), 1.5f);
            Quaternion rotation = Quaternion.CreateFromAxisAngle(new Vector3(0f, 0f, 1f), 0.1f);
            vehicle2Model.rotation = Quaternion.Concatenate(vehicle2Model.rotation, rotation);
            vehicle1Model.rotation = Quaternion.Concatenate(vehicle1Model.rotation, rotation);

            scene.addNode(new SkyBoxNode("Textures/Sky/skyboxSpace", camera));
            updatables.Add((Logic.IUpdateable)scene);
            updatables.Add((Logic.IUpdateable)camera);

            lensFlare = new LensFlare(LensFlare.DefaultSunPos, camera.getPosition(), 200);
            particleSys = new ParticleSystemCPU(1000, RacingGame.contentManager, GraphicsManager.getDevice());
            

            light.diffuse = Color.Orange.ToVector3();
            light.setRadius(25.0f);
            light.castsShadows = false;
            light.specularFactor = 0.1f;
            lightNode.position = playerModel.position + new Vector3(10f, 0f, 10f);
            scene.addNode(lightNode);

            scenery = new Map.SceneryNode(new BoundingSphere(Vector3.Zero, 500.0f), 1);
            scene.addNode(scenery);
        }

        public static void addVehicle(int chosenVehicle)
        {
            switch (chosenVehicle)
            {
                case 0:
                    if (scene.find(vehicle2Model))
                    {
                        scene.remove(vehicle2Model);
                    }
                    if (!scene.find(vehicle1Model))
                    {
                        spawnRespawnAura(vehicle2Model.position);
                        scene.addNode(vehicle1Model);
                    }
                    break;
                case 1:
                    if (scene.find(vehicle1Model))
                    {
                        scene.remove(vehicle1Model);
                    }
                    if (!scene.find(vehicle2Model))
                    {
                        spawnRespawnAura(vehicle2Model.position);
                        scene.addNode(vehicle2Model);
                    }
                    break;
            }
            
        }

        public static void removeVehicles()
        {
            if (scene.find(vehicle2Model))
            {
                scene.remove(vehicle2Model);
            }
            if (scene.find(vehicle1Model))
            {
                scene.remove(vehicle1Model);
            }
        }

        public static void render()
        {
            graphicsManager.render(scene, camera);

            lensFlare.Render(Color.White);

            particleSys.Draw(graphicsManager.view, graphicsManager.proj);
        }

        public static void update(GameTime time)
        {
            lensFlare.setCameraPosForLensFlare(camera.getPosition());
            scenery.update(time);
            Quaternion rotation = Quaternion.CreateFromAxisAngle(new Vector3(0, 1.0f, 0), 0.001f);
            playerModel.rotation = Quaternion.Concatenate(playerModel.rotation, rotation);

            vehicle1Model.rotation = Quaternion.Concatenate(vehicle1Model.rotation, rotation);
            vehicle2Model.rotation = Quaternion.Concatenate(vehicle2Model.rotation, rotation);
            

            switch (Properties.Settings.Default.chosenVehicle)
            {
                case 0:
                    vehicle1Model.position = playerModel.position;
                    break;
                case 1:
                    vehicle2Model.position = playerModel.position;

                    vehicle2Model.position = Vector3.Transform(vehicle2Model.position, Matrix.CreateTranslation(new Vector3(0f, 0.1f, 0)));
                    break;
            }

            for (int i = 0; i < 12; ++i)
            {
                Vector3 pos = Vector3.Transform(
                    new Vector3(((float)rnd.NextDouble() - 0.5f) * 0.01f, ((float)rnd.NextDouble() - 0.5f) * 0.01f + 0.10f, 0.7f + 0.0f * (float)rnd.NextDouble()),
                    playerModel.rotation) + playerModel.position;

                Vector3 shipVel = Vector3.Zero;
                //Vector3 vel = new Vector3(((float)rnd.NextDouble() - 0.5f) * 0.1f, ((float)rnd.NextDouble() - 0.5f) * 0.1f, (float)rnd.NextDouble() * 3.0f);// +shipVel;
                Vector4 color = new Vector4(0f, 0f, 0f, 0f);
                particleSys.AddParticle(new Particle(pos)
                    {
                        Velocity = Vector3.Transform(shipVel, playerModel.rotation) + shipVel,
                        Color = color
                    });

            }
            particleSys.Update((float)time.ElapsedGameTime.TotalSeconds);
            
            lightNode.position = playerModel.position + new Vector3(1f, 4f, 0f);


            for (int i = 0; i < updatables.Count; i++)
            {
                if (updatables[i].update(time))
                    updatables.RemoveAt(i);
            }

        }

        private static float getRnd()
        {
            return ((float)RacingGame.random.NextDouble() - 0.5f);
        }

        public static void spawnRespawnAura(Vector3 position)
        {
            for (int i = 0; i < 150; ++i)
            {
                float rnd = (float)(getRnd() * 4 * Math.PI);
                Vector3 velocity = new Vector3((float)(Math.Cos(rnd)) * 10.0f, 0.0f, (float)(Math.Sin(rnd)) * 5.0f);
                Particle p = new Particle(position)
                {
                    DeltaSize = 1.5f,
                    initAlpha = 0.5f,
                    LifeTime = 0.5f,
                    Size = 1f,
                    Color = new Vector4(0.7f, 0.9f, 1.0f, 0.9f),
                    Velocity = velocity
                };
                particleSys.AddParticle(p);
            }
        }

        private class MenuRotationNode : Node
        {
            public MenuRotationNode()
            {
            }

            public override bool update(GameTime time)
            {
                base.update(time);
                return false;
            }

            public override void render()
            {
            }

            public override void draw(BoundingFrustum frustum)
            {
            }

            public override RenderQueues getRenderQueue()
            {
                return RenderQueues.Normal;
            }
        }
    }
}
