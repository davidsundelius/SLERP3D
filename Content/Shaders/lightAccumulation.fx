float4x4 WorldViewProj;
float4x4 WorldView;
float4x4 World;

float2 InvResolution;

//Projector light test
float4x4 LightViewProj;
//Spot light test
float LightAngle = 0.8f;

float Range;

float3 LightPos = float3(0.0f, 5.0f, 60.0f);
float3 LightDir = float3(0.0f, 1.0f, 0.0f);
float LightSpecular = 0.8f;
float3 LightColor = float3(0.8f, 0.8f, 0.8f);
float3 CameraPos = float3(0.0f, 0.0f, 0.0f);

float vsmMinVariance = 0.01f;

float FarClipDistance;

texture DepthBuffer;
texture NormalSpecularBuffer;
texture ShadowMap;
texture DecalTexture;

sampler DepthSampler = sampler_state
{
	Texture = <DepthBuffer>;
	sRGBTexture = false;
};

sampler GSampler = sampler_state
{
	Texture = <NormalSpecularBuffer>;
	sRGBTexture = false;
};

sampler Shadow = sampler_state
{
	Texture = <ShadowMap>;
	AddressU = Clamp;
	AddressV = Clamp;
	Mipfilter = Linear;
	Minfilter = Linear;
	Magfilter = Linear;
	sRGBTexture = false;
};

sampler DecalSampler = sampler_state
{
	Texture = <DecalTexture>;
	sRGBTexture = true;
};

struct PS_IN
{
	float4 pos : POSITION;
	float3 wsRay : NORMAL0;
};

PS_IN VS(float4 pos : POSITION0)
{
	PS_IN output;
	output.pos = mul(pos, WorldViewProj);
	
	float4 worldPos = mul(pos, World);
	output.wsRay = worldPos.xyz - CameraPos;
	
	return output;
}

static inline void ProcessInput(PS_IN input, float2 vPos, out float2 screen, out float3 worldPos, out float depth)
{
	screen = (vPos * InvResolution) + InvResolution * 0.5f;

	depth = tex2D(DepthSampler, screen).r;
	worldPos = normalize(input.wsRay) * depth + CameraPos;
}

static inline float4 CalculateLight(float2 screen, float3 worldPos)
{
	float4 normspec = tex2D(GSampler, screen);
	
	float3 lightDir = normalize(LightPos - worldPos);
	float3 reflection = reflect(lightDir, normspec.xyz);
	float3 viewVector = normalize(worldPos - CameraPos);
	return float4(saturate(dot(normspec.xyz, lightDir)) * LightColor, pow(saturate(dot(viewVector, reflection)), normspec.w) * LightSpecular);
}

static inline float ShadowFactor(float2 coords, float depth)
{
	float2 sample = 0.0f.xx;
	coords = coords + InvResolution * 0.5f;
	float filterWidth = 1.5f;
	
	
	/*for (int x = -1; x < 2; ++x)
	{
		for(int y = -1; y < 2; ++y)
		{
			sample += tex2D(Shadow, coords + float2(InvResolution.x * x, InvResolution.y * y) * filterWidth).xy;
		}
	}*/
	
	sample += tex2D(Shadow, coords - InvResolution * 0.5f).xy;
	sample += tex2D(Shadow, coords + InvResolution * 0.5f).xy;
	sample += tex2D(Shadow, coords + float2(InvResolution.x, -InvResolution.y) * 0.5f).xy;
	sample += tex2D(Shadow, coords - float2(InvResolution.x, -InvResolution.y) * 0.5f).xy;
	
	sample /= 4.0f;
	
	/*sample += tex2D(Shadow, coords + float2(-InvResolution.x, -InvResolution.y) * filterWidth).xy;
	sample += tex2D(Shadow, coords + float2(0, -InvResolution.y) * filterWidth).xy;
	sample += tex2D(Shadow, coords + float2(InvResolution.x, -InvResolution.y) * filterWidth).xy;
	
	sample += tex2D(Shadow, coords + float2(-InvResolution.x, 0) * filterWidth).xy;
	
	sample += tex2D(Shadow, coords + float2(InvResolution.x, 0) * filterWidth).xy;
	
	sample += tex2D(Shadow, coords + float2(-InvResolution.x, InvResolution.y) * filterWidth).xy;
	sample += tex2D(Shadow, coords + float2(0, InvResolution.y) * filterWidth).xy;
	sample += tex2D(Shadow, coords + float2(InvResolution.x, InvResolution.y) * filterWidth).xy;
	
	sample /= 9.0f;*/
	
	float variance = max(sample.y - sample.x * sample.x, vsmMinVariance);
	float delta = depth - sample.x;

	return max(variance / (variance + delta * delta), depth < sample.x);

}

float4 PS_Projector(PS_IN input, float2 vPos : VPOS) : COLOR0
{
	float3 worldPos;
	float depth;
	float2 screen;
	ProcessInput(input, vPos, screen, worldPos, depth);
		
	float4 lightProjPos = mul(float4(worldPos, 1.0f), LightViewProj);
	lightProjPos.xyz /= lightProjPos.w;
		
	float4 color;
	
	if (max(lightProjPos.x, max(lightProjPos.y, lightProjPos.z)) > 1.0f || min(lightProjPos.x, min(lightProjPos.y, lightProjPos.z)) < -1.0f)
	{
		color = 0.0f.xxxx;
	} else
	{
		lightProjPos.xy = (lightProjPos.xy + float2(1.0f, 1.0f)) * 0.5f;
		lightProjPos.y = 1.0f - lightProjPos.y;
		
		float distance = length(worldPos - LightPos);
		
		color = CalculateLight(screen, worldPos) * (1.0f - tex2D(DecalSampler, lightProjPos.xy)) * saturate(1.0f - distance / Range);	
	}
	return color;
}

float4 PS_Projector_Shadow(PS_IN input, float2 vPos : VPOS) : COLOR0
{
	float3 worldPos;
	float depth;
	float2 screen;
	ProcessInput(input, vPos, screen, worldPos, depth);
		
	float4 lightProjPos = mul(float4(worldPos, 1.0f), LightViewProj);
	lightProjPos.xyz /= lightProjPos.w;
				
	float4 color;
	if (max(lightProjPos.x, max(lightProjPos.y, lightProjPos.z)) > 1.0f || min(lightProjPos.x, min(lightProjPos.y, lightProjPos.z)) < -1.0f)
	{
		color = 0.0f.xxxx;
	} else
	{
		float3 lightToPos = worldPos - LightPos;
		float distance = length(lightToPos);
		lightToPos.z = -lightToPos.z;
		
		lightProjPos.xy = (lightProjPos.xy + float2(1.0f, 1.0f)) * 0.5f;
		lightProjPos.y = 1.0f - lightProjPos.y;
		
		float shadowFalloffFactor = ShadowFactor(lightProjPos.xy, distance) * saturate(1.0f - distance / Range);
		
		color = CalculateLight(screen, worldPos) * shadowFalloffFactor * (1.0f - tex2D(DecalSampler, lightProjPos.xy)).xxxx;
	}
	return color;
}

float4 PS_Point(PS_IN input, float2 vPos : VPOS) : COLOR0
{
	float3 worldPos;
	float depth;
	float2 screen;
	ProcessInput(input, vPos, screen, worldPos, depth);

	float3 lightToPos = worldPos - LightPos;
	float distance = length(lightToPos);
	return CalculateLight(screen, worldPos) * saturate(1.0f - distance / Range);	
}

float4 PS_Spot(PS_IN input, float2 vPos : VPOS) : COLOR0
{
	float3 worldPos;
	float depth;
	float2 screen;
	ProcessInput(input, vPos, screen, worldPos, depth);

	float3 lightToPos = worldPos - LightPos;
	float distance = length(lightToPos);
	float v = acos(dot(LightDir, normalize(lightToPos)));
	float fallOff = saturate(1.0f - pow(smoothstep(0.0f, LightAngle, v), 3));
	return CalculateLight(screen, worldPos) * saturate(1.0f - distance / Range) * fallOff;
}

float4 PS_Spot_Shadow(PS_IN input, float2 vPos : VPOS) : COLOR0
{
	float3 worldPos;
	float depth;
	float2 screen;
	ProcessInput(input, vPos, screen, worldPos, depth);
		
	float4 lightProjPos = mul(float4(worldPos, 1.0f), LightViewProj);
	lightProjPos.xyz /= lightProjPos.w;
			
	float3 lightToPos = worldPos - LightPos;
	float distance = length(lightToPos);
	float v = acos(dot(LightDir, normalize(lightToPos)));
	
	lightToPos.z = -lightToPos.z;
	
	lightProjPos.xy = (lightProjPos.xy + float2(1.0f, 1.0f)) * 0.5f;
	lightProjPos.y = 1.0f - lightProjPos.y;
	
	float shadowFalloffFactor = ShadowFactor(lightProjPos.xy, distance) * saturate(1.0f - distance / Range) * saturate(1.0f - pow(smoothstep(0.0f, LightAngle, v), 3));
	
	return CalculateLight(screen, worldPos) * shadowFalloffFactor;
}

technique LightAccumulation
{
	pass projector
	{
		VertexShader = compile vs_3_0 VS();
		PixelShader = compile ps_3_0 PS_Projector();
		SRGBWriteEnable = false;
	}
	
	pass projectorShadow
	{
		VertexShader = compile vs_3_0 VS();
		PixelShader = compile ps_3_0 PS_Projector_Shadow();
		SRGBWriteEnable = false;
	}
	
	pass point
	{
		VertexShader = compile vs_3_0 VS();
		PixelShader = compile ps_3_0 PS_Point();
		SRGBWriteEnable = false;
	}
	
	pass spot
	{
		VertexShader = compile vs_3_0 VS();
		PixelShader = compile ps_3_0 PS_Spot();
		SRGBWriteEnable = false;
	}
	
	pass spotShadow
	{
		VertexShader = compile vs_3_0 VS();
		PixelShader = compile ps_3_0 PS_Spot_Shadow();
		SRGBWriteEnable = false;
	}
	
}