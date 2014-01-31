using Microsoft.Xna.Framework;

namespace RacingGame.Collision
{
    /// <summary>
    /// The collision objects for static environment meshes.
    /// Author: Rickard von Haugwitz
    /// </summary>
    class KDTreeObject : TriangleMeshObject
    {
        private KDTree kdt;

        public KDTreeObject(TriangleMesh meshObj) : base(meshObj)
        {
            kdt = new KDTree(meshObj);
            //kdt.SaveToFile("kdtree");
        }

        public override ResultRay test(Ray r, float? maxTracingDistance)
        {
            Matrix invTransform = Matrix.Invert(transformation);

            Ray objSpaceRay = new Ray();
            objSpaceRay.Position = Vector3.Transform(r.Position, invTransform);
            objSpaceRay.Direction = Vector3.Transform(r.Direction, Matrix.Transpose(transformation));

            Triangle? tri = kdt.RayTraversal(objSpaceRay, maxTracingDistance);

            ResultRay res = new ResultRay();
            res.t = Intersection.test(objSpaceRay, tri);
            res.ray = r;
            res.tri = tri;

            return res;
        }
    }
}
