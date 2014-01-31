using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using PIXTools;

namespace RacingGame.Graphics
{
    public enum RenderQueues
    {
        Normal,
        Scenery,
        Skybox,
        Shield,
        ParticleSystem,
        NumQueues = 5,
    };

    /// <summary>
    /// Takes input from the game and presents it onto the screen
    /// Author: Daniel Lindén
    /// </summary>
    class GraphicsManager
    {
        private GraphicsDeviceManager graphics;
        GraphicsDevice device;
        private ContentManager content;

        public Matrix view;
        public Matrix proj;

        private Vector2 resolution;

        private Camera camera;

        private int rTWidth, rTHeight;

        private MultiSampleType rtMS;

        private int lastResHeight;
        private int lastResWidth;

        private GraphicsDeviceCapabilities caps;

        #region Rendertargets
        private RenderTarget2D shadowMapRT;
        private RenderTarget2D normalSpecularRT;
        private RenderTarget2D lightAccumulationRT;
        private RenderTarget2D depthBufferRT;
        private RenderTarget2D screenBuffer;
        private RenderTarget2D ambientOcclusion;
        private RenderTarget2D intermediateBuffer;
        private DepthStencilBuffer depthStencil;
        private DepthStencilBuffer shadowDepthBuffer;
        #endregion

        #region Effects
        private IEffect mainEffect;
        private ShadingEffect shadingEffect;
        private GBufferEffect gbufferEffect;
        private ShadowMapEffect shadowMapEffect;
        private LightAccumulationEffect lightAccumulationEffect;

        private EmptyPostProcess emptyPostProcess;
        private BlurPostProcess blurPostProcess;
        private BloomPostProcess bloomPostProcess;
        private MotionBlurEffect motionBlurEffect;

        private SSAOEffect ssaoEffect;

        private IEffect currentEffect;
        #endregion

        Mesh screenQuad;

        //Objects and lights used for rendering the current frame.
        private RenderQueue[] renderQueues = new RenderQueue[(int)RenderQueues.NumQueues];
        private LinkedList<Light> lights = new LinkedList<Light>();

        private static GraphicsManager instance;

        public bool DemonMode
        {
            get
            {
                if (bloomPostProcess != null)
                    return bloomPostProcess.DemonMode;
                else
                    return false;
            }
            set
            {
                if (bloomPostProcess != null)
                    bloomPostProcess.DemonMode = value;
            }
        }

        public GraphicsManager(GraphicsDeviceManager graphics)
        {
            instance = this;
            this.graphics = graphics;
            device = graphics.GraphicsDevice;
        }

        public static GraphicsManager getInstance()
        {
            return instance;
        }

        public static GraphicsDeviceManager getGraphics()
        {
            return instance.graphics;
        }

        public static GraphicsDevice getDevice()
        {
            return instance.graphics.GraphicsDevice;
        }

        public void initialize()
        {
            Sys.Logger log = Sys.Logger.getInstance();
            log.print("Initializing graphics engine");

            resolution.X = Properties.Settings.Default.Width;
            resolution.Y = Properties.Settings.Default.Height;

            graphics.IsFullScreen = Properties.Settings.Default.Fullscreen;
            graphics.PreferredBackBufferWidth = Properties.Settings.Default.Width;
            graphics.PreferredBackBufferHeight = Properties.Settings.Default.Height;
            graphics.ApplyChanges();

            content = RacingGame.contentManager;
            mainEffect = new GenericEffect(content.Load<Effect>("Shaders/mainEffect"));
            shadingEffect = new ShadingEffect();
            lightAccumulationEffect = new LightAccumulationEffect();
            shadowMapEffect = new ShadowMapEffect();
            gbufferEffect = new GBufferEffect();
            emptyPostProcess = new EmptyPostProcess();
            blurPostProcess = new BlurPostProcess();
            ssaoEffect = new SSAOEffect();
            bloomPostProcess = new BloomPostProcess();
            motionBlurEffect = new MotionBlurEffect();
            currentEffect = mainEffect;

            screenQuad = Mesh.createFullScreenQuad();

            caps = GraphicsAdapter.DefaultAdapter.GetCapabilities(DeviceType.Hardware);

            int width = device.Viewport.Width;
            int height = device.Viewport.Height;

            depthStencil = device.DepthStencilBuffer;

            log.print("Creating render targets");
            createRenderTargets(width, height, false);

            for (int i = 0; i < (int)RenderQueues.NumQueues; ++i)
            {
                renderQueues[i] = new RenderQueue();
            }
            log.print("Render targets successfully created");
            
            log.print("Graphics engine successfully initialized");

        }

        public void createRenderTargets(int width, int height, bool multiSampleShadowMaps)
        {
            rTWidth = width;
            rTHeight = height;
            MultiSampleType shadowMS = MultiSampleType.None;
            if (multiSampleShadowMaps)
            {
                shadowMS = MultiSampleType.FourSamples;
            }

            rtMS = MultiSampleType.None;

            shadowMapRT = new RenderTarget2D(device, width, height, 1, SurfaceFormat.Vector2, shadowMS, 0);
            shadowDepthBuffer = new DepthStencilBuffer(device, depthStencil.Width, depthStencil.Height, DepthFormat.Depth24Stencil8, shadowMS, 0);
    
            normalSpecularRT = new RenderTarget2D(device, width, height, 1, SurfaceFormat.HalfVector4, rtMS, 0);
            lightAccumulationRT = new RenderTarget2D(device, width, height, 1, SurfaceFormat.HalfVector4, rtMS, 0, RenderTargetUsage.PreserveContents);

            if (caps.PrimitiveCapabilities.SupportsMultipleRenderTargetsIndependentBitDepths)
            {
                depthBufferRT = new RenderTarget2D(device, width, height, 1, SurfaceFormat.Vector4, rtMS, 0);
            }
            else
            {
                depthBufferRT = new RenderTarget2D(device, width, height, 1, SurfaceFormat.HalfVector4, rtMS, 0);
            }

            screenBuffer = new RenderTarget2D(device, width, height, 1, SurfaceFormat.HalfVector4, rtMS, 0);
            intermediateBuffer = new RenderTarget2D(device, width, height, 1, SurfaceFormat.HalfVector4);

            if (Properties.Settings.Default.UseSSAO)
            {
                setAmbientOcclusion();
            }

        }

        public void setAmbientOcclusion()
        {
            try
            {
                ambientOcclusion = new RenderTarget2D(device, rTWidth / 4, rTHeight / 4, 1, SurfaceFormat.HalfSingle, rtMS, 0);
            }
            catch (System.Exception)
            {
                ambientOcclusion = new RenderTarget2D(device, rTWidth / 4, rTHeight / 4, 1, SurfaceFormat.Single, rtMS, 0);
            }
        }

        public void render(Scene scene, Camera camera)
        {
            if ((States.Settings.updatedUI == true) && device.Viewport.Height != lastResHeight
                                                  && device.Viewport.Width != lastResWidth)
            {
                createRenderTargets(device.Viewport.Width, device.Viewport.Height, false);
                lastResHeight = device.Viewport.Height;
                lastResWidth = device.Viewport.Width;
                States.Settings.updatedUI = false;
            }

            this.camera = camera;

            for (int i = 0; i < (int)RenderQueues.NumQueues; ++i)
            {
                renderQueues[i].clear();
            }

            lights.Clear();

            view = camera.getViewMatrix();
            proj = camera.getProjectionMatrix();

            scene.render(camera.getFrustum());

            device.RenderState.DepthBufferEnable = true;
            device.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
            device.RenderState.AlphaBlendEnable = false;
            device.RenderState.AlphaTestEnable = false;
            

            device.RenderState.BlendFunction = BlendFunction.Add;
            device.RenderState.SourceBlend = Blend.One;
            device.RenderState.AlphaDestinationBlend = Blend.DestinationAlpha;
            device.RenderState.AlphaSourceBlend = Blend.SourceAlpha;
            device.RenderState.DestinationBlend = Blend.One;

            gBufferPass();
            lightAccumulationPass();

            if (Properties.Settings.Default.UseSSAO)
            {
                device.SetRenderTarget(0, ambientOcclusion);
                ssaoEffect.renderSSAO(depthBufferRT, normalSpecularRT.GetTexture());
                blurPostProcess.postProcess(ambientOcclusion, ambientOcclusion);
            }

            shadingPass();
            device.RenderState.DepthBufferWriteEnable = false;
            motionBlurEffect.postProcess(screenBuffer, intermediateBuffer, depthBufferRT.GetTexture());

            bloomPostProcess.postProcess(intermediateBuffer, null);
            //emptyPostProcess.postProcess(screenBuffer, null);
            device.RenderState.DepthBufferWriteEnable = true;
        }

        private void gBufferPass()
        {
            PIXTools.PIXTools.BeginEvent("G-buffer pass");

            device.DepthStencilBuffer = depthStencil;
            device.SetRenderTarget(0, normalSpecularRT);
            device.SetRenderTarget(1, depthBufferRT);
            device.Clear(ClearOptions.DepthBuffer | ClearOptions.Target, new Color(0.0f, 0.0f, 0.0f, 0.0f), 1.0f, 0);

            currentEffect = gbufferEffect;
            gbufferEffect.setCamera(camera);
            gbufferEffect.begin();

            renderQueues[(int)RenderQueues.Normal].render();

            gbufferEffect.end();

            //XNA seems to like to clear the depth stencil buffer randomly, so let's hide it!
            //And for the record; this actually fixed a bug I was having. In your face, XNA!
            //device.DepthStencilBuffer = hideTheDepthBuffer;
            //Update: This is actually a feature which can be removed by specifying RenderTargetUsage.PreserveContents on creation...

            device.SetRenderTarget(0, null);
            device.SetRenderTarget(1, null);

            PIXTools.PIXTools.EndEvent();
        }

        private void drawShadowLight(Light light)
        {
            device.DepthStencilBuffer = shadowDepthBuffer;
            PIXTools.PIXTools.BeginEvent("Shadow map generation");

            device.RenderState.DepthBufferFunction = CompareFunction.LessEqual;
            device.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
            device.RenderState.DepthBufferWriteEnable = true;
            device.RenderState.AlphaBlendEnable = false;
            
            shadowMapEffect.begin();
            currentEffect = shadowMapEffect;

            ShadowMapInfo smInfo = light.getShadowMaps()[0];
            shadowMapEffect.setShadowMapInfo(smInfo);
            device.SetRenderTarget(0, shadowMapRT);
            device.Clear(ClearOptions.DepthBuffer | ClearOptions.Target, Color.Black, 1.0f, 0);
            renderQueues[(int)RenderQueues.Normal].render();
            shadowMapEffect.end();

            //blurPostProcess.postProcess(shadowMapRT, shadowMapRT);

            device.SetRenderTarget(0, lightAccumulationRT);
            Texture shadowTexture = shadowMapRT.GetTexture();

            PIXTools.PIXTools.EndEvent();
            PIXTools.PIXTools.BeginEvent("Light accumulation");
                       
            device.RenderState.DepthBufferWriteEnable = false;
            device.RenderState.AlphaBlendEnable = true;

            device.RenderState.DepthBufferFunction = CompareFunction.Greater;
            device.RenderState.CullMode = CullMode.CullClockwiseFace;

            currentEffect = lightAccumulationEffect;

            //The coast should be clear, reenable the old depth stencil.
            device.DepthStencilBuffer = depthStencil;
            lightAccumulationEffect.begin();

            lightAccumulationEffect.setLightType(light.getLightType(), true);
            lightAccumulationEffect.setShadowMap(shadowTexture);

            lightAccumulationEffect.setLight(light);
            light.render();

            lightAccumulationEffect.end();

            PIXTools.PIXTools.EndEvent();

        }

        private void lightAccumulationPass()
        {
            PIXTools.PIXTools.BeginEvent("Light accumulation pass");

            device.SetRenderTarget(0, lightAccumulationRT);
            device.Clear(ClearOptions.Target, new Color(0.0f, 0.0f, 0.0f, 0.0f), 1.0f, 0);

            lightAccumulationEffect.setCamera(camera);
            lightAccumulationEffect.setDepthBuffer(depthBufferRT.GetTexture());
            lightAccumulationEffect.setNormalSpecularBuffer(normalSpecularRT.GetTexture());

            int numShadowMaps = (int)Properties.Settings.Default.NumberOfShadowMaps;

            PIXTools.PIXTools.BeginEvent("Shadowed lights");
            for (int i = 0; i < numShadowMaps; ++i)
            {
                Light shadowLight = getClosestLight();
                if (shadowLight == null)
                {
                    break;
                }

                if (shadowLight.getLightType() == LightType.Point)
                {
                    ShadowMapInfo[] smInfo = shadowLight.getShadowMaps();

                    foreach (ShadowMapInfo sm in smInfo)
                    {
                        drawShadowLight(new ProjectedLight(shadowLight, sm));
                    }
                }
                else
                {
                    drawShadowLight(shadowLight);
                }
                
            }
            PIXTools.PIXTools.EndEvent();
            PIXTools.PIXTools.BeginEvent("Other lights");
            device.RenderState.DepthBufferWriteEnable = false;
            device.RenderState.AlphaBlendEnable = true;

            device.DepthStencilBuffer = depthStencil;

            device.RenderState.DepthBufferFunction = CompareFunction.Greater;
            device.RenderState.CullMode = CullMode.CullClockwiseFace;

            currentEffect = lightAccumulationEffect;
            lightAccumulationEffect.begin();

            foreach (Light l in lights)
            {
                lightAccumulationEffect.setLightType(l.getLightType(), false);

                setLight(l);
                l.render();

            }

            lightAccumulationEffect.end();


            //Quickly hide the depth stencil buffer so XNA can't clear it
            //device.DepthStencilBuffer = hideTheDepthBuffer;

            lightAccumulationEffect.setDepthBuffer((Texture)null);
            lightAccumulationEffect.setNormalSpecularBuffer((Texture)null);

            device.RenderState.DepthBufferFunction = CompareFunction.LessEqual;
            device.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
            device.RenderState.AlphaBlendEnable = false;


            PIXTools.PIXTools.EndEvent();
            PIXTools.PIXTools.EndEvent();
        }

        private void shadingPass()
        {
            PIXTools.PIXTools.BeginEvent("Shading pass");

            device.SetRenderTarget(0, screenBuffer);

            device.Clear(new Color(0.0f, 0.0f, 0.0f, 0.0f));

            //And finally set back the depth stencil buffer, we don't need it after this pass anyway
            device.DepthStencilBuffer = depthStencil;
            
            device.RenderState.DepthBufferEnable = false;

            currentEffect = mainEffect;

            mainEffect.begin("Main", "SkyBox");
            mainEffect.setCamera(camera);

            device.RenderState.CullMode = CullMode.None;

            renderQueues[(int)RenderQueues.Skybox].render();
            mainEffect.end();


            device.RenderState.CullMode = CullMode.CullCounterClockwiseFace;

            device.RenderState.AlphaBlendEnable = true;
            device.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
            device.RenderState.SourceBlend = Blend.SourceAlpha;

            renderQueues[(int)RenderQueues.Scenery].render();

            device.RenderState.DepthBufferEnable = true;
            device.RenderState.AlphaBlendEnable = false;
            device.RenderState.DepthBufferWriteEnable = true;


            currentEffect = shadingEffect;
            shadingEffect.setCamera(camera);
            shadingEffect.setLightAccumulationBuffer(lightAccumulationRT.GetTexture());

            if (Properties.Settings.Default.UseSSAO)
            {
                shadingEffect.setAmbientOcclusionBuffer(ambientOcclusion.GetTexture());
            }
            else
            {
                shadingEffect.setAmbientOcclusionBuffer(null);
            }


            
            
            shadingEffect.begin();
            renderQueues[(int)RenderQueues.Normal].render();
            shadingEffect.end();

            shadingEffect.setLightAccumulationBuffer((Texture)null);

            device.RenderState.AlphaBlendEnable = true;
            device.RenderState.SourceBlend = Blend.One;
            device.RenderState.DestinationBlend = Blend.One;
            device.RenderState.AlphaDestinationBlend = Blend.One;
            device.RenderState.AlphaSourceBlend = Blend.One;
            device.RenderState.AlphaBlendOperation = BlendFunction.Min;

            renderQueues[(int)RenderQueues.Shield].render();

            device.RenderState.AlphaBlendEnable = false;

            

            PIXTools.PIXTools.EndEvent();
        }

        private Light getClosestLight()
        {


            LinkedListNode<Light> closest = lights.First;

            while (closest != null && closest.Value.castsShadows == false)
            {
                closest = closest.Next;
            }

            if (closest == null)
            {
                return null;
            }

            float lowestDistance = (camera.getPosition() - closest.Value.position).LengthSquared();

            LinkedListNode<Light> l = lights.First;
            while (l.Next != null)
            {
                l = l.Next;
                if (l.Value.castsShadows)
                {
                    float distance = (camera.getPosition() - l.Value.position).LengthSquared();
                    if (distance < lowestDistance)
                    {
                        closest = l;
                        lowestDistance = distance;
                    }
                }
            }

            lights.Remove(closest);

            return closest.Value;
        }

        public void setEffect(IEffect effect)
        {
            currentEffect = effect;
        }
        public void setLight(Light l)
        {
            currentEffect.setLight(l);

        }

        public Camera getCamera()
        {
            return camera;
        }

        public void drawNode(Node node, RenderQueues queue)
        {
            renderQueues[(int)queue].addNode(node);
        }

        public void addLight(Light light)
        {
            lights.AddLast(light);
        }

        public void setWorldMatrix(Matrix wrld)
        {
            if (currentEffect != null)
                currentEffect.setWorldMatrix(wrld);
        }

        public void setTexture(Texture texture)
        {
            if (currentEffect != null)
                currentEffect.setDecalTexture(texture);
        }

        public Vector2 getResolution()
        {
            return resolution;
        }

        public void setResolution(int resWidth, int resHeight)
        {
            resolution.X = resWidth;
            resolution.Y = resHeight;
            graphics.PreferredBackBufferWidth = resWidth;
            graphics.PreferredBackBufferHeight = resHeight;
            graphics.ApplyChanges();
        }

        public Rectangle resolutionRect
        {
            get
            {
                return new Rectangle(0, 0, (int)resolution.X, (int)resolution.Y);
            }
        }

        public void updateShader()
        {
            if (currentEffect != null)
                currentEffect.commit();
        }
    }
}
