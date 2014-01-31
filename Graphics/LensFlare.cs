#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;
using RacingGame.Graphics;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace RacingGame.Graphics
{
    public class LensFlare : IDisposable
    {
        private static Graphics.GraphicsManager graphicsManager = Graphics.GraphicsManager.getInstance();
        private static GraphicsDeviceManager graphics = Graphics.GraphicsManager.getGraphics();
        Vector3 cameraPos;
        private static SpriteBatch alphaSprite;
        private static SpriteBatch additiveSprite;

        private BasicEffect basicEffect;

        VertexPositionColor[] queryVertices;

        // How big a rectangle should we examine when issuing our occlusion queries?
        // Increasing this makes the flares fade out more gradually when the sun goes
        // behind scenery, while smaller query areas cause sudden on/off transitions.
        private float querySize;

        // An occlusion query is used to detect when the sun is hidden behind scenery.
        OcclusionQuery occlusionQuery;
        bool occlusionQueryActive;
        float occlusionAlpha;

        #region Variables
        /// <summary>
        /// Default sun position!
        /// </summary>
        public static Vector3 DefaultSunPos =
            new Vector3(0, 300, +15000);

        /// <summary>
        /// Put light at a little different location because we want
        /// to see the sun, but still have the light come more from the top.
        /// </summary>
        public static Vector3 DefaultLightPos =
            //new Vector3(+5500, -9250, +15000);
            new Vector3(+8500, -7250, +15000);

        /// <summary>
        /// Returns rotated sun position for the game.
        /// </summary>
        /// <param name="rotation">Rotation</param>
        /// <returns>Vector 3</returns>
        public static Vector3 RotateSun(float rotation)
        {
            Vector3 sunPos = DefaultSunPos;
            Vector2 right = new Vector2(
                +(float)Math.Cos(rotation),
                +(float)Math.Sin(rotation));
            Vector2 up = new Vector2(
                +(float)Math.Sin(rotation),
                -(float)Math.Cos(rotation));

            // Only rotate x and z
            return new Vector3(
                -right.X * sunPos.X - up.X * sunPos.Z,
                sunPos.Y,
                -right.Y * sunPos.X - up.Y * sunPos.Z);
        }

        /// <summary>
        /// Lens flare origin in 3D.
        /// </summary>
        private static Vector3 lensOrigin3D;

        /// <summary>
        /// Screen flare size (resolution dependant, 250 is for 1024*768).
        /// </summary>
        private static int ScreenFlareSize = 250;

        /// <summary>
        /// Flare texture types.
        /// </summary>
        protected const int
            SunFlareType = 0,
            GlowFlareType = 1,
            LensFlareType = 2,
            StreaksType = 3,
            RingType = 4,
            HaloType = 5,
            CircleType = 6,
            NumberOfFlareTypes = 7;

        /// <summary>
        /// Flare textures
        /// </summary>
        protected Texture2D[] flareTextures =
            new Texture2D[NumberOfFlareTypes];

        /// <summary>
        /// Flare texture names
        /// </summary>
        string[] flareTextureNames = new string[]
            {
                "Sun",
                "Glow",
                "Lens",
                "Streaks",
                "Ring",
                "Halo",
                "Circle",
            };

        /// <summary>
        /// Load textures, should be called in constructor.
        /// </summary>
        private void LoadTextures()
        {
            for (int num = 0; num < NumberOfFlareTypes; num++)
                flareTextures[num] = RacingGame.contentManager.Load<Texture2D>("Textures//LensFlare//" + flareTextureNames[num]);
        }
        #endregion

        #region Flare data struct
        /// <summary>
        /// Flare data struct for the quick and easy flare type list below.
        /// </summary>
        protected struct FlareData
        {
            /// <summary>
            /// Type of flare, see above.
            /// </summary>
            public int type;
            /// <summary>
            /// Position of flare (1=origin, 0=center of screen, -1=other side)
            /// </summary>
            public float position;
            /// <summary>
            /// Scale of flare in relation to MaxFlareSize.
            /// </summary>
            public float scale;
            /// <summary>
            /// Color of this flare.
            /// </summary>
            public Color color;

            /// <summary>
            /// Constructor to set all values.
            /// </summary>
            /// <param name="setType">Set type</param>
            /// <param name="setPosition">Set position</param>
            /// <param name="setScale">Set scale</param>
            /// <param name="setColor">Set Color</param>
            public FlareData(int setType, float setPosition,
                float setScale, Color setColor)
            {
                type = setType;
                position = setPosition;
                scale = setScale;
                color = setColor;
            }
        }

        /// <summary>
        /// Flare types for the lens flares.
        /// </summary>
        protected FlareData[] flareTypes = new FlareData[]
        {
            // Small red/yellow/gray halo behind sun
            new FlareData(
            CircleType, 1.2f, 0.55f, new Color(175, 175, 255, 20)),

            // The sun, sun+streaks+glow+red ring
            //small sun:
            //new FlareData(
            //SunFlareType, 1.0f, 0.6f, new Color(255, 255, 255, 255)),
            //new FlareData(
            //StreaksType, 1.0f, 1.5f, new Color(255, 255, 255, 128)),
            //new FlareData(
            //GlowFlareType, 1.0f, 1.7f, new Color(255, 255, 200, 100)),
            //bigger sun and much bigger glow effect:
            new FlareData(
            SunFlareType, 1.0f, 0.9f, new Color(255, 255, 255, 255)),
            new FlareData(
            StreaksType, 1.0f, 1.8f, new Color(255, 255, 255, 128)),
            new FlareData(
            StreaksType, 1.0f, 1.8f, new Color(255, 255, 255, 128)),
            new FlareData(
            GlowFlareType, 1.0f, 2.6f, new Color(255, 255, 225, 20)),
            //new FlareData(
            new FlareData(
            GlowFlareType, 1.0f, 2.6f, new Color(0, 0, 200, 20)),
            new FlareData(
            GlowFlareType, 1.0f, 1.3f, new Color(0, 0, 100, 40)),
            //new FlareData(
            //RingNum, 1.0f, 0.9f, new Color(255, 120, 120, 150)),

            // 3 blue circles at 0.5 distance
            new FlareData(
            CircleType, 0.5f, 0.12f, new Color(60, 60, 180, 35)),
            new FlareData(
            CircleType, 0.45f, 0.46f, new Color(100, 100, 200, 60)),
            new FlareData(
            CircleType, 0.4f, 0.17f, new Color(120, 120, 220, 40)),

            new FlareData(
            RingType, 0.15f, 0.2f, new Color(60, 60, 255, 100)),
            new FlareData(
            LensFlareType, -0.5f, 0.2f, new Color(255, 60, 60, 130)),
            new FlareData(
            LensFlareType, -0.15f, 0.15f, new Color(255, 60, 60, 90)),
            new FlareData(
            HaloType, -0.3f, 0.6f, new Color(60, 60, 255, 180)),
            
            // 3 red halos and circles on the opposite side of the blue halos
            new FlareData(
            HaloType, -0.4f, 0.2f, new Color(220, 80, 80, 98)),
            //new FlareData(
            //HaloNum, -0.45f, 0.6f, new Color(220, 80, 80, 95)),
            new FlareData(
            CircleType, -0.5f, 0.1f, new Color(220, 80, 80, 85)),

            new FlareData(
            HaloType, -0.6f, 0.5f, new Color(60, 60, 255, 80)),
            new FlareData(
            RingType, -0.8f, 0.3f, new Color(90, 60, 255, 110)),

            new FlareData(
            HaloType, -0.95f, 0.5f, new Color(60, 60, 255, 120)),
            new FlareData(
            CircleType, -1.0f, 0.15f, new Color(60, 60, 255, 85)),
        }; // flareTypes[]
        #endregion

        #region Properties
        /// <summary>
        /// Origin 3D
        /// </summary>
        /// <returns>Vector3</returns>
        public static Vector3 Origin3D
        {
            set
            {
                lensOrigin3D = value;
            }
            get
            {
                return lensOrigin3D;
            }
        }
        #endregion

        #region Helpers

        private void determineScreenFlareSize()
        {
            switch (Properties.Settings.Default.Height)
            {
                case 1024:
                    ScreenFlareSize = 200;
                    break;
                case 768:
                    ScreenFlareSize = 250;
                    break;
                case 600:
                    ScreenFlareSize = 300;
                    break;
                case 720:
                    ScreenFlareSize = 225;
                    break;
                case 900:
                    ScreenFlareSize = 200;
                    break;
                case 1080:
                    ScreenFlareSize = 150;
                    break;
            }
        }

        #endregion

        #region Constructors
        /// <summary>
        /// Create lens flare base
        /// </summary>
        public LensFlare(Vector3 setLensOrigin3D, Vector3 cameraPos, int querySize)
        {
            if (alphaSprite == null)
                alphaSprite = new SpriteBatch(graphics.GraphicsDevice);

            if (additiveSprite == null)
                additiveSprite = new SpriteBatch(graphics.GraphicsDevice);

            lensOrigin3D = setLensOrigin3D;
            this.cameraPos = cameraPos;
            this.querySize = querySize;
            LoadTextures();
            
            determineScreenFlareSize();

            // Effect and vertex declaration for drawing occlusion query polygons.
            basicEffect = new BasicEffect(graphics.GraphicsDevice, null);

            basicEffect.View = Matrix.Identity;
            basicEffect.VertexColorEnabled = true;

            // Create vertex data for the occlusion query polygons.
            queryVertices = new VertexPositionColor[4];

            queryVertices[0].Position = new Vector3(-querySize / 2, -querySize / 2, -1);
            queryVertices[1].Position = new Vector3(querySize / 2, -querySize / 2, -1);
            queryVertices[2].Position = new Vector3(querySize / 2, querySize / 2, -1);
            queryVertices[3].Position = new Vector3(-querySize / 2, querySize / 2, -1);

            // Create the occlusion query object.
            occlusionQuery = new OcclusionQuery(graphics.GraphicsDevice);
        }
        #endregion

        #region Dispose
        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing">Disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                for (int num = 0; num < NumberOfFlareTypes; num++)
                    if (flareTextures[num] != null)
                        flareTextures[num].Dispose();
            }
        }
        #endregion

        #region Render
        /// <summary>
        /// Current sun intensity used for our lens flare effect.
        /// </summary>
        float sunIntensity = 0.0f;

        public void setCameraPosForLensFlare(Vector3 cameraPos)
        {
            this.cameraPos = cameraPos;
        }

        /// <summary>
        /// Render lens flare.
        /// Checks the zbuffer if objects are occluding the lensflare sun.
        /// </summary>
        public void Render(Color sunColor)
        {
            determineScreenFlareSize();
            ScreenFlareSize = ScreenFlareSize * (int)graphicsManager.getResolution().X / 1024;
            Vector3 relativeLensPos = lensOrigin3D + cameraPos;

            // Only show lens flare if facing in the right direction
            if (GraphicsHelper.IsInFrontOfCamera(relativeLensPos) == false)
                return;

            // Convert 3D point to 2D!
            Point lensOrigin =
                GraphicsHelper.Convert3DPointTo2D(relativeLensPos);

            Vector2 lightPosition = new Vector2(lensOrigin.X,
                                                lensOrigin.Y);

            // Check whether the light is hidden behind the scenery.
            UpdateOcclusion(lightPosition);

            // If it is not visible, do not draw the flare effect.
            if (!(occlusionAlpha > 0))
            {
                return;
            }

            // Check sun occlusion itensity and fade it in and out!
            float thisSunIntensity = 0.75f;

            sunIntensity = thisSunIntensity * 0.1f + sunIntensity * 0.9f;

            // We can skip rendering the sun if the itensity is to low
            if (sunIntensity < 0.01f)
                return;

            int resWidth = (int)graphicsManager.getResolution().X,
                resHeight = (int)graphicsManager.getResolution().Y;
            Point center = new Point(resWidth / 2, resHeight / 2);
            Point relOrigin = new Point(
                center.X - lensOrigin.X, center.Y - lensOrigin.Y);

            // Check if origin is on screen, fade out at borders
            float alpha = 1.0f;
            float distance = Math.Abs(Math.Max(relOrigin.X, relOrigin.Y));
            if (distance > resHeight / 1.75f)
            {
                distance -= resHeight / 1.75f;
                // If distance is more than half the resolution, don't show anything!
                if (distance > resHeight / 1.75f)
                    return;
                alpha = 1.0f - (distance / ((float)resHeight / 1.75f));
                if (alpha > 1.0f)
                    alpha = 1.0f;
            }

            // Use square of sunIntensity for lens flares because we want
            // them to get very weak if sun is not fully visible.
            alpha *= sunIntensity * sunIntensity;


            additiveSprite.Begin(SpriteBlendMode.Additive, SpriteSortMode.BackToFront, SaveStateMode.SaveState);
            alphaSprite.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.BackToFront, SaveStateMode.SaveState);

            foreach (FlareData data in flareTypes)
            {
                int size = (int)(ScreenFlareSize * data.scale);
                RenderOnScreen(flareTextures[data.type],
                    new Rectangle(
                    (int)(center.X - relOrigin.X * data.position - size / 2),
                    (int)(center.Y - relOrigin.Y * data.position - size / 2),
                    size, size),
                    new Rectangle(0, 0, flareTextures[data.type].Width, flareTextures[data.type].Height),
                    GraphicsHelper.ApplyAlphaToColor(//MixAlphaToColor
                    GraphicsHelper.MultiplyColors(sunColor, data.color),
                    ((float)data.color.A / 255.0f) *
                    // For the sun and glow flares try always to use max. intensity
                    (data.type == SunFlareType || data.type == GlowFlareType ?
                    sunIntensity : alpha)),
                    SpriteBlendMode.Additive);
            }
            additiveSprite.End();
            alphaSprite.End();
        }
        #endregion

        public void RenderOnScreen(Texture2D texture, Rectangle rect, Rectangle pixelRect,
        Color color, SpriteBlendMode blendMode)
        {
            if (blendMode == SpriteBlendMode.Additive)
                additiveSprite.Draw(texture, rect, pixelRect, color);
            else
                alphaSprite.Draw(texture, rect, pixelRect, color);
            //SpriteHelper.AddSpriteToRender(this, rect, pixelRect, color, blendMode);
        }

        void UpdateOcclusion(Vector2 lightPosition)
        {
            // Give up if the current graphics card does not support occlusion queries.
            if (!occlusionQuery.IsSupported)
                return;

            if (occlusionQueryActive)
            {
                // If the previous query has not yet completed, wait until it does.
                if (!occlusionQuery.IsComplete)
                    return;

                // Use the occlusion query pixel count to work
                // out what percentage of the sun is visible.
                float queryArea = querySize * querySize;

                occlusionAlpha = Math.Min(occlusionQuery.PixelCount / queryArea, 1);
            }

            // Set renderstates for drawing the occlusion query geometry. We want depth
            // tests enabled, but depth writes disabled, and we set ColorWriteChannels
            // to None to prevent this query polygon actually showing up on the screen.
            RenderState renderState = graphics.GraphicsDevice.RenderState;

            renderState.DepthBufferEnable = true;
            renderState.DepthBufferWriteEnable = false;
            renderState.AlphaTestEnable = false;
            renderState.ColorWriteChannels = ColorWriteChannels.None;

            // Set up our BasicEffect to center on the current 2D light position.
            Viewport viewport = graphics.GraphicsDevice.Viewport;

            basicEffect.World = Matrix.CreateTranslation(lightPosition.X,
                                                         lightPosition.Y, 0);

            basicEffect.Projection = Matrix.CreateOrthographicOffCenter(0,
                                                                        viewport.Width,
                                                                        viewport.Height,
                                                                        0, 0, 1);

            basicEffect.Begin();
            basicEffect.CurrentTechnique.Passes[0].Begin();

            // Issue the occlusion query.
            occlusionQuery.Begin();

            graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleFan,
                                              queryVertices, 0, 2);

            occlusionQuery.End();

            basicEffect.CurrentTechnique.Passes[0].End();
            basicEffect.End();

            // Reset renderstates.
            renderState.ColorWriteChannels = ColorWriteChannels.All;
            renderState.DepthBufferWriteEnable = true;

            occlusionQueryActive = true;
        }
    }
}
