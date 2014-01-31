using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RacingGame.Sys;

namespace RacingGame.States
{
    /// <summary>
    /// An abstract class for collecting and handling resources for the menu states
    /// Author: Magnus Olausson
    /// </summary>
    abstract class Menu : IGameState
    {
        protected Stack<States.IGameState> gameStates;
        protected static Graphics.GraphicsManager graphicsManager = Graphics.GraphicsManager.getInstance();
        protected static GraphicsDeviceManager graphics = Graphics.GraphicsManager.getGraphics();
        protected InputManager inputManager = Sys.InputManager.getInstance();
        protected SpriteBatch spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
        protected int mouseIsOverButton = -1;
        protected bool ignoreMouse = true;
        protected int buttonHighlighted = 0;
        public static bool updatedUI = false;
        public static Screen lastScreen;

        protected static bool transitionDone = false;
        protected static int inAlphaValue = 1;
        protected static int outAlphaValue = 200;
        protected static int titleOutAlphaValue = 255;
        protected static int fadeIncrement = 8;
        protected static int fadeDecrement = -8;
        protected static bool buttonFadedIn = false;
        protected static bool buttonFadedOut = false;
        protected static int fadeInButtonXPos = (int)(-buttonWidth);
        protected static int fadeOutButtonXPos = (int)(xRes / 16);
        protected static int settingsFinalYPos = (int)(yRes / 4.5 + 2 * buttonHeight);
        protected static int settingsYPos = 2 * buttonHeight;
        protected static int fadeInButtonFinalXPos = (int)(xRes / 16);
        protected static float moveRatioSettings = 0, moveRatioInFade = 0, moveRatioOutFade = 0, moveStep = 0.04f;

        protected static int glowAlpha = 255;
        protected static bool alphaGlowUp = true;

        public enum Screen { TITLESCREEN, MAINMENU, SETTINGS, NETWORK, GAMESCREEN, RESOLUTION, VEHICLE };

        protected static int xRes = (int)graphicsManager.getResolution().X;
        protected static int yRes = (int)graphicsManager.getResolution().Y;
        protected static int buttonHeight = (int)(yRes / 11); //11 /8.53
        protected static int buttonWidth = (int)(xRes / 2.38); //2.38 /1.85

        protected SpriteFont hudFont = RacingGame.contentManager.Load<SpriteFont>("Fonts//HUD");
        //Menu textures
        protected static Texture2D logo = RacingGame.contentManager.Load<Texture2D>("Textures//Menu//logo2");
        protected static Texture2D background = RacingGame.contentManager.Load<Texture2D>("Textures//Menu//backgroundTest");
        protected static Texture2D optionContainer = RacingGame.contentManager.Load<Texture2D>("Textures//Menu//optionContainer");
        protected static Texture2D cursor = RacingGame.contentManager.Load<Texture2D>("Textures//Menu//cursor");
        protected static Texture2D check = RacingGame.contentManager.Load<Texture2D>("Textures//Menu//check");
        protected static Texture2D on = RacingGame.contentManager.Load<Texture2D>("Textures//Menu//on");
        protected static Texture2D off = RacingGame.contentManager.Load<Texture2D>("Textures//Menu//off");
        protected static Texture2D borderUp = RacingGame.contentManager.Load<Texture2D>("Textures//Menu//borderUp");
        protected static Texture2D borderDown = RacingGame.contentManager.Load<Texture2D>("Textures//Menu//borderDown");
        //MainMenu textures
        protected static Texture2D mainMenu = RacingGame.contentManager.Load<Texture2D>("Textures//Menu//mainMenu");
        protected static Texture2D raceButton = RacingGame.contentManager.Load<Texture2D>("Textures//Menu//startRacing");
        protected static Texture2D settingsButton = RacingGame.contentManager.Load<Texture2D>("Textures//Menu//settings");
        protected static Texture2D exitButton = RacingGame.contentManager.Load<Texture2D>("Textures//Menu//quitGame");
        //Vehicle selection textures
        protected static Texture2D teds = RacingGame.contentManager.Load<Texture2D>("Textures//Menu//TEDS");
        protected static Texture2D dsmr900rgxd = RacingGame.contentManager.Load<Texture2D>("Textures//Menu//DMSR900RGXD");
        //Titlescreen textures
        protected static Texture2D pressToStart = RacingGame.contentManager.Load<Texture2D>("Textures//Menu//pressToStart");
        //Settings screen textures
        protected static Texture2D vSyncButton = RacingGame.contentManager.Load<Texture2D>("Textures//Menu//vSync");
        protected static Texture2D fullscreenButton = RacingGame.contentManager.Load<Texture2D>("Textures//Menu//fullscreen");
        protected static Texture2D resolution = RacingGame.contentManager.Load<Texture2D>("Textures//Menu//resolution");
        protected static Texture2D ssao = RacingGame.contentManager.Load<Texture2D>("Textures//Menu//ssao");
        protected static Texture2D easyControls = RacingGame.contentManager.Load<Texture2D>("Textures//Menu//easyControls");
        protected static Texture2D nrOfShadows = RacingGame.contentManager.Load<Texture2D>("Textures//Menu//nrOfShadows");
        protected static Texture2D arrowUp = RacingGame.contentManager.Load<Texture2D>("Textures//Menu//arrowUp");
        protected static Texture2D arrowDown = RacingGame.contentManager.Load<Texture2D>("Textures//Menu//arrowDown");
        protected static Texture2D backButton = RacingGame.contentManager.Load<Texture2D>("Textures//Menu//back");
        protected static Texture2D w1080Button = RacingGame.contentManager.Load<Texture2D>("Textures//Menu//1080");
        protected static Texture2D w900Button = RacingGame.contentManager.Load<Texture2D>("Textures//Menu//900");
        protected static Texture2D w720Button = RacingGame.contentManager.Load<Texture2D>("Textures//Menu//720");
        protected static Texture2D s1024Button = RacingGame.contentManager.Load<Texture2D>("Textures//Menu//1024");
        protected static Texture2D s768Button = RacingGame.contentManager.Load<Texture2D>("Textures//Menu//768");
        protected static Texture2D s600Button = RacingGame.contentManager.Load<Texture2D>("Textures//Menu//600");
        protected static Texture2D highlight1 = RacingGame.contentManager.Load<Texture2D>("Textures//Menu//highlight1");
        protected static Texture2D highlight2 = RacingGame.contentManager.Load<Texture2D>("Textures//Menu//highlight2");
        protected static Texture2D arrowUpHighlight = RacingGame.contentManager.Load<Texture2D>("Textures//Menu//arrowUpHighlight");
        protected static Texture2D arrowDownHighlight = RacingGame.contentManager.Load<Texture2D>("Textures//Menu//arrowDownHighlight");

        //Network screen textures
        protected static Texture2D networkGameButton = RacingGame.contentManager.Load<Texture2D>("Textures//Menu//startSession");
        protected static Texture2D localGameButton = RacingGame.contentManager.Load<Texture2D>("Textures//Menu//joinSession");

        //EndGame screen textures
        protected static Texture2D raceFinished = getTexture("racefinished");
        protected static Texture2D waitingForOtherPlayers = getTexture("waitingforotherplayerstofinish");
        protected static Texture2D place = getTexture("place");
        protected static Texture2D pilot = getTexture("pilot");
        protected static Texture2D time = getTexture("time");
        protected static Texture2D bestlap = getTexture("bestlap");
        protected static Texture2D continueButton = getTexture("continue");

        protected static Texture2D vehicleSelection = getTexture("selectVehicle");
        protected static Texture2D gameModeSelection = getTexture("selectGameMode");

        private static int resButtonXPos = (int)(xRes / 2);
        private static int resButtonYPos = (int)(yRes / 4.5);
        protected static int endGameW = (int) (xRes / 2);
        private static int endGameH = (int)(yRes / 15);
        protected static Rectangle
                         LogoRect = new Rectangle(0, 0, 1000, 700),
                         BackgroundRect = new Rectangle(0, 0, 1600, 900),
                         CursorRect = new Rectangle(0, 0, 30, 30),
                         CheckRect = new Rectangle(0, 0, 200, 200),
                         MenuButtonRect = new Rectangle(0, 0, 800, 130),
                         VehicleSelectionRect = new Rectangle(0, 0, 800, 200),
                         BorderRect = new Rectangle(0, 0, 1000, 250),
                         BorderUpPosistionRect = new Rectangle(0, 0, xRes, (int)(yRes / 3.1)),
                         BorderDownPosistionRect = new Rectangle(0, (int)(yRes - yRes / 3.1), xRes, (int)(yRes / 3.1)),
                         LabelPositionRect = new Rectangle((int)xRes / 16, 2 * buttonHeight, buttonWidth, buttonHeight),
                         endGameLabel = new Rectangle(0, 0, 720, 200),

                         logoPosRect =  new Rectangle((int) (xRes - (xRes / 3.6) - (xRes / 51.2)), (int) (yRes / 38.4), (int) (xRes / 3.6), (int) (yRes / 3.84)),
                         //Position rectangles for MainMenu buttons
                         raceButtonPositionRect = new Rectangle((int)(xRes / 16), (int)(yRes / 4.5 + buttonHeight), buttonWidth, buttonHeight),
                         settingsButtonPositionRect = new Rectangle((int)(xRes / 16), (int)(yRes / 4.5 + 2 * buttonHeight), buttonWidth, buttonHeight),
                         exitButtonPositionRect = new Rectangle((int)(xRes / 16), (int)(yRes / 4.5 + 6 * buttonHeight), buttonWidth, buttonHeight),
            //Position rectangles for vehicle selection
                         vehicleSelectionPosRect1 = new Rectangle((int)(xRes / 16), (int)(yRes / 4.5 + 2 * buttonHeight), (int) (buttonWidth * 0.8), (int) (buttonHeight * 1.54 * 0.8)),
                         vehicleSelectionPosRect2 = new Rectangle((int)(xRes / 16), (int)(yRes / 4.5 + 3.3 * buttonHeight), (int) (buttonWidth * 0.8), (int) (buttonHeight * 1.54 * 0.8)),
            //Position rectangles for Titescreen
                         pressToStartButtonPositionRect = new Rectangle((int)(xRes / 2 - (buttonWidth / 2)), (int)(yRes / 2 + 3 * buttonHeight), buttonWidth, buttonHeight),
            //Position rectangles for Settings screen
                         vSyncButtonPositionRect = new Rectangle((int)(xRes / 16), (int)(yRes / 4.5 + 1 * buttonHeight), buttonWidth, buttonHeight),
                         fullscreenButtonPositionRect = new Rectangle((int)(xRes / 16), (int)(yRes / 4.5 + 2 * buttonHeight), buttonWidth, buttonHeight),
                         resolutionButtonPositionRect = new Rectangle((int)(xRes / 16), (int)(yRes / 4.5 + 3 * buttonHeight), buttonWidth, buttonHeight),
                         ssaoButtonPositionRect = new Rectangle((int)(xRes / 16), (int)(yRes / 4.5 + 4 * buttonHeight), buttonWidth, buttonHeight),
                         easyControlsPositionRect = new Rectangle((int)(xRes / 2), (int)(yRes / 4.5 + buttonHeight), buttonWidth, buttonHeight),
                         nrOfShadowsPositionRect = new Rectangle((int)(xRes / 2), (int)(yRes / 4.5 + 2 * buttonHeight), buttonWidth, buttonHeight),
                         arrowUpPositionRect = new Rectangle((int)(xRes / 2 + (buttonWidth / 3)), (int)(yRes / 4.5 + 2.5 * buttonHeight), buttonHeight, buttonHeight),
                         arrowDownPositionRect = new Rectangle((int)(xRes / 2 + (buttonWidth / 3)), (int)(yRes / 4.5 + 3.5 * buttonHeight), buttonHeight, buttonHeight),
                         w1080ButtonPositionRect = new Rectangle(resButtonXPos, resButtonYPos + buttonHeight, buttonWidth, buttonHeight),
                         w900ButtonPositionRect = new Rectangle(resButtonXPos, resButtonYPos + 2 * buttonHeight, buttonWidth, buttonHeight),
                         w720ButtonPositionRect = new Rectangle(resButtonXPos, resButtonYPos + 3 * buttonHeight, buttonWidth, buttonHeight),
                         s1024ButtonPositionRect = new Rectangle((int)(xRes / 16), resButtonYPos + buttonHeight, buttonWidth, buttonHeight),
                         s768ButtonPositionRect = new Rectangle((int)(xRes / 16), resButtonYPos + 2 * buttonHeight, buttonWidth, buttonHeight),
                         s600ButtonPositionRect = new Rectangle((int)(xRes / 16), resButtonYPos + 3 * buttonHeight, buttonWidth, buttonHeight),
                         backButtonPositionRect = new Rectangle((int)(xRes / 16), (int)(yRes / 4.5 + 6 * buttonHeight), buttonWidth, buttonHeight),
            //Position rectangles for network screen 
                         startNetworkGameButtonPositionRect = raceButtonPositionRect,
                         startLocalGameButtonPositionRect = settingsButtonPositionRect,

                         //Position rectangles for endgame screen
                         waitingForOtherPlayersToFinishPos = new Rectangle((int)(xRes / 6), (int)(yRes / 32.0f), (int)(xRes - xRes / 3.0f), (int)(yRes / 10.0f)),
                         raceFinishedPos = new Rectangle((int)(xRes / 2 - (endGameW / 3)), (int)(yRes / 25.0f), (int) (endGameW * 1.5), (int) (endGameH * 1.5)),
                         placePos = new Rectangle(0, raceFinishedPos.Bottom + (int)(yRes / 16.0f), endGameW, endGameH),
                         pilotPos = new Rectangle((int) (1.2 * (xRes / 4)), raceFinishedPos.Bottom + (int)(yRes / 16.0f), endGameW, endGameH),
                         timePos = new Rectangle((int) (2.3 * (xRes / 4)), raceFinishedPos.Bottom + (int)(yRes / 16.0f), endGameW, endGameH),
                         bestlapPos = new Rectangle(xRes - (endGameW / 3), raceFinishedPos.Bottom + (int)(yRes / 16.0f), endGameW, endGameH);


        protected Rectangle[] ButtonPositionRects;

        protected Rectangle[] SettingsButtonPositionRects;

        protected Rectangle[] NetworkButtonRects;

        protected Rectangle[] ResolutionButtonPositionRects;

        protected Rectangle[] vSSButtonPosRect;

        protected Rectangle[] EndgameButtonPosRect;

        protected static Texture2D[] ButtonTextures = new Texture2D[]
            {
                raceButton,
                settingsButton,
                exitButton,
            };

        protected static Texture2D[] SettingsButtonTextures = new Texture2D[]
            {
                vSyncButton,
                fullscreenButton,
                resolution,
                ssao,
                easyControls,
                nrOfShadows,
                arrowUp,
                arrowDown,
                backButton,
            };

        protected static Texture2D[] ResolutionButtonTextures = new Texture2D[]
            {
                s1024Button,
                s768Button,
                s600Button,
                w1080Button,
                w900Button,
                w720Button,
                backButton,
            };

        protected static Texture2D[] NetworkButtonTextures = new Texture2D[]
            {
                networkGameButton,
                localGameButton,
                backButton,
            };

        protected static Texture2D[] VSSButtonTextures = new Texture2D[]
            {
                continueButton,
                teds,
                dsmr900rgxd,
                backButton,
            };

        protected static Texture2D[] EndGameButtonTextures = new Texture2D[]
        {
            continueButton,
        };

        public abstract void render();

        public abstract bool update(GameTime time);

        protected void updateMenuUI(int NumberOfButtons, KeyboardState keys)
        {
            if (Sys.InputManager.getInstance().keyboardDownJustPressed())
            {
                buttonHighlighted = (buttonHighlighted + 1) % NumberOfButtons;
                ignoreMouse = true;
            }
            else if (Sys.InputManager.getInstance().keyboardUpJustPressed())
            {
                buttonHighlighted =
                    (buttonHighlighted + NumberOfButtons - 1) % NumberOfButtons;
                ignoreMouse = true;
            }

            // Handle mouse input variables
            InputManager.mouseStateLastFrame = InputManager.mouseState;
            InputManager.mouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();

            // Update mouseXMovement and mouseYMovement
            InputManager.lastMouseXMovement += InputManager.mouseState.X - InputManager.mouseStateLastFrame.X;
            InputManager.lastMouseYMovement += InputManager.mouseState.Y - InputManager.mouseStateLastFrame.Y;
            InputManager.mouseXMovement = InputManager.lastMouseXMovement / 2.0f;
            InputManager.mouseYMovement = InputManager.lastMouseYMovement / 2.0f;
            InputManager.lastMouseXMovement -= InputManager.lastMouseXMovement / 2.0f;
            InputManager.lastMouseYMovement -= InputManager.lastMouseYMovement / 2.0f;

            // Check if mouse was moved this frame if it is not detected yet.
            if (InputManager.mouseDetected == false)
                InputManager.mouseDetected = InputManager.mouseState.X != InputManager.mouseStateLastFrame.X ||
                    InputManager.mouseState.Y != InputManager.mouseStateLastFrame.Y ||
                    InputManager.mouseState.LeftButton != InputManager.mouseStateLastFrame.LeftButton;
        }

        protected void fadePulse()
        {
            if (!transitionDone)
            {
                if (!buttonFadedIn)
                {
                    inAlphaValue += fadeIncrement;

                    if (inAlphaValue >= 255)
                    {
                        buttonFadedIn = true;
                    }
                }

                if (!buttonFadedOut)
                {
                    outAlphaValue += fadeDecrement;

                    if (outAlphaValue <= 0)
                    {
                        buttonFadedOut = true;
                    }
                }
            }
            if (buttonFadedIn && buttonFadedOut)
            {
                transitionDone = true;
            }

            if (alphaGlowUp)
                if (glowAlpha <= 255)
                    glowAlpha += 7;
                else
                    alphaGlowUp = false;
            else
                if (glowAlpha >= 50)
                    glowAlpha -= 7;
                else
                    alphaGlowUp = true;

        }

        protected static void resetTransition()
        {
            transitionDone = false;
            inAlphaValue = 1;
            outAlphaValue = 200;
            buttonFadedIn = false;
            buttonFadedOut = false;
        }

        protected void updateResolution()
        {
            xRes = (int)graphicsManager.getResolution().X;
            yRes = (int)graphicsManager.getResolution().Y;
            buttonHeight = (int)(yRes / 11);
            buttonWidth = (int)(xRes / 2.38);
            endGameW = (int) (xRes / 2);
            endGameH = (int)(yRes / 15);
        }

        //Updates all button sizes so that everything fits on screen regardless of the current resolution
        protected void updateButtonRectangles()
        {
            updateResolution();

            logoPosRect = new Rectangle((int)(xRes - (xRes / 3.6) - (xRes / 51.2)), (int)(yRes / 38.4), (int)(xRes / 3.6), (int)(yRes / 3.84));
            BorderUpPosistionRect = new Rectangle(0, 0, xRes, (int) (yRes / 3.1));
            BorderDownPosistionRect = new Rectangle(0, (int) (yRes - yRes / 3.1), xRes, (int)(yRes / 3.1));
            LabelPositionRect = new Rectangle((int)xRes / 16, 2 * buttonHeight, buttonWidth, buttonHeight);
            raceButtonPositionRect = new Rectangle((int)(xRes / 16), (int)(yRes / 4.5 + buttonHeight), buttonWidth, buttonHeight);
            
            
            settingsButtonPositionRect = new Rectangle((int)(xRes / 16), (int)(yRes / 4.5 + 2 * buttonHeight), buttonWidth, buttonHeight);
            exitButtonPositionRect = new Rectangle((int)(xRes / 16), (int)(yRes / 4.5 + 6 * buttonHeight), buttonWidth, buttonHeight);
            vehicleSelectionPosRect1 = new Rectangle((int)(xRes / 16), (int)(yRes / 4.5 + 2 * buttonHeight), (int)(buttonWidth * 0.8), (int)(buttonHeight * 1.54 * 0.8));
            vehicleSelectionPosRect2 = new Rectangle((int)(xRes / 16), (int)(yRes / 4.5 + 3.3 * buttonHeight), (int)(buttonWidth * 0.8), (int)(buttonHeight * 1.54 * 0.8));
                         
            startNetworkGameButtonPositionRect = raceButtonPositionRect;
            startLocalGameButtonPositionRect = settingsButtonPositionRect;

            vSyncButtonPositionRect = new Rectangle((int)(xRes / 16), (int)(yRes / 4.5 + 1 * buttonHeight), buttonWidth, buttonHeight);
            fullscreenButtonPositionRect = new Rectangle((int)(xRes / 16), (int)(yRes / 4.5 + 2 * buttonHeight), buttonWidth, buttonHeight);             
            resolutionButtonPositionRect = new Rectangle((int)(xRes / 16), (int)(yRes / 4.5 + 3 * buttonHeight), buttonWidth, buttonHeight);
            ssaoButtonPositionRect = new Rectangle((int)(xRes / 16), (int)(yRes / 4.5 + 4 * buttonHeight), buttonWidth, buttonHeight);
            easyControlsPositionRect = new Rectangle((int)(xRes / 2), (int)(yRes / 4.5 + buttonHeight), buttonWidth, buttonHeight);
            nrOfShadowsPositionRect = new Rectangle((int)(xRes / 2), (int)(yRes / 4.5 + 2 * buttonHeight), buttonWidth, buttonHeight);
            arrowUpPositionRect = new Rectangle((int)(xRes / 2 + (buttonWidth / 3)), (int)(yRes / 4.5 + 2.5 * buttonHeight), buttonHeight, buttonHeight);
            arrowDownPositionRect = new Rectangle((int)(xRes / 2 + (buttonWidth / 3)), (int)(yRes / 4.5 + 3.5 * buttonHeight), buttonHeight, buttonHeight);
            backButtonPositionRect = new Rectangle((int)(xRes / 16), (int)(yRes / 4.5 + 6 * buttonHeight), buttonWidth, buttonHeight);
            resButtonXPos = (int)(xRes / 2);
            resButtonYPos = (int)(yRes / 4.5);
            w1080ButtonPositionRect = new Rectangle(resButtonXPos, resButtonYPos + buttonHeight, buttonWidth, buttonHeight);
            w900ButtonPositionRect = new Rectangle(resButtonXPos, resButtonYPos + 2 * buttonHeight, buttonWidth, buttonHeight);
            w720ButtonPositionRect = new Rectangle(resButtonXPos, resButtonYPos + 3 * buttonHeight, buttonWidth, buttonHeight);
            s1024ButtonPositionRect = new Rectangle((int)(xRes / 16), resButtonYPos + buttonHeight, buttonWidth, buttonHeight);
            s768ButtonPositionRect = new Rectangle((int)(xRes / 16), resButtonYPos + 2 * buttonHeight, buttonWidth, buttonHeight);
            s600ButtonPositionRect = new Rectangle((int)(xRes / 16), resButtonYPos + 3 * buttonHeight, buttonWidth, buttonHeight);
       
             waitingForOtherPlayersToFinishPos = new Rectangle((int)(xRes / 6), (int)(yRes / 32.0f), (int)(xRes - xRes / 3.0f), (int)(yRes / 10.0f));
             raceFinishedPos = new Rectangle((int)(xRes / 2 - (endGameW / 3)), (int)(yRes / 25.0f), (int) (endGameW * 1.5), (int) (endGameH * 1.5));
             placePos = new Rectangle(0, raceFinishedPos.Bottom + (int)(yRes / 16.0f), endGameW, endGameH);
             pilotPos = new Rectangle((int) (1.2 * (xRes / 4)), raceFinishedPos.Bottom + (int)(yRes / 16.0f), endGameW, endGameH);
             timePos = new Rectangle((int) (2.3 * (xRes / 4)), raceFinishedPos.Bottom + (int)(yRes / 16.0f), endGameW, endGameH);
             bestlapPos = new Rectangle(xRes - (endGameW / 3), raceFinishedPos.Bottom + (int)(yRes / 16.0f), endGameW, endGameH);

        }

        protected static Texture2D getTexture(String name)
        {
            return RacingGame.contentManager.Load<Texture2D>("Textures//Menu//" + name);
        }
    }
}
