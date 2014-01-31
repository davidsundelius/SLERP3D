using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;

namespace RacingGame.Graphics
{
   
    class ParticleSystemCPU : ParticleSystemBase
    {
        private ParticleVertex[] m_Vertices;
        private float delta;
        

        public ParticleSystemCPU(int maxParticles, ContentManager cMgr, GraphicsDevice device) : base(maxParticles, cMgr, device)
        {
            m_Vertices = new ParticleVertex[maxParticles * 6];


        }

        private void UpdateThread1()
        {
            /*LinkedListNode<Particle> i = m_Particles.First;

            while (i != null)
            {
                UpdateParticle(i.Value, delta);

                i = i.Next;

                if (i != null)
                {
                    i = i.Next;
                }
            }

            thread1Done = true;*/
        }

        private void UpdateThread2()
        {
            /*LinkedListNode<Particle> i = m_Particles.First;

            if (i != null)
            {
                i = i.Next;
            }

            while (i != null)
            {
                UpdateParticle(i.Value, delta);

                i = i.Next;

                if (i != null)
                {
                    i = i.Next;
                }
            }

            thread2Done = true;
             * */
        
        }

        private void UpdateOneThread()
        {

            for (int i = 0; i < m_NumParticles;)
            {
                UpdateParticle(i, delta);
                DisplayParticle(i);

                if (m_Particles[i].Dead)
                {
                    if (i != m_NumParticles - 1)
                    {
                        m_Particles[i] = m_Particles[m_NumParticles - 1];
                    }

                    m_NumParticles--;
                    
                }
                ++i;
            }

            foreach (Particle p in m_NewParticles)
            {
                if (InsertParticle(p))
                {
                    int particleIndex = ++m_NumActiveParticles;
                    for (int j = 0; j < 6; ++j)
                    {
                        m_Vertices[particleIndex * 6 + j] = p.Vertices[j];
                    }
                    UpdateParticle(m_NumParticles - 1, delta);
                }
            }

            m_NewParticles.Clear();

        }

        private void DisplayParticle(int i)
        {
            int particleIndex = ++m_NumActiveParticles;
            for (int j = 0; j < 6; ++j)
            {
                m_Vertices[particleIndex * 6 + j] = m_Particles[i].Vertices[j];
            }
        }

        private void UpdateParticle(int i, float deltaTime)
        {
            m_Particles[i].Time += deltaTime;

            if (m_Particles[i].Time >= m_Particles[i].LifeTime)
            {
                m_Particles[i].Dead = true;
                return;
            }

            m_Particles[i].Velocity = m_Particles[i].Velocity + Gravity * deltaTime;
            m_Particles[i].Pos = m_Particles[i].Pos + m_Particles[i].Velocity * deltaTime;
            m_Particles[i].Size = m_Particles[i].Size + m_Particles[i].DeltaSize * deltaTime;

            Vector4 color = m_Particles[i].Color;
            color.W = MathHelper.Lerp(m_Particles[i].initAlpha, 0.0f, m_Particles[i].Time / m_Particles[i].LifeTime);
            m_Particles[i].Color = color;
        }

        public override void Update(float deltaTime)
        {
            m_NumActiveParticles = 0;

            delta = deltaTime;


            /*UpdateThread1();
            UpdateThread2();*/

            UpdateOneThread();
            

            /*thread1 = new Thread(new ThreadStart(UpdateOneThread));
            thread1.Start();*/

            /*thread2 = new Thread(new ThreadStart(UpdateThread2));
            clearThread = new Thread(new ThreadStart(ClearThread));

            thread1Done = false;
            thread2Done = false;

            thread1.Start();
            thread2.Start();

            while (!thread1Done && !thread2Done)
            {
            }

            clearThread.Start();*/

            /*LinkedListNode<Particle> i = m_Particles.First;

            while (i != null)
            {

                UpdateParticle(i.Value, deltaTime);

                if (i.Value.Dead)
                {
                    LinkedListNode<Particle> next = i.Next;
                    m_Particles.Remove(i);
                    i = next;
                    continue;
                }

                i = i.Next;
            }*/
        }

        public override void Draw(Matrix view, Matrix proj)
        {

           /* while (!thread1Done)
            {
            }*/

            if (m_NumActiveParticles == 0)
            {
                return;
            }

            //m_Device.Clear(Color.Black);

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

        }




    }

}