using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RacingGame.Graphics
{
    /// <summary>
    /// The effect used to create the g-buffers which are to be used in the light accumulation pass.
    /// Author: Daniel Lindén
    /// </summary>
    class GBufferEffect : IEffect
    {
        private Effect effect;
        private EffectPass currentPass;
        private EffectTechnique gBuffersTechnique;
        private EffectPass staticSpecularPass;

        private Matrix view;
        private Matrix proj;

        #region Effect parameters
        private EffectParameter worldViewProj;
        private EffectParameter worldView;
        private EffectParameter world;

        private EffectParameter farClipDistance;
        private EffectParameter specularFactor;
        private EffectParameter cameraPos;
        #endregion

        public GBufferEffect()
        {
            effect = RacingGame.contentManager.Load<Effect>("Shaders/Gbuffers");
            gBuffersTechnique = effect.Techniques["GBuffers"];
            staticSpecularPass = gBuffersTechnique.Passes["staticSpecular"];

            worldViewProj = effect.Parameters["WorldViewProj"];
            worldView = effect.Parameters["WorldView"];
            world = effect.Parameters["World"];

            farClipDistance = effect.Parameters["FarClipDistance"];
            specularFactor = effect.Parameters["SpecularFactor"];
            cameraPos = effect.Parameters["CameraPos"];

        }

        public void begin(string technique, string pass)
        {
            effect.Begin();
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
            staticSpecularPass.Begin();
            currentPass = staticSpecularPass;
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
            this.worldView.SetValue(worldView);
            this.worldViewProj.SetValue(worldView * proj);
        }

        public void setDecalTexture(Texture texture)
        {
            
        }

        public void setLight(Light l)
        {
        }

        public void setCamera(Camera c)
        {
            farClipDistance.SetValue(c.getFarDistance());
            cameraPos.SetValue(c.getPosition());
            view = c.getViewMatrix();
            proj = c.getProjectionMatrix();
        }


        public void commit()
        {
            effect.CommitChanges();
        }
    }
}
