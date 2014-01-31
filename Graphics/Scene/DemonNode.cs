using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RacingGame.Logic;

namespace RacingGame.Graphics
{
    class DemonNode : ShieldNode
    {
        public DemonNode(ShipNode ship, Model m)
            : base(ship, m)
        {
            shieldTexture = RacingGame.contentManager.Load<Texture>("Textures/demon");
        }
    }
}
