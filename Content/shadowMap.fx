float4x4 WorldViewProj;
float4x4 World;

struct VS_INPUT
{
	float4 pos : POSITION;
};

struct PS_INPUT
{
	float4 pos : POSITION;
	float2 depth : TEXCOORD0;
};

PS_INPUT VS_Main(VS_INPUT input)
{
	PS_INPUT output;
	output.pos = mul(input.pos, WorldViewProj);
	output.depth = output.pos.zw;
	
	return output;
}

float4 PS_Main(PS_INPUT input) : COLOR0
{
	return float4(input.depth.x / input.depth.y, 0.0f, 0.0f, 1.0f);
}

technique Main
{
	pass p0
	{
		VertexShader = compile vs_3_0 VS_Main();
		PixelShader = compile ps_3_0 PS_Main();
	}
}