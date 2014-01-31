using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RacingGame.Collision
{
    /// <summary>
    /// A class which forms a mesh that can be tested for intersection from rays.
    /// Author: Daniel Lindén
    /// </summary>
    class TriangleMesh
    {
        private List<Triangle> triangles = new List<Triangle>();
        private BoundingBox boundingBox = new BoundingBox(new Vector3(float.MaxValue), new Vector3(float.MinValue));
        private BoundingSphere boundingSphere = new BoundingSphere();

        public TriangleMesh()
        {
        }

        public TriangleMesh(ModelMesh mesh)
        {
            loadModelMesh(mesh);
        }

        public List<Triangle> Triangles
        {
            get
            {
                return triangles;
            }
        }

        public void loadModelMesh(ModelMesh m)
        {
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

            for (int i = 0; i < indices.Length / 3; ++i)
            {
                addTriangle(new Triangle(positions[indices[i * 3]], positions[indices[i * 3 + 1]], positions[indices[i * 3 + 2]]));
            }

            boundingSphere = m.BoundingSphere;
        }

        public BoundingBox getBoundingBox()
        {
            return boundingBox;
        }

        public BoundingSphere getBoundingSphere()
        {
            return boundingSphere;
        }

        public void addTriangle(Triangle t)
        {
            triangles.Add(t);

            boundingBox.Max = Vector3.Max(boundingBox.Max, t.vertex1);
            boundingBox.Max = Vector3.Max(boundingBox.Max, t.vertex2);
            boundingBox.Max = Vector3.Max(boundingBox.Max, t.vertex3);

            boundingBox.Min = Vector3.Min(boundingBox.Min, t.vertex1);
            boundingBox.Min = Vector3.Min(boundingBox.Min, t.vertex2);
            boundingBox.Min = Vector3.Min(boundingBox.Min, t.vertex3);
        }

        public float? test(Ray r, ref Triangle? tOut)
        {
            tOut = null;

            if (Intersection.test(r, boundingBox).HasValue)
            {
                float? low = null;

                foreach (Triangle t in triangles)
                {
             
                    float? testRes = Intersection.test(r, t);

                    if (testRes.HasValue)
                    {
                        if (!low.HasValue)
                        {
                            low = testRes;
                            tOut = t;
                        }
                        else if (testRes.Value < low.Value)
                        {
                            low = testRes;
                            tOut = t;
                        }
                    }
                    
                }

                return low;
            }

            return null;
        }
    }
}