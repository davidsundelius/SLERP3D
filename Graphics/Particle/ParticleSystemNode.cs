using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace RacingGame.Graphics
{
    /// <summary>
    /// Particle effect to make effects like water and explosion
    /// Author: David Sundelius
    /// </summary>
    class ParticleSystemNode : Node
    {
        private List<Particle> particles;
        private List<Emitter> emitters;
        private List<Magnet> magnets;
        private Vector3 gravity;

        public ParticleSystemNode(int nrOfParticles, ContentManager content, GraphicsDevice device)
        {
            //Maybe read particle effect from script file
            particles = new List<Particle>();
            emitters = new List<Emitter>();
            magnets = new List<Magnet>();
            gravity = Vector3.Zero;
        }

        public override bool update(GameTime time)
        {
            /*foreach(Emitter e in emitters) {
                e.update();
            }
            foreach (Particle p in particles)
            {
                CullMode cm = m_Device.RenderState.CullMode;
            m_Device.RenderState.CullMode = CullMode.None;
            m_Device.RenderState.AlphaBlendEnable = true;
            m_Device.RenderState.BlendFunction = BlendFunction.Add;
            m_Device.RenderState.DestinationBlend = Blend.One;
            m_Device.RenderState.DepthBufferWriteEnable = false;
            m_Device.RenderState.SourceBlend = Blend.SourceAlpha;

            m_Device.VertexDeclaration = m_VertexDecl;

            m_Effect.Parameters["Texture0"].SetValue(m_ParticleTexture);
            m_Effect.Parameters["view"].SetValue(view);
            m_Effect.Parameters["proj"].SetValue(proj);

            m_Effect.Begin();

            foreach (EffectPass p in m_Effect.Techniques["ParticleCPU"].Passes)
            {
                p.Begin();

                m_Device.DrawUserPrimitives(PrimitiveType.TriangleList, m_Vertices, 0, m_NumActiveParticles * 2);

                p.End();
            }

            m_Effect.End();
            m_Device.RenderState.CullMode = cm;

            m_Device.RenderState.DepthBufferWriteEnable = true;
            m_Device.RenderState.AlphaBlendEnable = false;
                p.update();
            }*/
            return particles.Count == 0;
        }

        public override RenderQueues getRenderQueue()
        {
            return RenderQueues.ParticleSystem;
        }

        public override void render()
        {
            
        }

        #region Helper classes
        /// <summary>
        /// A magnet point is used to apply force onto all particles
        /// onto a certain point
        /// </summary>
        class Magnet
        {
            Vector3 pos;
            float gravity;

            public Magnet(Vector3 pos, float gravity)
            {
                this.pos = pos;
                this.gravity = gravity;
            }

            public Vector3 getForce(Vector3 relPos)
            {
                return new Vector3(1, 1, 1);
                //Calculate the affected bodys force according to magnet
            }
        }
        /// <summary>
        /// Emitters emits new particles into the effect
        /// </summary>
        class Emitter
        {
            Vector3 pos;

            public Emitter()
            {
                pos = Vector3.Zero;
            }

            public bool update()
            {
                //Maybe emit particle
                return false;
            }
        }
        /// <summary>
        /// A particle is each element in the effect
        /// </summary>
        class Particle
        {
            protected Vector3 pos;
            protected Vector3 speed;
            protected float life;
            protected Vector4 color;

            public Particle()
            {
                pos = Vector3.Zero;
                speed = Vector3.Zero;
                life = 1.0f;
                color = Vector4.One;
            }

            public void render()
            {
                //Render the particle
            }

            public bool update()
            {
                //Apply magnet force
                //Apply gravity
                //Modify position according to speed
                pos += speed;
                return (life<=0.0f);
            }

            public void applyForce(Vector3 force)
            {
                //Modify the speedvector according to given force
            }
        }
        #endregion
    }
}
