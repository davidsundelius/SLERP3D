using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RacingGame.Logic
{
    /// <summary>
    /// Interface to allow objects to update
    /// Author: David Sundelius
    /// </summary>
    public interface IUpdateable
    {
        bool update(GameTime time);
    }
}
