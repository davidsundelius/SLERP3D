using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RacingGame.Collision
{
    /// <summary>
    /// The abstract class for different collision objects.
    /// Author: Daniel Lindén
    /// </summary>
    abstract class CollisionObject
    {
        private Vector3 pos = new Vector3(0.0f, 0.0f, 0.0f);
        private Quaternion rot = Quaternion.Identity;
        private float scale = 1.0f;
        private Matrix transform = Matrix.Identity;

        private bool updatedTransform = false;

        public Vector3 position
        {
            get
            {
                return pos;
            }
            set
            {
                pos = value;
                updatedTransform = true;
            }
        }

        public Quaternion rotation
        {
            get
            {
                return rot;
            }
            set
            {
                rot = value;
                updatedTransform = true;
            }
        }

        public float uniformScale
        {
            get
            {
                return scale;
            }
            set
            {
                scale = value;
                updatedTransform = true;
            }
        }

        public Matrix transformation
        {
            get
            {
                return transform;
            }
        }

        public ResultRay test(Ray r)
        {
            return test(r, null);
        }

        public abstract ResultRay test(Ray r, float? maxTracingDistance);

        public bool updateTransform()
        {
            if (updatedTransform)
            {
                transform = Matrix.CreateScale(uniformScale) * Matrix.CreateFromQuaternion(rotation) * Matrix.CreateTranslation(position);
            }

            return false;
        }
    }
}
