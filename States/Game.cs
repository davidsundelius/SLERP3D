using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RacingGame.Graphics;
using RacingGame;
using RacingGame.Network;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using RacingGame.Logic;
using Microsoft.Xna.Framework.Input;

namespace RacingGame.States
{
    struct FinishedPlayer
    {
        public string Name;
        public float Time;
        public float BestLap;
    }

    /// <summary>
    /// A game state used to keep track of ingame action
    /// Author: David Sundelius
    /// </summary>
    class Game : IGameState
    {
        #region Variables for fps
        private int frameCounter = 0,
                    frameTime = 0,
                    currentFrameRate = 0;
        #endregion

        public static Game Instance { get; private set; }

        private Collision.CollisionManager cm = Collision.CollisionManager.getInstance();
        #region Variables for rendering

        private Graphics.GraphicsManager graphicsManager;
        public static Scene scene = new Scene(); // temporary
        private HUD hud;
        private Camera camera;
        private ShipNode playerModel;
        private LensFlare lensFlare;

        public List<FinishedPlayer> finishedPlayers = new List<FinishedPlayer>();
        private ParticleSystemBase particleSys;
        private Random rnd = new Random();

        public static ParticleSystemBase ParticleSystem { get; private set; }

        #endregion

        #region Variables for logic
        private gameState state;
        private Logic.Ship ship;
        private List<Logic.IUpdateable> updatables;
        private Map.Level level;
        #endregion

        #region Variables for networking
        private P2PManager p2pManager = P2PManager.Instance;
        private bool isNetworked;
        #endregion

        #region Variables for gameplay
        private const int nrOfPowerups = 5;
        public Player Player
        {
            get;
            private set;
        }
        public int nrOfLaps {
            get;
            private set;
        }
        private int startTime;
        private int countDown;
        public int time
        {
            get;
            private set;
        }
        #endregion

        #region Helper enums
        /// <summary>
        /// Enumeration to control the INGAME state (do not confuse with the program states)
        /// WAITING is used when waiting for other players to join game
        /// INITIALIZE is used when the game has not yet started, before the light is green
        /// RUNNING is used when the game is running as normal
        /// PAUSED is when the player pauses the game
        /// FINISH is when the game is over, after the player finished the last lap
        /// TERMINATE is used to exit the game
        /// </summary>
        private enum gameState
        {
            WAITING,
            INITIALIZE,
            RUNNING,
            PAUSED,
            FINISH,
            TERMINATE
        }
        #endregion

        #region Helper methods
        public Powerups.IPowerup getNewPowerup()
        {
            Random rand = RacingGame.random;
            switch (rand.Next(nrOfPowerups))
            {
                case 0: //Shield
                    return new Powerups.Shield(ship);
                case 1: //Speedup
                    return new Powerups.Speedup(ship);
                case 2: //Missile
                    return new Powerups.Missile(ship);
                case 3: //Demon mode
                    return new Powerups.DemonMode(ship);
                case 4: //Power pack
                    return new Powerups.Powerpack(ship);
                default:
                    return null;
            }
        }

        private static float getRnd()
        {
            return ((float)RacingGame.random.NextDouble() - 0.5f);
        }

        public static void spawnExplosion(Vector3 pos)
        {
            int numParticlesExplosion = 300;
            for (int i = 0; i < numParticlesExplosion; ++i)
            {
                Vector3 velocity = new Vector3(getRnd() * 40.0f, getRnd() * 40.0f, getRnd() * 40.0f);
                Particle p = new Particle(pos)
                {
                    DeltaSize = 6.9f,
                    initAlpha = 0.9f,
                    LifeTime = 1.0f,
                    Size = 0.5f,
                    Color = new Vector4(0.9f, 0.15f, 0.07f, 0.9f),
                    Velocity = velocity
                };

                Game.ParticleSystem.AddParticle(p);
            }
            Collision.CollisionManager.getInstance().addExplosion(pos);
            Sound.SoundManager.getInstance().playSound("Explosion");
        }

        public static void spawnRespawnAura(Vector3 position)
        {
            for (int i = 0; i < 150; ++i)
            {
                float rnd = (float)(getRnd() * 4 * Math.PI);
                Vector3 velocity = new Vector3((float)(Math.Cos(rnd)) * 10.0f, 0.0f, (float)(Math.Sin(rnd)) * 5.0f);
                Particle p = new Particle(position)
                {
                    DeltaSize = 2.0f,
                    initAlpha = 0.9f,
                    LifeTime = 1.0f,
                    Size = 0.5f,
                    Color = new Vector4(0.7f, 0.9f, 1.0f, 0.9f),
                    Velocity = velocity
                };
                States.Game.ParticleSystem.AddParticle(p);
            }
            Sound.SoundManager.getInstance().playSound("Shield");
        }

        public static void spawnGlow(Vector3 position)
        {
            for (int i = 0; i < 50; i++)
            {
                float rand = getRnd() * 2.0f;
                States.Game.ParticleSystem.AddParticle(new Particle(position)
                {
                    Color = new Vector4(1.0f, 0.3f, 0.3f, 0.5f),
                    initAlpha = 0.5f,
                    LifeTime = 0.5f,
                    Velocity = new Vector3((float)Math.Sin(rand) * 5.0f, (getRnd() + 0.5f) * 5.0f, (float)Math.Cos(rand) * 5.0f),
                    DeltaSize = 0.0f,
                    Size = 0.5f
                });
            }
        }
        #endregion

        #region Constructors
        public Game(bool isNetworked)
        {
            Instance = this;

            this.graphicsManager = GraphicsManager.getInstance();
            this.isNetworked = isNetworked;
            initialize();
        }
        #endregion

        #region Methods for initialization
        /// <summary>
        /// Used to initialize a new game
        /// </summary>
        private void initialize()
        {
            Sound.SoundManager.getInstance().stopAll();

            //Initialize updatables
            updatables = new List<Logic.IUpdateable>();

            //Load level
            level = new Map.Level();
            scene.addNode(level);

            //Load and initialize player ships
            if (isNetworked)
            {
                playerModel = p2pManager.LocalGamer.Tag as ShipNode;
                p2pManager.SendPlayerData(Properties.Settings.Default.chosenVehicle);
            }
            else
                switch (Properties.Settings.Default.chosenVehicle)
                {
                    case 0:
                        playerModel = new ShipNode(RacingGame.contentManager.Load<Model>("Models/Ships/box_01"));
                        break;
                    case 1:
                        playerModel = new ShipNode(RacingGame.contentManager.Load<Model>("Models/Ships/ship_02"));
                        break;
                }
            Matrix mat = new Matrix();
            mat.Forward = Map.Level.startingPoints[p2pManager.CurrentNrOfPlayers-1].dir;
            mat.Up = Vector3.Up;
            mat.Right = Vector3.Cross(Map.Level.startingPoints[p2pManager.CurrentNrOfPlayers - 1].dir, Vector3.Up);

            playerModel.position = Map.Level.startingPoints[p2pManager.CurrentNrOfPlayers - 1].pos;
            playerModel.rotation = Quaternion.CreateFromRotationMatrix(mat);// Quaternion.CreateFromYawPitchRoll(-MathHelper.PiOver2, 0.0f, 0.0f);
            ship = new Logic.Ship(playerModel, this);
            scene.addNode(playerModel);

            //Initialize stuff read from levelfile
            initializeDecorations();
            initializeLights();
            initializePowerups();
            initializeCheckpoints();

            //Initialize camera
            camera = new Camera(playerModel, new Vector3(0.0f, 0.8f, 4.0f));

            //Initialize lensflare
            lensFlare = new LensFlare(LensFlare.DefaultSunPos, camera.getPosition(), 20);

            //Load skybox
            scene.addNode(new Map.SceneryNode(new BoundingSphere(Vector3.Zero, 300.0f), 10));
            scene.addNode(new SkyBoxNode("Textures/Sky/skyboxSpace", camera));

            nrOfLaps = 5;

            scene.update(new GameTime());

            //Initialize game logic
            //updatables.Add((Logic.IUpdateable)scene);
            updatables.Add((Logic.IUpdateable)camera);
            updatables.Add((Logic.IUpdateable)ship);
            if (isNetworked)
            {
                updatables.Add((Logic.IUpdateable)p2pManager);
                updatables.Add((Logic.IUpdateable)Collision.CollisionManager.getInstance());
                foreach (var gamer in p2pManager.networkSession.RemoteGamers)
                {
                    scene.addNode(gamer.Tag as Node);
                }
            }

            //Load gameplay objects
            Player = new Logic.Player(this, ship);
            
            //Initialize HUD
            hud = new HUD(this);

            //Set game to start waiting for players
            if (this.isNetworked)
            {
                state = gameState.WAITING;
                hud.showMessage("Waiting for players, press Boost to start game...");
            }
            else
            {
                state = gameState.RUNNING;
                ship.Active = true;
            }

            while (Map.Level.loaderThread.IsAlive)
            {
                System.Threading.Thread.Sleep(200);
            }
        }

        private void initializeDecorations()
        {
            //Load ship in background
            ModelNode shipnode = new ModelNode(RacingGame.contentManager.Load<Model>("Models/Scenery/spaceship_1"));
            shipnode.position = new Vector3(60.0f, 0.0f, 0.0f);
            scene.addNode(shipnode);

            //Load particle system
            particleSys = new ParticleSystemCPU(10000, RacingGame.contentManager, GraphicsManager.getDevice());
            ParticleSystem = particleSys;
        }

        private void initializePowerups()
        {
            int i = 0;
            foreach (Vector3 pos in Map.Level.powerups)
            {
                //Load powerup items and put them onto the level
                ModelNode powerupModel = new ModelNode(RacingGame.contentManager.Load<Model>("Models/Powerups/multi_star"));
                powerupModel.position = pos;// new Vector3(26.19372f, -6.109693f, -66.61001f);
                powerupModel.rotation = Quaternion.CreateFromYawPitchRoll(-MathHelper.PiOver2, 0.0f, 0.0f);
                Logic.PowerupItem powerupItem = new Logic.PowerupItem(5000, powerupModel);
                updatables.Add(powerupItem);
                scene.addNode(powerupModel);
            }
        }

        private void initializeLights()
        {
         
            Vector3[] lightPosList = {   
                                    new Vector3(-0.7f, 5.5f, -62.0f),
                                    new Vector3(-28.5f, 8.0f, -41.5f),
                                    new Vector3(-32.0f, 7.5f, 37.0f),
                                    new Vector3(10.74921f, 12.381724f, 60.87811f),
                                    new Vector3(28.24761f, 5.8313528f, 20.55669f),
                                    new Vector3(27.67364f, 5.3333852f, -29.83252f),
                                    new Vector3(26.19372f, -0.609693f, -66.61001f),
                                    new Vector3(-22.3f, 8.1f, -68.0f),
                                    new Vector3(-11.5f, 5.5f, -3.6f),
                                    new Vector3(-31.7f, 10.8f, -60.5f)
                                };

            foreach (Light l in Map.Level.lights)
            {

                LightNode lightNode = new LightNode(l);
                lightNode.position = l.position;
                scene.addNode(lightNode);
            }

        }

        private void initializeCheckpoints()
        {
            //Load checkpoints and place into the world
            int i=0;
            foreach (BoundingBox bb in Map.Level.checkPoints)
            {
                Checkpoint cp = new Logic.Checkpoint(i,bb);
                i++;
                updatables.Add(cp);
                //scene.addNode(bb);
            }
            Checkpoint.nrOfCheckpoints = i;
        }

        #endregion

        #region Methods for rendering
        /// <summary>
        /// Renders the gamestate to the screen
        /// </summary>
        public void render()
        {
            graphicsManager.render(scene, camera);
            if (isNetworked)
            {
                p2pManager.render();
            }
            lensFlare.Render(Color.White);
            particleSys.Draw(graphicsManager.view, graphicsManager.proj);
#if DEBUG
            //fps counter
            hud.renderDebug("Gamestate: " + state + ", FPS:  " + currentFrameRate);
#endif
           if (state == gameState.RUNNING)
                hud.render();
           else
                hud.renderMessage();
        }

        #endregion

        #region Methods for updating
        /// <summary>
        /// Update the game according to exterior and interior effects
        /// </summary>
        public bool update(GameTime time)
        {
            KeyboardState keys = Keyboard.GetState();
            switch(state) {
                case gameState.WAITING:
                    if (p2pManager.networkSession.IsEveryoneReady)
                    {
                        state = gameState.INITIALIZE;
                        hud.clearMessage();
                        startTime = (int)time.TotalGameTime.TotalMilliseconds;
                        countDown = 3;
                        hud.showMessage("Ready for takeoff...");
                    }
                    if (Sys.InputManager.getInstance().isBoost())
                    {
                        hud.showMessage("Waiting for other players to get ready...");
                        p2pManager.LocalGamer.IsReady = true;
                    }
                    updateObjects(time);
                    break;
                case gameState.INITIALIZE:
                    this.time = (int)time.TotalGameTime.TotalMilliseconds - startTime;
                    if (this.time > 1500)
                    {
                        if (countDown > 0)
                        {
                            Sound.SoundManager.getInstance().playSound("" + countDown);
                            hud.showMessage("" + countDown);
                            countDown--;
                        }
                        else
                        {
                            if (p2pManager.LocalGamer.IsHost)
                                p2pManager.StartGame();
                            foreach (var gamer in p2pManager.networkSession.RemoteGamers)
                            {
                                ShipNode sn = gamer.Tag as ShipNode;
                                Collision.CollisionManager.getInstance().addShip(sn);
                            }
                            state = gameState.RUNNING;
                            Sound.SoundManager.getInstance().playSound("Beep");
                            Sound.SoundManager.getInstance().playSound("MenuMusic");
                            ship.Active = true;
                            countDown = 1;
                            hud.showMessage("GO!");
                        }
                        startTime = (int)time.TotalGameTime.TotalMilliseconds;
                    }
                    updateObjects(time);
                    break;
                case gameState.PAUSED:
                    if (Sys.InputManager.getInstance().isPause())
                    {
                        state = gameState.RUNNING;
                    }
                    break;
                case gameState.RUNNING:
                    this.time = (int)time.TotalGameTime.TotalMilliseconds - startTime;
                    if (countDown == 1 && this.time > 2500)
                    {
                        countDown = 0;
                        hud.clearMessage();
                    }
                    if (Sys.InputManager.getInstance().isEscapeDown())
                    {
                        state = gameState.TERMINATE;
                    }
                    if (Sys.InputManager.getInstance().isPause() && !isNetworked)
                    {
                        state = gameState.PAUSED;
                    }
                    if (Player.gameFinished)
                    {
                        startTime = (int)time.TotalGameTime.TotalMilliseconds;
                        countDown = 1;
                        hud.showMessage("FINISH!");

                        float bestLap = float.MaxValue;
                        float totalTime = 0.0f;

                        foreach (int lap in Player.lapTimes)
                        {
                            totalTime += lap / 1000.0f;
                            if (lap == 0)
                                continue;

                            if (lap / 1000.0f < bestLap)
                            {
                                bestLap = lap / 1000.0f;
                            }
                        }

                        finishedPlayers.Add(new FinishedPlayer() { Name = P2PManager.Instance.LocalGamer.Gamertag, Time = totalTime, BestLap = bestLap });
                        p2pManager.SendFinished(totalTime, bestLap);
                        state = gameState.FINISH;
                    }
                    updateObjects(time);
                    break;
                case gameState.FINISH:

                    if (Player.ship.powerup != null)
                    {
                        Player.ship.powerup.discard();
                    }
                    this.time = (int)time.TotalGameTime.TotalMilliseconds - startTime;
                    if (countDown == 1 && this.time > 1000)
                    {
                        hud.showMessage("Waiting for other players to finish...");
                    }

                    if (finishedPlayers.Count >= p2pManager.CurrentNrOfPlayers)
                    {
                        
                        finishedPlayers.Sort(delegate(FinishedPlayer p1, FinishedPlayer p2) { return p1.Time.CompareTo(p2.Time); });
                        RacingGame.States.Pop();
                        RacingGame.States.Push(new EndGame(finishedPlayers.ToArray()));

                        //shuts down network session
                        Sound.SoundManager.getInstance().stopAll();
                        if (isNetworked)
                        {
                            try
                            {
                                p2pManager.networkSession.EndGame();
                                //mayhaps this will intefere with host migration?
                                if (p2pManager.networkSession.IsHost)
                                    //this is done so that the tag is set properly.
                                    p2pManager.networkSession.Dispose();
                            }
                            catch (Exception)
                            {
                            }

                            //so that no dublicate ships or anything else appears when it should not.
                            //this fixes the slowdowns experienced when restarting
                            //the game miltiple times.
                            scene.clear();
                        }
                        cm.powerUpClear();
                        updatables.Clear();
                        Sys.InputManager.getInstance().keysPressedReset(keys.GetPressedKeys());
                        return false;
                    }
                    updateObjects(time);
                    break;
                case gameState.TERMINATE:
                    //shuts down network session
                    Sound.SoundManager.getInstance().stopAll();
                    if (Player.ship.powerup != null)
                    {
                        Player.ship.powerup.discard();
                    }
                    if (isNetworked)
                    {
                        p2pManager.networkSession.EndGame();
                        //mayhaps this will intefere with host migration?
                        if (p2pManager.networkSession.IsHost)
                            //this is done so that the tag is set properly.
                            p2pManager.networkSession.Dispose();
                            //so that no dublicate ships or anything else appears when it should not.
                            //this fixes the slowdowns experienced when restarting
                            //the game miltiple times.
                            scene.clear();
                    }
                    cm.powerUpClear();
                    Sys.InputManager.getInstance().keysPressedReset(keys.GetPressedKeys());
                    return true;
            }

            //updates the fps counter
            frameCounter++;
            frameTime += time.ElapsedGameTime.Milliseconds;
            if (frameTime >= 500)
            {
                currentFrameRate = (int)(frameCounter / (frameTime / 1000.0f));
                frameTime = 0;
                frameCounter = 0;
            }

            return false;
        }

        /// <summary>
        /// Update the game objects
        /// </summary>
        private void updateObjects(GameTime time)
        {
            
            float step = (float)time.ElapsedGameTime.TotalSeconds;

            const float MINSTEP = 0.018f;

            GameTime t;

            do
            {
                if (step > MINSTEP)
                {
                    t = new GameTime(new TimeSpan(), new TimeSpan(), time.TotalGameTime, new TimeSpan((long)(MINSTEP * 10000000)));
                }
                else
                {
                    t = new GameTime(new TimeSpan(), new TimeSpan(), time.TotalGameTime, new TimeSpan((long)(step * 10000000)));
                }

                lensFlare.setCameraPosForLensFlare(camera.getPosition());
                for (int i = 0; i < updatables.Count; i++)
                {
                    if (updatables[i].update(t))
                        updatables.RemoveAt(i);
                }

                step -= MINSTEP;
            } while (step > 0.0f);

            scene.update(time);

            particleSys.Update((float)time.ElapsedGameTime.TotalSeconds);
        }
        #endregion
    }
}
