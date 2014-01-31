float gExposure;
float gDefog;
float3 gFogColor;

texture InputTexture;
texture LuminanceTexture;

///////////////

sampler InputSampler = sampler_state
{
	Texture = <InputTexture>;
	sRGBTexture = false;
	MinFilter = Linear;
    MipFilter = Linear;
    MagFilter = Linear;
};

sampler LuminanceSampler = sampler_state
{
	Texture = <LuminanceTexture>;
	sRGBTexture = false;
};

///////////////

struct a2v {
    float4 pos  : POSITION;
    float4 texcoord  : TEXCOORD0;
};

struct v2f {
    float4 pos  : POSITION;
    float2 UV 	: TEXCOORD0;
};

///////////////

v2f TonemapVS(a2v input)
{
	v2f OUT = (v2f)0;
    OUT.pos = input.pos;
    OUT.UV = input.texcoord;
    return OUT;
}

static half3 gsDefogColor = (gDefog*gFogColor);

half4 TonemapPS(v2f IN, uniform float3 DefogColor) : COLOR
{
	float lum = tex2D(LuminanceSampler, IN.UV).w;
    
    half3 c = tex2D(InputSampler, IN.UV).rgb;
    c = c - DefogColor;
    c = max(((half3)0), c);
    c *= gExposure;
    return half4(c.rgb, 1.0);
}

////////////////////

technique ToneMapPostProcess
{
	pass main
	{
		VertexShader = compile vs_3_0 TonemapVS();
		PixelShader = compile ps_3_0 TonemapPS(gsDefogColor);
		SRGBWriteEnable = true;
    }
}