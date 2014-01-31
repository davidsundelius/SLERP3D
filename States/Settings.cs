using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RacingGame.Sys;
using RacingGame.States;

namespace RacingGame.States
{
    /// <summary>
    /// A game state to show the settings menu
    /// Author: Magnus Olausson
    /// </summary>
    class Settings : Menu
    {
        const int NumberOfButtons = 9;

        public Settings(Stack<States.IGameState> gameStates)
        {
            base.gameStates = gameStates;
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
        }

        public override void render()
        {
            SettingsButtonPositionRects = new Rectangle[]
            {
                vSyncButtonPositionRect,
                fullscreenButtonPositionRect,
                resolutionButtonPositionRect,
                ssaoButtonPositionRect,
                easyControlsPositionRect,
                nrOfShadowsPositionRect,
                arrowUpPositionRect,
                arrowDownPositionRect,
                backButtonPositionRect,
            };

            ResolutionButtonPositionRects = new Rectangle[]
            {
                s1024ButtonPositionRect,
                s768ButtonPositionRect,
                s600ButtonPositionRect,
                w1080ButtonPositionRect,
                w900ButtonPositionRect,
                w720ButtonPositionRect,
                backButtonPositionRect,
            };

            spriteBatch.Begin();
            MenuBackground.render();
            spriteBatch.Draw(borderUp, BorderUpPosistionRect, BorderRect, Color.White);
            spriteBatch.Draw(borderDown, BorderDownPosistionRect, BorderRect, Color.White);
            //temp
            spriteBatch.Draw(logo, new Rectangle((int)(xRes - (xRes / 3.6) - (xRes / 51.2)), (int)(yRes / 38.4), (int)(xRes / 3.6), (int)(yRes / 3.84)), LogoRect, Color.White);
            spriteBatch.Draw(settingsButton, LabelPositionRect, MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(inAlphaValue, 0, 255)));
            if (lastScreen == Screen.MAINMENU)
            {
                spriteBatch.Draw(mainMenu, LabelPositionRect, MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(outAlphaValue, 0, 255)));
            }
                if (lastScreen == Screen.RESOLUTION)
            {
                spriteBatch.Draw(s1024Button, s1024ButtonPositionRect, MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(outAlphaValue, 0, 255)));
                spriteBatch.Draw(s768Button, s768ButtonPositionRect, MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(outAlphaValue, 0, 255)));
                spriteBatch.Draw(s600Button, s600ButtonPositionRect, MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(outAlphaValue, 0, 255)));
                spriteBatch.Draw(w1080Button, w1080ButtonPositionRect, MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(outAlphaValue, 0, 255)));
                spriteBatch.Draw(w900Button, w900ButtonPositionRect, MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(outAlphaValue, 0, 255)));
                spriteBatch.Draw(w720Button, w720ButtonPositionRect, MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(outAlphaValue, 0, 255)));
                spriteBatch.Draw(backButton, backButtonPositionRect, MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(outAlphaValue, 0, 255)));
            }

            if (InputManager.hasMouseMoved || InputManager.mouseLeftButtonJustPressed)
                ignoreMouse = false;

            for (int num = 0; num < NumberOfButtons; num++)
            {
                Boolean selected = num == buttonHighlighted;
                bool arrows = (num == 6 || num == 7);
                if (selected)
                {
                    Rectangle tempRect = SettingsButtonPositionRects[num];
                    if (arrows)
                    {
                        bool nr = (num == 6);
                        spriteBatch.Draw(nr ? arrowUpHighlight : arrowDownHighlight, nr ? arrowUpPositionRect : arrowDownPositionRect, CheckRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(glowAlpha, 0, 255)));
                    }
                    else
                    {
                        spriteBatch.Draw(highlight1, new Rectangle(tempRect.Left - buttonWidth / 10, tempRect.Top, tempRect.Width, tempRect.Height), MenuButtonRect, Color.White);
                        spriteBatch.Draw(highlight2, new Rectangle(tempRect.Left - buttonWidth / 10, tempRect.Top, tempRect.Width, tempRect.Height), MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(glowAlpha, 0, 255)));
                    }
                }
                spriteBatch.Draw(SettingsButtonTextures[num], SettingsButtonPositionRects[num], arrows ? CheckRect : MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(inAlphaValue, 0, 255)));

                Rectangle r = SettingsButtonPositionRects[num];
                //VSYNC
                if (num == 0)
                {
                    if (Properties.Settings.Default.VSync)
                        spriteBatch.Draw(on, new Rectangle(r.Right - r.Height, r.Top, r.Height, r.Height), CheckRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(inAlphaValue, 0, 255)));
                    else
                        spriteBatch.Draw(off, new Rectangle(r.Right - r.Height, r.Top, r.Height, r.Height), CheckRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(inAlphaValue, 0, 255)));
                }
                //Fullscreen
                if (num == 1)
                {
                    if (graphics.IsFullScreen)
                        spriteBatch.Draw(on, new Rectangle(r.Right - r.Height, r.Top, r.Height, r.Height), CheckRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(inAlphaValue, 0, 255)));
                    else
                        spriteBatch.Draw(off, new Rectangle(r.Right - r.Height, r.Top, r.Height, r.Height), CheckRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(inAlphaValue, 0, 255)));
                }
                if (num == 3)
                {
                    if (Properties.Settings.Default.UseSSAO)
                        spriteBatch.Draw(on, new Rectangle(r.Right - r.Height, r.Top, r.Height, r.Height), CheckRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(inAlphaValue, 0, 255)));
                    else
                        spriteBatch.Draw(off, new Rectangle(r.Right - r.Height, r.Top, r.Height, r.Height), CheckRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(inAlphaValue, 0, 255)));
                }
                if (num == 4)
                {
                    if (Properties.Settings.Default.easyControls)
                        spriteBatch.Draw(on, new Rectangle(r.Right - r.Height, r.Top, r.Height, r.Height), CheckRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(inAlphaValue, 0, 255)));
                    else
                        spriteBatch.Draw(off, new Rectangle(r.Right - r.Height, r.Top, r.Height, r.Height), CheckRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(inAlphaValue, 0, 255)));
                }
                if (num == 6)
                {
                    Color outlineColor = new Color(255, 255, 255);
                    spriteBatch.DrawString(hudFont, "" + Properties.Settings.Default.NumberOfShadowMaps, new Vector2((int) r.Right + (r.Height / 2), (int)(yRes / 4.5 + 3.2 * buttonHeight)), outlineColor, 0, new Vector2(0, 0), 1.0f, SpriteEffects.None, 0.5f);
                }

                if (InputManager.mouseInBox(SettingsButtonPositionRects[num]))
                        mouseIsOverButton = num;
            }
            if (!ignoreMouse && mouseIsOverButton >= 0 && mouseIsOverButton != 5)
                buttonHighlighted = mouseIsOverButton;

                        //draw cursor
            if (InputManager.MouseDetected)
            {
                Rectangle cursorPosRect = new Rectangle(InputManager.mousePos.X, InputManager.mousePos.Y, 30, 30);
                spriteBatch.Draw(cursor, cursorPosRect, CursorRect, Color.White);
            }
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
                        //VSYNC
                    case 0:
                        graphics.SynchronizeWithVerticalRetrace = !graphics.SynchronizeWithVerticalRetrace;
                        Properties.Settings.Default.VSync = graphics.SynchronizeWithVerticalRetrace;
                        goto default;
                    //FULLSCREEN
                    case 1:
                        graphics.ToggleFullScreen();
                        goto default;
                    //RESOLUTION
                    case 2:
                        resetTransition();
                        lastScreen = Screen.SETTINGS;
                        gameStates.Push(new States.ResolutionSelectionState());
                        break;
                    //SSAO
                    case 3:
                        if (!Properties.Settings.Default.UseSSAO)
                            graphicsManager.setAmbientOcclusion();
                        Properties.Settings.Default.UseSSAO = !Properties.Settings.Default.UseSSAO;
                        goto default;
                    //EASY CONTROLS
                    case 4:
                        if (!Properties.Settings.Default.easyControls)
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
                        goto default;
                    //ARROWUP
                    case 6:
                        Properties.Settings.Default.NumberOfShadowMaps = ++Properties.Settings.Default.NumberOfShadowMaps;
                        goto default;
                    //ARROWDOWN
                    case 7:
                        Properties.Settings.Default.NumberOfShadowMaps = --Properties.Settings.Default.NumberOfShadowMaps;
                        goto default;
                    //BACK
                    case 8:
                        resetTransition();
                        lastScreen = Screen.SETTINGS;
                        return true;
                    default:
                        Properties.Settings.Default.Save();
                        graphics.ApplyChanges();
                        break;
                }
            }
            Sys.InputManager.getInstance().keysPressedReset(keys.GetPressedKeys());
            //one must hover over the button to be able to select it. this is why
            //mouseIsOverButton is reset here
            mouseIsOverButton = -1;

            if (inputManager.isEscapeDown())
            {
                MainMenu.resetTransition();
                lastScreen = Screen.SETTINGS;
                return true;
            }
            else
                return false;
        }
    }
}
