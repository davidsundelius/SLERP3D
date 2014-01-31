using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RacingGame.Logic;
using RacingGame.Graphics;

namespace RacingGame.Powerups
{
    class Powerpack : IPowerup, Logic.IUpdateable
    {
        private bool dieWithUpdate = false;
        private Ship ship;

        public Powerpack(Ship ship)
        {
            this.ship = ship;
        }

        #region IPowerup Members
        public void use(GameTime gt)
        {
            States.Game.spawnGlow(ship.Position);
            ship.Health = 1.0f;
            dieWithUpdate = true;
            Network.P2PManager.Instance.SendPowerpack();
            Sound.SoundManager.getInstance().playSound("Shield");
        }

        public void discard()
        {
            dieWithUpdate = true;
        }

        public bool inUse()
        {
            return false;
        }
        #endregion

        #region IUpdateable Members
        bool Logic.IUpdateable.update(GameTime time)
        {
            return dieWithUpdate;
        }
        #endregion

        public override string ToString()
        {
 	         return "Power pack";
        }
    }
}
