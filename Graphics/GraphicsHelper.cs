using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;
using RacingGame.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace RacingGame.Graphics
{
    public class GraphicsHelper
    {
        private static Graphics.GraphicsManager graphicsManager = Graphics.GraphicsManager.getInstance();
        private static GraphicsDeviceManager graphics = Graphics.GraphicsManager.getGraphics();

        public static Point Convert3DPointTo2D(Vector3 point)
        {
            Vector4 result4 = Vector4.Transform(point,
                GraphicsManager.getInstance().view * GraphicsManager.getInstance().proj);

            if (result4.W == 0)
                result4.W = float.Epsilon;
            Vector3 result = new Vector3(
                result4.X / result4.W,
                result4.Y / result4.W,
                result4.Z / result4.W);

            if (result4.Z < 0.0f)
            {
                return new Point(-1000, -1000);
            }

            // Output result from 3D to 2D
            int i = (int)Math.Round(+result.X * ((int)graphicsManager.getResolution().X / 2)) + ((int)graphicsManager.getResolution().X / 2);
            int p = (int)Math.Round(-result.Y * ((int)graphicsManager.getResolution().Y / 2)) + ((int)graphicsManager.getResolution().Y / 2);
            return new Point(
                (int)Math.Round(+result.X * ((int)graphicsManager.getResolution().X / 2)) + ((int)graphicsManager.getResolution().X / 2),
                (int)Math.Round(-result.Y * ((int)graphicsManager.getResolution().Y / 2)) + ((int)graphicsManager.getResolution().Y / 2));
        }
        public static bool IsInFrontOfCamera(Vector3 point)
        {
            Vector4 result = Vector4.Transform(
                new Vector4(point.X, point.Y, point.Z, 1),
                GraphicsManager.getInstance().view * GraphicsManager.getInstance().proj);

            // Is result in front?
            return result.Z > result.W - 0.5f; //nearplane
        }
        public static Color MultiplyColors(Color color1, Color color2)
        {
            // Quick check if any of the colors is white,
            // multiplying won't do anything then.
            if (color1 == Color.White)
                return color2;
            if (color2 == Color.White)
                return color1;

            // Get values from color1
            float redValue1 = color1.R / 255.0f;
            float greenValue1 = color1.G / 255.0f;
            float blueValue1 = color1.B / 255.0f;
            float alphaValue1 = color1.A / 255.0f;

            // Get values from color2
            float redValue2 = color2.R / 255.0f;
            float greenValue2 = color2.G / 255.0f;
            float blueValue2 = color2.B / 255.0f;
            float alphaValue2 = color2.A / 255.0f;

            // Multiply everything using our floats
            return new Color(
                (byte)(StayInRange(redValue1 * redValue2, 0, 1) * 255.0f),
                (byte)(StayInRange(greenValue1 * greenValue2, 0, 1) * 255.0f),
                (byte)(StayInRange(blueValue1 * blueValue2, 0, 1) * 255.0f),
                (byte)(StayInRange(alphaValue1 * alphaValue2, 0, 1) * 255.0f));
        }

        public static float StayInRange(float val, float min, float max)
        {
            if (val < min)
                return min;
            if (val > max)
                return max;
            return val;
        }

        public static Color ApplyAlphaToColor(Color col, float newAlpha)
        {
            if (newAlpha < 0)
                newAlpha = 0;
            if (newAlpha > 1)
                newAlpha = 1;
            return new Color(
                (byte)(col.R),
                (byte)(col.G),
                (byte)(col.B),
                (byte)(newAlpha * 255.0f));
        }
    }
}
