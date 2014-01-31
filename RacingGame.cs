using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;

namespace RacingGame
{
    /// <summary>
    /// The main class of the game, controls mainloop and
    /// sends update/render calls down the hierarchy
    /// Author: David Sundelius
    /// </summary>
    public class RacingGame : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager graphics;
        private Graphics.GraphicsManager graphicsManager;
        private Stack<States.IGameState> gameStates = new Stack<States.IGameState>();
        public static Stack<States.IGameState> States;

        public static ContentManager contentManager;
        public static Random random;

        

        private bool useMenus = true;

        public RacingGame(bool menus)
        {
            Sys.Logger.getInstance().clear();
            Sys.Logger.getInstance().print("Starting up SLERP 3D");

            States = gameStates;

            random = new Random();

            Window.Title = "SLERP 3D";
            graphics = new GraphicsDeviceManager(this);
            Components.Add(new GamerServicesComponent(this));
            Content.RootDirectory = "Content";
            contentManager = Content;
            useMenus = menus;
            graphics.PreparingDeviceSettings += this.graphicsAdapterSettings;
            graphics.ApplyChanges();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            Sys.Logger logger = Sys.Logger.getInstance();
            logger.print("Initializing game");
            graphicsManager = new Graphics.GraphicsManager(graphics);
            graphicsManager.initialize();
            this.IsFixedTimeStep = false;
            graphics.SynchronizeWithVerticalRetrace = Properties.Settings.Default.VSync;
            graphics.ApplyChanges();
            base.Initialize();

            Map.Level.startLoad("NewLevel.xml");

            if (useMenus)
            {
                gameStates.Push(new States.MainMenu(gameStates));
                gameStates.Push(new States.Titlescreen());
            }
            else
            {
                gameStates.Push(new States.Game(false));
            }
            logger.print("Initialization successfully completed");
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            // spriteBatch = new SpriteBatch(GraphicsDevice);
            // Load game content
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // Unload content
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Implement update logic
            base.Update(gameTime);

            //Send updates down to the current state
            if (gameStates.Peek().update(gameTime))
            {
                gameStates.Pop();
                if (gameStates.Count == 0)
                {
                    Exit();
                }
            }
        }

        /// <summary>
        /// Method used to render stuff onto the screen
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            if (gameStates.Count > 0)
                gameStates.Peek().render();
        }

        void graphicsAdapterSettings(object sender, PreparingDeviceSettingsEventArgs args)
        {
            Sys.Logger.getInstance().print("Modifying graphics adapter settings");
            args.GraphicsDeviceInformation.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;
            args.GraphicsDeviceInformation.PresentationParameters.BackBufferFormat = SurfaceFormat.Color;
            args.GraphicsDeviceInformation.PresentationParameters.MultiSampleQuality = 0;
            args.GraphicsDeviceInformation.PresentationParameters.MultiSampleType = MultiSampleType.None;
                
            foreach (GraphicsAdapter adapter in GraphicsAdapter.Adapters)
            {
                if (adapter.Description.Contains("PerfHUD"))
                {
                    args.GraphicsDeviceInformation.Adapter = adapter;
                    args.GraphicsDeviceInformation.DeviceType = DeviceType.Reference;
                    break;
                }
            }
            Sys.Logger.getInstance().print("Graphics adapter settings successfully modified");
        }
    }
}
