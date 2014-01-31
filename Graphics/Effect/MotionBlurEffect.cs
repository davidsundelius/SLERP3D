using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace RacingGame.Graphics
{
    class MotionBlurEffect : PostProcessEffect
    {
        private Effect effect;
        private EffectTechnique mbTech;
        private EffectPass mbPass;
        private EffectParameter invResolution;
        private EffectParameter depthTex;
        private EffectParameter sceneTex;
        private EffectParameter topLeft;
        private EffectParameter topRight;
        private EffectParameter bottomLeft;
        private EffectParameter bottomRight;
        private EffectParameter prevViewProj;
        private EffectParameter cameraPos;

        private Matrix viewProj;

        public MotionBlurEffect()
        {
            effect = RacingGame.contentManager.Load<Effect>("Shaders/motionBlur");
            mbTech = effect.Techniques["MotionBlur"];
            mbPass = mbTech.Passes["p0"];

            sceneTex = effect.Parameters["SceneTex"];
            depthTex = effect.Parameters["DepthTex"];
            invResolution = effect.Parameters["InvResolution"];
            topLeft = effect.Parameters["TopLeft"];
            topRight = effect.Parameters["TopRight"];
            bottomLeft = effect.Parameters["BottomLeft"];
            bottomRight = effect.Parameters["BottomRight"];
            prevViewProj = effect.Parameters["PrevViewProj"];
            cameraPos = effect.Parameters["CameraPos"];

            viewProj = GraphicsManager.getInstance().view * GraphicsManager.getInstance().proj;


        }
        /*
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

        }*/

        public void postProcess(RenderTarget2D input, RenderTarget2D output, Texture depth)
        {
            GraphicsDevice device = GraphicsManager.getDevice();

            device.SetRenderTarget(0, output);

            depthTex.SetValue(depth);
            sceneTex.SetValue(input.GetTexture());

            Vector2 invRes = new Vector2(1.0f / input.Width, 1.0f / input.Height);
            invResolution.SetValue(invRes);

            Camera cam = GraphicsManager.getInstance().getCamera();

            cameraPos.SetValue(cam.getPosition());

            float fovY = cam.getFov();
            float aspect = cam.getAspectRatio();

            float tanY = (float)Math.Tan(fovY / 2.0f);
            float tanX = (float)Math.Tan((fovY * aspect) / 2.0f);


            /* 
             Vector3 upLeft = rotation.Transform(Math::Vector3(-tanX, tanY, 1.0f).Normalized());
             Vector3 upRight = rotation.Transform(Math::Vector3(tanX, tanY, 1.0f).Normalized());
             Vector3 downLeft = rotation.Transform(Math::Vector3(-tanX, -tanY, 1.0f).Normalized());
             Vector3 downRight = rotation.Transform(Math::Vector3(tanX, -tanY, 1.0f).Normalized());*/

            Matrix rotation = new Matrix();
            Vector3 dir = cam.getDirection();
            Vector3 up = cam.getUp();
            Vector3 right = Vector3.Cross(dir, up);
            up = Vector3.Cross(right, dir);

            rotation.Forward = dir;
            rotation.Up = up;
            rotation.Right = right;

            Vector3 upLeft = Vector3.Transform(Vector3.Normalize(new Vector3(-tanX, -tanY, -1.0f)), rotation);
            Vector3 upRight = Vector3.Transform(Vector3.Normalize(new Vector3(tanX, -tanY, -1.0f)), rotation);
            Vector3 downLeft = Vector3.Transform(Vector3.Normalize(new Vector3(-tanX, tanY, -1.0f)), rotation);
            Vector3 downRight = Vector3.Transform(Vector3.Normalize(new Vector3(tanX, tanY, -1.0f)), rotation);

            topLeft.SetValue(upLeft);
            topRight.SetValue(upRight);
            bottomLeft.SetValue(downLeft);
            bottomRight.SetValue(downRight);

            prevViewProj.SetValue(viewProj);

            effect.Begin();
            mbPass.Begin();

            drawQuad();

            mbPass.End();
            effect.End();

            viewProj = GraphicsManager.getInstance().view * GraphicsManager.getInstance().proj;
        }

        public override void postProcess(RenderTarget2D input, RenderTarget2D output)
        {
            throw new NotImplementedException();
        }
    }
}
