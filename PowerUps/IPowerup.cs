using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RacingGame.Logic;


namespace RacingGame.Powerups
{
    /// <summary>
    /// Interface describing a general powerup
    /// Author: Alexander Göransson
    /// </summary>
    interface IPowerup : Logic.IUpdateable
    {
        void use(GameTime gt);
        /// <summary>
        /// i.e. the SpeedUp Powerup will need to deactivate its
        /// effects when being thrown away (if it was in use)
        /// </summary>
        void discard();

        bool inUse();
    }
}
