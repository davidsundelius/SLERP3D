using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RacingGame.Graphics
{
    enum LightType
    {
        Point,
        Projected,
        Directional,
        Spot,
        NumTypes = 4,
        None
    }
    /// <summary>
    /// An abstract light to be implemented for different types of lights.
    /// Author: Daniel Lindén
    /// </summary>
    abstract class Light : Logic.IUpdateable
    {
        public virtual Vector3 position { get; set; }
        public Vector3 direction { get; protected set; }
        public Vector3 up { get; protected set; }
        public Texture texture = null;
        public bool castsShadows = true;

        public Vector3 diffuse = Color.White.ToVector3();
        public float specularFactor = 0.4f;

        public virtual void setDirection(Vector3 direction, Vector3 up)
        {
            this.direction = direction;
            this.up = up;
        }

        public abstract BoundingBox getBoundingBox();
        public abstract float getRange();
        public abstract LightType getLightType();
        public abstract bool contains(Vector3 point);
        public abstract ShadowMapInfo[] getShadowMaps();
        public abstract void render();
        public abstract bool update(GameTime delta);
    }
}
