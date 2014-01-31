using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RacingGame.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RacingGame.Map
{
    class FloatingLight : Logic.IUpdateable
    {
        private ModelNode model;
        private LightNode light;
        private LightNode castLight;
        private Vector3 position;
        private Vector3 direction;
        private ParticleSystemBase pSys;
        private float time = 0.0f;

        public FloatingLight(Scene scene, LightNode light, ParticleSystemBase particleSys)
        {
            model = new ModelNode(RacingGame.contentManager.Load<Model>("Models/Scenery/flyingLight"));
            this.light = light;
            model.position = light.position;
            model.rotation = Quaternion.CreateFromYawPitchRoll(-MathHelper.PiOver2, 0.0f, 0.0f);
            scene.addNode(model);
            scene.addNode(light);

            PointLight pl = new PointLight();
            pl.castsShadows = false;
            pl.diffuse = Vector3.One * 200.0f;
            pl.setRadius(0.5f);
            pl.position = light.position;
            castLight = new LightNode(pl);
            castLight.position = light.position;
            scene.addNode(castLight);

            position = light.position;
            direction = light.getLight().direction;

            pSys = particleSys;
        }

        public bool update(GameTime gt)
        {
            model.position = position + Vector3.Up * (float)Math.Sin(time / 2.0f) * 0.9f + Vector3.Backward * (float)Math.Cos(time / 2.0f) * 0.9f;
            light.position = model.position;
            castLight.position = model.position - Vector3.Down * 0.01f;
            time += (float)gt.ElapsedGameTime.TotalSeconds;

            Vector4 color = new Vector4(0.9f, 0.15f, 0.07f, 0.6f);
            for (int i = 0; i < 3; ++i)
            {
                pSys.AddParticle(new Particle(model.position + Vector3.Forward * 0.6f)
                    {
                        Velocity = Vector3.Down * 2.0f,
                        Color = color
                    });
                pSys.AddParticle(new Particle(model.position - Vector3.Forward * 0.6f)
                    {
                        Velocity = Vector3.Down * 2.0f,
                        Color = color
                    });
            }

            return false;
        }
    }
}
