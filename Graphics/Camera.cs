using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RacingGame.Logic;

namespace RacingGame.Graphics
{
    /// <summary>
    /// An instance of this object is able to control the camera
    /// Author: Daniel Lindén
    /// </summary>
    class Camera : Logic.IUpdateable
    {
        private Node target;
        private Vector3 offset;
        private Vector3 position;
        private Vector3 lookAt;
        private Vector3 up = new Vector3(0.0f, 1.0f, 0.0f);
        private Vector3 lookAtOffset;

        private float fov = MathHelper.PiOver4;
        private float near = 0.1f;
        private float far = 1000.0f;
        private float aspect = 0.0f;

        private Matrix projection;
        private Matrix view;

        private BoundingFrustum frustum;

        public Camera(Node followTarget, Vector3 offset)
        {
            target = followTarget;
            this.offset = offset;

            frustum = new BoundingFrustum(Matrix.Identity);

            Matrix newPos = Matrix.Identity;
            newPos.Translation = offset;
            newPos = Matrix.CreateFromQuaternion(target.rotation) * newPos;
            newPos.Translation += target.position;

            position = newPos.Translation;
            lookAt = target.position;
            lookAtOffset = Vector3.Zero;

            setProjection(fov, near, far);
        }

        public Camera(Node followTarget, Vector3 offset, Vector3 lookAtOffset)
        {
            target = followTarget;
            this.offset = offset;
            this.lookAtOffset = lookAtOffset;

            frustum = new BoundingFrustum(Matrix.Identity);

            Matrix newPos = Matrix.Identity;
            newPos.Translation = offset;
            newPos = Matrix.CreateFromQuaternion(target.rotation) * newPos;
            newPos.Translation += target.position;

            position = newPos.Translation;
            lookAt = target.position + lookAtOffset;

            setProjection(fov, near, far);
        }

        public bool update(GameTime time)
        {
            Vector3 newPos = Vector3.Transform(offset, target.rotation);
            Vector3 newLAO = Vector3.Transform(lookAtOffset, target.rotation);
            newPos += target.position + newLAO;

            if (lookAtOffset == Vector3.Zero)
            {
                position = Vector3.SmoothStep(position, newPos, 0.4f);
            }
            else
            {
                position = newPos;
            }
            Vector3 diffVec = position - target.position;
            float offsetLength = offset.Length();
            position = MathHelper.Clamp(diffVec.Length(), offsetLength, offsetLength * 2) * Vector3.Normalize(diffVec) + target.position;
            lookAt = target.position + newLAO;

            view = Matrix.CreateLookAt(position, lookAt, up);
            frustum.Matrix = view * projection;

            return false;
        }
        #region Getters and setters

        public float getFov()
        {
            return fov;
        }

        public float getAspectRatio()
        {
            return aspect;
        }

        public Vector2 getZoomValues() 
        {
            Vector2 zoom = new Vector2();
            zoom.X = 1.0f / (float)Math.Tan(fov * GraphicsManager.getDevice().Viewport.AspectRatio / 2.0f);
            zoom.Y = 1.0f / (float)Math.Tan(fov / 2.0f);

            return zoom;
        }

        public Vector3 getDirection()
        {
            return Vector3.Normalize(lookAt - position);
        }

        public Vector3 getUp()
        {
            return up;
        }

        public void setProjection(float fieldOfView, float nearDistance, float farDistance)
        {
            fov = fieldOfView;
            near = nearDistance;
            far = farDistance;

            aspect = GraphicsManager.getDevice().Viewport.AspectRatio;
            projection = Matrix.CreatePerspectiveFieldOfView(fov, aspect, near, far);
            frustum.Matrix = view * projection;
        }

        public BoundingFrustum getFrustum()
        {
            return frustum;
        }

        public void setUpVector(Vector3 up)
        {
            this.up = up;
        }

        public Vector3 getPosition()
        {
            return position;
        }

        public void setTarget(Node target)
        {
            this.target = target;
        }

        public void setOffset(Vector3 offset)
        {
            this.offset = offset;
        }

        public Matrix getViewMatrix()
        {
            return view;
        }

        public Matrix getProjectionMatrix()
        {
            return projection;
        }
        public float getFarDistance()
        {
            return far;
        }
        #endregion

    }
}
