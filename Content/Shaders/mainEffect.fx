float4x4 WorldViewProj;
float4x4 WorldView;
float4x4 World;

texture DecalTexture;

float3 ambient = float3(0.3f, 0.3f, 0.3f);
float3 LightPos = float3(0.0f, 5.0f, 60.0f);
float3 LightColor = float3(0.8f, 0.8f, 0.8f);
float4 objColor = float4(1.0f, 1.0f, 1.0f, 1.0f);

sampler DecalSampler = sampler_state
{
	Texture = <DecalTexture>;
	AddressU = Wrap;
	AddressV = Wrap;
	Minfilter = Linear;
	Mipfilter = Point;
	Magfilter = Linear;
	sRGBTexture = true;
};

samplerCUBE SkyBoxSampler = sampler_state
{
	Texture = <DecalTexture>;
	sRGBTexture = true;
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

struct VS_SKY_INPUT
{
	float4 pos : POSITION;
	float3 uv : TEXCOORD0;
};

struct PS_SKY_INPUT
{
	float4 pos : POSITION;
	float3 uv : TEXCOORD0;
};


PS_INPUT VS_Main(VS_INPUT input)
{
	PS_INPUT output;
	output.worldPos = mul(input.pos, World).xyz;
	output.pos = mul(input.pos, WorldViewProj);
	output.norm = mul(input.norm, World);
	output.uv = input.uv;
	
	return output;
}

float4 PS_Main(PS_INPUT input) : COLOR0
{
	float4 tex = tex2D(DecalSampler, input.uv);
	/*tex = objColor;*/
	
	float3 lightDir = normalize(LightPos - input.worldPos);

	return float4(ambient * tex.xyz + saturate(dot(normalize(input.norm), lightDir)) * tex.xyz * LightColor, 1.0f);
}


PS_SKY_INPUT VS_Sky_Main(VS_SKY_INPUT input)
{
	PS_SKY_INPUT output;
	output.pos = mul(input.pos, WorldViewProj);
	output.uv = input.uv;
	
	return output;
}

float4 PS_Sky_Main(PS_SKY_INPUT input) : COLOR0
{
	return float4(texCUBE(SkyBoxSampler, input.uv).xyz, 1.0f);
}


technique Main
{
	pass Main
	{
		VertexShader = compile vs_3_0 VS_Main();
		PixelShader = compile ps_3_0 PS_Main();
		SRGBWriteEnable = false;
	}
	
	pass SkyBox
	{
		VertexShader = compile vs_3_0 VS_Sky_Main();
		PixelShader = compile ps_3_0 PS_Sky_Main();
		SRGBWriteEnable = false;
	}
}