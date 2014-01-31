using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RacingGame.Graphics
{
    class BlurPostProcess : PostProcessEffect
    {
        private Effect effect;
        private EffectTechnique tech;
        private EffectPass passVertical;
        private EffectPass passHorizontal;
        private EffectParameter texture;
        private EffectParameter invResolution;
        private EffectParameter blurWeights;
        private RenderTarget2D intermediateBuffer;

        public BlurPostProcess()
        {
            effect = RacingGame.contentManager.Load<Effect>("Shaders/blurPostProcess");
            tech = effect.Techniques["Blur"];
            passVertical = tech.Passes["vertical"];
            passHorizontal = tech.Passes["horizontal"];

            texture = effect.Parameters["InputTexture"];
            invResolution = effect.Parameters["InvResolution"];
            blurWeights = effect.Parameters["Weights"];

            float[] weights = new float[9];
            float sum = 0.0f;
            for (int i = 0; i < 9; ++i)
            {
                weights[i] = gaussian(0.3f, (i - 4.0f) / 4.0f);
                sum += weights[i];
            
            }

            for (int i = 0; i < 9; ++i)
            {
                weights[i] /= sum;
            }

            blurWeights.SetValue(weights);
        }

        private float gaussian(float variance, float x)
        {
	        float a = 1 / ((float)Math.Sqrt(variance) * 2.506628f);
	        float exp = -((x * x) / (2 * variance));
	        return a * (float)Math.Pow(Math.E, exp);
        }
		

        public override void postProcess(RenderTarget2D input, RenderTarget2D output)
        {
            GraphicsDevice device = GraphicsManager.getDevice();

            if (intermediateBuffer == null || intermediateBuffer.Height != input.Height || intermediateBuffer.Width != input.Width
                || intermediateBuffer.MultiSampleType != input.MultiSampleType)
            {
                intermediateBuffer = new RenderTarget2D(input.GraphicsDevice, input.Width, input.Height, 1, input.Format, input.MultiSampleType, input.MultiSampleQuality);
            }
            invResolution.SetValue(new Vector2(1.0f / input.Width, 1.0f / input.Height));

            device.SetRenderTarget(0, intermediateBuffer);

            texture.SetValue(input.GetTexture());

            effect.Begin();
            passHorizontal.Begin();
            drawQuad();
            passHorizontal.End();

            device.SetRenderTarget(0, output);
            texture.SetValue(intermediateBuffer.GetTexture());

            passVertical.Begin();
            drawQuad();
            passVertical.End();

            effect.End();

            texture.SetValue((Texture)null);
        }
    }
}
