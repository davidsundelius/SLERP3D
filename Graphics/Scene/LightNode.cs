using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RacingGame.Graphics
{
    /// <summary>
    /// A scene node which contains a light of any type.
    /// Author: Daniel Lindén
    /// </summary>
    class LightNode : Node
    {
        public Light light;
        public LightNode(Light l)
        {
            light = l;
        }

        public override bool update(GameTime time)
        {
            base.update(time);
            light.position = position;
            light.update(time);
            return false;
        }

        public override void render()
        {
            light.render();
        }

        public Light getLight()
        {
            return light;
        }

        public override void draw(BoundingFrustum viewFrustum)
        {
            if (viewFrustum.Intersects(light.getBoundingBox()))
                GraphicsManager.getInstance().addLight(light);
        }

        public override RenderQueues getRenderQueue()
        {
            return RenderQueues.Normal;
        }
    }
}
