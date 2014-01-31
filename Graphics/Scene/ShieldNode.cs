using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RacingGame.Logic;

namespace RacingGame.Graphics
{
    class ShieldNode : ModelNode
    {
        private Effect effect;
        private EffectTechnique tech;
        private EffectPass pass;
        private EffectParameter offset;
        private EffectParameter texture;
        private EffectParameter viewProj;
        private EffectParameter world;
        protected Texture shieldTexture;

        private Vector2 offs = Vector2.Zero;

        private ShipNode shipNode;

        public ShieldNode(ShipNode ship, Model m)
            : base(m)
        {
            shipNode = ship;

            shieldTexture = RacingGame.contentManager.Load<Texture>("Textures/shield");
            effect = RacingGame.contentManager.Load<Effect>("Shaders/shield");
            tech = effect.Techniques["Shield"];
            pass = tech.Passes["p0"];
            offset = effect.Parameters["Offset"];
            texture = effect.Parameters["DecalTexture"];
            viewProj = effect.Parameters["ViewProj"];
            world = effect.Parameters["World"];
        }

        public override void draw(BoundingFrustum viewFrustum)
        {
            if (Visible)
            {
                GraphicsManager.getInstance().drawNode(this, getRenderQueue());
            }           
        }

        public override void render()
        {
            if (Visible)
            {
                PIXTools.PIXTools.BeginEvent("Shield");

                GraphicsManager.getInstance().setEffect(null);

                Matrix vp = GraphicsManager.getInstance().view * GraphicsManager.getInstance().proj;

                effect.Begin();
                texture.SetValue(shieldTexture);
                viewProj.SetValue(vp);
                world.SetValue(Matrix.CreateScale(1.05f) * shipNode.transformation);

                offs += new Vector2(0.001f, 0.001f);
                offset.SetValue(offs);

                pass.Begin();

                base.render();

                pass.End();

                effect.End();

                PIXTools.PIXTools.EndEvent();
            }
        }

        public override RenderQueues getRenderQueue()
        {
            return RenderQueues.Shield;
        }

    }
}
