using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RacingGame.Logic;

namespace RacingGame.Powerups
{
    class Speedup : IPowerup , Logic.IUpdateable
    {
        // variables that this class changes in a Ship
        private readonly float handlingCoeff = 0.8f;
        private readonly float accelerationCoeff = 1.2f;
        private readonly float maxSpeedCoeff = 1.3f;

        // time before it is removed
        private readonly float milliSecondsToLive = 1000 * 3.0f;
        private bool dieWithUpdate = false;

        // for computing when to kill the powerup
        private float milliSecondsSinceUse = 0;
        private bool isActive = false;

        private Ship s;

        public Speedup(Ship s)
        {
            this.s = s;
        }
        /************************************************
         *      METHODS FOR USE WITHIN THIS CLASS       *
         ************************************************/
        private void initializePowerup()
        {
            s.HandlingModifier      = handlingCoeff;
            s.AccelerationModifier  = accelerationCoeff;
            s.MaxSpeedModifier      = maxSpeedCoeff;
        }
        private void removePowerup()
        {
            s.HandlingModifier      = 1.0f;
            s.AccelerationModifier  = 1.0f;
            s.MaxSpeedModifier      = 1.0f;
        }

        /************************************************
         *      INTERFACE METHODS                       *
         ************************************************/
        #region IPowerup Members
        public void use(GameTime gt)
        {
            s.SpeedPower = true;
            //initializePowerup();
            isActive = true;
            Network.P2PManager.Instance.SendSpeedup(true);
            Sound.SoundManager.getInstance().playSound("Shield");
        }

        public void discard()
        {
            s.SpeedPower = false;
            dieWithUpdate = true;
            Network.P2PManager.Instance.SendSpeedup(false);
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

        ~Speedup()
        {
            if (s != null)
            {
                s.SpeedPower = false;
            }
        }

        public override string ToString()
        {
            return "Speedup";
        }
    }
}
