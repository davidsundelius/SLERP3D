using System;
using Microsoft.Xna.Framework;

namespace RacingGame.Collision
{
    /// <summary>
    /// A simple struct that represents a triangle.
    /// Author: Daniel Lindén
    /// </summary>
    [Serializable]
    struct Triangle
    {
        public readonly Vector3 vertex1;
        public readonly Vector3 vertex2;
        public readonly Vector3 vertex3;
        public readonly Vector3 normal;

        public Triangle(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            vertex1 = v1;
            vertex2 = v2;
            vertex3 = v3;

            normal = Vector3.Cross(v3 - v1, v2 - v1);
            normal.Normalize();
        }

        /*public readonly Plane plane;
        public readonly Plane barycentricPlane1;
        public readonly Plane barycentricPlane2;

        public Triangle(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            vertex1 = v1;
            vertex2 = v2;
            vertex3 = v3;

            plane = new Plane(v1, v2, v3);
            plane.Normal = Vector3.Cross(vertex2 - vertex1, vertex3 - vertex1);

            float triNormalLengthSq = plane.Normal.LengthSquared();

            barycentricPlane1 = new Plane();
            barycentricPlane1.Normal = Vector3.Cross(vertex3 - vertex1, plane.Normal) / triNormalLengthSq;
            barycentricPlane1.D = -Vector3.Dot(barycentricPlane1.Normal, vertex1);

            barycentricPlane2 = new Plane();
            barycentricPlane2.Normal = Vector3.Cross(plane.Normal, vertex2 - vertex1) / triNormalLengthSq;
            barycentricPlane2.D = -Vector3.Dot(barycentricPlane2.Normal, vertex1);
        }*/
    }
}