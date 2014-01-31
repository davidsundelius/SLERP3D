using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RacingGame.States;

namespace RacingGame.Logic
{
    /// <summary>
    /// Describes a player of the game
    /// Author: David Sundelius
    /// </summary>
    class Player
    {
        private Game game;
        public bool gameFinished
        {
            get;
            private set;
        }

        public Ship ship
        {
            get;
            private set;
        }
        public Checkpoint lastCp
        {
            get;
            private set;
        }
        public int lap {
            get;
            private set;
        }
        public int[] lapTimes
        {
            get;
            private set;
        }
        public int bestLap
        {
            get;
            private set;
        }

        private int nextCheckpoint;

        public Player(Game game, Ship ship)
        {
            gameFinished=false;
            lap = 1;
            this.game = game;
            lapTimes = new int[game.nrOfLaps+1];
            this.ship = ship;
            lastCp = Checkpoint.finishLine;
            lastCp.passCheckpoint(ship.Position, ship.Rotation);
            if (Checkpoint.nrOfCheckpoints == 1)
            {
                nextCheckpoint = 0;
            }
            else
            {
                nextCheckpoint = 1;
            }
        }

        public void passCheckpoint(Checkpoint cp)
        {
            if (nextCheckpoint==cp.CheckPointNr)
            {
                lastCp = cp;
                cp.passCheckpoint(ship.Position, ship.Rotation);
                if (cp.CheckPointNr == 0)
                {
                    finishLap(0);
                }
                else
                {
                    Sound.SoundManager.getInstance().playSound("Beep");
                }
                if (nextCheckpoint == Checkpoint.nrOfCheckpoints - 1)
                    nextCheckpoint = 0;
                else
                    nextCheckpoint++;
            }
        }

        private void finishLap(int totalTime) {
            Sound.SoundManager.getInstance().playSound("Shield");

            lapTimes[lap] = game.time;
            for (int i = lap-1; i > 0; i--)
            {
                lapTimes[lap] -= lapTimes[i];
            }
            if (bestLap == 0 || bestLap>lapTimes[lap])
            {
                bestLap = lapTimes[lap];
            }
            lap++;
            if (game.nrOfLaps+1 == lap)
                gameFinished = true;
        }
    }
}
