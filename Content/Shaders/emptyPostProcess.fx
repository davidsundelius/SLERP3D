texture InputTexture;

sampler InputSampler = sampler_state
{
	Texture = <InputTexture>;
	sRGBTexture = false;
};

void VS(float4 pos : POSITION, float2 uv : TEXCOORD0, out float4 oPos : POSITION, out float2 oUv : TEXCOORD0)
{
	oPos = pos;
	oUv = uv;
}

float4 PS(float2 uv : TEXCOORD0) : COLOR0
{
	return tex2D(InputSampler, uv);
}

technique EmptyPostProcess
{
	pass p0
	{
		VertexShader = compile vs_3_0 VS();
		PixelShader = compile ps_3_0 PS();
		SRGBWriteEnable = true;
	}
}