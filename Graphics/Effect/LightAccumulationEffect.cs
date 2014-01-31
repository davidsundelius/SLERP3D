using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RacingGame.Graphics
{
    /// <summary>
    /// The effect used to create the light accumulation  buffer.
    /// Author: Daniel Lindén
    /// </summary>
    class LightAccumulationEffect : IEffect
    {
        private Effect effect;
        private EffectPass currentPass;
        private EffectTechnique lightingTechnique;
        private EffectPass projectorPass;
        private EffectPass projectorShadowPass;
        private EffectPass pointPass;
        private EffectPass spotPass;
        private EffectPass spotShadowPass;
        private bool isSet = false;

        private Matrix view;
        private Matrix proj;

        #region Effect parameters
        private EffectParameter worldViewProj;
        private EffectParameter worldView;
        private EffectParameter world;

        private EffectParameter lightViewProj;
        private EffectParameter range;
        private EffectParameter lightAngle;

        private EffectParameter lightPos;
        private EffectParameter lightDir;
        private EffectParameter lightSpecular;
        private EffectParameter lightColor;

        private EffectParameter cameraPos;
        private EffectParameter farClipDistance;

        private EffectParameter resolution;

        private EffectParameter depthBuffer;
        private EffectParameter normalSpecularBuffer;
        private EffectParameter shadowMap;
        private EffectParameter decalTexture;
        #endregion

        public LightAccumulationEffect()
        {
            effect = RacingGame.contentManager.Load<Effect>("Shaders/lightAccumulation");
            lightingTechnique = effect.Techniques["LightAccumulation"];
            projectorPass = lightingTechnique.Passes["projector"];
            projectorShadowPass = lightingTechnique.Passes["projectorShadow"];

            pointPass = lightingTechnique.Passes["point"];

            spotPass = lightingTechnique.Passes["spot"];
            spotShadowPass = lightingTechnique.Passes["spotShadow"];

            worldViewProj = effect.Parameters["WorldViewProj"];
            worldView = effect.Parameters["WorldView"];
            world = effect.Parameters["World"];

            lightViewProj = effect.Parameters["LightViewProj"];
            range = effect.Parameters["Range"];
            lightAngle = effect.Parameters["LightAngle"];

            lightPos = effect.Parameters["LightPos"];
            lightDir = effect.Parameters["LightDir"];
            lightSpecular = effect.Parameters["LightSpecular"];
            lightColor = effect.Parameters["LightColor"];
            cameraPos = effect.Parameters["CameraPos"];

            resolution = effect.Parameters["InvResolution"];

            depthBuffer = effect.Parameters["DepthBuffer"];
            normalSpecularBuffer = effect.Parameters["NormalSpecularBuffer"];
            shadowMap = effect.Parameters["ShadowMap"];

            decalTexture = effect.Parameters["DecalTexture"];

            farClipDistance = effect.Parameters["FarClipDistance"];

        }

        public void begin(string technique, string pass)
        {
            throw new NotSupportedException();
        }

        public void begin()
        {
            resolution.SetValue(new Vector2(1.0f / GraphicsManager.getDevice().Viewport.Width, 1.0f / GraphicsManager.getDevice().Viewport.Height));
            isSet = true;

            effect.Begin();
        }

        public void setLightType(LightType type, bool useShadow)
        {
            if (!isSet)
            {
                throw new InvalidOperationException();
            }
            EffectPass pass = null;

            if (type == LightType.Projected)
            {
                if (useShadow)
                {
                    pass = projectorShadowPass;
                }
                else
                {
                    pass = projectorPass;
                }
            }
            else if (type == LightType.Point)
            {
                pass = pointPass;
  
            }
            else if (type == LightType.Spot)
            {
                if (useShadow)
                {
                    pass = spotShadowPass;
                }
                else
                {
                    pass = spotPass;
                }
            }

            if (pass == null)
            {
                effect.End();
            }


            if (pass != currentPass)
            {
                if (currentPass != null)
                {
                    currentPass.End();
                }

                pass.Begin();
                currentPass = pass;
                
            }
        }

        public void end()
        {
            if (currentPass != null)
            {
                currentPass.End();
                currentPass = null;
            }

            if (isSet)
            {
                effect.End();
                isSet = false;
            }
        }

        public void setWorldMatrix(Matrix world)
        {
            this.world.SetValue(world);
            Matrix worldView = world * view;
            this.worldView.SetValue(worldView);
            this.worldViewProj.SetValue(worldView * proj);
        }

        public void setDecalTexture(Texture texture)
        {
            
        }

        public void setDepthBuffer(Texture depthBuf)
        {
            depthBuffer.SetValue(depthBuf);
        }

        public void setNormalSpecularBuffer(Texture buffer)
        {
            normalSpecularBuffer.SetValue(buffer);
        }

        public void setShadowMap(Texture shadowMap)
        {
            this.shadowMap.SetValue(shadowMap);
        }

        public void setLight(Light l)
        {
            lightPos.SetValue(l.position);
            lightColor.SetValue(l.diffuse);
            lightSpecular.SetValue(l.specularFactor);
            if (l.GetType() == typeof(ProjectedLight))
            {
                ShadowMapInfo info = l.getShadowMaps()[0];
                lightViewProj.SetValue(info.view * info.projection);
            }
            else if (l.getLightType() == LightType.Spot)
            {
                lightDir.SetValue(l.direction);
                SpotLight light = (SpotLight)l;
                lightAngle.SetValue(light.lightAngle);
                ShadowMapInfo info = l.getShadowMaps()[0];
                lightViewProj.SetValue(info.view * info.projection);
            }

            range.SetValue(l.getRange());

            decalTexture.SetValue(l.texture);
        }

        public void setCamera(Camera c)
        {
            view = c.getViewMatrix();
            proj = c.getProjectionMatrix();
            cameraPos.SetValue(c.getPosition());
            farClipDistance.SetValue(c.getFarDistance());
        }


        public void commit()
        {
            effect.CommitChanges();
        }

    }
}
