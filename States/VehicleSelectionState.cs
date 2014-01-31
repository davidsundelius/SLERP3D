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
    class VehicleSelectionState : Menu
    {
        private Stack<IGameState> gameStates;

        const int NumberOfButtons = 4;

        public VehicleSelectionState(Stack<IGameState> gameStates)
        {
            this.gameStates = gameStates;
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
        }

        public override void render()
        {
            vSSButtonPosRect = new Rectangle[]
            {
                raceButtonPositionRect,
                vehicleSelectionPosRect1,
                vehicleSelectionPosRect2,
                backButtonPositionRect,
            };

            MenuBackground.render();
            spriteBatch.Begin();
            spriteBatch.Draw(borderUp, BorderUpPosistionRect, BorderRect, Color.White);
            spriteBatch.Draw(borderDown, BorderDownPosistionRect, BorderRect, Color.White);
            //temp
            spriteBatch.Draw(logo, new Rectangle((int)(xRes - (xRes / 3.6) - (xRes / 51.2)), (int)(yRes / 38.4), (int)(xRes / 3.6), (int)(yRes / 3.84)), LogoRect, Color.White);
            spriteBatch.Draw(vehicleSelection, LabelPositionRect, MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(inAlphaValue, 0, 255)));

            if (lastScreen == Screen.MAINMENU)
            {
                spriteBatch.Draw(mainMenu, LabelPositionRect, MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(outAlphaValue, 0, 255)));
                spriteBatch.Draw(raceButton, raceButtonPositionRect, MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(outAlphaValue, 0, 255)));
                spriteBatch.Draw(settingsButton, settingsButtonPositionRect, MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(outAlphaValue, 0, 255)));
                spriteBatch.Draw(exitButton, exitButtonPositionRect, MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(outAlphaValue, 0, 255)));            
            }

            if (lastScreen == Screen.NETWORK)
            {
                spriteBatch.Draw(gameModeSelection, LabelPositionRect, MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(outAlphaValue, 0, 255)));
                spriteBatch.Draw(networkGameButton, startNetworkGameButtonPositionRect, MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(outAlphaValue, 0, 255)));
                spriteBatch.Draw(localGameButton, startLocalGameButtonPositionRect, MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(outAlphaValue, 0, 255)));
                spriteBatch.Draw(backButton, backButtonPositionRect, MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(outAlphaValue, 0, 255)));   
            }


            for (int num = 0; num < NumberOfButtons; num++)
            {
                Boolean selected = num == buttonHighlighted;

                if (selected)
                {
                    Rectangle tempRect = vSSButtonPosRect[num];
                    spriteBatch.Draw(highlight1, new Rectangle(tempRect.Left - buttonWidth / 10, tempRect.Top, tempRect.Width, tempRect.Height), MenuButtonRect, Color.White);
                    spriteBatch.Draw(highlight2, new Rectangle(tempRect.Left - buttonWidth / 10, tempRect.Top, tempRect.Width, tempRect.Height), MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(glowAlpha, 0, 255)));

                    switch (Properties.Settings.Default.chosenVehicle)
                    {
                        case 0:
                            MenuBackground.addVehicle(Properties.Settings.Default.chosenVehicle);
                            break;
                        case 1:
                            MenuBackground.addVehicle(Properties.Settings.Default.chosenVehicle);
                            break;
                    }
                }

                if (num == 1 || num == 2)
                {
                    spriteBatch.Draw(VSSButtonTextures[num], vSSButtonPosRect[num], VehicleSelectionRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(inAlphaValue, 0, 255)));
                }
                else
                {
                    spriteBatch.Draw(VSSButtonTextures[num], vSSButtonPosRect[num], MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(inAlphaValue, 0, 255)));
                }

                if (InputManager.mouseInBox(vSSButtonPosRect[num]))
                    mouseIsOverButton = num;
            }

            if (InputManager.hasMouseMoved || InputManager.mouseLeftButtonJustPressed)
                ignoreMouse = false;

            //draw cursor
            if (InputManager.MouseDetected)
            {
                Rectangle cursorPosRect = new Rectangle(InputManager.mousePos.X, InputManager.mousePos.Y, 30, 30);
                spriteBatch.Draw(cursor, cursorPosRect, CursorRect, Color.White);
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
                    //continue
                    case 0:
                        gameStates.Push(new States.NetworkState(gameStates));
                        MenuBackground.removeVehicles();
                        resetTransition();
                        lastScreen = Screen.VEHICLE;
                        break;
                    //option 1
                    case 1:
                        Properties.Settings.Default.chosenVehicle = 0;
                        Properties.Settings.Default.Save();
                        break;
                    //option 2
                    case 2:
                        Properties.Settings.Default.chosenVehicle = 1;
                        Properties.Settings.Default.Save();
                        break;
                    //BACK
                    case 3:
                        lastScreen = Screen.VEHICLE;
                        resetTransition();
                        MenuBackground.removeVehicles();
                        return true;
                }
            }
            Sys.InputManager.getInstance().keysPressedReset(keys.GetPressedKeys());
            //one must hover over the button to be able to select it. this is why
            //mouseIsOverButton is reset here
            mouseIsOverButton = -1;

            if (inputManager.isEscapeDown())
            {
                lastScreen = Screen.VEHICLE;
                resetTransition();
                return true;
            }
            else
                return false;
        }
    }
}
