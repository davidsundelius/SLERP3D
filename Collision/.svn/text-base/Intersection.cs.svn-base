using Microsoft.Xna.Framework;

namespace RacingGame.Collision
{
    /// <summary>
    /// A class that provides static methods for intersection testing of various sorts.
    /// Author: Daniel Lindén
    /// </summary>
    class Intersection
    {
        public static readonly float EPSILON = 0.00001f;

        public static float? test(Ray r, Triangle? t)
        {
            if (!t.HasValue)
                return null;
            Triangle tri = t.Value;
            Vector3 edge1 = tri.vertex2 - tri.vertex1;
            Vector3 edge2 = tri.vertex3 - tri.vertex1;
            Vector3 p = Vector3.Cross(r.Direction, edge2);
            float det = Vector3.Dot(edge1, p);

            if (det > -EPSILON && det < EPSILON)
            {
                return null;
            }

            float invDet = 1.0f / det;

            Vector3 tv = r.Position - tri.vertex1;

            float u = Vector3.Dot(tv, p) * invDet;

            if (u < 0.0f || u > 1.0f)
            {
                return null; 
            }

            Vector3 q = Vector3.Cross(tv, edge1);

            float v = Vector3.Dot(r.Direction, q) * invDet;

            if (v < 0.0f || (v + u) > 1.0f)
            {
                return null;
            }
            float value = Vector3.Dot(edge2, q) * invDet;

            if (value < 0.0f)
            {
                return null;
            }
            return value;
        }

        /*public static float? test(Ray r, Triangle t)
        {
            if (t == null)
                return null;
            // calculate barycentric coordinates (u, v) and distance t to ray origin of intersection point P
            float det = Vector3.Dot(r.Direction, t.plane.Normal);
            if (det > -EPSILON && det < EPSILON)
                return null;
            float tPrime = t.plane.D - Vector3.Dot(r.Position, t.plane.Normal);
            Vector3 PPrime = det * r.Position + tPrime * r.Direction;
            float uPrime = Vector3.Dot(PPrime, t.barycentricPlane1.Normal) + det * t.barycentricPlane1.D;
            float vPrime = Vector3.Dot(PPrime, t.barycentricPlane2.Normal) + det * t.barycentricPlane2.D;
            Vector3 tuv = (1 / det) * new Vector3(tPrime, uPrime, vPrime);
            if (tuv.Y >= 0 && tuv.Z >= 0 && tuv.Y + tuv.Z <= 1 && tuv.X >= 0)
                return tuv.X;
            return null;
        }*/

        public static float? test(Ray r, Plane p)
        {
            return r.Intersects(p);
        }

        public static float? test(Ray r, BoundingBox b)
        {
            return r.Intersects(b);
        }
    }
}