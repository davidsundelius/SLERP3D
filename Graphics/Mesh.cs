using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RacingGame.Graphics
{
    class Mesh
    {
        private VertexBuffer vertexBuffer;
        private int numVerts;
        private IndexBuffer indexBuffer;
        private int numIndices;
        private VertexDeclaration vertexDecl;


        public void setVertexData(VertexPositionNormalTexture[] vertices)
        {
            vertexBuffer = new VertexBuffer(GraphicsManager.getDevice(), VertexPositionNormalTexture.SizeInBytes * vertices.Count(), BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices);
            numVerts = vertices.Count();
            vertexDecl = new VertexDeclaration(GraphicsManager.getDevice(), VertexPositionNormalTexture.VertexElements);

        }

        public void setIndexData(int[] indices)
        {
            indexBuffer = new IndexBuffer(GraphicsManager.getDevice(), sizeof(int) * indices.Length, BufferUsage.WriteOnly, IndexElementSize.ThirtyTwoBits);
            indexBuffer.SetData(indices);
            numIndices = indices.Length;
        }

        public void render()
        {
            GraphicsDevice device = GraphicsManager.getDevice();
            device.Indices = indexBuffer;
            device.VertexDeclaration = vertexDecl;
            device.Vertices[0].SetSource(vertexBuffer, 0, VertexPositionNormalTexture.SizeInBytes);
            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, numVerts, 0, numIndices / 3);
            
        }
        /// <summary>
        /// A method which creates a mesh of a viewing frustum.
        /// </summary>
        /// <param name="fovY">The vertical field of view</param>
        /// <param name="aspect">The width / height aspect ratio</param>
        /// <param name="near">The distance to the near plane</param>
        /// <param name="far">The distance to the far plane</param>
        /// <returns></returns>
        public static Mesh createFrustum(float fovY, float aspect, float near, float far)
        {
            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[8];
            int[] indices = new int[36];

            float fovX = fovY * aspect;
            float nearOffsX = (float)Math.Tan(fovX / 2.0f) * near;
            float nearOffsY = (float)Math.Tan(fovY / 2.0f) * near;

            float farOffsX = (float)Math.Tan(fovX / 2.0f) * far;
            float farOffsY = (float)Math.Tan(fovY / 2.0f) * far;

            vertices[0] = new VertexPositionNormalTexture(new Vector3(nearOffsX, -nearOffsY, -near), Vector3.Zero, Vector2.Zero);
            vertices[1] = new VertexPositionNormalTexture(new Vector3(-nearOffsX, -nearOffsY, -near), Vector3.Zero, Vector2.Zero);
            vertices[2] = new VertexPositionNormalTexture(new Vector3(nearOffsX, nearOffsY, -near), Vector3.Zero, Vector2.Zero);
            vertices[3] = new VertexPositionNormalTexture(new Vector3(-nearOffsX, nearOffsY, -near), Vector3.Zero, Vector2.Zero);

            vertices[4] = new VertexPositionNormalTexture(new Vector3(farOffsX, -farOffsY, -far), Vector3.Zero, Vector2.Zero);
            vertices[5] = new VertexPositionNormalTexture(new Vector3(-farOffsX, -farOffsY, -far), Vector3.Zero, Vector2.Zero);
            vertices[6] = new VertexPositionNormalTexture(new Vector3(farOffsX, farOffsY, -far), Vector3.Zero, Vector2.Zero);
            vertices[7] = new VertexPositionNormalTexture(new Vector3(-farOffsX, farOffsY, -far), Vector3.Zero, Vector2.Zero);

            //Near
            indices[0] = 0;
            indices[1] = 3;
            indices[2] = 2;
            indices[3] = 0;
            indices[4] = 1;
            indices[5] = 3;

            //Far
            indices[6] = 5;
            indices[7] = 6;
            indices[8] = 7;
            indices[9] = 5;
            indices[10] = 4;
            indices[11] = 6;

            //Top
            indices[12] = 2;
            indices[13] = 7;
            indices[14] = 6;
            indices[15] = 2;
            indices[16] = 3;
            indices[17] = 7;

            //Bottom
            indices[18] = 1;
            indices[19] = 4;
            indices[20] = 5;
            indices[21] = 1;
            indices[22] = 0;
            indices[23] = 4;

            //Right
            indices[24] = 4;
            indices[25] = 2;
            indices[26] = 6;
            indices[27] = 4;
            indices[28] = 0;
            indices[29] = 2;

            //Left
            indices[30] = 1;
            indices[31] = 7;
            indices[32] = 3;
            indices[33] = 1;
            indices[34] = 5;
            indices[35] = 7;

            Mesh mesh = new Mesh();
            mesh.setVertexData(vertices);
            mesh.setIndexData(indices);
            return mesh;
        }

        public static Mesh createFrustum(BoundingFrustum frustum)
        {
            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[8];
            int[] indices = new int[36];

            /*
             * frustum.GetCorners()
             * Points 0 to 3 correspond to the near face in a clockwise order starting at its 
             * upper-left corner when looking toward the origin from the positive z direction. 
             * Points 4 to 7 correspond to the far face in a clockwise order starting at its upper-left
             * corner when looking toward the origin from the positive z direction.
             * 
             * v[0] = 2
             * v[1] = 3
             * v[2] = 1
             * v[3] = 0
             * */
            
            Vector3[] corners = frustum.GetCorners();
            vertices[0] = new VertexPositionNormalTexture(corners[2], Vector3.Zero, Vector2.Zero);
            vertices[1] = new VertexPositionNormalTexture(corners[3], Vector3.Zero, Vector2.Zero);
            vertices[2] = new VertexPositionNormalTexture(corners[1], Vector3.Zero, Vector2.Zero);
            vertices[3] = new VertexPositionNormalTexture(corners[0], Vector3.Zero, Vector2.Zero);

            vertices[4] = new VertexPositionNormalTexture(corners[6], Vector3.Zero, Vector2.Zero);
            vertices[5] = new VertexPositionNormalTexture(corners[7], Vector3.Zero, Vector2.Zero);
            vertices[6] = new VertexPositionNormalTexture(corners[5], Vector3.Zero, Vector2.Zero);
            vertices[7] = new VertexPositionNormalTexture(corners[4], Vector3.Zero, Vector2.Zero);

            //Near
            indices[0] = 0;
            indices[1] = 3;
            indices[2] = 2;
            indices[3] = 0;
            indices[4] = 1;
            indices[5] = 3;

            //Far
            indices[6] = 5;
            indices[7] = 6;
            indices[8] = 7;
            indices[9] = 5;
            indices[10] = 4;
            indices[11] = 6;

            //Top
            indices[12] = 2;
            indices[13] = 7;
            indices[14] = 6;
            indices[15] = 2;
            indices[16] = 3;
            indices[17] = 7;

            //Bottom
            indices[18] = 1;
            indices[19] = 4;
            indices[20] = 5;
            indices[21] = 1;
            indices[22] = 0;
            indices[23] = 4;

            //Right
            indices[24] = 4;
            indices[25] = 2;
            indices[26] = 6;
            indices[27] = 4;
            indices[28] = 0;
            indices[29] = 2;

            //Left
            indices[30] = 1;
            indices[31] = 7;
            indices[32] = 3;
            indices[33] = 1;
            indices[34] = 5;
            indices[35] = 7;

            Mesh mesh = new Mesh();
            mesh.setVertexData(vertices);
            mesh.setIndexData(indices);
            return mesh;
        }

        public static Mesh createFullScreenQuad()
        {
            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[4];
            int[] indices = new int[6];

            vertices[0] = new VertexPositionNormalTexture(new Vector3(-1.0f, 1.0f, 0.0f), Vector3.Zero, new Vector2(0.0f, 0.0f));
            vertices[1] = new VertexPositionNormalTexture(new Vector3(1.0f, 1.0f, 0.0f), Vector3.Zero, new Vector2(1.0f, 0.0f));
            vertices[2] = new VertexPositionNormalTexture(new Vector3(-1.0f, -1.0f, 0.0f), Vector3.Zero, new Vector2(0.0f, 1.0f));
            vertices[3] = new VertexPositionNormalTexture(new Vector3(1.0f, -1.0f, 0.0f), Vector3.Zero, new Vector2(1.0f, 1.0f));

            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 3;
            indices[3] = 0;
            indices[4] = 3;
            indices[5] = 2;

            Mesh mesh = new Mesh();
            mesh.setVertexData(vertices);
            mesh.setIndexData(indices);
            return mesh;
        }

        public static Mesh createSphere(int segments, float radius)
        {
            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[segments * segments + 2];
            int[] indices = new int[6 * segments * segments];

            Mesh mesh = new Mesh();
            return mesh;
        }

        public static Mesh createCube(float width, float height, float depth)
        {
            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[8];
            int[] indices = new int[36];

            float zOffs = depth / 2.0f;
            float yOffs = height / 2.0f;
            float xOffs = width / 2.0f;

            vertices[0] = new VertexPositionNormalTexture(new Vector3(xOffs, -yOffs, zOffs), Vector3.Zero, Vector2.Zero);
            vertices[1] = new VertexPositionNormalTexture(new Vector3(-xOffs, -yOffs, zOffs), Vector3.Zero, Vector2.Zero);
            vertices[2] = new VertexPositionNormalTexture(new Vector3(xOffs, yOffs, zOffs), Vector3.Zero, Vector2.Zero);
            vertices[3] = new VertexPositionNormalTexture(new Vector3(-xOffs, yOffs, zOffs), Vector3.Zero, Vector2.Zero);

            vertices[4] = new VertexPositionNormalTexture(new Vector3(xOffs, -yOffs, -zOffs), Vector3.Zero, Vector2.Zero);
            vertices[5] = new VertexPositionNormalTexture(new Vector3(-xOffs, -yOffs, -zOffs), Vector3.Zero, Vector2.Zero);
            vertices[6] = new VertexPositionNormalTexture(new Vector3(xOffs, yOffs, -zOffs), Vector3.Zero, Vector2.Zero);
            vertices[7] = new VertexPositionNormalTexture(new Vector3(-xOffs, yOffs, -zOffs), Vector3.Zero, Vector2.Zero);

            //Near
            indices[0] = 0;
            indices[1] = 3;
            indices[2] = 2;
            indices[3] = 0;
            indices[4] = 1;
            indices[5] = 3;

            //Far
            indices[6] = 5;
            indices[7] = 6;
            indices[8] = 7;
            indices[9] = 5;
            indices[10] = 4;
            indices[11] = 6;

            //Top
            indices[12] = 2;
            indices[13] = 7;
            indices[14] = 6;
            indices[15] = 2;
            indices[16] = 3;
            indices[17] = 7;

            //Bottom
            indices[18] = 1;
            indices[19] = 4;
            indices[20] = 5;
            indices[21] = 1;
            indices[22] = 0;
            indices[23] = 4;

            //Right
            indices[24] = 4;
            indices[25] = 2;
            indices[26] = 6;
            indices[27] = 4;
            indices[28] = 0;
            indices[29] = 2;

            //Left
            indices[30] = 1;
            indices[31] = 7;
            indices[32] = 3;
            indices[33] = 1;
            indices[34] = 5;
            indices[35] = 7;

            Mesh mesh = new Mesh();
            mesh.setVertexData(vertices);
            mesh.setIndexData(indices);
            return mesh;
        }

        public static Mesh createCube(BoundingFrustum frustum)
        {
            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[8];
            int[] indices = new int[36];

            Vector3 min = new Vector3(float.MaxValue);
            Vector3 max = new Vector3(float.MinValue);

            foreach (Vector3 v in frustum.GetCorners())
            {
                min = Vector3.Min(v, min);
                max = Vector3.Max(v, max);
            }
            

            vertices[0] = new VertexPositionNormalTexture(new Vector3(max.X, min.Y, max.X), Vector3.Zero, Vector2.Zero);
            vertices[1] = new VertexPositionNormalTexture(new Vector3(min.X, min.Y, max.X), Vector3.Zero, Vector2.Zero);
            vertices[2] = new VertexPositionNormalTexture(new Vector3(max.X, max.Y, max.X), Vector3.Zero, Vector2.Zero);
            vertices[3] = new VertexPositionNormalTexture(new Vector3(min.X, max.Y, max.X), Vector3.Zero, Vector2.Zero);

            vertices[4] = new VertexPositionNormalTexture(new Vector3(max.X, min.Y, min.X), Vector3.Zero, Vector2.Zero);
            vertices[5] = new VertexPositionNormalTexture(new Vector3(min.X, min.Y, min.X), Vector3.Zero, Vector2.Zero);
            vertices[6] = new VertexPositionNormalTexture(new Vector3(max.X, max.Y, min.X), Vector3.Zero, Vector2.Zero);
            vertices[7] = new VertexPositionNormalTexture(new Vector3(min.X, max.Y, min.X), Vector3.Zero, Vector2.Zero);

            //Near
            indices[0] = 0;
            indices[1] = 3;
            indices[2] = 2;
            indices[3] = 0;
            indices[4] = 1;
            indices[5] = 3;

            //Far
            indices[6] = 5;
            indices[7] = 6;
            indices[8] = 7;
            indices[9] = 5;
            indices[10] = 4;
            indices[11] = 6;

            //Top
            indices[12] = 2;
            indices[13] = 7;
            indices[14] = 6;
            indices[15] = 2;
            indices[16] = 3;
            indices[17] = 7;

            //Bottom
            indices[18] = 1;
            indices[19] = 4;
            indices[20] = 5;
            indices[21] = 1;
            indices[22] = 0;
            indices[23] = 4;

            //Right
            indices[24] = 4;
            indices[25] = 2;
            indices[26] = 6;
            indices[27] = 4;
            indices[28] = 0;
            indices[29] = 2;

            //Left
            indices[30] = 1;
            indices[31] = 7;
            indices[32] = 3;
            indices[33] = 1;
            indices[34] = 5;
            indices[35] = 7;

            Mesh mesh = new Mesh();
            mesh.setVertexData(vertices);
            mesh.setIndexData(indices);
            return mesh;
        }

    }
}
