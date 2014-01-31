using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RacingGame.Logic;

namespace RacingGame.Graphics
{
    /// <summary>
    /// A node which can be stored in a scene graph
    /// Author: Daniel Lindén
    /// </summary>
    public abstract class Node : Logic.IUpdateable
    {
        private Vector3 pos = new Vector3(0.0f, 0.0f, 0.0f);
        private Quaternion rot = Quaternion.Identity;
        private float scale = 1.0f;
        private Matrix transform = Matrix.Identity;
        private bool visible = true;

        public bool Visible
        {
            set
            {
                visible = value;
            }
            get
            {
                return visible;
            }
        }

        private bool updatedTransform = false;

        public virtual bool update(GameTime time)
        {
            if (updatedTransform)
            {
                transform = Matrix.CreateScale(uniformScale) * Matrix.CreateFromQuaternion(rotation) * Matrix.CreateTranslation(position);
            }

            return false;
        }

        public abstract void render();
        public abstract RenderQueues getRenderQueue();

        public virtual void draw(BoundingFrustum viewFrustum)
        {
            if (visible)
            {
                GraphicsManager.getInstance().drawNode(this, getRenderQueue());
            }
        }

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
    }
}
