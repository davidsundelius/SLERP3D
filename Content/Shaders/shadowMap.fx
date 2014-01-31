float4x4 WorldViewProj;
float4x4 WorldView;
float4x4 World;

struct VS_INPUT
{
	float4 pos : POSITION;
};

struct PS_INPUT
{
	float4 pos : POSITION;
	float3 viewPos : NORMAL0;
};

PS_INPUT VS_Main(VS_INPUT input)
{
	PS_INPUT output;
	output.pos = mul(input.pos, WorldViewProj);
	output.viewPos = mul(input.pos, WorldView);
	
	return output;
}

float4 PS_Main(PS_INPUT input) : COLOR0
{
	float len = length(input.viewPos);
	return float4(len, len * len, 0.0f, 1.0f);
}

technique shadowMap
{
	pass normal
	{
		VertexShader = compile vs_3_0 VS_Main();
		PixelShader = compile ps_3_0 PS_Main();
		SRGBWriteEnable = false;
	}
}