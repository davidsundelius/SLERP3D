using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PIXTools;

namespace RacingGame.Graphics
{
    class BloomPostProcess : PostProcessEffect
    {
        GraphicsDevice device = GraphicsManager.getDevice();

        Effect bloom;
        EffectTechnique tech;
        EffectPass passGlare;
        EffectPass passLuminance;
        EffectPass passCommit;
        EffectPass passDownsample;
        EffectPass passAfterImage;
        EffectPass passToneMap;
        EffectParameter inputTexture;
        EffectParameter outputTexture;
        EffectParameter luminanceTexture;
        EffectParameter invResolution;
        EffectParameter afterImageTexture;
        EffectParameter demonMode;
        EffectParameter afterImage;
        EffectParameter glareParams;

        private RenderTarget2D intermediateBuffer;
        private RenderTarget2D luminanceBuffer;
        private RenderTarget2D bloomBuffer;
        private RenderTarget2D toneMapBuffer;

        private BlurPostProcess blurPostProcess = new BlurPostProcess();

        private float afterImageRatio = 0.0f;
        private Vector4 bloomParameters = new Vector4();
        private bool demonModeOn = false;
        private bool demonModeChanged = false;

        public Texture2D Luminance
        {
            get { return luminanceBuffer.GetTexture(); }
        }

        public bool DemonMode
        {
            get
            {
                return demonModeOn;
            }
            set
            {
                if (value)
                {
                    afterImageRatio = 0.6f;
                    bloomParameters = new Vector4(0.3f, 0.3f, 0.3f, 3);
                }
                else
                {
                    afterImageRatio = 0.5f;
                    bloomParameters = new Vector4(0.4f, 0.4f, 0.4f, 2f);
                }
                demonModeOn = value;
                demonModeChanged = true;
            }
        }

        public BloomPostProcess()
        {
            bloom = RacingGame.contentManager.Load<Effect>("Shaders/bloomPostProcess");
            tech = bloom.Techniques["BloomPostProcess"];
            passDownsample = tech.Passes["downsample"];
            passGlare = tech.Passes["glare"];
            passLuminance = tech.Passes["luminance"];
            passCommit = tech.Passes["commit"];
            passAfterImage = tech.Passes["afterImage"];
            passToneMap = tech.Passes["tonemap"];
            inputTexture = bloom.Parameters["InputTexture"];
            outputTexture = bloom.Parameters["OutputTexture"];
            luminanceTexture = bloom.Parameters["LuminanceTexture"];
            afterImageTexture = bloom.Parameters["AfterImageTexture"];
            demonMode = bloom.Parameters["DemonMode"];
            afterImage = bloom.Parameters["AfterImage"];
            glareParams = bloom.Parameters["GlareParams"];
            invResolution = bloom.Parameters["InvResolution"];

            DemonMode = false;
        }

        public override void postProcess(RenderTarget2D input, RenderTarget2D output)
        {
            const int downSample = 4;

            if (intermediateBuffer == null || intermediateBuffer.Height != input.Height / downSample || intermediateBuffer.Width != input.Width / downSample
                || intermediateBuffer.MultiSampleType != input.MultiSampleType)
            {
                bloomBuffer = new RenderTarget2D(input.GraphicsDevice, input.Width / downSample, input.Height / downSample, 1,
                    input.Format, input.MultiSampleType, input.MultiSampleQuality);
                intermediateBuffer = new RenderTarget2D(input.GraphicsDevice, input.Width / downSample, input.Height / downSample, 1,
                    input.Format, input.MultiSampleType, input.MultiSampleQuality);
                toneMapBuffer = new RenderTarget2D(input.GraphicsDevice, input.Width, input.Height, 1,
                    input.Format, input.MultiSampleType, input.MultiSampleQuality);
                luminanceBuffer = new RenderTarget2D(input.GraphicsDevice, 1, 1, 1,
                    input.Format, input.MultiSampleType, input.MultiSampleQuality);
            }

            Vector2 res = new Vector2(1.0f / input.Width, 1.0f / input.Height);
            invResolution.SetValue(res);

            if (demonModeChanged)
            {
                demonMode.SetValue(demonModeOn);
                afterImage.SetValue(afterImageRatio);
                glareParams.SetValue(bloomParameters);
                demonModeChanged = false;
            }

            device.SetRenderTarget(0, bloomBuffer);

            inputTexture.SetValue(input.GetTexture());

            bloom.Begin();

            // downsample pass
            PIXTools.PIXTools.BeginEvent("downsample pass");
            passDownsample.Begin();
            drawQuad();
            passDownsample.End();
            PIXTools.PIXTools.EndEvent();

            // luminance pass
            device.SetRenderTarget(0, luminanceBuffer);

            inputTexture.SetValue(bloomBuffer.GetTexture());

            Vector2 dsRes = res * downSample;
            invResolution.SetValue(dsRes);

            PIXTools.PIXTools.BeginEvent("luminance pass");
            passLuminance.Begin();
            drawQuad();
            passLuminance.End();
            PIXTools.PIXTools.EndEvent();

            // glare pass
            device.SetRenderTarget(0, bloomBuffer);
            luminanceTexture.SetValue(luminanceBuffer.GetTexture());

            PIXTools.PIXTools.BeginEvent("bloom pass");
            passGlare.Begin();
            drawQuad();
            passGlare.End();
            PIXTools.PIXTools.EndEvent();

            bloom.End();

            // blur
            PIXTools.PIXTools.BeginEvent("blur pass");
            blurPostProcess.postProcess(bloomBuffer, bloomBuffer);
            PIXTools.PIXTools.EndEvent();

            bloom.Begin();

            // afterimage pass
            device.SetRenderTarget(0, intermediateBuffer);
            inputTexture.SetValue(bloomBuffer.GetTexture());

            PIXTools.PIXTools.BeginEvent("afterimage pass");
            passAfterImage.Begin();
            drawQuad();
            passAfterImage.End();
            PIXTools.PIXTools.EndEvent();

            // tone map pass
            device.SetRenderTarget(0, toneMapBuffer);
            inputTexture.SetValue(input.GetTexture());

            PIXTools.PIXTools.BeginEvent("tonemap pass");
            passToneMap.Begin();
            drawQuad();
            passToneMap.End();
            PIXTools.PIXTools.EndEvent();

            // commit pass
            device.SetRenderTarget(0, output);
            inputTexture.SetValue(intermediateBuffer.GetTexture());
            outputTexture.SetValue(toneMapBuffer.GetTexture());

            PIXTools.PIXTools.BeginEvent("commit glare pass");
            passCommit.Begin();
            drawQuad();
            passCommit.End();
            PIXTools.PIXTools.EndEvent();

            bloom.End();

            afterImageTexture.SetValue(intermediateBuffer.GetTexture());
            inputTexture.SetValue((Texture)null);
        }
    }
}
