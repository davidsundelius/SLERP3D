using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RacingGame.States
{
    /// <summary>
    /// Interface to describe a game state
    /// Author: ---
    /// </summary>
    public interface IGameState : Logic.IUpdateable
    {
        void render();
    }
}
