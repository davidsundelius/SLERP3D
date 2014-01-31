using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RacingGame.Logic;
using RacingGame.Graphics;

namespace RacingGame.Collision
{
    /// <summary>
    /// The class which manages collision objects.
    /// Author: Daniel Lindén
    /// </summary>
    class CollisionManager : Logic.IUpdateable
    {
        private List<ShipNode> shipObjects = new List<ShipNode>();
        /// <summary>
        /// CollisionObject/Vector3[2] {current position, previous position}
        /// </summary>
        private Hashtable shipPositions = new Hashtable();
        /// <summary>
        /// BoundingSphere/float (time since initialization)
        /// </summary>
        private List<Explosion> explosions = new List<Explosion>();
        private List<PowerupItem> powerupObjects = new List<PowerupItem>();
        private List<CollisionObject> mapObjects = new List<CollisionObject>();
        private List<Checkpoint> checkpointObjects = new List<Checkpoint>();

        private static CollisionManager instance = new CollisionManager();

        private class Explosion
        {
            public BoundingSphere boundingSphere;
            public readonly int updatesActive = 0;
            private int updatesSinceActivation;

            public Explosion(BoundingSphere bs, int u)
            {
                boundingSphere = bs;
                updatesActive = u;
            }

            public void Update()
            {
                updatesSinceActivation++;
            }

            public bool HasExpired()
            {
                return updatesSinceActivation > updatesActive;
            }
        }

        private CollisionManager()
        {
        }

        public static CollisionManager getInstance()
        {
            return instance;
        }

        public bool update(GameTime gt)
        {
            foreach (ModelNode node in shipObjects)
            {
                CollisionObject c = node.collisionObject;
                ((Vector3[])shipPositions[c])[1] = ((Vector3[])shipPositions[c])[0];
                ((Vector3[])shipPositions[c])[0] = c.position;
                c.updateTransform();
            }
            for (int i = 0; i < explosions.Count; i++)
            {
                explosions[i].Update();
                if (explosions[i].HasExpired())
                    explosions.Remove(explosions[i]);
            }
            return false;
        }

        public void addExplosion(Vector3 position)
        {
            explosions.Add(new Explosion(new BoundingSphere(position, 10f), 1));
        }

        public void addShip(ShipNode n)
        {
            shipObjects.Add(n);
            shipPositions.Add(n.collisionObject, new Vector3[2]);
        }

        public void addCheckpoint(Checkpoint obj)
        {
            checkpointObjects.Add(obj);
        }

        public void addPowerupItem(Logic.PowerupItem powerup)
        {
            powerupObjects.Add(powerup);
        }

        public void addMapObject(CollisionObject obj)
        {
            mapObjects.Add(obj);
        }

        public ResultRay testVsShips(Ray r)
        {
            Vector3 foo = new Vector3();
            bool bar = false;
            return testVsShips(r, ref foo, ref bar);
        }

        public ResultRay testVsShips(Ray r, ref Vector3 velocity, ref bool isDemon)
        {
            ResultRay res = new ResultRay();
            ShipNode shipNode = null;
            foreach (ShipNode n in shipObjects)
            {
                if (!n.Visible)
                    continue;
                CollisionObject c = n.collisionObject;
                if (!res.t.HasValue)
                {
                    res = c.test(r);
                    shipNode = n;
                }
                else
                {
                    ResultRay tmpTest = c.test(r);
                    if (tmpTest.t.HasValue)
                    {
                        if (res.t.Value > tmpTest.t.Value)
                        {
                            res = tmpTest;
                            shipNode = n;
                        }
                    }
                }
            }
            if (res.obj != null)
            {
                velocity = ((Vector3[])shipPositions[res.obj])[1] - res.obj.position;
                isDemon = shipNode.demonMode;
            }
            return res;
        }

        public Vector3[] testVsExplosions(CollisionObject obj)
        {
            List<Vector3> distances = new List<Vector3>(explosions.Count);
            BoundingSphere shipBS = ((TriangleMeshObject)obj).getTriangleMesh().getBoundingSphere();
            shipBS.Center += obj.position;
            foreach (Explosion explosion in explosions)
            {
                if (explosion.boundingSphere.Intersects(shipBS))
                    distances.Add(explosion.boundingSphere.Center - shipBS.Center);
            }
            return distances.ToArray();
        }

        public Logic.PowerupItem testVsPowerups(CollisionObject obj)
        {
            BoundingSphere bs = ((TriangleMeshObject)obj).getTriangleMesh().getBoundingSphere();
            bs.Center += obj.position;
            foreach (var powerup in powerupObjects)
            {
                if (bs.Intersects(powerup.BoundingSphere))
                    return powerup;
            }
            return null;
        }

        public ResultRay testVsMap(Ray r, float? maxTracingDistance)
        {
            ResultRay res = new ResultRay();

            foreach (CollisionObject c in mapObjects)
            {
                if (!res.t.HasValue)
                {
                    res = c.test(r, maxTracingDistance);
                    res.obj = c;
                }
                else
                {
                    ResultRay tmpTest = c.test(r, maxTracingDistance);
                    if (tmpTest.t.HasValue)
                    {
                        if (res.t.Value > tmpTest.t.Value)
                        {
                            res = tmpTest;
                            res.obj = c;
                        }
                    }
                }
            }

            return res;
        }

        /// <summary>
        /// Tests whether a CollisionObject is within the map's bounding box
        /// </summary>
        /// <param name="obj">The object to be tested against the map's bounding box</param>
        /// <returns></returns>
        public bool testVsMap(CollisionObject obj)
        {
            BoundingSphere bs = ((TriangleMeshObject)obj).getTriangleMesh().getBoundingSphere();
            bs.Center += obj.position;
            foreach (TriangleMeshObject c in mapObjects)
            {
                if (c.getTriangleMesh().getBoundingBox().Intersects(bs))
                    return true;
            }
            return false;
        }

        public Checkpoint testVsCheckpoint(CollisionObject obj)
        {
            BoundingSphere bs = ((TriangleMeshObject)obj).getTriangleMesh().getBoundingSphere();
            bs.Center += obj.position;
            foreach (var checkpoint in checkpointObjects)
            {
                if (checkpoint.boundingBox.Intersects(bs))
                    return checkpoint;
            }
            return null;
        }

        public void clear()
        {
            mapObjects.Clear();
            shipObjects.Clear();
        }

        public void powerUpClear()
        {
            powerupObjects.Clear();
        }

    }
}