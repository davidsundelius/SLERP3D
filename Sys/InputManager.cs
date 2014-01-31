using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace RacingGame.Sys
{
    /// <summary>
    /// A singleton used to control the input flow from user
    /// Author: David Sundelius & Magnus Olausson
    /// </summary>
    class InputManager
    {
        // The singleton
        private static InputManager instance = new InputManager();

        private static List<Keys> keysPressedLastFrame = new List<Keys>();

        public static MouseState mouseState, mouseStateLastFrame;
        public static float mouseXMovement, mouseYMovement;
        public static float lastMouseXMovement, lastMouseYMovement;
        public static bool mouseDetected = false;
        private static Graphics.GraphicsManager graphicsManager = Graphics.GraphicsManager.getInstance();

        // Get the singleton
        public static InputManager getInstance()
        {
            return instance;
        }

        private InputManager()
        {
        }

        private bool isKeyDown(Keys k)
        {
            KeyboardState kbs = Keyboard.GetState();
            return kbs.IsKeyDown(k);
        }

        #region Key matches
        public bool isThrottle()
        {
            return isKeyDown(Properties.Controls.Default.Throttle);
        }

        public bool isTurnLeft()
        {
            return isKeyDown(Properties.Controls.Default.TurnLeft);
        }

        public bool isTurnRight()
        {
            return isKeyDown(Properties.Controls.Default.TurnRight);
        }

        public bool isStrafeLeft()
        {
            return isKeyDown(Properties.Controls.Default.StrafeLeft);
        }

        public bool isStrafeRight()
        {
            return isKeyDown(Properties.Controls.Default.StrafeRight);
        }

        public bool isLeftBrake()
        {
            return isKeyDown(Properties.Controls.Default.LeftBrake);
        }

        public bool isRightBrake()
        {
            return isKeyDown(Properties.Controls.Default.RightBrake);
        }

        public bool isFireWeapon()
        {
            return isKeyDown(Properties.Controls.Default.FireWeapon);
        }
        public bool isDiscardWeapon()
        {
            return isKeyDown(Properties.Controls.Default.DiscardWeapon);
        }
        public bool isBoost()
        {
            return isKeyDown(Properties.Controls.Default.Boost);
        }

        public bool isReverse()
        {
            return isKeyDown(Properties.Controls.Default.Reverse);
        }

        public bool isJump()
        {
            return isKeyDown(Properties.Controls.Default.Jump);
        }

        //Methods for controlling static set keys
#if DEBUG
        public bool isDebug()
        {
            return isKeyDown(Keys.Insert);
        }
#endif
        public bool isPause()
        {
            return isKeyDown(Keys.Pause);
        }

        public bool isEscapeDown()
        {
            return isKeyDown(Keys.Escape);
        }

        public bool isAnyKeyDown()
        {
            KeyboardState kbs = Keyboard.GetState();
            return (kbs.GetPressedKeys().Length > 0);
        }
        #endregion

        #region Menu Mouse Input

        public static bool MouseDetected
        {
            get
            {
                return mouseDetected;
            }
        }

        public static Point mousePos
        {
            get
            {
                return new Point(mouseState.X, mouseState.Y);
            }
        }

        public static float MouseXMovement
        {
            get
            {
                return mouseXMovement;
            }
        }

        public static float MouseYMovement
        {
            get
            {
                return mouseYMovement;
            }
        }

        public static bool hasMouseMoved
        {
            get
            {
                if (mouseXMovement > 1 || mouseYMovement > 1)
                    return true;
                return false;
            }
        }

        public static bool mouseLeftButtonPressed
        {
            get
            {
                return mouseState.LeftButton == ButtonState.Pressed;
            }
        }

        public static bool mouseLeftButtonJustPressed
        {
            get
            {
                return mouseState.LeftButton == ButtonState.Pressed &&
                       mouseStateLastFrame.LeftButton == ButtonState.Released;
            }
        }

        public static bool mouseInBox(Rectangle rect)
        {
            bool ret = mouseState.X >= rect.X &&
                       mouseState.Y >= rect.Y &&
                       mouseState.X < rect.Right &&
                       mouseState.Y < rect.Bottom;
            bool lastRet = mouseStateLastFrame.X >= rect.X &&
                           mouseStateLastFrame.Y >= rect.Y &&
                           mouseStateLastFrame.X < rect.Right &&
                           mouseStateLastFrame.Y < rect.Bottom;

            // Highlight happend?
            if (ret && lastRet == false)
            {
                //Sound.SoundManager.getInstance().playSound("Crash");
            }
            return ret;
        }
        #endregion

        #region Menu Keyboard Input
        public bool keyboardKeyJustPressed(Keys key)
        {
            return isKeyDown(key) &&
                keysPressedLastFrame.Contains(key) == false;
        }

        public bool keyboardSpaceJustPressed()
        {
            return isKeyDown(Keys.Space) &&
                keysPressedLastFrame.Contains(Keys.Space) == false;
        }

        public bool keyboardEnterJustPressed()
        {
            return isKeyDown(Keys.Enter) &&
                keysPressedLastFrame.Contains(Keys.Enter) == false;
        }

        public bool keyboardUpJustPressed()
        {
            return isKeyDown(Keys.Up) &&
                keysPressedLastFrame.Contains(Keys.Up) == false;
        }

        public bool keyboardDownJustPressed()
        {
            return isKeyDown(Keys.Down) &&
                keysPressedLastFrame.Contains(Keys.Down) == false;
        }

        public void keysPressedReset(Keys[] pressedKeys)
        {
            keysPressedLastFrame = new List<Keys>(pressedKeys);
        }
        #endregion
    }
}
