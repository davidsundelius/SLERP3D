using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RacingGame.States
{
    /// <summary>
    /// A game state to show the settings menu
    /// Author: ---
    /// </summary>
    class Options : IGameState
    {
        Graphics.GraphicsManager graphicsManager;

        public Options(Graphics.GraphicsManager graphicsManager)
        {
            this.graphicsManager = graphicsManager;
        }

        public void render()
        {
            graphicsManager.render("Options");
        }

        public bool update(float delta)
        {
            return Sys.InputManager.getInstance().isAnyKeyDown();
        }

    }
}
