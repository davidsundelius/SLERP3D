float4x4 WorldViewProj;
float4x4 World;

texture DecalTexture;

float3 ambient = float3(0.3f, 0.3f, 0.3f);
float3 lightPos = float3(0.0f, 5.0f, 60.0f);
float3 lightColor = float3(0.8f, 0.8f, 0.8f);
float4 objColor = float4(1.0f, 0.3f, 1.0f, 1.0f);

sampler DecalSampler = sampler_state
{
	Texture = <DecalTexture>;
	AddressU = Wrap;
	AddressV = Wrap;
	Minfilter = Linear;
	Mipfilter = Point;
	Magfilter = Linear;
};

struct VS_INPUT
{
	float4 pos : POSITION;
	float2 uv : TEXCOORD0;
	float3 norm : NORMAL0;
};

struct PS_INPUT
{
	float4 pos : POSITION;
	float2 uv : TEXCOORD0;
	float3 norm : NORMAL0;
	float3 worldPos : NORMAL1;
};

PS_INPUT VS_Main(VS_INPUT input)
{
	PS_INPUT output;
	output.worldPos = mul(input.pos, World).xyz;
	output.pos = mul(input.pos, WorldViewProj);
	output.norm = mul(input.norm, WorldViewProj);
	output.uv = input.uv;
	
	return output;
}

float4 PS_Main(PS_INPUT input) : COLOR0
{
	//input.worldPos.xz is just temporarty, switch when proper uv
	float4 tex = tex2D(DecalSampler, input.worldPos.xz*0.1);
	
	float3 lightDir = lightPos - input.worldPos;

	return float4(ambient + saturate(dot(input.norm, lightDir)) * tex.rgb * lightColor, tex.a);
}

technique Main
{
	pass p0
	{
		VertexShader = compile vs_3_0 VS_Main();
		PixelShader = compile ps_3_0 PS_Main();
	}
}