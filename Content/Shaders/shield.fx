float4x4 ViewProj;
float4x4 World;
float2 Offset;

texture DecalTexture;

sampler Decal = sampler_state
{
	Texture = <DecalTexture>;
	sRGBTexture = true;
};

struct VS_IN
{
	float4 pos : POSITION0;
	float2 uv : TEXCOORD0;
};

struct PS_IN
{
	float4 pos : POSITION;
	float2 uv : TEXCOORD0;
};

PS_IN VS(VS_IN input)
{
	PS_IN output;
	output.pos = mul(input.pos, mul(World, ViewProj));
	output.uv = input.uv;
	
	return output;
}

float4 PS(PS_IN input) : COLOR0
{
	return float4(tex2D(Decal, input.uv + Offset).rgb, 1.0f / 255.0f) * 0.5f;
}

technique Shield
{
	pass p0
	{
		VertexShader = compile vs_3_0 VS();
		PixelShader = compile ps_3_0 PS();
		SRGBWriteEnable = false;
	}	
}