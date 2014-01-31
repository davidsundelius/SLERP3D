using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RacingGame.Graphics;
using RacingGame.Sys;

namespace RacingGame.States
{
    /// <summary>
    /// A game state to show the main menu, inherits from Menu
    /// Author: Magnus Olausson
    /// </summary>
    class MainMenu : Menu
    {
        const int NumberOfButtons = 3;



        public MainMenu(Stack<States.IGameState> gameStates)
        {
            base.gameStates = gameStates;
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
            updatedUI = true;
            MenuBackground.initialize();
        }

        public override void render()
        {
            ButtonPositionRects = new Rectangle[]
            {
                raceButtonPositionRect,
                settingsButtonPositionRect,
                exitButtonPositionRect,
            };

            SettingsButtonPositionRects = new Rectangle[]
            {
                vSyncButtonPositionRect,
                fullscreenButtonPositionRect,
                resolutionButtonPositionRect,
                backButtonPositionRect,
            };

            if (Properties.Settings.Default.easyControls)
            {
                Properties.Controls controls = Properties.Controls.Default;
                controls.Throttle = controls.easyThrottle;
                controls.TurnLeft = controls.easyLeft;
                controls.TurnRight = controls.easyRight;
                controls.Reverse = controls.easyReverse;
                controls.LeftBrake = Keys.None;
                controls.RightBrake = Keys.None;
                controls.StrafeLeft = Keys.None;
                controls.StrafeRight = Keys.None;
                Properties.Settings.Default.easyControls = true;
            }
            else
            {
                Properties.Controls.Default.Reset();
                Properties.Settings.Default.easyControls = false;
            }
            Properties.Settings.Default.Save();

            MenuBackground.render();
            spriteBatch.Begin();
            
            spriteBatch.Draw(borderUp, BorderUpPosistionRect, BorderRect, Color.White);
            spriteBatch.Draw(borderDown, BorderDownPosistionRect, BorderRect, Color.White);
            //temp
            spriteBatch.Draw(logo, logoPosRect, LogoRect, Color.White);
            spriteBatch.Draw(mainMenu, LabelPositionRect, MenuButtonRect, Color.White);

            if (lastScreen == Screen.SETTINGS)
            {
                spriteBatch.Draw(settingsButton, LabelPositionRect, MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(outAlphaValue, 0, 255)));
                spriteBatch.Draw(vSyncButton, vSyncButtonPositionRect, MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(outAlphaValue, 0, 255)));
                spriteBatch.Draw(fullscreenButton, fullscreenButtonPositionRect, MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(outAlphaValue, 0, 255)));
                spriteBatch.Draw(resolution, resolutionButtonPositionRect, MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(outAlphaValue, 0, 255)));
                spriteBatch.Draw(ssao, ssaoButtonPositionRect, MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(outAlphaValue, 0, 255)));
                spriteBatch.Draw(easyControls, easyControlsPositionRect, MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(outAlphaValue, 0, 255)));
                spriteBatch.Draw(nrOfShadows, nrOfShadowsPositionRect, MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(outAlphaValue, 0, 255)));
                spriteBatch.Draw(backButton, backButtonPositionRect, MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(outAlphaValue, 0, 255)));
            }
            else if (lastScreen == Screen.TITLESCREEN)
            {
                spriteBatch.Draw(pressToStart, pressToStartButtonPositionRect, MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(outAlphaValue, 0, 255)));
                spriteBatch.Draw(borderUp, new Rectangle(0, yRes / 2 - BorderRect.Height, xRes, BorderRect.Height), BorderRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(outAlphaValue, 0, 255)));
                spriteBatch.Draw(borderDown, new Rectangle(0, yRes / 2, xRes, BorderRect.Height), BorderRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(outAlphaValue, 0, 255)));
                spriteBatch.Draw(logo, new Rectangle(xRes / 2 - 275, (int)(yRes / 2.2 - 200), 550, 400), LogoRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(outAlphaValue, 0, 255)));
            }
            else if (lastScreen == Screen.VEHICLE)
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

                if (lastScreen == Screen.SETTINGS && num == 1)
                {
                    if (selected)
                    {
                        Rectangle tempRect = ButtonPositionRects[num];
                        spriteBatch.Draw(highlight1, new Rectangle(tempRect.Left - buttonWidth / 10, tempRect.Top, tempRect.Width, tempRect.Height), MenuButtonRect, Color.White);
                        spriteBatch.Draw(highlight2, new Rectangle(tempRect.Left - buttonWidth / 10, tempRect.Top, tempRect.Width, tempRect.Height), MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(glowAlpha, 0, 255)));
                    }
                    spriteBatch.Draw(ButtonTextures[num], ButtonPositionRects[num], MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(inAlphaValue, 0, 255)));
                }
                else
                {
                    if (selected)
                    {
                        Rectangle tempRect = ButtonPositionRects[num];
                        spriteBatch.Draw(highlight1, new Rectangle(tempRect.Left - buttonWidth / 10, tempRect.Top, tempRect.Width, tempRect.Height), MenuButtonRect, Color.White);
                        spriteBatch.Draw(highlight2, new Rectangle(tempRect.Left - buttonWidth / 10, tempRect.Top, tempRect.Width, tempRect.Height), MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(glowAlpha, 0, 255)));
                    }
                    spriteBatch.Draw(ButtonTextures[num], ButtonPositionRects[num], MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(inAlphaValue, 0, 255)));
                }

                if (InputManager.mouseInBox(ButtonPositionRects[num]))
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

            fadePulse();
            /*
             * if the spacebar or "enter" button is the source of a keyboard input, 
             * the correct action is taken depending which button is highlighted at the moment.
            */
            if ((mouseIsOverButton >= 0 && InputManager.mouseLeftButtonJustPressed) ||
                inputManager.keyboardSpaceJustPressed() ||
                inputManager.keyboardEnterJustPressed())
            {
                switch (buttonHighlighted)
                {
                    //RACE!
                    case 0:
                        gameStates.Push(new States.VehicleSelectionState(gameStates));
                        //gameStates.Push(new States.VehicleSelectionState(gameStates));
                        /*FinishedPlayer[] fplayers = new FinishedPlayer[]
                        {
                            new FinishedPlayer() { Name = "Daniel", BestLap = 10.0f, Time = 30.0f },
                            new FinishedPlayer() { Name = "Warmaster", BestLap = 12.34f, Time = 42.6142f}
                        };
                        gameStates.Push(new EndGame(fplayers));*/
                        goto default;
                    //SETTINGS
                    case 1:
                        gameStates.Push(new States.Settings(gameStates));
                        goto default;
                    //EXIT
                    case 2:
                        return true;
                    default:
                        lastScreen = Screen.MAINMENU;
                        resetTransition();
                        break;
                }
            }
            Sys.InputManager.getInstance().keysPressedReset(keys.GetPressedKeys());
            //one must hover over the button to be able to select it. this is why
            //mouseIsOverButton is reset here
            mouseIsOverButton = -1;

            //if (InputManager.getInstance().isEscapeDown())
            //    return true;
            //else
                return false;
        }
    }
}
