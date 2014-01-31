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
    class ResolutionSelectionState : Menu
    {
        const int NumberOfButtons = 7;
        private int currentRes;

        public ResolutionSelectionState()
        {
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
            currentRes = getCurrResNr();
        }

        private int getCurrResNr()
        {
            int output = 1;
            switch (yRes)
            {
                case 1024:
                    output = 0;
                    break;
                case 768:
                    output = 1;
                    break;
                case 600:
                    output = 2;
                    break;
                case 1080:
                    output = 3;
                    break;
                case 900:
                    output = 4;
                    break;
                case 720:
                    output = 5;
                    break;
            }
            return output;
        }

        public override void render()
        {
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

            SettingsButtonPositionRects = new Rectangle[]
            {
                vSyncButtonPositionRect,
                fullscreenButtonPositionRect,
                resolutionButtonPositionRect,
                ssaoButtonPositionRect,
                easyControlsPositionRect,
                nrOfShadowsPositionRect,
                backButtonPositionRect,
            };

            spriteBatch.Begin();
            MenuBackground.render();
            spriteBatch.Draw(borderUp, BorderUpPosistionRect, BorderRect, Color.White);
            spriteBatch.Draw(borderDown, BorderDownPosistionRect, BorderRect, Color.White);
            //temp
            spriteBatch.Draw(logo, new Rectangle((int)(xRes - (xRes / 3.6) - (xRes / 51.2)), (int)(yRes / 38.4), (int)(xRes / 3.6), (int)(yRes / 3.84)), LogoRect, Color.White);

            if (lastScreen == Screen.SETTINGS)
            {
                spriteBatch.Draw(vSyncButton, vSyncButtonPositionRect, MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(outAlphaValue, 0, 255)));
                spriteBatch.Draw(fullscreenButton, fullscreenButtonPositionRect, MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(outAlphaValue, 0, 255)));
                spriteBatch.Draw(resolution, resolutionButtonPositionRect, MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(outAlphaValue, 0, 255)));
                spriteBatch.Draw(ssao, ssaoButtonPositionRect, MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(outAlphaValue, 0, 255)));
                spriteBatch.Draw(easyControls, easyControlsPositionRect, MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(outAlphaValue, 0, 255)));
                spriteBatch.Draw(nrOfShadows, nrOfShadowsPositionRect, MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(outAlphaValue, 0, 255)));
                spriteBatch.Draw(backButton, backButtonPositionRect, MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(outAlphaValue, 0, 255)));
            }

            if (InputManager.hasMouseMoved || InputManager.mouseLeftButtonJustPressed)
                ignoreMouse = false;

            for (int num = 0; num < NumberOfButtons; num++)
            {
                Boolean selected = num == buttonHighlighted;
                Rectangle tempRect = ResolutionButtonPositionRects[num];
                if (selected)
                {
                    spriteBatch.Draw(highlight1, new Rectangle(tempRect.Left - buttonWidth / 10, tempRect.Top, tempRect.Width, tempRect.Height), MenuButtonRect, Color.White);
                    spriteBatch.Draw(highlight2, new Rectangle(tempRect.Left - buttonWidth / 10, tempRect.Top, tempRect.Width, tempRect.Height), MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(glowAlpha, 0, 255)));
                }
                spriteBatch.Draw(ResolutionButtonTextures[num], ResolutionButtonPositionRects[num], MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(inAlphaValue, 0, 255)));

                if (currentRes == num)
                {
                    spriteBatch.Draw(check, new Rectangle((int) (tempRect.Right -  2.3 * buttonHeight), tempRect.Top, buttonHeight, buttonHeight), CheckRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(inAlphaValue, 0, 255)));
                }
                if (InputManager.mouseInBox(ResolutionButtonPositionRects[num]))
                    mouseIsOverButton = num;
            }
            if (!ignoreMouse && mouseIsOverButton >= 0)
                buttonHighlighted = mouseIsOverButton;

            //draw cursor
            if (InputManager.MouseDetected)
            {
                Rectangle cursorPosRect = new Rectangle(InputManager.mousePos.X, InputManager.mousePos.Y, 30, 30);
                spriteBatch.Draw(cursor, cursorPosRect, CursorRect, Color.White);
            }
            spriteBatch.End();
        }

        public override bool update(Microsoft.Xna.Framework.GameTime time)
        {
            KeyboardState keys = Keyboard.GetState();
            base.updateMenuUI(NumberOfButtons, keys);
            MenuBackground.update(time);

            fadePulse();

            if ((mouseIsOverButton >= 0 && InputManager.mouseLeftButtonJustPressed) ||
                inputManager.keyboardSpaceJustPressed() ||
                inputManager.keyboardEnterJustPressed())
            {
                Sys.InputManager.getInstance().keysPressedReset(keys.GetPressedKeys());
                switch (buttonHighlighted)
                {
                    case 0:
                        graphicsManager.setResolution(1280, 1024);
                        currentRes = 0;
                        goto default;
                    case 1:
                        graphicsManager.setResolution(1024, 768);
                        currentRes = 1;
                        goto default;
                    case 2:
                        graphicsManager.setResolution(800, 600);
                        currentRes = 2;
                        goto default;
                    case 3:
                        graphicsManager.setResolution(1920, 1080);
                        currentRes = 3;
                        goto default;
                    case 4:
                        graphicsManager.setResolution(1600, 900);
                        currentRes = 4;
                        goto default;
                    case 5:
                        graphicsManager.setResolution(1280, 720);
                        currentRes = 5;
                        goto default;
                    case 6:
                        lastScreen = Screen.RESOLUTION;
                        resetTransition();
                        return true;
                    default:
                        updateButtonRectangles();
                        updatedUI = true;
                        Properties.Settings.Default.Height = (int)graphicsManager.getResolution().Y;
                        Properties.Settings.Default.Width = (int)graphicsManager.getResolution().X;
                        Properties.Settings.Default.Save();
                        break;
                }
            }
            Sys.InputManager.getInstance().keysPressedReset(keys.GetPressedKeys());
            //one must hover over the button to be able to select it. this is why
            //mouseIsOverButton is reset here
            mouseIsOverButton = -1;

            if (inputManager.isEscapeDown())
            {
                resetTransition();
                lastScreen = Screen.RESOLUTION;
                return true;
            }
            else
                return false;
        }
    }


}
