using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RacingGame.Graphics
{
    /// <summary>
    /// A projected light is a type of light that looks like a frustum. Possibly could 
    /// have a intensity texture in the future.
    /// Author: Daniel Lindén
    /// </summary>
    class ProjectedLight : Light
    {
        private Mesh graphicsCube;
        ShadowMapInfo[] shadowMapInfo = new ShadowMapInfo[1];
        private float range;
        private BoundingBox boundingBox;

        public override Vector3 position
        {
            get
            {
                return base.position;
            }
            set
            {
                base.position = value;
                updateView();
            }
        }

        public ProjectedLight()
        {
            shadowMapInfo[0] = new ShadowMapInfo();
            setProjection(MathHelper.PiOver2, 1.0f, 1.0f, 100.0f);
            base.position = Vector3.Zero;
            base.direction = Vector3.Forward;
            updateView();
        }

        public ProjectedLight(Light l, ShadowMapInfo smInfo)
        {
            shadowMapInfo[0] = smInfo;
            graphicsCube = Mesh.createCube(new BoundingFrustum(smInfo.projection));
            base.position = smInfo.position;
            base.direction = smInfo.direction;
            base.diffuse = l.diffuse;
            base.specularFactor = l.specularFactor;
            base.up = smInfo.up;
            range = l.getRange();
        }

        public override ShadowMapInfo[] getShadowMaps()
        {
            return shadowMapInfo;
        }

        public override void setDirection(Vector3 direction, Vector3 up)
        {
            base.setDirection(direction, up);
            updateView();
        }

        public override void render()
        {
            GraphicsManager.getInstance().setWorldMatrix(Matrix.CreateWorld(position, direction, up));
            GraphicsManager.getInstance().updateShader();
            graphicsCube.render();
        }

        public override bool update(GameTime delta)
        {
            return false;
        }

        private void updateView()
        {
            shadowMapInfo[0].view = Matrix.CreateLookAt(position, position + direction, up);
            shadowMapInfo[0].frustum.Matrix = shadowMapInfo[0].view * shadowMapInfo[0].projection;
            boundingBox = BoundingBox.CreateFromPoints(shadowMapInfo[0].frustum.GetCorners());
        }

        public void setProjection(float fieldOfViewY, float aspect, float near, float far)
        {
            range = far - near;
            shadowMapInfo[0].projection = Matrix.CreatePerspectiveFieldOfView(fieldOfViewY, aspect, near, far);
            graphicsCube = Mesh.createCube(new BoundingFrustum(shadowMapInfo[0].projection));
            shadowMapInfo[0].frustum.Matrix = shadowMapInfo[0].view * shadowMapInfo[0].projection;
            boundingBox = BoundingBox.CreateFromPoints(shadowMapInfo[0].frustum.GetCorners());
        }

        public override bool contains(Vector3 point)
        {
            return shadowMapInfo[0].frustum.Contains(point) != ContainmentType.Disjoint;
        }

        public override LightType getLightType()
        {
            return LightType.Projected;
        }

        public override float getRange()
        {
            return range;
        }

        public override BoundingBox getBoundingBox()
        {
            return boundingBox;
        }
    }
}
