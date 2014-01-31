using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Input;
using RacingGame;
using RacingGame.Network;
using RacingGame.Sys;

namespace RacingGame.States
{
    class NetworkState : Menu
    {
        private Stack<IGameState> gameStates;
        private P2PManager p2pManager = P2PManager.Instance;

        const int NumberOfButtons = 3;

        public NetworkState(Stack<IGameState> gameStates)
        {
            this.gameStates = gameStates;
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
        }

        public override void render()
        {
            updateButtonRectangles();
            NetworkButtonRects = new Rectangle[]
            {
                startNetworkGameButtonPositionRect,
                startLocalGameButtonPositionRect,
                backButtonPositionRect,
            };

            MenuBackground.render();
            spriteBatch.Begin();
            spriteBatch.Draw(borderUp, BorderUpPosistionRect, BorderRect, Color.White);
            spriteBatch.Draw(borderDown, BorderDownPosistionRect, BorderRect, Color.White);
            //temp
            spriteBatch.Draw(logo, new Rectangle((int)(xRes - (xRes / 3.6) - (xRes / 51.2)), (int)(yRes / 38.4), (int)(xRes / 3.6), (int)(yRes / 3.84)), LogoRect, Color.White);
            spriteBatch.Draw(gameModeSelection, LabelPositionRect, MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(inAlphaValue, 0, 255)));
            if (lastScreen == Screen.VEHICLE)
            {
                spriteBatch.Draw(vehicleSelection, LabelPositionRect, MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(outAlphaValue, 0, 255)));
                spriteBatch.Draw(continueButton, raceButtonPositionRect, MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(outAlphaValue, 0, 255)));
                spriteBatch.Draw(teds, vehicleSelectionPosRect1, VehicleSelectionRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(outAlphaValue, 0, 255)));
                spriteBatch.Draw(dsmr900rgxd, vehicleSelectionPosRect2, VehicleSelectionRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(outAlphaValue, 0, 255)));
                spriteBatch.Draw(backButton, backButtonPositionRect, MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(outAlphaValue, 0, 255)));
            }

            if (InputManager.hasMouseMoved || InputManager.mouseLeftButtonJustPressed)
                ignoreMouse = false;

            //draw cursor
            if (InputManager.MouseDetected)
            {
                Rectangle cursorPosRect = new Rectangle(InputManager.mousePos.X, InputManager.mousePos.Y, 30, 30);
                spriteBatch.Draw(cursor, cursorPosRect, CursorRect, Color.White);
            }

            for (int num = 0; num < NumberOfButtons; num++)
            {
                Boolean selected = num == buttonHighlighted;

                if (selected)
                {
                    Rectangle tempRect = NetworkButtonRects[num];
                    spriteBatch.Draw(highlight1, new Rectangle(tempRect.Left - buttonWidth / 10, tempRect.Top, tempRect.Width, tempRect.Height), MenuButtonRect, Color.White);
                    spriteBatch.Draw(highlight2, new Rectangle(tempRect.Left - buttonWidth / 10, tempRect.Top, tempRect.Width, tempRect.Height), MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(glowAlpha, 0, 255)));
                }
                spriteBatch.Draw(NetworkButtonTextures[num], NetworkButtonRects[num], MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(inAlphaValue, 0, 255)));

                if (InputManager.mouseInBox(NetworkButtonRects[num]))
                    mouseIsOverButton = num;
            }
            if (!ignoreMouse && mouseIsOverButton >= 0)
                buttonHighlighted = mouseIsOverButton;
            spriteBatch.End();
        }

        public override bool update(GameTime time)
        {
            KeyboardState keys = Keyboard.GetState();
            base.updateMenuUI(NumberOfButtons, keys);
            MenuBackground.update(time);

            if (p2pManager.CurrentNrOfPlayers == P2PManager.maxGamers)
            {
                startNetworkGame();
            }

            fadePulse();

            /*
             * if the spacebar or "enter" button is the source of a keyboard input, 
             * the correct action is taken depending which button is highlighted at the moment.
            */
            if ((mouseIsOverButton >= 0 && InputManager.mouseLeftButtonJustPressed) ||
                inputManager.keyboardSpaceJustPressed() ||
                inputManager.keyboardEnterJustPressed())
            {
                Sys.InputManager.getInstance().keysPressedReset(keys.GetPressedKeys());
                switch (buttonHighlighted)
                {
                    //START NETWORK GAME
                    case 0:
                        p2pManager.CreateSession();
                        lastScreen = Screen.NETWORK;
                        startNetworkGame();
                        break;
                    //START LOCAL GAME
                    case 1:
                        p2pManager.JoinSession();
                        lastScreen = Screen.NETWORK;
                        startNetworkGame();
                        break;
                    //BACK
                    case 2:
                        lastScreen = Screen.NETWORK;
                        resetTransition();
                        return true;
                }
            }
            Sys.InputManager.getInstance().keysPressedReset(keys.GetPressedKeys());
            //one must hover over the button to be able to select it. this is why
            //mouseIsOverButton is reset here
            mouseIsOverButton = -1;

            if (inputManager.isEscapeDown())
            {
                lastScreen = Screen.NETWORK;
                return true;
            }
            else
                return false;
        }

        void startNetworkGame()
        {
            if (Gamer.SignedInGamers.Count == 0)
            {
                // If there are no profiles signed in, we cannot proceed.
                // Show the Guide so the user can sign in.
                if (!Guide.IsVisible)
                    Guide.ShowSignIn(1, false);
            }

            if (p2pManager.networkSession != null) //p2pManager.IsEveryoneReady)
            {
               //p2pManager.networkSession.StartGame();

                gameStates.Push(new States.Game(true));
            }
        }
    }
}
