texture InputTexture;
texture OutputTexture;
texture LuminanceTexture;
texture AfterImageTexture;

float2 InvResolution;
float AfterImage;
bool DemonMode;
float4 GlareParams;	// xyz is the threshold values, w is the maximum bloom

const float3 LUMINANCE = {0.3, 0.59, 0.11};
const float DELTA = 0.0001;
const float WHITE = 1000000;

sampler InputSampler = sampler_state
{
	Texture = <InputTexture>;
	sRGBTexture = false;
	MinFilter = Linear;
    MipFilter = Linear;
    MagFilter = Linear;
};

sampler OutputSampler = sampler_state
{
	Texture = <OutputTexture>;
	sRGBTexture = true;
	MinFilter = Linear;
    MipFilter = Linear;
    MagFilter = Linear;
};

sampler LuminanceSampler = sampler_state
{
	Texture = <LuminanceTexture>;
	sRGBTexture = false;
	MinFilter = Linear;
    MipFilter = Linear;
    MagFilter = Linear;
};

sampler AfterImageSampler = sampler_state
{
	Texture = <AfterImageTexture>;
	sRGBTexture = false;
	MinFilter = Linear;
    MipFilter = Linear;
    MagFilter = Linear;
};

struct a2v
{
	float4 pos : POSITION;
	float2 uv : TEXCOORD0;
};

/////////////////

a2v VS(a2v IN)
{
	a2v OUT = (a2v)0;
	OUT.pos = IN.pos;
	OUT.uv = IN.uv + float2(InvResolution.x, InvResolution.y) * 0.5;
	return OUT;
}

float4 PS_Luminance(a2v IN) : COLOR
{
	float4 OUT;
	float4 prevLum = tex2D(LuminanceSampler, IN.uv);
	float avgLum;
	float lumSum = 0;
	float n = 0;
	for (float x = 0; x <= 1; x += 0.01)
	{
		for (float y = 0; y <= 1; y += 0.01)
		{
			float3 t = tex2D(InputSampler, IN.uv);
			float lum = dot(LUMINANCE, t);
			lumSum += log(DELTA + lum);
			n++;
		}
	}
	lumSum /= n;
	avgLum = exp(lumSum);
	OUT.xyz = avgLum;
	OUT.w = lerp(prevLum.w, avgLum, 0.01);
	return OUT;
}

float4 PS_Downsample(a2v input) : COLOR0
{
	float4 tex = tex2D(InputSampler, input.uv);
	return tex;
}

float4 PS_Glare(a2v IN) : COLOR
{
	float4 OUT;
	float4 vGlareAmount = tex2D(LuminanceSampler, IN.uv);
	float4 vGlareCol = tex2D(InputSampler, IN.uv);
	
	// remove low-luminance colours
	float3 vFinalGlare = saturate(vGlareCol.xyz - GlareParams.xyz) * GlareParams.w;
	
	// retrieve the glow texture stored in the alpha channel
	float3 glow = (1 - vGlareCol.w) * vGlareCol.xyz;
	
	// modulate brightest pixels by inverse luminance and add glow
	OUT.xyz = vFinalGlare.xyz * (1 - vGlareAmount) + glow;
	OUT.w = vGlareCol.w;
	return OUT;
}

float4 PS_AfterImage(a2v IN) : COLOR
{
	float4 old = tex2D(AfterImageSampler, IN.uv);
	float4 current = tex2D(InputSampler, IN.uv);
	
	return saturate(current + AfterImage * old);
}

float4 PS_Add(a2v IN) : COLOR
{
	float4 OUT;
	float interpolation;
	float4 vGlareCol = tex2D(InputSampler, IN.uv);
	float4 vScreenCol = tex2D(OutputSampler, IN.uv);
	
	// compute pixel luminance
	float fLum = dot(vScreenCol.xyz, LUMINANCE);
	
	if (DemonMode)
	{
		vScreenCol.xyz = fLum * 0.05;
		interpolation = 1;
	}
	else
	{
		interpolation = saturate(1 - fLum);
	}
	
	// compute scene with full glare
	float3 vFinalGlareCol = saturate(vGlareCol.xyz + vScreenCol.xyz);
	
	// interpolate between current scene and scene with full glare
	OUT.xyz = saturate(lerp(vScreenCol.xyz, vFinalGlareCol.xyz, interpolation));
	OUT.w = vScreenCol.w;
	return OUT;
}

const float3x3 RGB2XYZ = {
	0.4125, 0.3576, 0.1805,
	0.2126, 0.7152, 0.0722,
	0.0193, 0.1192, 0.9505};

const float3x3 XYZ2RGB = {
	3.2410, -1.5374, -0.4986,
	-0.9692, 1.8760, 0.0416,
	0.0556, -0.2040, 1.0570};

float4 TonemapPS(a2v IN, uniform float white) : COLOR
{
	float4 OUT;
	
	float4 avgLum = tex2D(LuminanceSampler, IN.uv);
    float4 c = tex2D(InputSampler, IN.uv);
    
    float3 XYZ = mul(RGB2XYZ, c.rgb);	// RGB -> XYZ conversion
    
    // XYZ -> Yxy conversion
    float3 Yxy;
    Yxy.r = XYZ.g;
	float temp = dot(float3 (1.0, 1.0, 1.0), XYZ.rgb);
	Yxy.gb = XYZ.rg / temp;
	
	// calculate middle grey value
	float middleGrey = 1.03 - 2 / (2 + log10(avgLum.w + 1));
	
	float lumScaled = Yxy.r * middleGrey / (avgLum.w + 0.007);
	// Scale luminance to within the displayable range [0, 1]
	Yxy.r = (lumScaled * (1.0 + lumScaled / white))/(1.0 + lumScaled);
	
	// Yxy -> XYZ conversion
	XYZ.r = Yxy.r * Yxy.g / Yxy.b;
	XYZ.g = Yxy.r;
	XYZ.b = Yxy.r * (1 - Yxy.g - Yxy.b) / Yxy.b;
	
	// XYZ -> RGB conversion
	OUT.rgb = mul(XYZ2RGB, XYZ);
	
	OUT.a = c.a;
	return OUT;
}

///////////////

technique BloomPostProcess
{
	pass luminance
	{
		VertexShader = compile vs_3_0 VS();
		PixelShader = compile ps_3_0 PS_Luminance();
		SRGBWriteEnable = false;
	}
	
	pass downsample
	{
		VertexShader = compile vs_3_0 VS();
		PixelShader = compile ps_3_0 PS_Downsample();
		SRGBWriteEnable = false;
	}
	
	pass glare
	{
		VertexShader = compile vs_3_0 VS();
		PixelShader = compile ps_3_0 PS_Glare();
		SRGBWriteEnable = false;
	}
	
	pass afterImage
	{
		VertexShader = compile vs_3_0 VS();
		PixelShader = compile ps_3_0 PS_AfterImage();
		SRGBWriteEnable = false;
	}
	
	pass tonemap
	{
		VertexShader = compile vs_3_0 VS();
		PixelShader = compile ps_3_0 TonemapPS(WHITE);
		SRGBWriteEnable = false;
    }
	
	pass commit
	{
		VertexShader = compile vs_3_0 VS();
		PixelShader = compile ps_3_0 PS_Add();
		SRGBWriteEnable = true;
	}
}