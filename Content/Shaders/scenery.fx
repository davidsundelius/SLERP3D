float4x4 ViewProj;
float4x4 World;

float3 LightDirection = float3(-0.8944271909999f, 0.0f, 0.4472135954999579f);
float3 AmbientColor = float3(0.15f, 0.15f, 0.3f);
float3 AtmosphereColor = float3(0.8f, 0.8f, 1.0f);
float3 CameraPos;

texture DecalTexture;
texture CloudTexture;

sampler Decal = sampler_state
{
	Texture = <DecalTexture>;
	sRGBTexture = true;
};

struct VS_IN
{
	float4 pos : POSITION0;
	float3 normal : NORMAL0;
	float2 uv : TEXCOORD0;
};

struct PS_IN
{
	float4 pos : POSITION;
	float3 normal : NORMAL0;
	float4 worldPos : NORMAL1;
	float2 uv : TEXCOORD0;
};

PS_IN VS(VS_IN input)
{
	PS_IN output;
	output.pos = mul(input.pos, mul(World, ViewProj));
	output.worldPos = mul(input.pos, World);
	output.normal = mul(input.normal, World);
	output.uv = input.uv;
	
	return output;
}

float4 PS_Planet(PS_IN input) : COLOR0
{
	float atmosphereFactor = min(1, pow(1.5f - saturate(dot(input.normal, float3(0.0f, 0.0f, 1.0f))), 20));
	float shading = saturate(dot(input.normal, LightDirection));
	return float4((tex2D(Decal, input.uv).xyz * (1.0f - atmosphereFactor) + 
					AtmosphereColor * atmosphereFactor) * shading,
					1.0f - (atmosphereFactor * 0.3f * shading));
}

float4 PS_Object(PS_IN input) : COLOR0
{
	float shading = saturate(dot(input.normal, LightDirection));
	return float4(tex2D(Decal, input.uv).xyz * shading, 1.0f); 
}

technique Scenery
{
	pass planet
	{
		VertexShader = compile vs_3_0 VS();
		PixelShader = compile ps_3_0 PS_Planet();
		SRGBWriteEnable = false;
	}
	
	pass object
	{
		VertexShader = compile vs_3_0 VS();
		PixelShader = compile ps_3_0 PS_Object();
		SRGBWriteEnable = false;
	}
	
}