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
    //Not used, yet.
    public enum ParticleType
    {
        PARTICLE_TYPE_NORMAL = 0,
        PARTICLE_TYPE_FIRE
    }

    public struct Particle
    {/*
        public Particle()
        {
            Time = 0.0f;
            LifeTime = 0.5f;
            Dead = false;
            Velocity = new Vector3(0.0f, 0.0f, 0.0f);
            initAlpha = 1.0f;
            DeltaSize = 0.0f;
            m_Position = Vector3.Zero;
            m_Size = 0.2f;

            Pos = new Vector3(0.0f, 0.0f, 0.0f);

            InitVerts();

        }*/

        public Particle(Vector3 pos)
        {
            Time = 0.0f;
            LifeTime = 0.5f;
            Dead = false;
            Velocity = new Vector3(0.0f, 0.0f, 0.0f);
            initAlpha = 1.0f;
            DeltaSize = 0.0f;
            m_Position = Vector3.Zero;
            m_Size = 0.2f;
            Vertices = new ParticleVertex[6];

            Pos = pos;

            InitVerts();
        }


        public float Time;
        public float LifeTime;
        public bool Dead;
        public Vector3 Velocity;
        public float initAlpha;
        public float DeltaSize;
        private Vector3 m_Position;
        private float m_Size;

        public Vector3 Pos
        {
            get { return m_Position; }
            set
            {
                m_Position = value;
                Vertices[0].Pos = value;
                Vertices[1].Pos = value;
                Vertices[2].Pos = value;

                Vertices[3].Pos = value;
                Vertices[4].Pos = value;
                Vertices[5].Pos = value;
            }
        }

        public float Size
        {
            get { return m_Size; }
            set
            {
                m_Size = value;
                Vertices[0].Offs.X = -m_Size;
                Vertices[0].Offs.Y = m_Size;
                Vertices[1].Offs.X = m_Size;
                Vertices[1].Offs.Y = m_Size;
                Vertices[2].Offs.X = -m_Size;
                Vertices[2].Offs.Y = -m_Size;

                Vertices[3].Offs.X = -m_Size;
                Vertices[3].Offs.Y = -m_Size;
                Vertices[4].Offs.X = m_Size;
                Vertices[4].Offs.Y = m_Size;
                Vertices[5].Offs.X = m_Size;
                Vertices[5].Offs.Y = -m_Size;
            }
        }

        public Vector4 Color
        {
            set
            {
                Vertices[0].Color = value;
                Vertices[1].Color = value;
                Vertices[2].Color = value;

                Vertices[3].Color = value;
                Vertices[4].Color = value;
                Vertices[5].Color = value;
            }
            get
            {
                return Vertices[0].Color;
            }
        }

        private void InitVerts()
        {
            Vertices[0].UV = new Vector2(0.0f, 1.0f);
            Vertices[1].UV = new Vector2(1.0f, 1.0f);
            Vertices[2].UV = new Vector2(0.0f, 0.0f);

            Vertices[3].UV = new Vector2(0.0f, 0.0f);
            Vertices[4].UV = new Vector2(1.0f, 1.0f);
            Vertices[5].UV = new Vector2(1.0f, 0.0f);

            Vertices[0].Offs = new Vector2(-0.2f, 0.2f);
            Vertices[1].Offs = new Vector2(0.2f, 0.2f);
            Vertices[2].Offs = new Vector2(-0.2f, -0.2f);

            Vertices[3].Offs = new Vector2(-0.2f, -0.2f);
            Vertices[4].Offs = new Vector2(0.2f, 0.2f);
            Vertices[5].Offs = new Vector2(0.2f, -0.2f);

        }

        public ParticleVertex[] Vertices;
    }

    public struct ParticleVertex
    {
        public ParticleVertex(Vector3 pos)
        {
            Pos = pos;
            Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            UV = new Vector2(0.0f, 0.0f);
            Offs = new Vector2(-0.5f, -0.5f);
        }

        public Vector3 Pos;
        public Vector4 Color;
        public Vector2 UV;
        public Vector2 Offs;

        public static int SizeInBytes = sizeof(float) * 4;

        public static readonly VertexElement[] VertexElements =
            {
            new VertexElement(0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0),
            new VertexElement(0, sizeof(float) * 3, VertexElementFormat.Vector4, VertexElementMethod.Default, VertexElementUsage.Color, 0),
            new VertexElement(0, sizeof(float) * 7, VertexElementFormat.Vector2, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(0, sizeof(float) * 9, VertexElementFormat.Vector2, VertexElementMethod.Default, VertexElementUsage.Position, 1),
            };
    }

    abstract class ParticleSystemBase
    {
        public Vector3 Gravity = new Vector3(0.0f, -0.2f, 0.0f);

        protected Particle[] m_Particles;
        protected List<Particle> m_NewParticles = new List<Particle>();
        protected Effect m_Effect;
        protected Texture2D m_ParticleTexture;
        protected int m_NumActiveParticles = 0;
        protected ContentManager Content;
        protected VertexDeclaration m_VertexDecl;
        protected GraphicsDevice m_Device;
        protected int m_MaxParticles;
        protected int m_NumParticles = 0;


        public ParticleSystemBase(int maxParticles, ContentManager cMgr, GraphicsDevice device)
        {
            Content = cMgr;
            m_Device = device;

            m_MaxParticles = maxParticles;
            m_Effect = Content.Load<Effect>("Shaders/particle");
            m_ParticleTexture = Content.Load<Texture2D>("Textures/particle");
            m_VertexDecl = new VertexDeclaration(device, ParticleVertex.VertexElements);
            m_Particles = new Particle[maxParticles];
        }

        public void AddParticle(Particle particle)
        {
            if (m_NewParticles.Count > 10000)
            {
                m_NewParticles.Clear();
            }
            m_NewParticles.Add(particle);
        }

        protected bool InsertParticle(Particle particle)
        {
            if ((m_NumParticles + 2) >= m_MaxParticles)
            {
                return false;
            }
            m_Particles[m_NumParticles++] = particle;

            return true;
        }

        public abstract void Update(float deltaTime);

        public abstract void Draw(Matrix view, Matrix proj);




    }

}