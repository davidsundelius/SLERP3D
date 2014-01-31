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
    class EndGame : Menu
    {
        Game game;
        const int NumberOfButtons = 1;
        FinishedPlayer[] players;
        SpriteFont font;

        public EndGame(FinishedPlayer[] finishedPlayers)
        {
            players = finishedPlayers;
            font = RacingGame.contentManager.Load<SpriteFont>("Fonts/EndGame");
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
        }

        public override void render()
        {
            EndgameButtonPosRect = new Rectangle[]
            {
                backButtonPositionRect,
            };

            MenuBackground.render();
            spriteBatch.Begin();
            spriteBatch.Draw(borderUp, BorderUpPosistionRect, null, Color.White);
            spriteBatch.Draw(borderDown, BorderDownPosistionRect, BorderRect, Color.White);
            spriteBatch.Draw(raceFinished, raceFinishedPos, null, Color.White);
            spriteBatch.Draw(place, placePos, null, Color.White);
            spriteBatch.Draw(pilot, pilotPos, null, Color.White);
            spriteBatch.Draw(time, timePos, null, Color.White);
            spriteBatch.Draw(bestlap, bestlapPos, null, Color.White);

            Vector2 basePos = new Vector2(0f, (float) (yRes / 4 * 0.9));
            float xDelta = (float)((xRes - (endGameW / 30)) - placePos.Left) / 3.4f;
            int i = 1;
            basePos.X += xRes / 50f;
            Vector2 verticalOffs = new Vector2(0.0f, (yRes / 20));
            foreach (FinishedPlayer p in players)
            {
                spriteBatch.DrawString(font, i.ToString(), basePos + verticalOffs * i, Color.White);
                spriteBatch.DrawString(font, p.Name, basePos + verticalOffs * i + new Vector2(xDelta, 0.0f), Color.White);
                spriteBatch.DrawString(font, p.Time.ToString("0.00"), basePos + verticalOffs * i + new Vector2(xDelta, 0.0f) * 2, Color.White);
                spriteBatch.DrawString(font, p.BestLap.ToString("0.00"), basePos + verticalOffs * i + new Vector2(xDelta, 0.0f) * 3, Color.White);
                ++i;
            }

            //logo on top
            spriteBatch.Draw(logo, new Rectangle((int)(xRes / 2 - ((int)(xRes / 3.6) / 2)), (int)(yRes * 0.66), (int)(xRes / 3.6), (int)(yRes / 3.84)), LogoRect, Color.White);

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
                    Rectangle tempRect = EndgameButtonPosRect[num];
                    spriteBatch.Draw(highlight1, new Rectangle(tempRect.Left - buttonWidth / 10, tempRect.Top, tempRect.Width, tempRect.Height), MenuButtonRect, Color.White);
                    spriteBatch.Draw(highlight2, new Rectangle(tempRect.Left - buttonWidth / 10, tempRect.Top, tempRect.Width, tempRect.Height), MenuButtonRect, new Color(255, 255, 255, (byte)MathHelper.Clamp(glowAlpha, 0, 255)));
                }
                spriteBatch.Draw(EndGameButtonTextures[num], EndgameButtonPosRect[num], null, Color.White);

                if (InputManager.mouseInBox(EndgameButtonPosRect[num]))
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

            /*
             * if the spacebar or "enter" button is the source of a keyboard input, 
             * the correct action is taken depending which button is highlighted at the moment.
            */
            if ((mouseIsOverButton >= 0 && InputManager.mouseLeftButtonJustPressed) ||
                inputManager.keyboardSpaceJustPressed() ||
                inputManager.keyboardEnterJustPressed())
            {
                Sys.InputManager.getInstance().keysPressedReset(keys.GetPressedKeys());
                lastScreen = Screen.MAINMENU;
                return true;
            }
            Sys.InputManager.getInstance().keysPressedReset(keys.GetPressedKeys());
            //one must hover over the button to be able to select it. this is why
            //mouseIsOverButton is reset here
            mouseIsOverButton = -1;

            if (inputManager.isEscapeDown())
            {
                lastScreen = Screen.MAINMENU;
                return true;
            }
            else
                return false;
        }
    }
}
