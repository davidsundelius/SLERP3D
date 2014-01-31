using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RacingGame.Graphics;

namespace RacingGame.Map
{
    struct SceneryObject
    {
        public Model model;
        public List<Vector3> wayPoints;
        public int currentNode;
    }

    class SceneryNode : Node, Logic.IUpdateable
    {
        Model planet;
        float planetRotation = 0.0f;

        List<Model> ships = new List<Model>();

        Matrix projection;

        #region Effect stuff
        Effect sceneryEffect;
        EffectTechnique sceneryTech;
        EffectPass planetPass;

        EffectParameter decalTexture;
        EffectParameter lightDir;
        EffectParameter worldViewProj;
        EffectParameter world;
        EffectParameter cameraPos;
        #endregion


        public SceneryNode(BoundingSphere scene, int numShips)
        {
            sceneryEffect = RacingGame.contentManager.Load<Effect>("Shaders/scenery");
            sceneryTech = sceneryEffect.Techniques["Scenery"];
            planetPass = sceneryTech.Passes["planet"];

            decalTexture = sceneryEffect.Parameters["DecalTexture"];
            lightDir = sceneryEffect.Parameters["LightDirection"];
            worldViewProj = sceneryEffect.Parameters["WorldViewProj"];
            world = sceneryEffect.Parameters["World"];
            cameraPos = sceneryEffect.Parameters["CameraPos"];

            string[] files = { "Models/Scenery/spaceship_1" };

            planet = RacingGame.contentManager.Load<Model>("Models/Scenery/planet");

            

        }

        public override void render()
        {
            Camera cam = GraphicsManager.getInstance().getCamera();
            sceneryEffect.Begin();
            drawPlanet();

            sceneryEffect.End();
        }

        public override RenderQueues getRenderQueue()
        {
            return RenderQueues.Scenery;
        }


        public override bool update(GameTime time)
        {
            planetRotation += 0.01f * (float)time.ElapsedGameTime.TotalSeconds;
            return false;
        }

        private void drawPlanet()
        {
            Matrix wrld = Matrix.CreateWorld(Vector3.Forward * 60.0f, Vector3.Transform(Vector3.Forward, Quaternion.CreateFromAxisAngle(Vector3.Up, planetRotation)), Vector3.Up);
            world.SetValue(wrld);
            Matrix view = GraphicsManager.getInstance().view;
            view.Translation = Vector3.Zero;
            worldViewProj.SetValue(wrld * view * GraphicsManager.getInstance().proj);
            cameraPos.SetValue(GraphicsManager.getInstance().getCamera().getPosition());

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



        
    }
}
