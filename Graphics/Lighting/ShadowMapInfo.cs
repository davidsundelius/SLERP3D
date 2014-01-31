using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RacingGame.Graphics
{
    /// <summary>
    /// The information needed to render a shadow map.
    /// Author: Daniel Lindén
    /// </summary>
    class ShadowMapInfo
    {
        public Matrix view;
        public Matrix projection;
        public Vector3 position;
        public Vector3 direction;
        public Vector3 up;
        public BoundingFrustum frustum;
        public CubeMapFace cubeFace;

        public ShadowMapInfo()
        {
            view = Matrix.Identity;
            projection = Matrix.Identity;
            frustum = new BoundingFrustum(Matrix.Identity);
        }

        
    }
}
