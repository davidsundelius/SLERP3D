using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

namespace RacingGame.Graphics
{
    class EmptyPostProcess : PostProcessEffect
    {
        Effect effect;
        EffectTechnique tech;
        EffectPass pass;
        EffectParameter texture;

        public EmptyPostProcess()
        {
            effect = RacingGame.contentManager.Load<Effect>("Shaders/emptyPostProcess");
            tech = effect.Techniques["EmptyPostProcess"];
            pass = tech.Passes["p0"];
            texture = effect.Parameters["InputTexture"];
        }

        public override void postProcess(RenderTarget2D input, RenderTarget2D output)
        {
            GraphicsManager.getDevice().SetRenderTarget(0, output);

            texture.SetValue(input.GetTexture());

            effect.Begin();
            pass.Begin();
            drawQuad();
            pass.End();
            effect.End();

            texture.SetValue((Texture)null);
            
        }
    }
}
