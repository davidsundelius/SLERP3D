using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RacingGame.Graphics
{
    /// <summary>
    /// An object which is responsible for managing a render que
    /// Author: Daniel Lindén
    /// </summary>
    class RenderQueue
    {
        private List<Node> queue = new List<Node>();

        public void addNode(Node node)
        {
            queue.Add(node);
        }

        public void clear()
        {
            queue.Clear();
        }

        public void render()
        {
            foreach (Node n in queue)
            {
                n.render();
            }
        }

    }
}
