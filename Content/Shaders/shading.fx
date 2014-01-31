float4x4 WorldViewProj;
float4x4 World;

float2 InvResolution;

float3 AmbientLight = float3(0.2f, 0.2f, 0.4f);

texture DecalTexture;
texture LightAccumulationBuffer;
texture AmbientOcclusionBuffer;

sampler Decal = sampler_state
{
	Texture = <DecalTexture>;
	MipFilter = Linear;
	MinFilter = Anisotropic;
	MagFilter = Linear;
	
	MaxAnisotropy = 8;
	
	sRGBTexture = true;
};

sampler LightAccumulation = sampler_state
{
	Texture = <LightAccumulationBuffer>;
	sRGBTexture = false;
};

sampler AmbientOcclusion = sampler_state
{ 
	Texture = <AmbientOcclusionBuffer>;
	MagFilter = Linear;
	sRGBTexture = false;
};

struct VS_IN
{
	float4 pos : POSITION;
	float2 uv : TEXCOORD0;
	float3 norm : NORMAL0;
};

struct PS_IN
{
	float4 pos : POSITION;
	float2 uv : TEXCOORD0;
	float3 screen : TEXCOORD1;
};

PS_IN VS(VS_IN input)
{
	PS_IN output;
	output.pos = mul(input.pos, WorldViewProj);
	output.uv = input.uv;
	output.screen = output.pos.xyw;
	
	return output;
}

float4 PS_Phong(PS_IN input, float2 vPos : VPOS) : COLOR0
{
	float2 screen = (vPos * InvResolution) + float2(InvResolution.x, InvResolution.y) * 0.5f;
	
	float4 diffSpecular = tex2D(LightAccumulation, screen);
	float4 color = tex2D(Decal, input.uv);
	float AO = 1.0f - tex2D(AmbientOcclusion, screen).x;
	
	return float4((diffSpecular.xyz + AmbientLight)* AO * color.rgb + diffSpecular.w, color.a);
}

technique Shading
{
	pass phong
	{
		VertexShader = compile vs_3_0 VS();
		PixelShader = compile ps_3_0 PS_Phong();
		SRGBWriteEnable = false;
	}
}