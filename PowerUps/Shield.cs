using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RacingGame.Logic;
using RacingGame.Graphics;

namespace RacingGame.Powerups
{
    class Shield : IPowerup, Logic.IUpdateable
    {
        private Ship ship;

        // time before it is removed
        private readonly float milliSecondsToLive = 5000.0f;
        private bool dieWithUpdate = false;

        // for computing when to kill the powerup
        private float milliSecondsSinceUse = 0;
        private bool isActive = false;

        public Shield(Ship ship)
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
            ship.node.shield.Visible = true;
            Network.P2PManager.Instance.SendShield(true);
            Sound.SoundManager.getInstance().playSound("Shield");
        }

        public void discard()
        {
            dieWithUpdate = true;
            ship.Invincible = false;
            ship.node.shield.Visible = false;
            Network.P2PManager.Instance.SendShield(false);
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

        public override string ToString()
        {
            return "Shield";
        }
    }
}
