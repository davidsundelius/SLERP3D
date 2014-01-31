using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RacingGame.Collision
{
    /// <summary>
    /// Struct used for keeping track of data returned from a collision test.
    /// Author: Daniel Lindén
    /// </summary>
    struct ResultRay
    {
        /// <summary>
        /// The triangle which was found to intersect with the ray.
        /// Might be null if the object didn't consist of triangles.
        /// </summary>
        public Triangle? tri;
        /// <summary>
        /// The distance at which the ray collides with the object.
        /// In the ray formula: p(t) = o + t * d
        /// </summary>
        public float? t;
        /// <summary>
        /// The ray which was tested for collision.
        /// </summary>
        public Ray? ray;
        /// <summary>
        /// The object that the ray collided with.
        /// </summary>
        public CollisionObject obj;
    }
}
