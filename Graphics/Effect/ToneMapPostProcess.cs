using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RacingGame.Graphics
{
    class ToneMapPostProcess : PostProcessEffect
    {
        Effect effect;
        EffectTechnique tech;
        EffectPass passToneMap;
        EffectParameter texture;
        EffectParameter luminanceTexture;

        public Texture2D Luminance
        {
            get;
            set;
        }

        public ToneMapPostProcess()
        {
            effect = RacingGame.contentManager.Load<Effect>("Shaders/toneMapPostProcessNew");
            tech = effect.Techniques["ToneMapPostProcess"];
            passToneMap = tech.Passes["main"];
            texture = effect.Parameters["InputTexture"];
            luminanceTexture = effect.Parameters["LuminanceTexture"];
        }

        public override void postProcess(RenderTarget2D input, RenderTarget2D output)
        {
            luminanceTexture.SetValue(Luminance);

            GraphicsDevice device = GraphicsManager.getDevice();

            device.SetRenderTarget(0, output);

            texture.SetValue(input.GetTexture());

            effect.Begin();

            passToneMap.Begin();
            drawQuad();
            passToneMap.End();

            effect.End();

            texture.SetValue((Texture)null);
        }
    }
}
