using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace RacingGame.Graphics
{
    /// <summary>
    /// The HUD class is used to draw the heads up display
    /// to present data to the user on the game screen.
    /// Author: David Sundelius
    /// </summary>
    class HUD
    {
        private Graphics.GraphicsManager graphicsManager;
        private SpriteBatch spriteBatch;

        private SpriteFont hudFont = RacingGame.contentManager.Load<SpriteFont>("Fonts//HUD");
        private SpriteFont timeFont = RacingGame.contentManager.Load<SpriteFont>("Fonts//HUDtime");
        private SpriteFont laptimeFont = RacingGame.contentManager.Load<SpriteFont>("Fonts//HUDlaptime");
        private SpriteFont messageFont = RacingGame.contentManager.Load<SpriteFont>("Fonts//HUDmessage");
        private Texture2D texIndicator = RacingGame.contentManager.Load<Texture2D>("Textures//HUD//testHUD");
        private Texture2D texVel = RacingGame.contentManager.Load<Texture2D>("Textures//Asphalt");
        private Texture2D hudBackground = RacingGame.contentManager.Load<Texture2D>("Textures//HUD//hudBackground");
        private Texture2D hudBackgroundLeft = RacingGame.contentManager.Load<Texture2D>("Textures//HUD//hudBackgroundLeft");
        private Texture2D boost = RacingGame.contentManager.Load<Texture2D>("Textures//HUD//boost");
        private Texture2D boostFront = RacingGame.contentManager.Load<Texture2D>("Textures//HUD//boostFront");
        private Texture2D power = RacingGame.contentManager.Load<Texture2D>("Textures//HUD//power");
        private Texture2D powerFront = RacingGame.contentManager.Load<Texture2D>("Textures//HUD//powerFront");
        private Texture2D speed = RacingGame.contentManager.Load<Texture2D>("Textures//HUD//speed");
        private Texture2D speedFront = RacingGame.contentManager.Load<Texture2D>("Textures//HUD//speedFront");
        private Texture2D powerBackHud = RacingGame.contentManager.Load<Texture2D>("Textures//HUD//powerBackHud");
        private Texture2D speedBackHud = RacingGame.contentManager.Load<Texture2D>("Textures//HUD//speedBackHud");
        private Texture2D boostBackHud = RacingGame.contentManager.Load<Texture2D>("Textures//HUD//boostBackHud");
        private Texture2D pwrupBackHud = RacingGame.contentManager.Load<Texture2D>("Textures//HUD//pwrupBackHud");
        private Texture2D transBar = RacingGame.contentManager.Load<Texture2D>("Textures//HUD//transBar");

        private Rectangle hudBackPlateSpeed;
        private Rectangle hudSpeed;
        private Rectangle hudSpeedFront;
        private Rectangle hudBackPlateBoost;
        private Rectangle hudBoost;
        private Rectangle hudBoostFront;
        private Rectangle hudBackPlatePower;
        private Rectangle hudPower;
        private Rectangle hudPowerFront;
        private Rectangle hudBackPlatePWRUP;

        private Rectangle speedBarRect;
        private Rectangle boostBarRect;
        private Rectangle powerBarRect;

        private Rectangle timeRect;
        private Rectangle powerupRect;
        private Rectangle indicatorsRect;
        private Rectangle velocityRect;

        private States.Game game;

        private String message="";
        private float alpha=0.0f;
        private bool alphaUp = true;

        //boost
        private float boostFlash = 0;
        private int glowAlpha = 0;
        private bool alphaGlowUp = true;

        //power
        private float oldHP = 1f;
        private bool powerSlide = false;
        private float inprogOld = 1f;
        private float inprogNew = 1f;
        private float powerFlashState = 0;
        private float powerFlashIncrease = 0.04f;

        private Color textColor = new Color(180, 180, 180);
        private Color outlineColor = new Color(60, 60, 60);//new Color(100,255,100);
        private Color messageColor;

        public HUD(States.Game game)
        {
            graphicsManager = Graphics.GraphicsManager.getInstance();
            spriteBatch = new SpriteBatch(Graphics.GraphicsManager.getGraphics().GraphicsDevice);
            timeRect = new Rectangle(0, 0, 200, 100);
            powerupRect = new Rectangle((int)(graphicsManager.getResolution().X / 2 - 75), 100, 150, 150);
            messageColor = textColor;
            this.game = game;
        }

        public void render()
        {
 
            spriteBatch.Begin();

            string time = getTimeString(game.time);

            string lastLapTime = "";
            string bestLapTime = "";
            if (game.Player.lap != 1)
            {
                lastLapTime = getTimeString(game.Player.lapTimes[game.Player.lap - 1]);
                bestLapTime = getTimeString(game.Player.bestLap);
            }
            
            string laps = "Laps: " + game.Player.lap + "/" + game.nrOfLaps;
            //spriteBatch.Draw(testHUD, timeRect, Color.White);
            //spriteBatch.Draw(testHUD, powerupRect, Color.White);
            
            //indicatorsRect = new Rectangle((int)(graphicsManager.getResolution().X - 400), (int)(graphicsManager.getResolution().Y - 100), 300, 25);

            //right side
            hudBackPlateSpeed = new Rectangle((int)(graphicsManager.getResolution().X - 336 - 30), (int)(graphicsManager.getResolution().Y - 90), 336, 58);
            hudSpeed = new Rectangle(hudBackPlateSpeed.Left + 70, hudBackPlateSpeed.Top + 10, (int)((game.Player.ship.Velocity.Length() / game.Player.ship.MaxSpeed) * 280), 40);
            hudSpeedFront = new Rectangle(hudSpeed.X, hudSpeed.Y, 200, hudSpeed.Height);

            hudBackPlateBoost = new Rectangle(hudBackPlateSpeed.Left + 20, hudBackPlateSpeed.Top - hudBackPlateSpeed.Height - 30, hudBackPlateSpeed.Width, hudBackPlateSpeed.Height);
            hudBoost = new Rectangle(hudSpeed.Left, hudBackPlateSpeed.Top - (hudBackPlateSpeed.Top - hudBackPlateBoost.Top) + 10, hudSpeed.Width, hudSpeed.Height);
            hudBoostFront = new Rectangle(hudBoost.X, hudBoost.Y, hudSpeedFront.Width, hudBoost.Height);

            //left side
            hudBackPlatePower = new Rectangle(0 + 30, hudBackPlateSpeed.Top, hudBackPlateSpeed.Width, hudBackPlateSpeed.Height);
            hudPower = new Rectangle(70 + 30, hudBackPlateSpeed.Top + 10, hudSpeed.Width, hudSpeed.Height);
            hudPowerFront = new Rectangle(hudPower.X, hudPower.Y, hudSpeedFront.Width, hudSpeed.Height);

            hudBackPlatePWRUP = new Rectangle(0 + 10, hudBackPlateSpeed.Top - hudBackPlateSpeed.Height - 30, hudBackPlateSpeed.Width, hudBackPlateSpeed.Height);


            float speedColorAlpha = (game.Player.ship.Velocity.Length() / (game.Player.ship.MaxSpeed - 200));
            float speedfloat = game.Player.ship.Velocity.Length();
            speedBarRect = new Rectangle(hudBackPlateSpeed.Left + (int) (((hudBackPlateSpeed.Right - hudBackPlateSpeed.Left) * 0.6) * (game.Player.ship.Velocity.Length() / ((game.Player.ship.MaxSpeed) - 130))), hudBackPlateSpeed.Top, hudBackPlateSpeed.Width, hudBackPlateSpeed.Height);
            //spriteBatch.Draw(hudBackground, hudBackPlateSpeed, Color.White);
            spriteBatch.Draw(speedBackHud, hudBackPlateSpeed, Color.White);
            spriteBatch.Draw(speedBackHud, hudBackPlateSpeed, new Color(1, 1, 0, speedColorAlpha));
            spriteBatch.Draw(transBar, speedBarRect, Color.Yellow);
            //spriteBatch.Draw(speed, hudSpeed, Color.White);
            //spriteBatch.Draw(speedFront, hudSpeedFront, Color.White);

            bool boostColor = game.Player.ship.Boost >= boostFlash;
            boostFlash = game.Player.ship.Boost;
            boostBarRect = new Rectangle(hudBackPlateBoost.Left + (int)(((hudBackPlateBoost.Right - hudBackPlateBoost.Left) * 0.6) * (game.Player.ship.Boost)), hudBackPlateBoost.Top, hudBackPlateBoost.Width, hudBackPlateBoost.Height);
            //spriteBatch.Draw(hudBackground, hudBackPlateBoost, Color.White);
            spriteBatch.Draw(boostBackHud, hudBackPlateBoost, Color.White);
            spriteBatch.Draw(boostBackHud, hudBackPlateBoost, boostColor ? Color.White : new Color(0, 0, 255, (byte)MathHelper.Clamp(glowAlpha, 0, 255)));
            spriteBatch.Draw(transBar, boostBarRect, Color.Blue);
            //spriteBatch.Draw(boost, hudBoost, Color.White;
            //spriteBatch.Draw(boostFront, hudBoostFront, Color.White);

            
            if (powerSlide)
                {
                    if (inprogOld != inprogNew)
                    {
                        //continue powerslide
                        inprogOld = MathHelper.SmoothStep(inprogOld, inprogNew, powerFlashState);
                        powerFlashState += powerFlashIncrease;
                    }
                    else
                    {
                        //reset powerslide
                        powerSlide = false;
                        powerFlashState = 0;
                    }
                }
            else
            {
                if (oldHP != game.Player.ship.Health)
                {
                    //start powerslide
                    inprogNew = game.Player.ship.Health;
                    inprogOld = oldHP;
                    powerSlide = true;
                }
                oldHP = game.Player.ship.Health;
            }

            
            
            powerBarRect = new Rectangle(hudBackPlatePower.Left + (int)(((hudBackPlatePower.Right - hudBackPlatePower.Left) * 0.6) * (game.Player.ship.Health)), hudBackPlatePower.Top, hudBackPlatePower.Width, hudBackPlatePower.Height);
            //spriteBatch.Draw(hudBackgroundLeft, hudBackPlatePower, Color.White);
            spriteBatch.Draw(powerBackHud, hudBackPlatePower, Color.White);
            spriteBatch.Draw(powerBackHud, hudBackPlatePower,  powerSlide ? new Color(255, 0, 0, (byte)MathHelper.Clamp(glowAlpha, 0, 255)) : Color.White);
            spriteBatch.Draw(transBar, powerBarRect, Color.Red);
            //spriteBatch.Draw(power, hudPower, Color.White);
            //spriteBatch.Draw(powerFront, hudPowerFront, Color.White);

            bool inUse;
            if(game.Player.ship.powerup!=null)
                inUse = game.Player.ship.powerup.inUse();
            else
                inUse = false;

            spriteBatch.Draw(pwrupBackHud, hudBackPlatePWRUP, Color.White);
            spriteBatch.Draw(pwrupBackHud, hudBackPlatePWRUP, inUse ? new Color(119, 34, 136, (byte)MathHelper.Clamp(glowAlpha, 0, 255)) : Color.White);

            if (alphaGlowUp)
                if (glowAlpha <= 255)
                    glowAlpha += 92;
                else
                    alphaGlowUp = false;
            else
                if (glowAlpha >= 0)
                    glowAlpha -= 92;
                else
                    alphaGlowUp = true;

            string powerup = "-";
            if (game.Player.ship.powerup != null)
                powerup = game.Player.ship.powerup.ToString();
            drawOutlinedString(powerup, new Vector2(hudBackPlatePWRUP.Center.X + laptimeFont.MeasureString(powerup).X, hudBackPlatePWRUP.Center.Y + 30), textColor, laptimeFont);

            drawOutlinedString(laps, new Vector2((int)(graphicsManager.getResolution().X / 2 + timeFont.MeasureString(laps).X / 2.2), (int)(graphicsManager.getResolution().Y) - 120), textColor, hudFont);
            //float ressy = graphicsManager.getResolution().X / 2;
            drawOutlinedString(time, new Vector2((int)(graphicsManager.getResolution().X / 2 + timeFont.MeasureString(time).X / 3.5), (int)((graphicsManager.getResolution().Y) - 80)), textColor, timeFont);
            drawOutlinedString(lastLapTime, new Vector2((int)(graphicsManager.getResolution().X / 2 + laptimeFont.MeasureString(lastLapTime).X * 0.9), (int)(graphicsManager.getResolution().Y) - 35), textColor, laptimeFont);
            drawOutlinedString(bestLapTime, new Vector2((int)(graphicsManager.getResolution().X / 2 + laptimeFont.MeasureString(bestLapTime).X * 0.9), (int)(graphicsManager.getResolution().Y) - 35 + 20), Color.Gold, laptimeFont);
            spriteBatch.End();

            renderMessage();
        }

        public void renderDebug(String debug)
        {
            spriteBatch.Begin();
            drawOutlinedString(debug, new Vector2((int)(graphicsManager.getResolution().X), 50), textColor, hudFont);
            spriteBatch.End();
        }

        public void renderMessage()
        {
            spriteBatch.Begin();
            if (!message.Equals(""))
            {
                if (alphaUp)
                    if (alpha <= 0.99f)
                        alpha += 0.01f;
                    else
                    {
                        alphaUp = false;
                        alpha = 1.0f;
                    }
                else
                    if (alpha >= 0.01f)
                        alpha -= 0.01f;
                    else
                    {
                        alphaUp = true;
                        alpha = 0.0f;
                    }

                //Draw text
                messageColor.A = (byte)(alpha * 255);
                if (message.Length > 5)
                    drawOutlinedString(message, new Vector2((int)(graphicsManager.getResolution().X / 2 + hudFont.MeasureString(message).X / 2), (int)(graphicsManager.getResolution().Y / 2 + hudFont.MeasureString(message).Y / 2) - 200), messageColor, hudFont);
                else
                    drawOutlinedString(message, new Vector2((int)(graphicsManager.getResolution().X / 2), (int)(graphicsManager.getResolution().Y / 2 + messageFont.MeasureString(message).Y / 2) - 200), messageColor, messageFont);
            }
            spriteBatch.End();
        }

        public void clearMessage()
        {
            message = "";
        }

        public void showMessage(String message)
        {
            alpha = 0.99f;
            this.message = message;
        }

        public void drawOutlinedString(String text, Vector2 pos, Color color, SpriteFont font)
        {
            outlineColor.A = color.A;
            spriteBatch.DrawString(font, text, new Vector2(pos.X - 1, pos.Y), outlineColor, 0, hudFont.MeasureString(text), 1.0f, SpriteEffects.None, 0.5f);
            spriteBatch.DrawString(font, text, new Vector2(pos.X + 1, pos.Y), outlineColor, 0, hudFont.MeasureString(text), 1.0f, SpriteEffects.None, 0.5f);
            spriteBatch.DrawString(font, text, new Vector2(pos.X, pos.Y - 1), outlineColor, 0, hudFont.MeasureString(text), 1.0f, SpriteEffects.None, 0.5f);
            spriteBatch.DrawString(font, text, new Vector2(pos.X, pos.Y + 1), outlineColor, 0, hudFont.MeasureString(text), 1.0f, SpriteEffects.None, 0.5f);

            spriteBatch.DrawString(font, text, pos, color, 0, hudFont.MeasureString(text), 1.0f, SpriteEffects.None, 0.5f);
        }

        private string getTimeString(int time) {
            return String.Format("{0:00}", ((time / 1000) / 60)) + ":" + String.Format("{0:00}", ((time / 1000) % 60)) + ":" + String.Format("{0:000}", (time % 1000));
        }
    }
}
