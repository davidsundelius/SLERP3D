using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RacingGame.Logic;
using Microsoft.Xna.Framework;

namespace RacingGame.Graphics
{
    /// <summary>
    /// A scene graph class used for keeping track of objects in a scene.
    /// Author: Daniel Lindén
    /// </summary>
    class Scene : Logic.IUpdateable
    {
        private List<Node> nodes = new List<Node>();

        public void addNode(Node node)
        {
            nodes.Add(node);
        }

        public void clear()
        {
            nodes.Clear();
        }

        public bool update(GameTime time)
        {
            foreach (Node n in nodes)
            {
                n.update(time);
            }

            return false;
        }

        public void render(BoundingFrustum viewFrustum)
        {
            foreach (Node n in nodes)
            {
                n.draw(viewFrustum);
            }
        }

        public bool remove(Node n)
        {
            return nodes.Remove(n);
        }

        public bool find(Node n)
        {
            foreach (Node k in nodes)
            {
                if (k.Equals(n))
                    return true;
            }
            return false;
        }

    }
}
