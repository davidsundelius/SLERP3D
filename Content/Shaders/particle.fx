float4x4 view;
float4x4 proj;
Texture Texture0;

sampler ParticleSampler = sampler_state
{
	Texture = <Texture0>;
	sRGBTexture = true;
};

struct VS_IN
{
    float4 Pos : POSITION0;
    float2 UV : TEXCOORD0;
    float4 Color : COLOR0;
    float2 offs : POSITION1;

};

struct PS_IN
{
	float4 Pos : POSITION0;
	float2 UV : TEXCOORD0;
	float4 Color : COLOR0;
};

PS_IN VS_Main(VS_IN input)
{
   PS_IN ret;
   ret.Pos = mul(mul(input.Pos, view) + float4(input.offs, 0.0f, 0.0f), proj);
   ret.UV = input.UV;
   ret.Color = input.Color;
   
   return ret;
   
}

float4 PS_Main(PS_IN input) : COLOR0
{
    return tex2D(ParticleSampler, input.UV) * input.Color;
}

technique ParticleCPU
{
    pass p0
    {

        VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 PS_Main();
        SRGBWriteEnable = false;
    }
}

technique ParticleGPUUpdate
{
	pass p0
	{
	}
}


technique ParticleGPUDraw
{
	pass p0
	{
	}
}