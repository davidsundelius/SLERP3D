using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace RacingGame.Graphics
{
    /// <summary>
    /// An effect used for the particle system
    /// Author: David Sundelius
    /// </summary>
    class ParticleEffect : IEffect
    {
        private Effect effect;
        //private EffectPass currentPass;

        public ParticleEffect(Effect effect)
        {
            this.effect = effect;
        }

        public void begin(string technique, string pass)
        {
            effect.Begin();
        }

        public void end()
        {
            effect.End();
        }

        public void setWorldMatrix(Microsoft.Xna.Framework.Matrix world)
        {
            throw new NotImplementedException();
        }

        public void setDecalTexture(Microsoft.Xna.Framework.Graphics.Texture texture)
        {
            throw new NotImplementedException();
        }

        public void setLight(Light l)
        {
            throw new NotImplementedException();
        }

        public void setCamera(Camera c)
        {
            throw new NotImplementedException();
        }

        public void commit()
        {
            return;
        }
    }
}
