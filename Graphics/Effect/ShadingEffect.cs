using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RacingGame.Graphics
{
    /// <summary>
    /// Effect used for the final shading of the objects.
    /// Author: Daniel Lindén
    /// </summary>
    class ShadingEffect : IEffect
    {
        private Effect effect;
        private EffectPass currentPass;
        private EffectTechnique shadingTechnique;
        private EffectPass phongPass;

        private Matrix view;
        private Matrix proj;

        #region Effect parameters
        private EffectParameter worldViewProj;
        private EffectParameter world;

        private EffectParameter invResolution;

        private EffectParameter decalTexture;
        private EffectParameter lightAccumulationBuffer;
        private EffectParameter ambientOcclusionBuffer;
        #endregion

        public ShadingEffect()
        {
            effect = RacingGame.contentManager.Load<Effect>("Shaders/shading");
            shadingTechnique = effect.Techniques["Shading"];
            phongPass = shadingTechnique.Passes["phong"];

            worldViewProj = effect.Parameters["WorldViewProj"];
            world = effect.Parameters["World"];

            invResolution = effect.Parameters["InvResolution"];

            decalTexture = effect.Parameters["DecalTexture"];
            lightAccumulationBuffer = effect.Parameters["LightAccumulationBuffer"];
            ambientOcclusionBuffer = effect.Parameters["AmbientOcclusionBuffer"];
        }

        public void begin(string technique, string pass)
        {
            effect.Begin();
            invResolution.SetValue(new Vector2(1.0f / GraphicsManager.getDevice().Viewport.Width, 1.0f / GraphicsManager.getDevice().Viewport.Height));
            currentPass = effect.Techniques[technique].Passes[pass];
            if (currentPass != null)
            {
                currentPass.Begin();
            }
            else
            {
                effect.End();
                throw new InvalidPassException(pass);
            }
        }

        public void begin()
        {
            effect.Begin();
            invResolution.SetValue(new Vector2(1.0f / GraphicsManager.getDevice().Viewport.Width, 1.0f / GraphicsManager.getDevice().Viewport.Height));
            phongPass.Begin();
            currentPass = phongPass;
        }

        public void end()
        {
            if (currentPass != null)
            {
                currentPass.End();
                effect.End();
                currentPass = null;
            }
        }

        public void setWorldMatrix(Matrix world)
        {
            this.world.SetValue(world);
            Matrix worldView = world * view;
            this.worldViewProj.SetValue(worldView * proj);
        }

        public void setDecalTexture(Texture texture)
        {
            decalTexture.SetValue(texture);
        }

        public void setLightAccumulationBuffer(Texture texture)
        {
            lightAccumulationBuffer.SetValue(texture);
        }

        public void setAmbientOcclusionBuffer(Texture texture)
        {
            ambientOcclusionBuffer.SetValue(texture);
        }

        public void setLight(Light l)
        {
        }

        public void setCamera(Camera c)
        {
            view = c.getViewMatrix();
            proj = c.getProjectionMatrix();
        }


        public void commit()
        {
            effect.CommitChanges();
        }
    }
}
