using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RacingGame.Logic;
using RacingGame.Graphics;

namespace RacingGame.Powerups
{
    class DemonMode : IPowerup, Logic.IUpdateable
    {
        // time before it is removed
        private readonly float milliSecondsToLive = 1000 * 5.0f;
        private bool dieWithUpdate = false;

        // for computing when to kill the powerup
        private float milliSecondsSinceUse = 0;
        private bool isActive = false;
        private Ship ship;

        public DemonMode(Ship ship)
        {
            this.ship = ship;
        }

        #region IPowerup Members
        public void use(GameTime gt)
        {
            if (isActive)
                return;
            isActive = true;
            ship.Invincible = true;
            GraphicsManager.getInstance().DemonMode = true;
            Network.P2PManager.Instance.SendDemon(true);
            Sound.SoundManager.getInstance().playSound("Demon");
        }
        public void discard()
        {
            if (isActive)
                GraphicsManager.getInstance().DemonMode = false;
            dieWithUpdate = true;
            ship.Invincible = false;
            Network.P2PManager.Instance.SendDemon(false);
        }

        public bool inUse()
        {
            return isActive;
        }

        #endregion

        #region IUpdateable Members
        bool Logic.IUpdateable.update(GameTime time)
        {
            if (isActive)
            {
                milliSecondsSinceUse += time.ElapsedGameTime.Milliseconds;
                dieWithUpdate = milliSecondsToLive <= milliSecondsSinceUse || dieWithUpdate;
            }
            return dieWithUpdate;
        }
        #endregion

        public override string  ToString()
        {
 	         return "Demon Mode!";
        }
    }
}
