using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

namespace RacingGame.Graphics
{
    abstract class PostProcessEffect
    {
        private Mesh screenQuad = null;

        protected PostProcessEffect()
        {
            screenQuad = Mesh.createFullScreenQuad();
        }

        public abstract void postProcess(RenderTarget2D input, RenderTarget2D output);

        protected void drawQuad()
        {
            screenQuad.render();
        }
    }
}
