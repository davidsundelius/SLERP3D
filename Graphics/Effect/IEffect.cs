using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RacingGame.Graphics
{
    public class InvalidPassException : Exception
    {
        private string passName = "";
        public InvalidPassException(string pass)
        {
            passName = pass;
        }
        public override string Message
        {
            get
            {
                return "The specified pass was invalid. Pass " + passName + " could not be found.";
            }
        }
    }

    /// <summary>
    /// The interface for effects.
    /// Author: Daniel Lindén
    /// </summary>
    interface IEffect
    {
        void begin(string technique, string pass);
        void end();
        void setWorldMatrix(Matrix world);
        void setDecalTexture(Texture texture);
        void setLight(Light l);
        void setCamera(Camera c);
        void commit();
    }
}
