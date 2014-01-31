float3 TopLeft;
float3 TopRight;
float3 BottomLeft;
float3 BottomRight;

float3 CameraPos;

float2 InvResolution;

float4x4 PrevViewProj;

Texture DepthTex;
Texture SceneTex;

sampler Depth = sampler_state
{
	Texture = <DepthTex>;
};

sampler Scene = sampler_state
{
	Texture = <SceneTex>;
	AddressU = Clamp;
	AddressV = Clamp;
};


struct VS_INPUT
{
	float4 pos : POSITION0;
	float2 uv : TEXCOORD0;
};

struct PS_INPUT
{
	float4 pos : POSITION0;
	float2 uv : TEXCOORD0;
	float3 viewDir : NORMAL0;
};

PS_INPUT VS_Main(VS_INPUT input)
{
	PS_INPUT output;
	output.pos = input.pos;
	output.uv = input.uv + InvResolution * 0.5f;
	
	output.viewDir = normalize(lerp(lerp(BottomLeft, BottomRight, input.uv.x), lerp(TopLeft, TopRight, input.uv.x), input.uv.y));
	return output;
}

float4 PS_Main(PS_INPUT input) : COLOR0
{
	float depth = tex2D(Depth, input.uv).x;
	
	float4 color = tex2D(Scene, input.uv);
	
	if (depth > 0.1f)
	{	
		float4 worldPos = float4(input.viewDir * depth + CameraPos, 1.0f);
		float4 prevPos = mul(worldPos, PrevViewProj);
		prevPos.xy /= prevPos.w;
		
		float2 currentPos = float2(input.uv.x * 2.0f - 1.0f, (1.0f - input.uv.y) * 2 - 1.0f);
		
		float2 velocity = (currentPos - prevPos.xy) / 30.0f;
		
		
		float weight = 1.0f;
		
		for (int i = 1; i < 4; ++i)
		{

			color += tex2D(Scene, input.uv + velocity * i);
			weight += 1;
		}
		
		color /= weight;
	}
	
	return color;	
}

technique MotionBlur
{
	pass p0
	{
		VertexShader = compile vs_3_0 VS_Main();
		PixelShader = compile ps_3_0 PS_Main();
	}
}