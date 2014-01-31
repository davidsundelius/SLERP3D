using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;

namespace RacingGame.Graphics
{
    class ParticleSystemGPU : ParticleSystemBase
    {
        private RenderTarget2D[] m_PosTime;
        private RenderTarget2D[] m_VelLife;
        private RenderTarget2D[] m_TexAliveSize;

        private int m_Width;
        private int m_Height;


        ParticleSystemGPU(int maxParticles, ContentManager cMgr, GraphicsDevice device)
            : base(maxParticles, cMgr, device)
        {
            m_Width = 1;
            m_Height = 1;

            while (m_Width * m_Height < m_MaxParticles)
            {
                if (m_Width >= m_Height)
                {
                    m_Width <<= 1;
                }
                else
                {
                    m_Height <<= 1;
                }
            }


            m_PosTime = new RenderTarget2D[2];
            m_PosTime[0] = new RenderTarget2D(m_Device, m_Width, m_Height, 1, SurfaceFormat.Vector4);
            m_PosTime[1] = new RenderTarget2D(m_Device, m_Width, m_Height, 1, SurfaceFormat.Vector4);

            m_VelLife = new RenderTarget2D[2];
            m_VelLife[0] = new RenderTarget2D(m_Device, m_Width, m_Height, 1, SurfaceFormat.Vector4);
            m_VelLife[1] = new RenderTarget2D(m_Device, m_Width, m_Height, 1, SurfaceFormat.Vector4);

            m_TexAliveSize = new RenderTarget2D[2];
            m_TexAliveSize[0] = new RenderTarget2D(m_Device, m_Width, m_Height, 1, SurfaceFormat.Vector4);
            m_TexAliveSize[1] = new RenderTarget2D(m_Device, m_Width, m_Height, 1, SurfaceFormat.Vector4);


        }

        public override void Update(float deltaTime)
        {
        }

        public override void Draw(Matrix view, Matrix proj)
        {
        }

    }
}
