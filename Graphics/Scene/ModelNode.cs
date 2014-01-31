using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RacingGame.Graphics
{
    /// <summary>
    /// A node used to render models in the game
    /// Author: Daniel Lindén
    /// </summary>
    class ModelNode : Node
    {
        protected Model model;
        protected Texture texture;
        public Collision.CollisionObject collisionObject
        {
            get;
            private set;
        }

        public ModelNode(Model m)
        {
            setModel(m);
        }

        public Model getModel()
        {
            return model;
        }

        public void setModel(Model m)
        {
            model = m;
            texture = ((BasicEffect)model.Meshes[0].MeshParts[0].Effect).Texture;
            if (model.Meshes.Count >= 2)
            {
                collisionObject = new Collision.TriangleMeshObject(model.Meshes[1]);
            }
            else
            {
                collisionObject = new Collision.TriangleMeshObject(model.Meshes[0]);
            }
        }

        public override void draw(BoundingFrustum viewFrustum)
        {
            BoundingSphere bs = model.Meshes[0].BoundingSphere;
            bs.Center += position;
            if (viewFrustum.Intersects(bs))
                base.draw(viewFrustum);
        }

        public override void render()
        {
            GraphicsManager.getInstance().setWorldMatrix(transformation);

            GraphicsDevice device = GraphicsManager.getDevice();

            ModelMesh mesh = model.Meshes[0];
            {
                device.Indices = mesh.IndexBuffer;

                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    GraphicsManager.getInstance().setTexture(texture);
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

        public override RenderQueues getRenderQueue()
        {
            return RenderQueues.Normal;
        }

        public override bool update(GameTime gt)
        {
            base.update(gt);
            collisionObject.position = position;
            collisionObject.rotation = rotation;
            collisionObject.uniformScale = uniformScale;
            collisionObject.updateTransform();
            return false;
        }
    }
}