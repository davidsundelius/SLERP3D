using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RacingGame.Graphics;
using RacingGame.Collision;

namespace RacingGame.Logic
{
    /// <summary>
    /// The item that is picked up on the track to
    /// make a player get a powerup
    /// Author: David Sundelius
    /// </summary>
    class PowerupItem : Logic.IUpdateable
    {
        private bool active;
        private int respawnTime;
        private long startRespawnTime;
        private bool blowUpNextUpdate;
        private ModelNode node;

        public CollisionObject CollisionObject
        {
            get
            {
                return node.collisionObject;
            }
        }

        public BoundingSphere BoundingSphere
        {
            get
            {
                BoundingSphere bs = node.getModel().Meshes[0].BoundingSphere;
                bs.Center += node.position;
                return bs;
            }
        }

        public PowerupItem(int respawnTime, ModelNode node)
        {
            active = true;
            this.respawnTime = respawnTime;
            this.node = node;
            CollisionManager.getInstance().addPowerupItem(this);
        }

        public bool update(GameTime time)
        {
            float rot = 0.03f;
            Quaternion q = Quaternion.CreateFromAxisAngle(Vector3.Up, rot);
            node.rotation = Quaternion.Concatenate(node.rotation, q);

            node.update(time);
            if (blowUpNextUpdate)
            {
                startRespawnTime = (int)time.TotalGameTime.TotalMilliseconds;
                node.Visible = false;
                active = false;
                blowUpNextUpdate = false;
            }
            if ((int)time.TotalGameTime.TotalMilliseconds > startRespawnTime + respawnTime)
            {
                active = true;
                node.Visible = true;
            }
            return false;
        }

        public bool blowUp()
        {
            if (active && !blowUpNextUpdate)
            {
                blowUpNextUpdate = true;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
