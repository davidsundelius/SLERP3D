using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RacingGame.Logic;
using RacingGame.Graphics;
using RacingGame.Collision;

namespace RacingGame.Powerups
{
    class Missile : IPowerup, Logic.IUpdateable
    {
        private Ship ship;
        private static readonly int particlesPerMeter = 3;

        private float timeSinceTrigger = 0.0f;
        private bool awaitingBlast = false;
        Vector3 explosionPos = Vector3.Zero;
        Vector3 normal = Vector3.Up;

        private bool isUsed = false;

        public Missile(Ship s)
        {
            ship = s;
        }

        public static void spawnTrailParticles(Vector3 start, Vector3 end)
        {
            ParticleSystemBase particleSys = States.Game.ParticleSystem;
            Vector3 delta = (end - start);
            Vector3 dir = Vector3.Normalize(delta);
            int numParticles = (int)(delta.Length() * particlesPerMeter);
            float step = 1.0f / particlesPerMeter;

            for (int i = 0; i < numParticles; ++i)
            {
                Particle p = new Particle(start + i * step * dir)
                {
                    DeltaSize = 0.9f,
                    initAlpha = 0.3f,
                    LifeTime = 1.0f,
                    Size = 0.5f,
                    Color = new Vector4(0.2f, 0.2f, 0.2f, 0.3f)
                };

                particleSys.AddParticle(p);
            }
        }

        public void use(GameTime gt)
        {
            isUsed = true;

            if (awaitingBlast)
            {
                return;
            }

            
            Vector3 pos = ship.Position;
            Vector3 dir = ship.Dir;

            Ray r = new Ray(pos, dir);

            ResultRay res = CollisionManager.getInstance().testVsShips(r);

            if (!res.t.HasValue)
            {
                res = CollisionManager.getInstance().testVsMap(r, null);

                if (!res.t.HasValue)
                {
                    explosionPos = pos + r.Direction * 100.0f;
                    normal = -dir;
                }
            }
            else
            {
                //Do something against the other ship
            }

            if (res.t.HasValue)
            {
                explosionPos = pos + res.t.Value * res.ray.Value.Direction;
                normal = res.tri.Value.normal;
            }

            spawnTrailParticles(pos, explosionPos);
            Network.P2PManager.Instance.SendMissile(pos, explosionPos);

            awaitingBlast = true;
        }

        public void discard()
        {
            isUsed = true;
        }

        public bool inUse()
        {
            return isUsed;
        }

        public bool update(GameTime time)
        {
            if (awaitingBlast)
            {
                timeSinceTrigger += (float)time.ElapsedGameTime.TotalSeconds;

                if (timeSinceTrigger > 0.2f)
                {
                    States.Game.spawnExplosion(explosionPos);
                    awaitingBlast = false;
                }
            }
            return isUsed && !awaitingBlast;
        }

        public override string ToString()
        {
            return "Missile";
        }
    }
}
