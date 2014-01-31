texture InputTexture;
texture OutputTexture;
texture LuminanceTexture;
texture AfterImageTexture;

float2 InvResolution;

float4 vGlareParams = {0.4, 0.4, 0.4, 2};
const float3 weights = {0.3, 0.59, 0.11};

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
	sRGBTexture = false;
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
	OUT.uv = IN.uv + float2(InvResolution.x, InvResolution.y);
	return OUT;
}

float4 PS_Luminance(a2v IN) : COLOR
{
	float4 OUT;
	float4 vGlareAmount = tex2D(LuminanceSampler, IN.uv);
	float4 col = {0,0,0,1};
	for (float x = 0; x <= 1; x += 0.05)
	{
		for (float y = 0; y <= 1; y += 0.05)
		{
			float3 t = tex2D(InputSampler, IN.uv);
			col.xyz += t;
		}
	}
	col.xyz /= 400;
	float3 lum = saturate(col.xyz);
	OUT.xyz = dot(weights, lum.xyz);
	OUT.w = lerp(vGlareAmount.w, OUT.x, 0.5);
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
	float3 vFinalGlare = saturate(vGlareCol.xyz - vGlareParams.xyz) * vGlareParams.w;
	
	// modulate brightest pixels by inverse luminance
	OUT.xyz = vFinalGlare.xyz * (1 - vGlareAmount);
	OUT.w = vGlareCol.w;
	return OUT;
}

float4 PS_AfterImage(a2v IN) : COLOR
{
	float4 old = tex2D(AfterImageSampler, IN.uv);
	float4 current = tex2D(InputSampler, IN.uv);
	
	return saturate(current + 0.4 * old);
}

float4 PS_Add(a2v IN) : COLOR0
{
	float4 OUT;
	float4 vGlareCol = tex2D(InputSampler, IN.uv);
	float4 vScreenCol = tex2D(OutputSampler, IN.uv);
	
	// compute scene with full glare
	float3 vFinalGlareCol = saturate(vGlareCol.xyz + vScreenCol.xyz);
	
	// compute scene luminance
	float fLum = dot(tex2D(LuminanceSampler, IN.uv).x, weights);
	
	// interpolate between current scene and scene with full glare
	OUT.xyz = saturate(lerp(vScreenCol.xyz, vFinalGlareCol.xyz, 1 - fLum));
	OUT.w = vScreenCol.w;
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
	
	pass commit
	{
		VertexShader = compile vs_3_0 VS();
		PixelShader = compile ps_3_0 PS_Add();
		SRGBWriteEnable = false;
	}
}