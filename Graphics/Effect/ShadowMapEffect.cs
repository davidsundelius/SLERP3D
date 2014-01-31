using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RacingGame.Graphics
{
    /// <summary>
    /// The effect used to create shadow maps.
    /// Author: Daniel Lindén
    /// </summary>
    class ShadowMapEffect : IEffect
    {
        private Effect effect;
        private EffectTechnique shadowMapTechnique;
        private EffectPass normalPass;
        private EffectPass currentPass;

        private Matrix view;
        private Matrix proj;

        #region Effect parameters
        private EffectParameter worldViewProj;
        private EffectParameter worldView;
        private EffectParameter world;
        #endregion

        public ShadowMapEffect()
        {
            effect = RacingGame.contentManager.Load<Effect>("Shaders/shadowMap");
            shadowMapTechnique = effect.Techniques["shadowMap"];
            normalPass = shadowMapTechnique.Passes["normal"];

            world = effect.Parameters["World"];
            worldViewProj = effect.Parameters["WorldViewProj"];
            worldView = effect.Parameters["WorldView"];

            world = effect.Parameters["World"];
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
            normalPass.Begin();
            currentPass = normalPass;
        }

        public void end()
        {
            currentPass.End();
            currentPass = null;
            effect.End();
        }

        public void setWorldMatrix(Microsoft.Xna.Framework.Matrix world)
        {
            this.world.SetValue(world);
            worldView.SetValue(world * view);
            worldViewProj.SetValue(world * view * proj);
        }

        public void setShadowMapInfo(ShadowMapInfo sm)
        {
            view = sm.view;
            proj = sm.projection;
        }

        public void setDecalTexture(Microsoft.Xna.Framework.Graphics.Texture texture)
        {
        }

        public void setLight(Light l)
        {
        }

        public void setCamera(Camera c)
        {
        }

        public void commit()
        {
            effect.CommitChanges();
        }
    }
}
