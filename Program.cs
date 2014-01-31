using System;

namespace RacingGame
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the game
        /// Author: Team Racinggame
        /// </summary>
        static void Main(string[] args)
        {
            bool menu = true;
            if (args.Length > 0 && args[0] == "-nomenu")
            {
                menu = false;
            }
            
            using (RacingGame game = new RacingGame(menu))
            {
                game.Run();
            }
        }
    }
}

