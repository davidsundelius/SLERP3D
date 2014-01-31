float4x4 WorldViewProj;
float4x4 WorldView;
float4x4 World;

float3 CameraPos;

float FarClipDistance = 100.0f;

float SpecularFactor = 10.0f;

struct VS_IN
{
	float4 pos : POSITION0;
	float3 normal : NORMAL0;
};

struct PS_IN
{
	float4 pos : POSITION;
	float3 normal : NORMAL0;
	float3 viewNormal : NORMAL1;
	float3 worldRay : NORMAL2;
};

PS_IN VS(VS_IN input)
{
	PS_IN output;
	output.pos = mul(input.pos, WorldViewProj);
	output.normal = mul(input.normal, World);
	output.viewNormal = mul(input.normal, WorldView);
	output.worldRay = (mul(input.pos, World).xyz - CameraPos);
	
	return output;
}

float4 PS(PS_IN input, out float4 depth : COLOR1) : COLOR0
{
	//depth = float4(input.depth.x, 0.0f, 0.0f, 1.0f);
	depth = float4(length(input.worldRay), normalize(input.viewNormal));
	return float4(input.normal, SpecularFactor);
}

technique GBuffers
{
	pass staticSpecular
	{
		VertexShader = compile vs_3_0 VS();
		PixelShader = compile ps_3_0 PS();
		SRGBWriteEnable = false;

	}
}