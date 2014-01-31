using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace RacingGame.States
{
    /// <summary>
    /// A game state to show the titlescreen
    /// Author: Magnus Olausson
    /// </summary>
    class Titlescreen : Menu
    {
        public Titlescreen()
        {
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
            Sound.SoundManager.getInstance().playSound("MenuMusic");
        }

        public override void render()
        {

            MenuBackground.render();
            spriteBatch.Begin();
            //spriteBatch.Draw(background, graphicsManager.resolutionRect, BackgroundRect, Color.White);

            spriteBatch.Draw(borderDown, new Rectangle(0, yRes / 2 - BorderRect.Height, xRes, BorderRect.Height), BorderRect, Color.White);
            spriteBatch.Draw(borderUp, new Rectangle(0, yRes / 2, xRes, BorderRect.Height), BorderRect, Color.White);
            spriteBatch.Draw(logo, new Rectangle(xRes / 2 - 275, (int) (yRes / 2.2 - 200), 550, 400), LogoRect, Color.White);
            spriteBatch.Draw(pressToStart, pressToStartButtonPositionRect, MenuButtonRect, Color.White);
            spriteBatch.End();
        }

        public override bool update(GameTime time)
        {
            MenuBackground.update(time);
            KeyboardState keys = Keyboard.GetState();
            if (Sys.InputManager.getInstance().isAnyKeyDown())
            {
                Sys.InputManager.getInstance().keysPressedReset(keys.GetPressedKeys());
                lastScreen = Screen.TITLESCREEN;
                return true;
            }
            return false;
        }
    }
}
