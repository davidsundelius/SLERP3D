using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace RacingGame.Collision
{
    /// <summary>
    /// The collision objects for triangle meshes.
    /// Author: Daniel Lindén
    /// </summary>
    class TriangleMeshObject : CollisionObject
    {
        protected TriangleMesh triMesh;

        public TriangleMeshObject(TriangleMesh meshObj)
        {
            triMesh = meshObj;
        }

        public TriangleMeshObject(ModelMesh mesh)
        {
            triMesh = new TriangleMesh();
            triMesh.loadModelMesh(mesh);
        }

        public TriangleMesh getTriangleMesh()
        {
            return triMesh;
        }

        public override ResultRay test(Ray r, float? maxTracingDistance)
        {
            Matrix invTransform = Matrix.Invert(transformation);

            Ray objSpaceRay = new Ray();
            objSpaceRay.Position = Vector3.Transform(r.Position, invTransform);
            objSpaceRay.Direction = Vector3.Transform(r.Direction, Matrix.Transpose(transformation));

            ResultRay res = new ResultRay();
            res.t = triMesh.test(objSpaceRay, ref res.tri);
            res.ray = r;
            if (res.tri.HasValue)
            {
                res.tri = new Triangle(Vector3.Transform(res.tri.Value.vertex1, transformation),
                    Vector3.Transform(res.tri.Value.vertex2, transformation), Vector3.Transform(res.tri.Value.vertex3, transformation));
                res.obj = this;
            }

            return res;
        }
    }
}
