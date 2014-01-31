using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RacingGame.Graphics
{
    /// <summary>
    /// A simple wrapper for effect used for rendering of, almost, arbitrary effects.
    /// Author: Daniel Lindén
    /// </summary>
    class GenericEffect : IEffect
    {
        private Effect effect;
        private EffectPass currentPass;

        private Matrix view;
        private Matrix proj;

        #region Effect parameters
        private EffectParameter worldViewProj;
        private EffectParameter worldView;
        private EffectParameter world;

        private EffectParameter decalTexture;
        #endregion

        public GenericEffect(Effect effect)
        {
            this.effect = effect;

            worldViewProj = effect.Parameters["WorldViewProj"];
            worldView = effect.Parameters["WorldView"];
            world = effect.Parameters["World"];

            decalTexture = effect.Parameters["DecalTexture"];
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
            decalTexture.SetValue(texture);
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
