texture InputTexture;

float2 InvResolution;
float Weights[9];

sampler InputSampler = sampler_state
{
	Texture = <InputTexture>;
	MagFilter = Linear;
	sRGBTexture = false;
};

void VS(float4 pos : POSITION, float2 uv : TEXCOORD0, out float4 oPos : POSITION, out float2 oUv : TEXCOORD0)
{
	oPos = pos;
	oUv = uv + InvResolution * 0.5f;
}

float4 PS_Vertical(float2 uv : TEXCOORD0) : COLOR0
{
	float4 color = 
				tex2D(InputSampler, uv + float2(InvResolution.x * -4.0f, 0.0f)) * Weights[0];
	color +=	tex2D(InputSampler, uv + float2(InvResolution.x * -3.0f, 0.0f)) * Weights[1];
	color +=	tex2D(InputSampler, uv + float2(InvResolution.x * -2.0f, 0.0f)) * Weights[2];
	color +=	tex2D(InputSampler, uv + float2(InvResolution.x * -1.0f, 0.0f)) * Weights[3];
	color +=	tex2D(InputSampler, uv) * Weights[4];
	color +=	tex2D(InputSampler, uv + float2(InvResolution.x * 1.0f, 0.0f)) * Weights[5];
	color +=	tex2D(InputSampler, uv + float2(InvResolution.x * 2.0f, 0.0f)) * Weights[6];
	color +=	tex2D(InputSampler, uv + float2(InvResolution.x * 3.0f, 0.0f)) * Weights[7];
	color +=	tex2D(InputSampler, uv + float2(InvResolution.x * 4.0f, 0.0f)) * Weights[8];
	
	return color;
}

float4 PS_Horizontal(float2 uv : TEXCOORD0) : COLOR0
{
	float4 color = 
				tex2D(InputSampler, uv + float2(0.0f, InvResolution.y * -4.0f)) * Weights[0];
	color +=	tex2D(InputSampler, uv + float2(0.0f, InvResolution.y * -3.0f)) * Weights[1];
	color +=	tex2D(InputSampler, uv + float2(0.0f, InvResolution.y * -2.0f)) * Weights[2];
	color +=	tex2D(InputSampler, uv + float2(0.0f, InvResolution.y * -1.0f)) * Weights[3];
	color +=	tex2D(InputSampler, uv) * Weights[4];
	color +=	tex2D(InputSampler, uv + float2(0.0f, InvResolution.y * 1.0f)) * Weights[5];
	color +=	tex2D(InputSampler, uv + float2(0.0f, InvResolution.y * 2.0f)) * Weights[6];
	color +=	tex2D(InputSampler, uv + float2(0.0f, InvResolution.y * 3.0f)) * Weights[7];
	color +=	tex2D(InputSampler, uv + float2(0.0f, InvResolution.y * 4.0f)) * Weights[8];

	
	return color;
}


technique Blur
{
	pass vertical
	{
		VertexShader = compile vs_3_0 VS();
		PixelShader = compile ps_3_0 PS_Vertical();
	}
	
	pass horizontal
	{
		VertexShader = compile vs_3_0 VS();
		PixelShader = compile ps_3_0 PS_Horizontal();
	}
}