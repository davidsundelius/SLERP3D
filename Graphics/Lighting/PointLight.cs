using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RacingGame.Graphics
{
    /// <summary>
    /// The light which represents a point light.
    /// Author: Daniel Lindén
    /// </summary>
    class PointLight : Light
    {
        //Change this to a sphere once someone writes the mesh creation code for that
        static private Mesh graphicsBox = null;
        ShadowMapInfo[] shadowMapInfo = new ShadowMapInfo[6];
        BoundingBox boundingBox;

        public float radius { get; private set; }

        public override Vector3 position
        {
            get
            {
                return base.position;
            }
            set
            {
                base.position = value;
                updateBoundingBox();
                updateView();
            }
        }

        public PointLight()
        {
            for (int i = 0; i < 6; ++i)
            {
                shadowMapInfo[i] = new ShadowMapInfo();
            }

            setRadius(10.0f);

            if (graphicsBox == null)
            {
                graphicsBox = Mesh.createCube(2, 2, 2);
            }

            updateView();
 
        }

        public void setRadius(float radius)
        {
            this.radius = radius;
            updateBoundingBox();
            updateProjection();
        }

        public override ShadowMapInfo[] getShadowMaps()
        {
            return shadowMapInfo;
        }

        public override void render()
        {
            GraphicsManager.getInstance().setWorldMatrix(Matrix.CreateScale(radius) * Matrix.CreateTranslation(position));
            GraphicsManager.getInstance().updateShader();
            graphicsBox.render();
        }

        public override bool update(GameTime delta)
        {
            return false;
        }

        public override bool contains(Vector3 point)
        {
            float distance = (point - position).LengthSquared();

            return distance < (radius * radius);
        }

        private void updateBoundingBox()
        {
            boundingBox = new BoundingBox(position - Vector3.One * radius, position + Vector3.One * radius);
        }

        private void updateProjection()
        {
            for (int i = 0; i < 6; ++i)
            {
                shadowMapInfo[i].projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 1.0f, 0.1f, radius);
                shadowMapInfo[i].frustum = new BoundingFrustum(shadowMapInfo[i].view * shadowMapInfo[i].projection);
            }
        }
        private void updateView()
        {
            foreach (ShadowMapInfo inf in shadowMapInfo)
            {
                inf.position = position;
            }

            shadowMapInfo[0].view = Matrix.CreateLookAt(position, position + Vector3.Right, Vector3.Up);
            shadowMapInfo[0].cubeFace = CubeMapFace.PositiveX;
            shadowMapInfo[0].direction = Vector3.Right;
            shadowMapInfo[0].up = Vector3.Up;

            shadowMapInfo[1].view = Matrix.CreateLookAt(position, position + Vector3.Left, Vector3.Up);
            shadowMapInfo[1].cubeFace = CubeMapFace.NegativeX;
            shadowMapInfo[1].direction = Vector3.Left;
            shadowMapInfo[1].up = Vector3.Up;

            shadowMapInfo[2].view = Matrix.CreateLookAt(position, position + Vector3.Up, Vector3.Backward);
            shadowMapInfo[2].cubeFace = CubeMapFace.PositiveY;
            shadowMapInfo[2].direction = Vector3.Up;
            shadowMapInfo[2].up = Vector3.Backward;

            shadowMapInfo[3].view = Matrix.CreateLookAt(position, position + Vector3.Down, Vector3.Forward);
            shadowMapInfo[3].cubeFace = CubeMapFace.NegativeY;
            shadowMapInfo[3].direction = Vector3.Down;
            shadowMapInfo[3].up = Vector3.Forward;

            shadowMapInfo[4].view = Matrix.CreateLookAt(position, position + Vector3.Forward, Vector3.Up);
            shadowMapInfo[4].cubeFace = CubeMapFace.PositiveZ;
            shadowMapInfo[4].direction = Vector3.Forward;
            shadowMapInfo[4].up = Vector3.Up;

            shadowMapInfo[5].view = Matrix.CreateLookAt(position, position + Vector3.Backward, Vector3.Up);
            shadowMapInfo[5].cubeFace = CubeMapFace.NegativeZ;
            shadowMapInfo[5].direction = Vector3.Backward;
            shadowMapInfo[5].up = Vector3.Up;

            for (int i = 0; i < 6; ++i)
            {
                shadowMapInfo[i].frustum = new BoundingFrustum(shadowMapInfo[i].view * shadowMapInfo[i].projection);
            }
        }

        public override LightType getLightType()
        {
            return LightType.Point;
        }

        public override float getRange()
        {
            return radius;
        }

        public override BoundingBox getBoundingBox()
        {
            return boundingBox;
        }
    }
}
