using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace RacingGame.Graphics
{
    class SSAOEffect : PostProcessEffect
    {
        private Effect effect;
        private EffectTechnique ssaoTech;
        private EffectPass ssaoPass;
        private EffectParameter normalBuffer;
        private EffectParameter depthBuffer;
        private EffectParameter randomBuffer;
        private EffectParameter invResolution;
        private EffectParameter resolution;
        private EffectParameter zoomValues;
        private EffectParameter frustumDir;
        private EffectParameter view;

        private Texture randomTexture;


        public SSAOEffect()
        {
            effect = RacingGame.contentManager.Load<Effect>("Shaders/SSAOEffect");
            ssaoTech = effect.Techniques["SSAO"];
            ssaoPass = ssaoTech.Passes["SSAO"];

            normalBuffer = effect.Parameters["NormalBuffer"];
            depthBuffer = effect.Parameters["DepthBuffer"];
            randomBuffer = effect.Parameters["RandomBuffer"];
            invResolution = effect.Parameters["InvResolution"];
            resolution = effect.Parameters["Resolution"];
            zoomValues = effect.Parameters["ZoomValues"];
            frustumDir = effect.Parameters["FrustumDir"];
            view = effect.Parameters["View"];


            randomTexture = RacingGame.contentManager.Load<Texture>("Textures/SSAO/noise_1");
        }

        public void renderSSAO(RenderTarget2D depth, Texture normals)
        {
            PIXTools.PIXTools.BeginEvent("SSAO");

            depthBuffer.SetValue(depth.GetTexture());
            Vector2 invRes = new Vector2(1.0f / depth.Width, 1.0f / depth.Height);
            invResolution.SetValue(invRes);
            resolution.SetValue(new Vector2((float)depth.Width, (float)depth.Height));

            Camera cam = GraphicsManager.getInstance().getCamera();

            Vector2 zoom = cam.getZoomValues();
            zoomValues.SetValue(zoom);

            float fovY = cam.getFov();
            float aspect = cam.getAspectRatio();

            float tanY = (float)Math.Tan(fovY / 2.0f);
            float tanX = (float)Math.Tan((fovY * aspect) / 2.0f);

            Vector2 frustDir = new Vector2(tanX, -tanY);
            frustumDir.SetValue(frustDir);

            Matrix viewProj = GraphicsManager.getInstance().view * GraphicsManager.getInstance().proj;
            view.SetValue(viewProj);

            randomBuffer.SetValue(randomTexture);
            normalBuffer.SetValue(normals);


            effect.Begin();
            ssaoPass.Begin();
            drawQuad();
            ssaoPass.End();
            effect.End();

            PIXTools.PIXTools.EndEvent();

        }

        public override void postProcess(RenderTarget2D input, RenderTarget2D output)
        {
            throw new InvalidOperationException();
        }

    }
}
