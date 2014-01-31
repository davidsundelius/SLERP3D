using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RacingGame.Collision;
using RacingGame.Graphics;
using RacingGame.Sys;
using RacingGame.States;

namespace RacingGame.Logic
{
    /// <summary>
    /// Describes a ship in the game.
    /// Author: Rickard von Haugwitz
    /// </summary>
    class Ship : Logic.IUpdateable
    {
        #region Fields

        private enum Direction
        {
            Left,
            Right,
            None
        }

        // Constants depending on the ship type, in case different models will be implemented
        public readonly float Handling = 6.0f;
        public readonly float Acceleration = 0.7f;
        public readonly float MaxSpeed = 300.0f;
        public readonly float DesiredHeight = 0.7f;

        // Variables describing the current state of the ship
        public ShipNode node
        {
            get;
            private set;
        }
        private Vector3 heading;
        private Vector3 velocity = Vector3.Zero;
        private bool usePowerup;
        private float boostRemaining = 1.0f;
        private bool active;
        private double timeOfDeath;

        private Direction rolling = Direction.None;

        private float distanceToGround;
        private Vector3 groundNormal = Vector3.Up;

        private Ray?[,] downRays;
        private Ray?[,] upRays;
        /// <summary>
        /// [left, forwards, right, backwards]
        /// </summary>
        private Ray?[][,] sideRays = new Ray?[4][,];

        private float deltaT;

        private States.Game game;

        #region Properties

        public float HandlingModifier
        {
            get;
            set;
        }
        public float AccelerationModifier
        {
            get;
            set;
        }
        public float MaxSpeedModifier
        {
            get;
            set;
        }
        public float Health
        {
            get;
            set;
        }
        public float Boost
        {
            get
            {
                return boostRemaining;
            }
            set
            {
                boostRemaining = value;
            }
        }
        public bool Active
        {
            get
            {
                return active;
            }
            set
            {
                active = value;
            }
        }
        public bool Invincible;
        public Vector3 Velocity
        {
            get
            {
                return velocity - project(velocity, Vector3.Transform(Vector3.Up, node.rotation));
            }
        }

        public Vector3 Position
        {
            get
            {
                return node.position;
            }
        }

        public Quaternion Rotation
        {
            get
            {
                return node.rotation;
            }
        }
        public Vector3 Dir
        {
            get
            {
                return Vector3.Transform(Vector3.Forward, node.rotation);
            }
        }

        public bool SpeedPower
        {
            get;
            set;
        }
        public Powerups.IPowerup powerup
        {
            get;
            private set;
        }


        #endregion

        // "Jag är JÄTTEbra på att göra texturer! Men de blir inte så snygga." / Daniel

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new Ship instance from a given node.
        /// </summary>
        /// <param name="node">The node representing the ship in the graphics</param>
        public Ship(ShipNode node, States.Game game)
        {
            this.node = node;
            resetShip();
            Active = false;
            initRays(3, 4, 3, 5, 2, 3);
            this.game = game;
        }

        #endregion

        #region Collision detection

        private void initRays(int numDownX, int numDownZ, int numPerSideY, int numPerSideZ, int numUpX, int numUpZ)
        {
            TriangleMeshObject collisionObject = new TriangleMeshObject(node.getModel().Meshes[0]);
            BoundingBox boundingBox = collisionObject.getTriangleMesh().getBoundingBox();
            BoundingBox scaledBB = new BoundingBox(boundingBox.Min * 0.9f, boundingBox.Max * 0.9f);

            // Initialize down rays
            downRays = new Ray?[numDownX, numDownZ];

            float xDeltaDown = (scaledBB.Max.X - scaledBB.Min.X) / numDownX;
            float zDeltaDown = (scaledBB.Max.Z - scaledBB.Min.Z) / numDownZ;
            for (int x = 0; x < numDownX; x++)
            {
                float xPos = xDeltaDown * x + scaledBB.Min.X;
                for (int z = 0; z < numDownZ; ++z)
                {
                    float zPos = zDeltaDown * z + scaledBB.Min.Z;
                    ResultRay res = collisionObject.test(
                        new Ray(new Vector3(xPos, boundingBox.Min.Y, zPos), Vector3.Up));

                    if (res.t.HasValue)
                    {
                        float yPos = (res.ray.Value.Position + res.ray.Value.Direction * res.t.Value).Y;
                        downRays[x, z] = new Ray(new Vector3(xPos, yPos, zPos), Vector3.Down);
                    }
                    else
                    {
                        downRays[x, z] = null;
                    }
                }
            }

            // Initialize up rays
            upRays = new Ray?[numUpX, numUpZ];

            float xDeltaUp = (scaledBB.Max.X - scaledBB.Min.X) / numUpX;
            float zDeltaUp = (scaledBB.Max.Z - scaledBB.Min.Z) / numUpZ;
            for (int x = 0; x < numUpX; x++)
            {
                float xPos = xDeltaUp * x + scaledBB.Min.X;
                for (int z = 0; z < numUpZ; ++z)
                {
                    float zPos = zDeltaUp * z + scaledBB.Min.Z;
                    ResultRay res = collisionObject.test(
                        new Ray(new Vector3(xPos, boundingBox.Max.Y, zPos), Vector3.Down));

                    if (res.t.HasValue)
                    {
                        float yPos = (res.ray.Value.Position + res.ray.Value.Direction * res.t.Value).Y;
                        upRays[x, z] = new Ray(new Vector3(xPos, yPos, zPos), Vector3.Up);
                    }
                    else
                    {
                        upRays[x, z] = null;
                    }
                }
            }

            // Initialize side rays
            for (int i = 0; i < 4; i++)
            {
                sideRays[i] = new Ray?[numPerSideY, numPerSideZ];
                for (int j = 0; j < numPerSideY; j++)  // Create numPerSideY rays in the Y dimension
                {
                    float yPos = ((scaledBB.Max.Y + scaledBB.Min.Y) / (numPerSideY + 1)) * (j + 1);
                    Vector3 delta = Vector3.Zero;
                    Vector3 startPos = new Vector3();
                    Vector3 testDirection = new Vector3();
                    if (i % 2 == 0)
                    {
                        startPos.Y = yPos;
                        startPos.Z = scaledBB.Min.Z;
                        if (i == 0) // left
                        {
                            startPos.X = boundingBox.Min.X;
                            testDirection = Vector3.Right;
                        }
                        else   // right
                        {
                            startPos.X = boundingBox.Max.X;
                            testDirection = Vector3.Left;
                        }

                        delta = new Vector3(0.0f, 0.0f, (scaledBB.Max.Z - scaledBB.Min.Z) / numPerSideZ);

                    }
                    else
                    {
                        startPos.Y = yPos;
                        startPos.X = scaledBB.Min.X;
                        if (i == 1) // forwards
                        {
                            startPos.Z = boundingBox.Min.Z;
                            testDirection = Vector3.Backward;
                        }
                        else   // backwards
                        {
                            startPos.Z = boundingBox.Max.Z;
                            testDirection = Vector3.Forward;
                        }

                        delta = new Vector3((scaledBB.Max.X - scaledBB.Min.X) / numPerSideZ, 0.0f, 0.0f);
                    }

                    for (int step = 0; step < numPerSideZ; ++step)
                    {

                        ResultRay test = collisionObject.test(new Ray(startPos + delta * step, testDirection));

                        if (test.t.HasValue)
                        {
                            Vector3 intersectPoint = test.ray.Value.Position + test.ray.Value.Direction * test.t.Value;
                            sideRays[i][j, step] = new Ray(intersectPoint, -testDirection);
                        }
                        else
                        {
                            sideRays[i][j, step] = null;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles collision detection and related functionality.
        /// </summary>
        private void handleCollisions()
        {
            // evaluate the down rays and update the ground normal with an average obtained from the results
            Vector3? scaledNormal = evaluateRays(downRays, null);
            if (scaledNormal.HasValue)  // did we get any usable results?
            {
                distanceToGround = scaledNormal.Value.Length();
                groundNormal = scaledNormal.Value;
                groundNormal.Normalize();
            }
            else
            {
                distanceToGround = 0.0f;
                groundNormal = Vector3.Transform(Vector3.Up, node.rotation);
            }

            Vector3[] sides = { Vector3.Left, Vector3.Forward, Vector3.Right, Vector3.Backward };
            // evaluate side rays
            for (int i = 0; i < sides.Length; i++)
            {
                evaluateRays(sideRays[i], Vector3.Dot(velocity, sides[i]) * deltaT);
            }

            // evaluate up rays
            evaluateRays(upRays, Vector3.Dot(velocity, Vector3.Up) * deltaT);

            // test vs explosions
            foreach (Vector3 d in CollisionManager.getInstance().testVsExplosions(node.collisionObject))
            {
                Vector3 force = -d;
                force.Normalize();
                float dist = d.Length();
                force *= 5f / dist;
                velocity += force;
                if (!Invincible)
                {
                    Health -= 1 / dist;
                }
            }

            // test to see if we have fallen off the track
            if (!CollisionManager.getInstance().testVsMap(node.collisionObject))
                Health = 0.0f;  // if we have fallen off, destroy the ship

            // test vs powerup items
            PowerupItem powerupRes = CollisionManager.getInstance().testVsPowerups(node.collisionObject);
            if (powerupRes != null)
            {
                if (powerupRes.blowUp())
                {
                    if (powerup == null)
                        powerup = game.getNewPowerup();
                }
            }

            // test vs checkpoints
            Checkpoint checkpointRes = CollisionManager.getInstance().testVsCheckpoint(node.collisionObject);
            if (checkpointRes != null)
                game.Player.passCheckpoint(checkpointRes);
        }

        /// <summary>
        /// Evaluates an array of rays, performing collisions where necessary. Returns an average of the
        /// target normals, scaled to the average distance to the targets.
        /// </summary>
        /// <param name="rays">A set of rays to be evaluated, all pointing in the same direction</param>
        /// <returns>The average normal of the targets, scaled to the distance to the target, or null.</returns>
        private Vector3? evaluateRays(Ray?[,] rays, float? maxTracingDistance)
        {
            float? averageDistance = null;
            Vector3 averageNormal = Vector3.Zero;

            foreach (Ray? ray in rays)
            {
                if (!ray.HasValue)
                    continue;

                Ray test = new Ray(Vector3.Transform(ray.Value.Position, node.transformation),
                    Vector3.Transform(ray.Value.Direction, node.rotation));

                // test vs other ships
                Vector3 vs = new Vector3();
                bool isDemon = false;
                ResultRay shipRes = CollisionManager.getInstance().testVsShips(test, ref vs, ref isDemon);
                if (shipRes.t.HasValue)
                {
                    // would we pass through the target on the next update?
                    if (shipRes.t.Value <= deltaT * Vector3.Dot(velocity, -shipRes.tri.Value.normal))
                    {
                        vs /= deltaT;   // adjust the other ship's velocity to the same unit
                        float m1 = ((TriangleMeshObject)node.collisionObject).getTriangleMesh().getBoundingSphere().Radius;
                        float m2 = ((TriangleMeshObject)shipRes.obj).getTriangleMesh().getBoundingSphere().Radius;
                        const float Cr = 1.0f; // coefficient of restitution
                        if (!Invincible)
                        {
                            Health -= (velocity.Length() / 350.0f);
                        }
                        velocity = (Cr * m2 * (vs - velocity) + m1 * velocity - m2 * vs) / (m1 + m2);

                        if (isDemon)
                            Health -= 666.0f;  // if the colliding ship is a demon... INSTADEATH!
                    }
                }

                // test vs map
                ResultRay res = CollisionManager.getInstance().testVsMap(test, maxTracingDistance);
                if (res.t.HasValue)
                {
                    if (!averageDistance.HasValue)  // is this is the first ray to have a value for t?
                        averageDistance = res.t.Value;
                    else
                        averageDistance = (averageDistance + res.t.Value) / 2;

                    // would we pass through the target on the next update?
                    if (res.t.Value <= deltaT * Vector3.Dot(velocity, -res.tri.Value.normal))
                    {
                        Vector3 vn = project(velocity, res.tri.Value.normal);    // normal component of velocity
                        Vector3 vt = velocity - vn;  // tangential component
                        velocity = 0.8f * vt - 0.2f * vn;
                        for (int i = 0; i < 5; ++i)
                        {
                            Random rnd = RacingGame.random;
                            Vector3 offs = new Vector3((float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble()) * 0.2f;
                            States.Game.ParticleSystem.AddParticle(new Particle(res.t.Value * res.ray.Value.Direction + res.ray.Value.Position + offs)
                            {
                                Color = new Vector4(0.7f, 0.7f, 0.1f, 0.5f),
                                initAlpha = 0.5f,
                                LifeTime = 0.5f,
                                Velocity = res.tri.Value.normal,
                                DeltaSize = 0.0f,
                                Size = 0.1f
                            });
                        }
                        if (!Invincible && vn.Length()>30.0f)
                        {
                            Health -= (vn.Length() / 350.0f); // 250.0f);
                            Sound.SoundManager.getInstance().playSound("Crash");
                        }
                    }
                }
                if (res.tri.HasValue)
                    averageNormal += res.tri.Value.normal;
            }
            if (averageNormal == Vector3.Zero || !averageDistance.HasValue)
                return null;
            averageNormal.Normalize();
            return averageDistance.Value * averageNormal;
        }

        #endregion

        #region Physics

        /// <summary>
        /// Takes a vector representing the force exerted on the ship by itself and applies
        /// physics.
        /// </summary>
        /// <param name="force">The force exerted on the ship</param>
        /// <returns>The resulting force</returns>
        private Vector3 handlePhysics(Vector3 force)
        {
            Vector3 shipUp = Vector3.Transform(Vector3.Up, node.rotation);
            Vector3 groundRight = Vector3.Cross(heading, groundNormal);
            Vector3 groundForwards = Vector3.Cross(groundNormal, groundRight);
            groundRight.Normalize(); groundForwards.Normalize();

            // Interpolate to make the ship smoothly follow the ground
            if (distanceToGround > 0)
            {
                float amount = MathHelper.SmoothStep(0f, 0.1f,
                    MathHelper.Clamp(DesiredHeight / distanceToGround, 0, 1));
                Vector3 newUp = Vector3.SmoothStep(shipUp, groundNormal, amount);
                // Rotate to the new rotation obtained from the ground's normal, if needed
                if (Vector3.Distance(shipUp, groundNormal) > 0.01f)
                {
                    node.rotation = Quaternion.Concatenate(node.rotation, createRotationBetween(shipUp, newUp));
                }
            }

            // adjust speed if necessary
            if (velocity.Length() > 0.5f * MaxSpeed * MaxSpeedModifier)
            {
                float speedAlongHeading = (project(velocity, heading) - MaxSpeed * MaxSpeedModifier * heading).Length();
                // are we speeding forwards or backwards?
                if (speedAlongHeading > 0 || speedAlongHeading < -1.5f * MaxSpeed * MaxSpeedModifier)
                    force = Vector3.Lerp(force, force - project(force, heading), 0.7f);   // slow down
                float speedSideways = project(velocity, groundRight).Length();
                // are we speeding sideways?
                if (speedSideways > 0.5f * MaxSpeed * MaxSpeedModifier)
                    force = Vector3.Lerp(force, force - project(force, groundRight), 0.7f);   // slow down
            }

            // adjust the distance to the ground
            Vector3 g = 0.07f * 9.81f * Vector3.Down;
            if (distanceToGround > 0)
            {
                float q = DesiredHeight / distanceToGround;
                float a = g.Length();
                if (q > 1)
                    q = (float)Math.Sqrt(Math.Sqrt(q));
                a *= q;
                force += a * groundNormal;
            }
            force += g;    // apply gravity

            // calculate drag
            Vector3 drag = -0.01f * velocity;
            drag *= 1 + Math.Abs(Vector3.Dot(drag, groundRight));   // going sideways results in higher drag
            if (force != Vector3.Zero)
                drag -= project(drag, force);   // do not apply drag in the direction we are thrusting

            return force + drag;
        }

        #endregion

        #region Input handling and maneuvering

        private Vector3 handleInput()
        {
            Vector3 force = Vector3.Zero;
            
            InputManager input = InputManager.getInstance();

            if (input.isThrottle())
                force += Acceleration * AccelerationModifier * heading;
            
            if (input.isBoost())
            {
                if (boostRemaining > 0.007f)
                {
                    force += 1.3f * Acceleration * AccelerationModifier * heading;
                    MaxSpeedModifier += 0.001f;
                    boostRemaining -= 0.007f;
                    node.useBoost = true;
                }
                else
                {
                    node.useBoost = false;
                }

            }
            else	// If not boosting, replenish boost
            {
                node.useBoost = false;
                if (MaxSpeedModifier > 1.0f)
                    MaxSpeedModifier -= 0.001f;
                if (boostRemaining > 1.0f)
                {
                    boostRemaining = 1.0f;
                }
                boostRemaining += 0.0006f;
            }
            if (input.isReverse())
            {
                if (Vector3.Dot(velocity, heading) > 0)
                    force += brake(Direction.Left) + brake(Direction.Right);
                force += -0.5f * Acceleration * AccelerationModifier * heading;
            }

            if (input.isTurnLeft())
                turn(Direction.Left);
            if (input.isTurnRight())
                turn(Direction.Right);

            if (input.isStrafeLeft())
                force += strafe(Direction.Left);
            if (input.isStrafeRight())
                force += strafe(Direction.Right);

            if (input.isLeftBrake())
                force += brake(Direction.Left);
            if (input.isRightBrake())
                force += brake(Direction.Right);

            if (input.isFireWeapon())
            {
                if (powerup != null)
                    usePowerup = true;
            }
            if (input.isDiscardWeapon() && powerup != null)
            {
                powerup.discard();
                powerup = null;
            }

            if (input.isJump())
            {
                if (boostRemaining > 0.009f && distanceToGround > 0)
                {
                    force += (DesiredHeight / distanceToGround) * groundNormal;
                    boostRemaining -= 0.009f;
                }
            }

            // Roll
            Vector3 right = Vector3.Transform(Vector3.Right, node.rotation);
            Vector3 groundRight = Vector3.Cross(heading, Vector3.Up);
            float r = Vector3.Dot(right, groundRight);
            if (rolling != Direction.None && r > 0.6f)
            {
                int sign = 0;
                if (rolling == Direction.Left)
                    sign = -1;
                else if (rolling == Direction.Right)
                    sign = 1;
                Quaternion rot = Quaternion.CreateFromAxisAngle(heading, sign * 10 * deltaT);
                node.rotation = Quaternion.Concatenate(node.rotation, rot);
            }
            rolling = Direction.None;

            return force;
        }

        #region Maneuvering

        private void turn(Direction dir)
        {
            int sign;
            switch (dir)
            {
                case Direction.Left:
                    sign = -1;
                    break;
                case Direction.Right:
                    sign = 1;
                    break;
                default:
                    sign = 0;
                    break;
            }
            Quaternion rotation = Quaternion.CreateFromAxisAngle(groundNormal, -sign * Handling * deltaT);
            rolling = dir;
            node.rotation = Quaternion.Concatenate(node.rotation, rotation);
        }

        private Vector3 strafe(Direction dir)
        {
            Vector3 groundRight = Vector3.Cross(heading, groundNormal);
            Vector3 direction;
            switch (dir)
            {
                case Direction.Left:
                    direction = -groundRight;
                    break;
                case Direction.Right:
                    direction = groundRight;
                    break;
                default:
                    direction = Vector3.Zero;
                    break;
            }
            rolling = dir;
            return 0.5f * direction;
        }

        private Vector3 brake(Direction dir)
        {
            int sign;
            Vector3 side;
            switch (dir)
            {
                case Direction.Left:
                    sign = 1;
                    side = Vector3.Left;
                    break;
                case Direction.Right:
                    sign = -1;
                    side = Vector3.Right;
                    break;
                default:
                    sign = 0;
                    side = Vector3.Zero;
                    break;
            }
            float velocityScalarProjection = Vector3.Dot(velocity, heading);
            int directionSign = Math.Sign(velocityScalarProjection);
            Vector3 brakingVector = deltaT * velocityScalarProjection *
            Vector3.Transform(0.5f * directionSign * side + Vector3.Backward, node.rotation);
            node.rotation = Quaternion.Concatenate(node.rotation,
            Quaternion.CreateFromAxisAngle(Vector3.Transform(Vector3.Up, node.rotation),
            sign * deltaT * 20.0f * directionSign * brakingVector.Length()));
            return brakingVector;
        }

        #endregion

        #endregion        

        #region IUpdatable Members

        public bool update(GameTime gt)
        {
            deltaT = gt.ElapsedGameTime.Milliseconds / 5000.0f;

#if DEBUG
            if (InputManager.getInstance().isDebug())
            {
                Health = 0.0f;
            }
#endif

            if (Health <= 0.0f)
            {
                if (active)
                {
                    States.Game.spawnExplosion(Position);
                    Network.P2PManager.Instance.SendDestroyed();
                    timeOfDeath = gt.TotalGameTime.TotalMilliseconds;
                    active = false;
                    node.Visible = false;
                }
                else
                {
                    deltaT = 0;
                    if (timeOfDeath + 3000.0 < gt.TotalGameTime.TotalMilliseconds)
                    {
                        respawn();
                    }
                }
                
            }

            if (SpeedPower)
            {
                deltaT *= 1.6f;
            }

            if (usePowerup)
            {
                usePowerup = false;
                powerup.use(gt);
            }
            if(powerup != null) 
            {
                if (powerup.update(gt))
                {
                    powerup.discard();
                    powerup = null;
                }
            }

            heading = Vector3.Transform(Vector3.Forward, node.rotation);
            Vector3 force = Vector3.Zero;

            if (active)
                force += handleInput();

            velocity += handlePhysics(force);

            handleCollisions();

            node.position += deltaT * velocity;
            node.setVelocity(velocity);
            node.useSpeed = SpeedPower;
            return false;
        }

        #endregion

        #region Private helper methods

        /// <summary>
        /// Projects a Vector3 onto another Vector3
        /// </summary>
        /// <param name="u">The vector to project</param>
        /// <param name="v">The vector to project upon</param>
        /// <returns></returns>
        private Vector3 project(Vector3 u, Vector3 v)
        {
            return (Vector3.Dot(u, v) / Vector3.Dot(v, v)) * v;
        }

        /// <summary>
        /// Creates a unit quaternion for rotation between the vectors u and v.
        /// </summary>
        /// <param name="u">The start vector</param>
        /// <param name="v">The destination vector</param>
        /// <returns>A quaternion representing the rotation from u to v</returns>
        private Quaternion createRotationBetween(Vector3 u, Vector3 v)
        {
            return Quaternion.Normalize(Quaternion.CreateFromAxisAngle(Vector3.Cross(u, v),
                    (float)(Math.Sqrt(u.LengthSquared() + v.LengthSquared()) *
                    Vector3.Dot(u, v))));
        }

        /// <summary>
        /// Makes the ship respawn at the last checkpoint
        /// </summary>
        private void respawn()
        {
            node.position = game.Player.lastCp.PlayerPassPos+(Vector3.Up*1.5f);
            node.collisionObject.position = game.Player.lastCp.PlayerPassPos;
            node.rotation = game.Player.lastCp.PlayerPassRot;
            resetShip();
            Active = true;
            node.Visible = true;
            States.Game.spawnRespawnAura(Position);
            Network.P2PManager.Instance.SendRespawn(Position);
        }

        /// <summary>
        /// Resets the ships environmental variables
        /// </summary>
        private void resetShip()
        {
            Health = 1;
            MaxSpeedModifier = 1.0f;
            HandlingModifier = 1.0f;
            AccelerationModifier = 1.0f;
            if (powerup != null)
            {
                powerup.discard();
                powerup = null;
            }
            usePowerup = false;
            velocity = Vector3.Zero;
        }
        #endregion
    }
}