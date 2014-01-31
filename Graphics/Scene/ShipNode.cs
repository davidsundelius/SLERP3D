using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RacingGame.Logic;

namespace RacingGame.Graphics
{
    class ShipNode : ModelNode
    {
        private PointLight flameLight;
        private ProjectedLight headLight;
        public bool useSpeed = false;
        public bool useBoost = false;
        private Vector3 velocity;
        public bool demonMode
        {
            get
            {
                return demonNode.Visible;
            }

            set
            {
                demonNode.Visible = value;
            }
        }

        private Vector3 prevPos = Vector3.Zero;
        private Vector3 prevVel = Vector3.Zero;
        public ShieldNode shield;

        private DemonNode demonNode;

        public ShipNode(Model m) : base(m)
        {
            shield = new ShieldNode(this, m);
            shield.Visible = false;

            demonNode = new DemonNode(this, m);

            demonMode = false;

            flameLight = new PointLight();
            flameLight.diffuse = new Vector3(0.5f, 0.5f, 0.9f);
            flameLight.setRadius(5.0f);
            flameLight.position = Vector3.Zero;
            flameLight.specularFactor = 0.0f;
            flameLight.castsShadows = false;

            headLight = new ProjectedLight();
            headLight.diffuse = new Vector3(2.0f);
            headLight.setProjection(MathHelper.PiOver4, 1.6f, 0.1f, 15.0f);
            headLight.position = Vector3.Zero;
            headLight.setDirection(Vector3.Forward, Vector3.Up);
            headLight.texture = RacingGame.contentManager.Load<Texture>("Textures/headlights");
            headLight.castsShadows = false;
        }

        public void setVelocity(Vector3 v)
        {
            prevVel = velocity;
            velocity = v / 5.0f;
        }

        public override void draw(BoundingFrustum viewFrustum)
        {
            if (Visible)
            { 

                if (viewFrustum.Intersects(flameLight.getBoundingBox()))
                {
                    GraphicsManager.getInstance().addLight(flameLight);
                }

                if (viewFrustum.Intersects(headLight.getBoundingBox()))
                {
                    GraphicsManager.getInstance().addLight(headLight);
                }

                shield.draw(viewFrustum);
                demonNode.draw(viewFrustum);
            }

            base.draw(viewFrustum);
        }

        private void spawnParticles()
        {
            Random rnd = RacingGame.random;

            ParticleSystemBase particleSys = States.Game.ParticleSystem;

            Vector3 shipBack = Vector3.Transform(Vector3.Backward, rotation);
            Vector3 shipUp = Vector3.Transform(Vector3.Up, rotation);
            flameLight.position = 0.8f * shipBack + position;
            headLight.position = -0.8f * shipBack + position;
            headLight.setDirection(-shipBack, shipUp);

            for (int i = 0; i < 12; ++i)
            {

                Vector3 pos = Vector3.Transform(new Vector3(0.0f, 0.0f + 0.1f, 0.7f), rotation) + position;

                Vector3 shipVel = velocity;
                Vector3 vel = new Vector3(((float)rnd.NextDouble() - 0.5f) * 0.1f, ((float)rnd.NextDouble() - 0.5f) * 0.1f, (float)rnd.NextDouble() * 3.0f);

                Vector4 color;
                float size = 0.15f;

                

                if (useSpeed)
                {
                    color = new Vector4(0.15f, 0.8f, 0.07f, 0.15f);
                    flameLight.diffuse = new Vector3(0.15f, 0.8f, 0.07f) * 3.0f;
                    size = 0.4f;
                }
                else if (useBoost)
                {
                    size = 0.3f;
                    color = new Vector4(0.5f, 0.5f, 0.9f, 0.15f);
                    flameLight.diffuse = new Vector3(0.5f, 0.5f, 0.9f) * 3.0f;
                }
                else
                {
                    color = new Vector4(0.5f, 0.5f, 0.9f, 0.15f);
                    flameLight.diffuse = new Vector3(0.5f, 0.5f, 0.9f) * 1.5f;
                    //color = new Vector4(0.9f, 0.15f, 0.07f, 0.15f); //Old red color
                }

                particleSys.AddParticle(new Particle(pos)
                {
                    Velocity = Vector3.Transform(vel, rotation) + shipVel,
                    Color = color,
                    Size = size,
                    DeltaSize = 0.0f,
                    initAlpha = color.W
                });
            }

            prevPos = position;
        }

        public override bool update(GameTime gt)
        {
            if (!Visible)
            {
                return false;
            }

            spawnParticles();

            return base.update(gt);
        }

        public override void render()
        {/*
            if (demonMode)
            {
                GraphicsManager.getInstance().setWorldMatrix(transformation);

                GraphicsDevice device = GraphicsManager.getDevice();

                ModelMesh mesh = model.Meshes[0];
                {
                    device.Indices = mesh.IndexBuffer;

                    foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    {
                        GraphicsManager.getInstance().setTexture(null);
                        GraphicsManager.getInstance().updateShader();

                        device.Vertices[0].SetSource(mesh.VertexBuffer, meshPart.StreamOffset,
                            meshPart.VertexStride);

                        device.VertexDeclaration = meshPart.VertexDeclaration;

                        device.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                            meshPart.BaseVertex, 0, meshPart.NumVertices, meshPart.StartIndex,
                            meshPart.PrimitiveCount);

                    }
                }
            }
            else*/
                base.render();
        }
    }
}
