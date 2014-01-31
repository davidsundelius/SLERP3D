using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RacingGame.Graphics
{
    /// <summary>
    /// Handles the sky animations and rendering of a
    /// suitable sky technique
    /// Author: Daniel Lindén
    /// </summary>
    class SkyBoxNode : Node
    {
        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;
        private VertexDeclaration vertexDecl;
        private TextureCube texture;
        private Camera camera;

        public SkyBoxNode(string textureFileName, Camera c)
        {
            SkyBoxVertex[] verts = new SkyBoxVertex[8];
            verts[0] = new SkyBoxVertex(new Vector3(-1.0f, -1.0f, -1.0f), new Vector3(-1.0f, -1.0f, -1.0f));
            verts[1] = new SkyBoxVertex(new Vector3(-1.0f, -1.0f, 1.0f), new Vector3(-1.0f, -1.0f, 1.0f));
            verts[2] = new SkyBoxVertex(new Vector3(-1.0f, 1.0f, 1.0f), new Vector3(-1.0f, 1.0f, 1.0f));
            verts[3] = new SkyBoxVertex(new Vector3(-1.0f, 1.0f, -1.0f), new Vector3(-1.0f, 1.0f, -1.0f));
            verts[4] = new SkyBoxVertex(new Vector3(1.0f, -1.0f, -1.0f), new Vector3(1.0f, -1.0f, -1.0f));
            verts[5] = new SkyBoxVertex(new Vector3(1.0f, -1.0f, 1.0f), new Vector3(1.0f, -1.0f, 1.0f));
            verts[6] = new SkyBoxVertex(new Vector3(1.0f, 1.0f, 1.0f), new Vector3(1.0f, 1.0f, 1.0f));
            verts[7] = new SkyBoxVertex(new Vector3(1.0f, 1.0f, -1.0f), new Vector3(1.0f, 1.0f, -1.0f));

            int[] indices = new int[36];
            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 3;
            indices[3] = 1;
            indices[4] = 2;
            indices[5] = 3;
            indices[6] = 0;
            indices[7] = 1;
            indices[8] = 5;
            indices[9] = 0;
            indices[10] = 4;
            indices[11] = 5;
            indices[12] = 1;
            indices[13] = 2;
            indices[14] = 5;
            indices[15] = 2;
            indices[16] = 5;
            indices[17] = 6;
            indices[18] = 2;
            indices[19] = 3;
            indices[20] = 6;
            indices[21] = 3;
            indices[22] = 6;
            indices[23] = 7;
            indices[24] = 4;
            indices[25] = 5;
            indices[26] = 6;
            indices[27] = 4;
            indices[28] = 6;
            indices[29] = 7;
            indices[30] = 0;
            indices[31] = 3;
            indices[32] = 4;
            indices[33] = 3;
            indices[34] = 4;
            indices[35] = 7;

            vertexBuffer = new VertexBuffer(GraphicsManager.getDevice(), SkyBoxVertex.SizeInBytes * 8, BufferUsage.WriteOnly);
            vertexBuffer.SetData<SkyBoxVertex>(verts);

            indexBuffer = new IndexBuffer(GraphicsManager.getDevice(), sizeof(int) * 36, BufferUsage.WriteOnly, IndexElementSize.ThirtyTwoBits);
            indexBuffer.SetData<int>(indices);

            vertexDecl = new VertexDeclaration(GraphicsManager.getDevice(), SkyBoxVertex.vertexElements);

            texture = RacingGame.contentManager.Load<TextureCube>(textureFileName);

            camera = c;
        }

        public override void render()
        {
            GraphicsDevice device = GraphicsManager.getDevice();

            Matrix tfm = Matrix.Identity;
            tfm.Translation = camera.getPosition();
            tfm.M11 = 100.0f;
            tfm.M22 = 100.0f;
            tfm.M33 = 100.0f;

            GraphicsManager.getInstance().setWorldMatrix(tfm);
            GraphicsManager.getInstance().setTexture(texture);
            GraphicsManager.getInstance().updateShader();

            device.Indices = indexBuffer;
            device.VertexDeclaration = vertexDecl;
            device.Vertices[0].SetSource(vertexBuffer, 0, SkyBoxVertex.SizeInBytes);
            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 8, 0, 12);

        }

        public override RenderQueues getRenderQueue()
        {
            return RenderQueues.Skybox;
        }

        private struct SkyBoxVertex
        {
            public Vector3 position;
            public Vector3 uv;

            public static VertexElement[] vertexElements = 
            {
                new VertexElement(0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0),
                new VertexElement(0, sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 0),
            };

            public static int SizeInBytes = sizeof(float) * 6;

            public SkyBoxVertex(Vector3 pos, Vector3 texCoords)
            {
                position = pos;
                uv = texCoords;
            }

        }
    }
}
