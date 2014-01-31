using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RacingGame.Graphics;
using RacingGame.Collision;
using Microsoft.Xna.Framework;

namespace RacingGame.Logic
{
    /// <summary>
    /// Class that describe the state of a checkpoint
    /// Author: David Sundelius
    /// </summary>

    class Checkpoint : Logic.IUpdateable
    {
        public BoundingBox boundingBox
        {
            get;
            private set;
        }

        public Vector3 PlayerPassPos
        {
            get;
            private set;
        }

        public Quaternion PlayerPassRot
        {
            get;
            private set;
        }

        public int CheckPointNr
        {
            get;
            private set;
        }

        public static Checkpoint finishLine = null;
        public static int nrOfCheckpoints = 0;

        public Checkpoint(int nr, BoundingBox bb)
        {
            CheckPointNr = nr;
            boundingBox = bb;
            CollisionManager.getInstance().addCheckpoint(this);
            if (nr == 0)
                finishLine = this;
        }

        public bool update(GameTime time)
        {
            return false;
        }

        public void passCheckpoint(Vector3 position, Quaternion rotation)
        {
            PlayerPassPos = position;
            PlayerPassRot = rotation;
        }
    }
}
