using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RacingGame.Graphics
{
    /// <summary>
    /// The light which represents a directional light.
    /// Author: Daniel Lindén
    /// </summary>
    class DirectionalLight : Light
    {

        public override ShadowMapInfo[] getShadowMaps()
        {
            throw new NotImplementedException();
        }

        public override void render()
        {
            throw new NotImplementedException();
        }

        public override bool update(GameTime delta)
        {
            throw new NotImplementedException();
        }

        public override bool contains(Vector3 point)
        {
            throw new NotImplementedException();
        }

        public override LightType getLightType()
        {
            throw new NotImplementedException();
        }

        public override float getRange()
        {
            throw new NotImplementedException();
        }

        public override BoundingBox getBoundingBox()
        {
            throw new NotImplementedException();
        }
    }
}
