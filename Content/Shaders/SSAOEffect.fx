float2 InvResolution;
float2 Resolution;
float KernelRadius = 0.7f;
float DeltaScale = 300.0f;
float RangeScale = 3.0f;
float ViewportDepth = 1000.0f;
float DefaultOcclusion = 0.8f;

float2 ZoomValues;
float2 FrustumDir;

float4x4 View;

texture NormalBuffer;
texture DepthBuffer;
texture RandomBuffer;

sampler Normal = sampler_state
{
	Texture = <NormalBuffer>;
	AddressU = Clamp;
	AddressV = Clamp;
	MipFilter = None;
}; 

sampler Depth = sampler_state
{
	Texture = <DepthBuffer>;
	AddressU = Clamp;
	AddressV = Clamp;
	MipFilter = None;
}; 

sampler Random = sampler_state
{
	Texture = <RandomBuffer>;
	AddressU = Wrap;
	AddressV = Wrap;
	MipFilter = None;
};

struct VS_INPUT
{
	float4 pos : POSITION;
	float2 uv : TEXCOORD0;
};

struct PS_INPUT
{
	float4 pos : POSITION0;
	float2 uv : TEXCOORD0;
};

PS_INPUT VS_Main(VS_INPUT input)
{
	PS_INPUT output;
	output.pos = input.pos;
	output.uv = input.uv + InvResolution * 0.5f;
	return output;
}


float GetRandom(float2 uv)
{
	return frac(uv.x * (Resolution.x / 2.0f)) * 0.25f + frac(uv.y * (Resolution.y / 2.0f)) * 0.75f;
}

float3 GetPosition(float2 uv, float depth)
{
	return normalize(float3(lerp(-FrustumDir, FrustumDir, uv), 1.0f)) * depth;
}

float3 GetPosition(float2 uv)
{
	return GetPosition(uv, tex2D(Depth, uv).x);
	//return normalize(float3(lerp(-FrustumDir, FrustumDir, uv), 1.0f)) * tex2D(Depth, uv).x;
}

float2 ProjectPos(float3 pos)
{
	float2 proj = pos.xy / (pos.z * ZoomValues);
	proj.y = 1.0f - proj.y;
	
	return proj;
}

float4 PS_Main5(PS_INPUT input) : COLOR0
{
	const float3 kernel[14] = 
	{
		float3(1.0f, 1.0f, -1.0f), float3(1.0f, -1.0f, -1.0f),
		float3(1.0f, 1.0f, 1.0f), float3(1.0f, -1.0f, 1.0f),
		float3(-1.0f, 1.0f, -1.0f), float3(-1.0f, -1.0f, -1.0f),
		float3(-1.0f, 1.0f, 1.0f), float3(-1.0f, -1.0f, 1.0f),
		
		float3(1.0f, 0.0f, 0.0f), float3(-1.0f, 0.0f, 0.0f),
		float3(0.0f, 1.0f, 0.0f), float3(0.0f, -1.0f, 0.0f),
		float3(0.0f, 0.0f, 1.0f), float3(0.0f, 0.0f, -1.0f)
	};
	
	const float4 sample = tex2D(Depth, input.uv);
	const float3 pos = GetPosition(input.uv, sample.x);
	const float3 normal = sample.yzw * float3(1.0f, 1.0f, -1.0f);

	const float radius = KernelRadius * GetRandom(input.uv) / pos.z;
	const float3 random = tex2D(Random, input.uv * 10.0f * pos.z);
	
	float occlusion = 0.0f;
	
	for (int i = 0; i < 8; ++i)
	{
		const float3 coord = reflect(kernel[i] * radius, random);
		
		const float3 diff = GetPosition(input.uv + coord.xy) - pos;
		const float3 dir = normalize(diff);
		const float len = length(diff);
		
		occlusion += max(0.0f, dot(normal, dir)) * (1.0f / (1.0f + len * len)) * 2.0f;
	}
	
	occlusion /= 8.0f;
	
	return occlusion;
}


float4 PS_Main3(PS_INPUT input) : COLOR0
{
	/*const float3 kernel[8] = 
	{
		float3(-1.0f, -1.0f, -1.0f)	 , 
		float3(-1.0f, -1.0f, 1.0f)	 ,
		float3(-1.0f, 1.0f, -1.0f)	 , 
		float3(-1.0f, 1.0f, 1.0f)	 ,
		float3(1.0f, -1.0f, -1.0f)	 , 
		float3(1.0f, -1.0f, 1.0f)	 ,
		float3(1.0f, 1.0f, -1.0f)	 , 
		float3(1.0f, 1.0f, 1.0f)	 ,
		
	};*/
	
	const float2 kernel[8] = 
	{
		float2(1.0f, 1.0f),
		float2(1.0f, 0.0f),
		float2(1.0f, -1.0f),
		float2(0.0f, 1.0f),
		float2(0.0f, -1.0f),
		float2(-1.0f, 1.0f),
		float2(-1.0f, 0.0f),
		float2(-1.0f, -1.0f)
	};
	
	const float3 pos = GetPosition(input.uv);
	float3 normal = normalize(tex2D(Depth, input.uv).yzw);
			
	float occlusion = 0.0f;
	float3 random = normalize(tex2D(Random, input.uv * 50.0f) * 2.0f - 1.0f);
	float2 rndRadius = KernelRadius * InvResolution;
		
	for(int i = 0; i < 5; ++i)
	{
		//float2 coord = ProjectPos(reflect(kernel[i] * KernelRadius, random) + pos);
		
		//float3 rotatedKernel = reflect(kernel[i] * rndRadius, random);
		float2 coord = input.uv + kernel[i] * rndRadius;//reflect(kernel[i], random).xy * rndRadius;
		
		const float3 delta = GetPosition(coord) - pos;
		
		const float3 dir = normalize(delta);
		const float len = length(delta) * 0.1f;
		
		occlusion += saturate(dot(normal, dir) / (1.0f + len * len));
	}
	
	occlusion /= 5.0f;
	
	return float4(occlusion.xxx, 1.0f);
	
}


float4 PS_Main4(PS_INPUT input) : COLOR0
{
	const float3 kernel[32] = {
	float3(-0.83239283f,	-0.44756289f,	 0.30903095f),
	float3( 0.90639156f,	-0.04516602f,	-0.42470855f),
	float3(-0.33628762f,	 0.84032712f,	-0.21048622f),
	float3( 0.04993531f,	 0.19524365f,	 0.47035290f),
	float3( 0.08528281f,	 0.81061612f,	 0.62323567f),
	float3(-0.63118632f,	 0.99492894f,	-0.96386712f),
	float3(-0.70191771f,	 0.21874075f,	 0.35415204f),
	float3( 0.18320319f,	-0.71897132f,	 0.70762506f),
	float3(-0.31147896f,	-0.12761870f,	-0.49109890f),
	float3( 0.35994144f,	 0.56059594f,	-0.63082790f),
	float3( 0.28210980f,	-0.07804644f,	 0.86047998f),
	float3( 0.97045866f,	-0.67629552f,	 0.51620997f),
	float3(-0.82441779f,	 0.84114019f,	 0.31073760f),
	float3(-0.50787872f,	-0.62630968f,	 0.23078875f),
	float3( 0.61964473f,	-0.14521703f,	-0.51801957f),
	float3(-0.27541593f,	-0.58692956f,	-0.12092611f),
	float3(-0.75088402f,	 0.29013502f,	 0.21843082f),
	float3( 0.58516214f,	-0.91182246f,	 0.55516067f),
	float3( 0.26002171f,	 0.88570775f,	 0.79205751f),
	float3(-0.99731037f,	-0.62734728f,	 0.44209429f),
	float3( 0.79902971f,	-0.04246879f,	 0.94209344f),
	float3( 0.49533093f,	-0.44754167f,	 0.25369363f),
	float3( 0.90711223f,	 0.74868333f,	 0.05969858f),
	float3( 0.11800849f,	 0.65785785f,	-0.30209429f),
	float3( 0.05388736f,	 0.04043652f,	-0.43258666f),
	float3( 0.81798666f,	 0.46850436f,	-0.69409258f),
	float3(-0.99611205f,	-0.31281077f,	 0.80001651f),
	float3( 0.18695051f,	 0.14108199f,	-0.92439733f),
	float3( 0.08467281f,	 0.19084130f,	 0.92554670f),
	float3(-0.48965316f,	 0.53181100f,	 0.48391918f),
	float3( 0.76704829f,	 0.92117712f,	 0.58305840f),
	float3( 0.26306940f,	-0.23027360f,	 0.50646416f) };
	
	float3 pos = GetPosition(input.uv);	
	float occlusion = 0.0f;
	
	float3 random = tex2D(Random, input.uv * 100.0f) * 2.0f - 1.0f;
	
	float3 samplePos = pos + KernelRadius * reflect(kernel[0], random);
	float2 coord = input.uv + reflect(kernel[0], random).xy * KernelRadius / pos.z;
	
	float depth = tex2D(Depth, coord).x;
	
	float delta = max(0.0f, depth - pos.z);
	occlusion += 1.0f / (1.0f + delta * delta);	
	
	//return float4(samplePos.xy / samplePos.z, 0.0f.xx);
	//return float4(coord, 0.0f.xx);
	return occlusion;
		/*
#define SAMPLES 2
	
	for (int i = 0; i < 8 * SAMPLES; ++i)
	{
		float3 samplePos = pos + KernelRadius * reflect(kernel[i], random);
		float2 coord = (samplePos.xy / samplePos.z) * float2(0.75f, 1.0f) * 0.5f + 0.5f;
		
		float depth = tex2D(Depth, coord);
		
		float delta = max(0.0f, depth - pos.z);
		occlusion += 1.0f / (1.0f + delta * delta);		
	}
	
	return occlusion / (SAMPLES * 8.0f);	*/	
}
/*
float4 PS_Main2(PS_INPUT input) : COLOR0
{
	const float3 kernel[32] = {
	float3(-0.83239283f,	-0.44756289f,	 0.30903095f),
	float3( 0.90639156f,	-0.04516602f,	-0.42470855f),
	float3(-0.33628762f,	 0.84032712f,	-0.21048622f),
	float3( 0.04993531f,	 0.19524365f,	 0.47035290f),
	float3( 0.08528281f,	 0.81061612f,	 0.62323567f),
	float3(-0.63118632f,	 0.99492894f,	-0.96386712f),
	float3(-0.70191771f,	 0.21874075f,	 0.35415204f),
	float3( 0.18320319f,	-0.71897132f,	 0.70762506f),
	float3(-0.31147896f,	-0.12761870f,	-0.49109890f),
	float3( 0.35994144f,	 0.56059594f,	-0.63082790f),
	float3( 0.28210980f,	-0.07804644f,	 0.86047998f),
	float3( 0.97045866f,	-0.67629552f,	 0.51620997f),
	float3(-0.82441779f,	 0.84114019f,	 0.31073760f),
	float3(-0.50787872f,	-0.62630968f,	 0.23078875f),
	float3( 0.61964473f,	-0.14521703f,	-0.51801957f),
	float3(-0.27541593f,	-0.58692956f,	-0.12092611f),
	float3(-0.75088402f,	 0.29013502f,	 0.21843082f),
	float3( 0.58516214f,	-0.91182246f,	 0.55516067f),
	float3( 0.26002171f,	 0.88570775f,	 0.79205751f),
	float3(-0.99731037f,	-0.62734728f,	 0.44209429f),
	float3( 0.79902971f,	-0.04246879f,	 0.94209344f),
	float3( 0.49533093f,	-0.44754167f,	 0.25369363f),
	float3( 0.90711223f,	 0.74868333f,	 0.05969858f),
	float3( 0.11800849f,	 0.65785785f,	-0.30209429f),
	float3( 0.05388736f,	 0.04043652f,	-0.43258666f),
	float3( 0.81798666f,	 0.46850436f,	-0.69409258f),
	float3(-0.99611205f,	-0.31281077f,	 0.80001651f),
	float3( 0.18695051f,	 0.14108199f,	-0.92439733f),
	float3( 0.08467281f,	 0.19084130f,	 0.92554670f),
	float3(-0.48965316f,	 0.53181100f,	 0.48391918f),
	float3( 0.76704829f,	 0.92117712f,	 0.58305840f),
	float3( 0.26306940f,	-0.23027360f,	 0.50646416f) };
	
	
		 
}	*/ 
	 

float4 PS_Main(PS_INPUT input) : COLOR0
{
	const float3 kernel[8] =
	{
		normalize( float3( 1, 1, 1 ) )	* 0.125f,
		normalize( float3( -1,-1,-1 ) ) * 0.250f,
		normalize( float3( -1,-1, 1 ) ) * 0.375f,
		normalize( float3( -1, 1,-1 ) ) * 0.500f,
		normalize( float3( -1, 1 ,1 ) ) * 0.625f,
		normalize( float3( 1,-1,-1 ) )	* 0.750f,
		normalize( float3( 1,-1, 1 ) )	* 0.875f,
		normalize( float3( 1, 1,-1 ) )	* 1.000f
	};
	
	float occlusion = 0.0f;
		
	float4 sample = tex2D(Depth, input.uv);
	float depth = sample.x;
	float3 normal = sample.yzw;
	
	float3 kernelSize = float3((KernelRadius / depth).xx, KernelRadius / ViewportDepth);
	
	depth /= ViewportDepth;
	
	float deltaScale = ViewportDepth / kernelSize.z;
	
#define NUMPASSES 1
	for (int j = 0; j < NUMPASSES; ++j)
	{
		float3 randomDir = tex2D(Random, input.uv * (40.0f + j));	
		randomDir = randomDir * 2.0f - 1.0f;
		for (int i = 0; i < 8; ++i)
		{
			float3 rotKernel = reflect(kernel[i], randomDir) * kernelSize;
			float sampleDepth = tex2D(Depth, rotKernel.xy + input.uv).r / ViewportDepth + rotKernel.z;
			float delta = max(sampleDepth - depth, 0.0f);
			float range = delta / (RangeScale * kernelSize.z);
			
			occlusion += lerp(delta * DeltaScale, DefaultOcclusion, saturate(range));
			
			/*float delta = abs(sampleDepth - depth);
			float deltaScaled = saturate(delta * 500.0f);
			occlusion += lerp(0.9f, 0.0f, deltaScaled);*/
		}
	}
	
	//return lerp(0.1f, 1.0f, occlusion / 16.0f).xxxx;
	return float4((occlusion / (8.0f * NUMPASSES)).xxx, 1.0f);
			
	
	/*    float fDefVal = 0.55f;
	 float fDepthRangeScale = PS_NearFarClipDist.y / vSampleScale.z * 0.85f;

    for(int s=0; s<(bHQ ? 2 : 1); s++)
    {
      vDistance = fSceneDepth - arrSceneDepth2[s]; 
      float4 vDistanceScaled = vDistance * fDepthRangeScale;
      fRangeIsInvalid = (saturate( abs(vDistanceScaled) ) + saturate( vDistanceScaled ))/2;  
      vSkyAccess += lerp(saturate((-vDistance)*fDepthTestSoftness), fDefVal, fRangeIsInvalid);
    }
  }

  OUT.Color = dot( vSkyAccess, (bHQ ? 1/16.0f : 1/8.0f)*2.0 ) - SSAO_params.y; // 0.075f
  OUT.Color = saturate(lerp( 0.9f, OUT.Color, SSAO_params.x ));*/
}

technique SSAO
{
	pass SSAO
	{
		VertexShader = compile vs_3_0 VS_Main();
		PixelShader = compile ps_3_0 PS_Main5();
	}
}

/*
////////////////////////////////////////////////////////////////////////////
//
//  Crytek Engine Shader Source File
//  Copyright (C), Crytek Studios, 2001-2007
// -------------------------------------------------------------------------
//  File name:   AmbientOcclusion.cfx
//  Version:     v1.00
//  Created:     04/12/2006 by Vladimir Kajalin
//  Description: Implementation of SSAO, TerrainAO (2.5 D maps), Fill lights
// -------------------------------------------------------------------------
//  History:
//
////////////////////////////////////////////////////////////////////////////

#include "Common.cfi" 
#include "ModificatorVT.cfi"

// Shader global descriptions
float Script : STANDARDSGLOBAL
<
  string Script =
           "NoPreview;"
           "LocalConstants;"
           "ShaderDrawType = Custom;"
           "ShaderType = PostProcess;"
>; 

// original depth target
sampler2D sceneDepthSampler = sampler_state
{
 Texture = $ZTarget;
 MinFilter = POINT;
 MagFilter = POINT;
 MipFilter = POINT;
 AddressU = Clamp;
 AddressV = Clamp;
};

// downscaled depth target
sampler2D sceneDepthSamplerAO = sampler_state
{
 Texture = $ZTargetScaled;
 MinFilter = POINT;
 MagFilter = POINT;
 MipFilter = POINT;
 AddressU = Clamp;
 AddressV = Clamp;
};

//===========================================================================

float4 AOSectorRange;
float4 TerrainAOInfo;
float4 FillLightPos;
float4 FillLightColor;
float4 SSAO_params
float4x4 CompMatrix : PI_Composite;

/////////////////////////////
// structs


struct app2vertShadow
{
  IN_P
  IN_TBASE
  float3 viewDir : TEXCOORD1;
};
sampler2D sRotSampler4x4_16 = sampler_state
{
	Texture = $16PointsOnSphere;
	MinFilter = POINT;
	MagFilter = POINT;
	MipFilter = NONE; 
	AddressU = Wrap;
	AddressV = Wrap;	
};
// vPlane should be normalized
// the returned vector has the same length as vDir
float3 mirror( float3 vDir, float3 vPlane ) 
{
  return vDir - 2 * vPlane * dot(vPlane,vDir);
}

pixout_cl Deferred_SSAO_Pass_PS(vert2fragSSAO IN)
{
  pixout_cl OUT;
  
	// define kernel
	const half step = 1.f - 1.f/8.f;
	half n = 0;
	const half fScale = 0.025f; 
	const half3 arrKernel[8] =
	{
		normalize(half3( 1, 1, 1))*fScale*(n+=step),
		normalize(half3(-1,-1,-1))*fScale*(n+=step),
		normalize(half3(-1,-1, 1))*fScale*(n+=step),
		normalize(half3(-1, 1,-1))*fScale*(n+=step),
		normalize(half3(-1, 1 ,1))*fScale*(n+=step),
		normalize(half3( 1,-1,-1))*fScale*(n+=step),
		normalize(half3( 1,-1, 1))*fScale*(n+=step),
		normalize(half3( 1, 1,-1))*fScale*(n+=step),
	};

	// create random rot matrix
	half3 rotSample = tex2D(sRotSampler4x4_16, IN.ScreenTC.zw).rgb;
	rotSample = (2.0 * rotSample - 1.0);

  half fSceneDepth = tex2D( sceneDepthSampler, IN.ScreenTC.xy ).r;  	  

	// range conversions
  half fSceneDepthM = fSceneDepth * PS_NearFarClipDist.y;  

	half3 vSampleScale = SSAO_params.zzw
		* saturate(fSceneDepthM / 5.3f) // make area smaller if distance less than 5 meters
    * (1.f + fSceneDepthM / 8.f ); // make area bigger if distance more than 32 meters

  float fDepthRangeScale = PS_NearFarClipDist.y / vSampleScale.z * 0.85f;
	
	// convert from meters into SS units
	vSampleScale.xy *= 1.0f / fSceneDepthM;
	vSampleScale.z  *= 2.0f / PS_NearFarClipDist.y;

  float fDepthTestSoftness = 64.f/vSampleScale.z;

	// sample
  half4 vSkyAccess = 0.f;
  half4 arrSceneDepth2[2];      
  half3 vIrrSample;
  half4 vDistance;
  float4 fRangeIsInvalid;

  const half bHQ = (GetShaderQuality()==QUALITY_HIGH);

  float fHQScale = 0.5f;

  for(int i=0; i<2; i++)
  {    
    vIrrSample = mirror(arrKernel[i*4+0], rotSample) * vSampleScale;		
    arrSceneDepth2[0].x = tex2D( sceneDepthSamplerAO, IN.ScreenTC.xy + vIrrSample.xy ).r + vIrrSample.z;  
    if (bHQ)
    {
      vIrrSample.xyz *= fHQScale;
      arrSceneDepth2[1].x = tex2D( sceneDepthSamplerAO, IN.ScreenTC.xy + vIrrSample.xy ).r + vIrrSample.z;  
    }

    vIrrSample = mirror(arrKernel[i*4+1], rotSample) * vSampleScale;		
    arrSceneDepth2[0].y = tex2D( sceneDepthSamplerAO, IN.ScreenTC.xy + vIrrSample.xy ).r + vIrrSample.z;  
    if (bHQ)
    {
      vIrrSample.xyz *= fHQScale;
      arrSceneDepth2[1].y = tex2D( sceneDepthSamplerAO, IN.ScreenTC.xy + vIrrSample.xy ).r + vIrrSample.z;  
    }

    vIrrSample = mirror(arrKernel[i*4+2], rotSample) * vSampleScale;		
    arrSceneDepth2[0].z = tex2D( sceneDepthSamplerAO, IN.ScreenTC.xy + vIrrSample.xy ).r + vIrrSample.z;  
    if (bHQ)
    {
      vIrrSample.xyz *= fHQScale;
      arrSceneDepth2[1].z = tex2D( sceneDepthSamplerAO, IN.ScreenTC.xy + vIrrSample.xy ).r + vIrrSample.z;  
    }

    vIrrSample = mirror(arrKernel[i*4+3], rotSample) * vSampleScale;		
    arrSceneDepth2[0].w = tex2D( sceneDepthSamplerAO, IN.ScreenTC.xy + vIrrSample.xy ).r + vIrrSample.z;  
    if (bHQ)
    {
      vIrrSample.xyz *= fHQScale;
      arrSceneDepth2[1].w = tex2D( sceneDepthSamplerAO, IN.ScreenTC.xy + vIrrSample.xy ).r + vIrrSample.z;  
    }

    float fDefVal = 0.55f;

    for(int s=0; s<(bHQ ? 2 : 1); s++)
    {
      vDistance = fSceneDepth - arrSceneDepth2[s]; 
      float4 vDistanceScaled = vDistance * fDepthRangeScale;
      fRangeIsInvalid = (saturate( abs(vDistanceScaled) ) + saturate( vDistanceScaled ))/2;  
      vSkyAccess += lerp(saturate((-vDistance)*fDepthTestSoftness), fDefVal, fRangeIsInvalid);
    }
  }

  OUT.Color = dot( vSkyAccess, (bHQ ? 1/16.0f : 1/8.0f)*2.0 ) - SSAO_params.y; // 0.075f
  OUT.Color = saturate(lerp( 0.9f, OUT.Color, SSAO_params.x ));
  
	return OUT;
}

pixout_cl Deferred_TerrainAO_Pass_PS(vert2fragSSAO IN)
{
  pixout_cl OUT;

  // reconstruct pixel world position
  half SceneDepth = tex2D( depthTargetSampler, IN.ScreenTC.xy ).r;  
	float3 vWSPos = vfViewPos.xyz + IN.WS_ViewVect * SceneDepth;

  // find terrain texture coordinates
  float2 texCoord = float2((vWSPos.y-AOSectorRange.y), (vWSPos.x-AOSectorRange.x)) * TerrainAOInfo.w;

  // get terrain and vegetation elevations
	half4 dataS0 = tex2D( TerrainInfoSampler0, texCoord );
	half4 dataS1 = tex2D( TerrainInfoSampler1, texCoord );
	half fTerrainZ = dataS1.a*(AOSectorRange.w-AOSectorRange.z)+AOSectorRange.z;
	half fVegetZMax = fTerrainZ + dataS1.g*32.f;

  // get initial sky amount, TODO: try pow() here
	OUT.Color = saturate(1.f-TerrainAOInfo.g*(fVegetZMax-vWSPos.z)); 

  // scale based on sky amount precomputed for terrain
	half fTerrainSkyAmount = dataS0.a * saturate(1.f - (fTerrainZ-vWSPos.z)*0.025f);
  OUT.Color = lerp(OUT.Color,1.f,fTerrainSkyAmount);

  // lerp into pure terrain sky amount near the ground
  half fHeightFactor = saturate((vWSPos.z-fTerrainZ)*0.5f);
  OUT.Color = lerp(fTerrainSkyAmount,OUT.Color,fHeightFactor);

  // apply sky brightening and fade on distance
  half fDistAtt = saturate(pow(SceneDepth*PS_NearFarClipDist.y/1024.f,3));
	OUT.Color = lerp(1.f, OUT.Color, (1.f - TerrainAOInfo.r)*(1.f - fDistAtt)); 

  return OUT;
}*/