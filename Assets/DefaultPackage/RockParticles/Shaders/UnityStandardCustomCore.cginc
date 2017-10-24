#ifndef UNITY_STANDARD_CORE_CUSTOM_INCLUDED
#define UNITY_STANDARD_CORE_CUSTOM_INCLUDED

#include "../../Plugins/builtin_shaders-5.6.0f3/CGIncludes/UnityCG.cginc"
#include "../../Plugins/builtin_shaders-5.6.0f3/CGIncludes/UnityShaderVariables.cginc"
#include "../../Plugins/builtin_shaders-5.6.0f3/CGIncludes/UnityInstancing.cginc"
#include "../../Plugins/builtin_shaders-5.6.0f3/CGIncludes/UnityStandardConfig.cginc"
#include "../../Plugins/builtin_shaders-5.6.0f3/CGIncludes/UnityStandardInput.cginc"
#include "../../Plugins/builtin_shaders-5.6.0f3/CGIncludes/UnityPBSLighting.cginc"
#include "../../Plugins/builtin_shaders-5.6.0f3/CGIncludes/UnityStandardUtils.cginc"
#include "../../Plugins/builtin_shaders-5.6.0f3/CGIncludes/UnityStandardBRDF.cginc"
#include "../../Plugins/builtin_shaders-5.6.0f3/CGIncludes/AutoLight.cginc"
#include "SimplexNoiseGrad3D.cginc"

//-------------------------------------------------------------------------------------
// counterpart for NormalizePerPixelNormal
// skips normalization per-vertex and expects normalization to happen per-pixel
half3 NormalizePerVertexNormal (float3 n) // takes float to avoid overflow
{
    #if (SHADER_TARGET < 30) || UNITY_STANDARD_SIMPLE
        return normalize(n);
    #else
        return n; // will normalize per-pixel instead
    #endif
}

half3 NormalizePerPixelNormal (half3 n)
{
	#if (SHADER_TARGET < 30) || UNITY_STANDARD_SIMPLE
		return n;
	#else
		return normalize(n);
	#endif
}


//-------------------------------------------------------------------------------------
UnityLight MainLight (half3 normalWorld)
{
	UnityLight l;
	#ifdef LIGHTMAP_OFF
		
		l.color = _LightColor0.rgb;
		l.dir = _WorldSpaceLightPos0.xyz;
		l.ndotl = LambertTerm (normalWorld, l.dir);
	#else
		// no light specified by the engine
		// analytical light might be extracted from Lightmap data later on in the shader depending on the Lightmap type
		l.color = half3(0.f, 0.f, 0.f);
		l.ndotl  = 0.f;
		l.dir = half3(0.f, 0.f, 0.f);
	#endif

	return l;
}

UnityLight AdditiveLight (half3 normalWorld, half3 lightDir, half atten)
{
	UnityLight l;

	l.color = _LightColor0.rgb;
	l.dir = lightDir;
	#ifndef USING_DIRECTIONAL_LIGHT
		l.dir = NormalizePerPixelNormal(l.dir);
	#endif
	l.ndotl = LambertTerm (normalWorld, l.dir);

	// shadow the light
	l.color *= atten;
	return l;
}

UnityLight DummyLight (half3 normalWorld)
{
	UnityLight l;
	l.color = 0;
	l.dir = half3 (0,1,0);
	l.ndotl = LambertTerm (normalWorld, l.dir);
	return l;
}

UnityIndirect ZeroIndirect ()
{
	UnityIndirect ind;
	ind.diffuse = 0;
	ind.specular = 0;
	return ind;
}

//-------------------------------------------------------------------------------------
// Common fragment setup

// deprecated
half3 WorldNormal(half4 tan2world[3])
{
	return normalize(tan2world[2].xyz);
}

// deprecated
#ifdef _TANGENT_TO_WORLD
	half3x3 ExtractTangentToWorldPerPixel(half4 tan2world[3])
	{
		half3 t = tan2world[0].xyz;
		half3 b = tan2world[1].xyz;
		half3 n = tan2world[2].xyz;

	#if UNITY_TANGENT_ORTHONORMALIZE
		n = NormalizePerPixelNormal(n);

		// ortho-normalize Tangent
		t = normalize (t - n * dot(t, n));

		// recalculate Binormal
		half3 newB = cross(n, t);
		b = newB * sign (dot (newB, b));
	#endif

		return half3x3(t, b, n);
	}
#else
	half3x3 ExtractTangentToWorldPerPixel(half4 tan2world[3])
	{
		return half3x3(0,0,0,0,0,0,0,0,0);
	}
#endif

half3 PerPixelWorldNormal(float4 i_tex, half4 tangentToWorld[3])
{
#ifdef _NORMALMAP
	half3 tangent = tangentToWorld[0].xyz;
	half3 binormal = tangentToWorld[1].xyz;
	half3 normal = tangentToWorld[2].xyz;

	#if UNITY_TANGENT_ORTHONORMALIZE
		normal = NormalizePerPixelNormal(normal);

		// ortho-normalize Tangent
		tangent = normalize (tangent - normal * dot(tangent, normal));

		// recalculate Binormal
		half3 newB = cross(normal, tangent);
		binormal = newB * sign (dot (newB, binormal));
	#endif

	half3 normalTangent = NormalInTangentSpace(i_tex);
	half3 normalWorld = NormalizePerPixelNormal(tangent * normalTangent.x + binormal * normalTangent.y + normal * normalTangent.z); // @TODO: see if we can squeeze this normalize on SM2.0 as well
#else
	half3 normalWorld = normalize(tangentToWorld[2].xyz);
#endif
	return normalWorld;
}

#ifdef _PARALLAXMAP
	#define IN_VIEWDIR4PARALLAX(i) NormalizePerPixelNormal(half3(i.tangentToWorldAndParallax[0].w,i.tangentToWorldAndParallax[1].w,i.tangentToWorldAndParallax[2].w))
	#define IN_VIEWDIR4PARALLAX_FWDADD(i) NormalizePerPixelNormal(i.viewDirForParallax.xyz)
#else
	#define IN_VIEWDIR4PARALLAX(i) half3(0,0,0)
	#define IN_VIEWDIR4PARALLAX_FWDADD(i) half3(0,0,0)
#endif

#if UNITY_SPECCUBE_BOX_PROJECTION || UNITY_LIGHT_PROBE_PROXY_VOLUME
	#define IN_WORLDPOS(i) i.posWorld
#else
	#define IN_WORLDPOS(i) half3(0,0,0)
#endif

#define IN_LIGHTDIR_FWDADD(i) half3(i.tangentToWorldAndLightDir[0].w, i.tangentToWorldAndLightDir[1].w, i.tangentToWorldAndLightDir[2].w)

#define FRAGMENT_SETUP(x) FragmentCommonData x = \
	FragmentSetup(i.tex, i.eyeVec, IN_VIEWDIR4PARALLAX(i), i.tangentToWorldAndParallax, IN_WORLDPOS(i));

#define FRAGMENT_SETUP_FWDADD(x) FragmentCommonData x = \
	FragmentSetup(i.tex, i.eyeVec, IN_VIEWDIR4PARALLAX_FWDADD(i), i.tangentToWorldAndLightDir, half3(0,0,0));

struct FragmentCommonData
{
	half3 diffColor, specColor;
	// Note: oneMinusRoughness & oneMinusReflectivity for optimization purposes, mostly for DX9 SM2.0 level.
	// Most of the math is being done on these (1-x) values, and that saves a few precious ALU slots.
	half oneMinusReflectivity, oneMinusRoughness;
	half3 normalWorld, eyeVec, posWorld;
	half alpha;

#if UNITY_OPTIMIZE_TEXCUBELOD || UNITY_STANDARD_SIMPLE
	half3 reflUVW;
#endif

#if UNITY_STANDARD_SIMPLE
	half3 tangentSpaceNormal;
#endif
};

#ifndef UNITY_SETUP_BRDF_INPUT
	#define UNITY_SETUP_BRDF_INPUT SpecularSetup
#endif

inline FragmentCommonData SpecularSetup (float4 i_tex)
{
	half4 specGloss = SpecularGloss(i_tex.xy);
	half3 specColor = specGloss.rgb;
	half oneMinusRoughness = specGloss.a;

	half oneMinusReflectivity;
	half3 diffColor = EnergyConservationBetweenDiffuseAndSpecular (Albedo(i_tex), specColor, /*out*/ oneMinusReflectivity);
	
	FragmentCommonData o = (FragmentCommonData)0;
	o.diffColor = diffColor;
	o.specColor = specColor;
	o.oneMinusReflectivity = oneMinusReflectivity;
	o.oneMinusRoughness = oneMinusRoughness;
	return o;
}

inline FragmentCommonData MetallicSetup (float4 i_tex)
{
	half2 metallicGloss = MetallicGloss(i_tex.xy);
	half metallic = metallicGloss.x;
	half oneMinusRoughness = metallicGloss.y;		// this is 1 minus the square root of real roughness m.

	half oneMinusReflectivity;
	half3 specColor;
	half3 diffColor = DiffuseAndSpecularFromMetallic (Albedo(i_tex), metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

	FragmentCommonData o = (FragmentCommonData)0;
	o.diffColor = diffColor;
	o.specColor = specColor;
	o.oneMinusReflectivity = oneMinusReflectivity;
	o.oneMinusRoughness = oneMinusRoughness;
	return o;
} 

inline FragmentCommonData FragmentSetup (float4 i_tex, half3 i_eyeVec, half3 i_viewDirForParallax, half4 tangentToWorld[3], half3 i_posWorld)
{
	i_tex = Parallax(i_tex, i_viewDirForParallax);

	half alpha = Alpha(i_tex.xy);
	#if defined(_ALPHATEST_ON)
		clip (alpha - _Cutoff);
	#endif

	FragmentCommonData o = UNITY_SETUP_BRDF_INPUT (i_tex);
	o.normalWorld = PerPixelWorldNormal(i_tex, tangentToWorld);
	o.eyeVec = NormalizePerPixelNormal(i_eyeVec);
	o.posWorld = i_posWorld;

	// NOTE: shader relies on pre-multiply alpha-blend (_SrcBlend = One, _DstBlend = OneMinusSrcAlpha)
	o.diffColor = PreMultiplyAlpha (o.diffColor, alpha, o.oneMinusReflectivity, /*out*/ o.alpha);
	return o;
}

inline UnityGI FragmentGI (FragmentCommonData s, half occlusion, half4 i_ambientOrLightmapUV, half atten, UnityLight light, bool reflections)
{
    UnityGIInput d;
    d.light = light;
    d.worldPos = s.posWorld;
    d.worldViewDir = -s.eyeVec;
    d.atten = atten;
    #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
        d.ambient = 0;
        d.lightmapUV = i_ambientOrLightmapUV;
    #else
        d.ambient = i_ambientOrLightmapUV.rgb;
        d.lightmapUV = 0;
    #endif

    d.probeHDR[0] = unity_SpecCube0_HDR;
    d.probeHDR[1] = unity_SpecCube1_HDR;
    #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
      d.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
    #endif
    #ifdef UNITY_SPECCUBE_BOX_PROJECTION
      d.boxMax[0] = unity_SpecCube0_BoxMax;
      d.probePosition[0] = unity_SpecCube0_ProbePosition;
      d.boxMax[1] = unity_SpecCube1_BoxMax;
      d.boxMin[1] = unity_SpecCube1_BoxMin;
      d.probePosition[1] = unity_SpecCube1_ProbePosition;
    #endif

    if(reflections)
    {
        Unity_GlossyEnvironmentData g = UnityGlossyEnvironmentSetup(1.0, -s.eyeVec, s.normalWorld, s.specColor);
        // Replace the reflUVW if it has been compute in Vertex shader. Note: the compiler will optimize the calcul in UnityGlossyEnvironmentSetup itself
        #if UNITY_STANDARD_SIMPLE
            g.reflUVW = s.reflUVW;
        #endif

        return UnityGlobalIllumination (d, occlusion, s.normalWorld, g);
    }
    else
    {
        return UnityGlobalIllumination (d, occlusion, s.normalWorld);
    }
}

inline UnityGI FragmentGI (FragmentCommonData s, half occlusion, half4 i_ambientOrLightmapUV, half atten, UnityLight light)
{
	return FragmentGI(s, occlusion, i_ambientOrLightmapUV, atten, light, true);
}


//-------------------------------------------------------------------------------------
half4 OutputForward (half4 output, half alphaFromSurface)
{
	#if defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)
		output.a = alphaFromSurface;
	#else
		UNITY_OPAQUE_ALPHA(output.a);
	#endif
	return output;
}

inline half4 VertexGIForward(VertexInput v, float3 posWorld, half3 normalWorld)
{
	half4 ambientOrLightmapUV = 0;
	// Static lightmaps
	#ifndef LIGHTMAP_OFF
		ambientOrLightmapUV.xy = v.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
		ambientOrLightmapUV.zw = 0;
	// Sample light probe for Dynamic objects only (no static or dynamic lightmaps)
	#elif UNITY_SHOULD_SAMPLE_SH
		#ifdef VERTEXLIGHT_ON
			// Approximated illumination from non-important point lights
			ambientOrLightmapUV.rgb = Shade4PointLights (
				unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
				unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
				unity_4LightAtten0, posWorld, normalWorld);
		#endif

		ambientOrLightmapUV.rgb = ShadeSHPerVertex (normalWorld, ambientOrLightmapUV.rgb);		
	#endif

	#ifdef DYNAMICLIGHTMAP_ON
		ambientOrLightmapUV.zw = v.uv2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
	#endif

	return ambientOrLightmapUV;
}

// ------------------------------------------------------------------
//  Base forward pass (directional light, emission, lightmaps, ...)

struct VertexOutputForwardBase
{
	float4 pos							: SV_POSITION;
	float4 tex							: TEXCOORD0;
	half3 eyeVec 						: TEXCOORD1;
	half4 tangentToWorldAndParallax[3]	: TEXCOORD2;	// [3x3:tangentToWorld | 1x3:viewDirForParallax]
	half4 ambientOrLightmapUV			: TEXCOORD5;	// SH or Lightmap UV
	SHADOW_COORDS(6)
	UNITY_FOG_COORDS(7)

	// next ones would not fit into SM2.0 limits, but they are always for SM3.0+
	#if UNITY_SPECCUBE_BOX_PROJECTION || UNITY_LIGHT_PROBE_PROXY_VOLUME
		float3 posWorld					: TEXCOORD8;
	#endif

	#if UNITY_OPTIMIZE_TEXCUBELOD
		#if UNITY_SPECCUBE_BOX_PROJECTION
			half3 reflUVW				: TEXCOORD9;
		#else
			half3 reflUVW				: TEXCOORD8;
		#endif
	#endif


	//
	//Added
	//
	//
	//
	#if defined(FIREBALL_SHADER)
	half3 originPos				: TEXCOORD10;
	#endif
	//
	//
	//
	//
	//


	UNITY_VERTEX_OUTPUT_STEREO
};

//
//Here
//
//Vertex Let's custom!!
//
//
//
VertexOutputForwardBase vertForwardBase (VertexInput v)
{
	UNITY_SETUP_INSTANCE_ID(v);
	VertexOutputForwardBase o;
	UNITY_INITIALIZE_OUTPUT(VertexOutputForwardBase, o);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

	//Add
	//
	//
	//
	#if defined(BLENDER_COORD)

		#if defined(SPIRAL_SHADER)
	    float y = v.vertex.y;
	    float x = v.vertex.x;
	    float z = v.vertex.z;
	    float m = sqrt(x * x + y * y);
	    float theta = z + m * _AmpMagnitude;
	    v.vertex.x = x * cos(theta * _Amount) - y * sin(theta * _Amount);
	    v.vertex.y = x * sin(theta * _Amount) + y * cos(theta * _Amount);
	    #endif

	    #if defined(THREEDGLITCH_SHADER)
	   	float x = v.vertex.x;
	    float z = v.vertex.z;
	    float y = v.vertex.y;
	    float cutNum = floor(z / _Cutsize);
	    float theta = _Phase +  _PhasePerCut * cutNum;
	    v.vertex.x = x * cos(theta) - y * sin(theta) + sin(_Timer * 2.0 + cutNum * 2.0);
	    v.vertex.y = x * sin(theta) + y * cos(theta);
	    #endif

	    #if defined(SPHERE_SHADER)
	    float3 center = float3(_CenterX, _CenterY, _CenterZ);
	    float3 ver = v.vertex.xyz - center.xyz;
	    float3 target = ver / sqrt(ver.x * ver.x + ver.y * ver.y + ver.z * ver.z) * _Radius + center;
	    v.vertex.xyz = v.vertex.xyz +  (target - v.vertex.xyz) * sqrt(_Amount);
	    #endif

	    #if defined(SLIDE_SHADER)
	    if(v.vertex.z > _SlideCutoff){	
	      	v.vertex.x += _Amount;
	    }
	    #endif

	    #if defined(SLITSCAN_SHADER)
	    v.vertex.x += _Amount * sin(v.vertex.z * 3.14 + _Phase);
	    #endif

	    #if defined(SPLINTER_SHADER)
	   	float2 _offset = float2(_OffsetX,_OffsetY);
		float2 _pos = float2((v.vertex.xz) / 0.0001) + _offset;
	    float3 c = tex2Dlod(_SplintMap, float4(_pos, 0, 0)).rgb;
	    v.vertex.xzy += v.normal * ((c.x + c.y + c.z) / 3.0) * _Amount;
	    #endif

	    #if defined(FIREBALL_SHADER)
	    float3 pos = v.vertex.xyz;
	    pos.xy += float2(cos(_Time.x * 4.0) * pos.x * 10.0,sin(_Time.x * 4.0) * pos.y * 10.0);
	   	float3 noise = (snoise_grad(pos * 42.324804580243) * pow(snoise(pos * 153.489320570), 2.0)) * 0.3 * v.vertex.z;
	    v.vertex.xyz += float3(noise.x * 0.5, noise.y * 0.5, noise.z); 
		float2 wave = float2(0, 0);
	    for(int i = 0; i < 8; i++){
	    	 wave += float2(cos(v.vertex.x * i + _Time.y) * noise.x * 0.01, sin(v.vertex.y * i + _Time.y) * noise.y * 0.01);
	    }
	    v.vertex.xy += wave;
	    if(v.vertex.z > 0){
	    	v.vertex.xy +=  v.vertex.z * 0.2 * float2(cos(v.vertex.z * 50.0 + _Time.y + snoise(pos * 43.489320570)), sin(v.vertex.z * 50.0 + _Time.y + snoise(pos * 123.489320570)));
	    }
			#if defined(FIREBALL_REVERSE)
		    	float3 timeWave = 0.01 * float3(sin(_Time.y) ,-cos(_Time.y * 1.02),-sin(_Time.y * 2.01) * cos(_Time.y * 1.51));
		    #else 
		    	float3 timeWave = 0.01 * float3(sin(_Time.y), cos(_Time.y * 1.02),sin(_Time.y * 2.01) * cos(_Time.y * 1.51));
		    #endif
	    v.vertex.xyz += timeWave;
	    #endif
	#else
		#if defined(SPIRAL_SHADER)
	    float z = v.vertex.z;
	    float x = v.vertex.x;
	    float y = v.vertex.y;
	    float m = sqrt(x * x + z * z);
	    float theta = y + m * _AmpMagnitude;
	    v.vertex.x = x * cos(theta * _Amount) - z * sin(theta * _Amount);
	    v.vertex.z = x * sin(theta * _Amount) + z * cos(theta * _Amount);
	    #endif

	    #if defined(THREEDGLITCH_SHADER)
	   	float x = v.vertex.x;
	    float y = v.vertex.y;
	    float z = v.vertex.z;
	    float cutNum = floor(y / _Cutsize);
	    float theta = _Phase +  _PhasePerCut * cutNum;
	    v.vertex.x = x * cos(theta) - z * sin(theta) + sin(_Timer * 2.0 + cutNum * 2.0);
	    v.vertex.z = x * sin(theta) + z * cos(theta);
	    #endif

	    #if defined(SPHERE_SHADER)
	    float3 center = float3(_CenterX, _CenterY, _CenterZ);
	    float3 ver = v.vertex.xyz - center.xyz;
	    float3 target = ver / sqrt(ver.x * ver.x + ver.y * ver.y + ver.z * ver.z) * _Radius + center;
	    v.vertex.xyz = v.vertex.xyz +  (target - v.vertex.xyz) * sqrt(_Amount);
	    #endif

	    #if defined(SLIDE_SHADER)
	    if(v.vertex.y > _SlideCutoff){	
	      	v.vertex.x += _Amount;
	    }
	    #endif

	    #if defined(SLITSCAN_SHADER)
	    v.vertex.x += _Amount * sin(v.vertex.y * 3.14 + _Phase);
	    #endif

	    #if defined(SPLINTER_SHADER)
	   	float2 _offset = float2(_OffsetX,_OffsetY);
		float2 _pos = float2((v.vertex.xy) / 0.0001) + _offset;
	    float3 c = tex2Dlod(_SplintMap, float4(_pos, 0, 0)).rgb;
	    v.vertex.xyz += v.normal * ((c.x + c.y + c.z) / 3.0) * _Amount;
	    #endif

	    #if defined(FIREBALL_SHADER)
	    float3 pos = v.vertex.xyz;
	    pos.xy += float2(cos(_Time.x * 4.0) * pos.x * 10.0,sin(_Time.x * 4.0) * pos.y * 10.0);
	   	float3 noise = (snoise_grad(pos * 42.324804580243) * pow(snoise(pos * 153.489320570), 2.0)) * 0.3 * v.vertex.z;
	    v.vertex.xyz += float3(noise.x * 0.5, noise.y * 0.5, noise.z); 
		float2 wave = float2(0, 0);
	    for(int i = 0; i < 8; i++){
	    	 wave += float2(cos(v.vertex.x * i + _Time.y) * noise.x * 0.01, sin(v.vertex.y * i + _Time.y) * noise.y * 0.01);
	    }
	    v.vertex.xy += wave;
	    if(v.vertex.z > 0){
	    	v.vertex.xy +=  v.vertex.z * 0.2 * float2(cos(v.vertex.z * 50.0 + _Time.y + snoise(pos * 43.489320570)), sin(v.vertex.z * 50.0 + _Time.y + snoise(pos * 123.489320570)));
	    }
			#if defined(FIREBALL_REVERSE)
		    	float3 timeWave = 0.01 * float3(sin(_Time.y) ,-cos(_Time.y * 1.02),-sin(_Time.y * 2.01) * cos(_Time.y * 1.51));
		    #else 
		    	float3 timeWave = 0.01 * float3(sin(_Time.y), cos(_Time.y * 1.02),sin(_Time.y * 2.01) * cos(_Time.y * 1.51));
		    #endif
	    v.vertex.xyz += timeWave;
	    #endif
	#endif

	#if defined(ROCKPARTICLE_SHADER)


	#endif
    //
    //
    //
    //EndAdd

	float4 posWorld = mul(unity_ObjectToWorld, v.vertex);
	#if UNITY_SPECCUBE_BOX_PROJECTION || UNITY_LIGHT_PROBE_PROXY_VOLUME
		o.posWorld = posWorld.xyz;
	#endif
	o.pos = UnityObjectToClipPos(v.vertex);

	#if defined(FIREBALL_SHADER)
	o.originPos = v.vertex;
	#endif
		
	o.tex = TexCoords(v);
	o.eyeVec = NormalizePerVertexNormal(posWorld.xyz - _WorldSpaceCameraPos);
	float3 normalWorld = UnityObjectToWorldNormal(v.normal);
	#ifdef _TANGENT_TO_WORLD
		float4 tangentWorld = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);

		float3x3 tangentToWorld = CreateTangentToWorldPerVertex(normalWorld, tangentWorld.xyz, tangentWorld.w);
		o.tangentToWorldAndParallax[0].xyz = tangentToWorld[0];
		o.tangentToWorldAndParallax[1].xyz = tangentToWorld[1];
		o.tangentToWorldAndParallax[2].xyz = tangentToWorld[2];
	#else
		o.tangentToWorldAndParallax[0].xyz = 0;
		o.tangentToWorldAndParallax[1].xyz = 0;
		o.tangentToWorldAndParallax[2].xyz = normalWorld;
	#endif
	//We need this for shadow receving
	TRANSFER_SHADOW(o);

	o.ambientOrLightmapUV = VertexGIForward(v, posWorld, normalWorld);
	
	#ifdef _PARALLAXMAP
		TANGENT_SPACE_ROTATION;
		half3 viewDirForParallax = mul (rotation, ObjSpaceViewDir(v.vertex));
		o.tangentToWorldAndParallax[0].w = viewDirForParallax.x;
		o.tangentToWorldAndParallax[1].w = viewDirForParallax.y;
		o.tangentToWorldAndParallax[2].w = viewDirForParallax.z;
	#endif

	#if UNITY_OPTIMIZE_TEXCUBELOD
		o.reflUVW 		= reflect(o.eyeVec, normalWorld);
	#endif

	UNITY_TRANSFER_FOG(o,o.pos);
	return o;
}

half4 fragForwardBaseInternal (VertexOutputForwardBase i)
{
	FRAGMENT_SETUP(s)
#if UNITY_OPTIMIZE_TEXCUBELOD
	s.reflUVW		= i.reflUVW;
#endif

	UnityLight mainLight = MainLight (s.normalWorld);
	half atten = SHADOW_ATTENUATION(i);


	half occlusion = Occlusion(i.tex.xy);
	UnityGI gi = FragmentGI (s, occlusion, i.ambientOrLightmapUV, atten, mainLight);

	half4 c = UNITY_BRDF_PBS (s.diffColor, s.specColor, s.oneMinusReflectivity, s.oneMinusRoughness, s.normalWorld, -s.eyeVec, gi.light, gi.indirect);
	c.rgb += UNITY_BRDF_GI (s.diffColor, s.specColor, s.oneMinusReflectivity, s.oneMinusRoughness, s.normalWorld, -s.eyeVec, occlusion, gi);
	c.rgb += Emission(i.tex.xy);

	UNITY_APPLY_FOG(i.fogCoord, c.rgb);

	//Add Frag cord here
	//
	//
	//
	//
	//

	#if defined(FIREBALL_SHADER)

	//Maybe main color

	c.rgb += float3(1.0, 1.0, 1.0) / ((abs(i.originPos.z) * 10.0) * (sqrt(pow(i.originPos.x, 2) + pow(i.originPos.y, 2)) * 300.0)) * 0.075;
	c.rgb += snoise_grad(i.originPos * 100) * 0.15;
	float alpha = 1.0 / ((abs(i.originPos.z) * 250.0) * (sqrt(pow(i.originPos.x, 2) + pow(i.originPos.y, 2)) * 50.0)) * 0.0001;
	//float alpha = 0;
	return OutputForward (c, alpha);
	#endif



	#if defined(TESTPATTERN_SHADER)

	i.tex.xy = i.tex.yx;
	//Speed is wave speed.
	float speed = _Speed;

	//Devide is screen devide num of y axis.
	float screenDevide = max(1, floor(_ScreenDevide));
	int devided = int(floor(i.tex.y * screenDevide)) ; //basically 0 or 1. 1 is phase reverse movement


	//Pattern is 0~7 // kick + 1, snere + 2 , hihat + 4
	//0:use navy and flesh color, sub screen devided 6 region and slow slide
	//1:same pattern 0 colors but more devided
	//2:add pattern 0, flash navy color 
	//3:mix 0 and 2
	//4:Low freq + mix 6 slight pattern //mix 0 and 6 // 
	//5:mix 1 and 7
	//6:use navy, flesh and red in retio 3:2:1
	//7:same pattern 3 colors and flash flesh color
	//c.rgb =  float3(,fmod(floor(i.tex.y * screenDevide), 2.0),fmod(floor(i.tex.y * screenDevide), 2.0));

	float x;
	if(devided % 2 == 0){
		x = i.tex.x + _Time.y;
	}else{
		x = 1.0 - i.tex.x + _Time.y;
	}

	float powerFreq =  _PowerFreq * 0.01;//0.02 
	//if(_PowerFreq < 500.0){
	//	powerFreq = 500.0 * 0.01;
	//}

	int pattern = int(floor(_Pattern)); 
	int xCol = abs(int(fmod(floor(x * powerFreq * 0.6), 2.0))); //0 or 1; //PowerFreq 0 - 1023?

	switch(pattern){
		case 0:
			//Low Freq
			xCol = abs(int(fmod(floor(x * powerFreq * 0.6), 2.0)));
			if(xCol == 0){
				c.rgb = _NavyBlue.rgb;
			}else{
				c.rgb = _FleshColor.rgb;
			}
			break;
		
		case 1:
			//Low Freq
			xCol = abs(int(fmod(floor(x * powerFreq * 0.6), 2.0)));
			if(xCol == 0){
				c.rgb = _NavyBlue.rgb;
			}else{
				c.rgb = _FleshColor.rgb;
			}

			//if(fmod(floor(_Time.z * 5.0), 2.0) == devided % 2){
			if(fmod(floor(_Time.y * 5.0), 2.0) == devided % 2){
				c.rgb = _NavyBlue.rgb;
			}
			break;

		case 2:
			//Low Freq
			if(devided % 2 == 0){
				xCol = abs(int(fmod(floor(x * powerFreq * 0.6), 2.0)));
					if(xCol == 0){
						c.rgb = _NavyBlue.rgb;
					}else{
						c.rgb = _FleshColor.rgb;
				}
			}else{
				if(xCol == 0){
						c.rgb = _NavyBlue.rgb;
				}else{
						c.rgb = _FleshColor.rgb;
				}

				//if(fmod(floor(_Time.z * 5.0), 2.0) == devided % 2){
				if(fmod(floor(_Time.y * 5.0), 2.0) == devided % 2){
					c.rgb = _NavyBlue.rgb;
				}
			}

			break;
		case 3:
			//if(devided % 2 == 0){

				
				xCol = abs(int(fmod(floor(x * 4.0), 2.0))); //0 to 1;  devided into 6 devision


				switch(xCol){
					case 0:
						c.rgb = _NavyBlue.rgb;
						break;
					case 1:
						c.rgb = _FleshColor.rgb;
						break;
					default:
						break;
				}

				//Add slight line
				xCol = abs(int(fmod(floor(x * powerFreq * 3), 6.0))); //0 to 5; 

				//float sliteLineWave = sin(x * 10.0 * 3.14) * sin(x * 22.0 * 3.14 + 1.2) * sin(x * 5.0 * 3.14 + 2.7);
				float sliteLineWave = sin(x * powerFreq * 1.0 ) * sin(x * powerFreq * 2.2  + 1.2) * sin(x * powerFreq * 0.5  + 2.7);
				if(sliteLineWave < 0 && sliteLineWave > -0.2){
					c.rgb = _Green.rgb;

				}else if(sliteLineWave < 0 && sliteLineWave > -0.4){
					c.rgb = _Purple.rgb;
					c.rgb = _Red.rgb;
				}//else if(sliteLineWave < 0 && sliteLineWave > -0.3){
				//	c.rgb = _Red.rgb;
				//}else{}	

//				switch(xCol){
//					case 0:
//					case 1:
//					case 2:
//						c.rgb = _NavyBlue.rgb;
//						break;
//					case 3:
//					case 4:
//						c.rgb = _FleshColor.rgb;
//						break;
//					case 5:
//						c.rgb = _Red.rgb;
//						break;
//					default:
//						break;
//				}

			//}else{
			//	xCol = abs(int(fmod(floor(x * powerFreq * 0.3), 2.0)));
			//	if(xCol == 0){
			//		c.rgb = _NavyBlue.rgb;
			//	}else{
			//		c.rgb = _FleshColor.rgb;
			//	}
			//}

			break;

		case 4:
			//Middle Freq
			xCol = abs(int(fmod(floor(x * powerFreq), 2.0)));
			if(xCol == 0){
				c.rgb = _NavyBlue.rgb;
			}else{
				c.rgb = _FleshColor.rgb;
			}
			break;

		case 5:

			if(devided % 2 == 0){

				xCol = abs(int(fmod(floor(x * powerFreq), 2.0)));
				if(xCol == 0){
					c.rgb = _NavyBlue.rgb;
				}else{
					c.rgb = _FleshColor.rgb;
				}
			}else{
				xCol = abs(int(fmod(floor(x * powerFreq * 3), 6.0))); //0 to 5; 

				switch(xCol){
					case 0:
					case 1:
					case 2:
						c.rgb = _NavyBlue.rgb;
						break;
					case 3:
					case 4:
						c.rgb = _FleshColor.rgb;
						break;
					case 5:
						c.rgb = _Red.rgb;
						break;
					default:
						break;
				}

				//if(fmod(floor(_Time.z * 5.0), 2.0) == devided % 2){
				if(fmod(floor(_Time.y * 5.0), 2.0) == devided % 2){
					c.rgb = _FleshColor.rgb;
				}
			}
			
			break;
		case 6:

			xCol = abs(int(fmod(floor(x * powerFreq * 3), 6.0))); //0 to 5; 

			switch(xCol){
				case 0:
				case 1:
				case 2:
					c.rgb = _NavyBlue.rgb;
					break;
				case 3:
				case 4:
					c.rgb = _FleshColor.rgb;
					break;
				case 5:
					c.rgb = _Red.rgb;
					break;
				default:
					break;
			}

			break;
		case 7:
			xCol = abs(int(fmod(floor(x * powerFreq * 3), 6.0))); //0 to 5; 

			switch(xCol){
				case 0:
				case 1:
				case 2:
					c.rgb = _NavyBlue.rgb;
					break;
				case 3:
				case 4:
					c.rgb = _FleshColor.rgb;
					break;
				case 5:
					c.rgb = _Red.rgb;
					break;
				default:
					break;
			}

			//if(fmod(floor(_Time.z * 5.0), 2.0) == devided % 2){
			if(fmod(floor(_Time.y * 5.0), 2.0) == devided % 2){
				c.rgb = _FleshColor.rgb;
			}
		
			
			break;
		default:
			break;
	}
	return OutputForward (c, 0);
	#endif
	//
	//
	//
	//
	//End Add

	return OutputForward (c, s.alpha);
}

half4 fragForwardBase (VertexOutputForwardBase i) : SV_Target	// backward compatibility (this used to be the fragment entry function)
{
	return fragForwardBaseInternal(i);
}

// ------------------------------------------------------------------
//  Additive forward pass (one light per pass)

struct VertexOutputForwardAdd
{
	float4 pos							: SV_POSITION;
	float4 tex							: TEXCOORD0;
	half3 eyeVec 						: TEXCOORD1;
	half4 tangentToWorldAndLightDir[3]	: TEXCOORD2;	// [3x3:tangentToWorld | 1x3:lightDir]
	LIGHTING_COORDS(5,6)
	UNITY_FOG_COORDS(7)

	// next ones would not fit into SM2.0 limits, but they are always for SM3.0+
#if defined(_PARALLAXMAP)
	half3 viewDirForParallax			: TEXCOORD8;
#endif

		//
	//Added
	//
	//
	//
	#if defined(FIREBALL_SHADER)
	half3 originPos				: TEXCOORD10;
	#endif
	//
	//
	//
	//
	//

	UNITY_VERTEX_OUTPUT_STEREO
};


//AddHere!!!
//
//
//
//
//
VertexOutputForwardAdd vertForwardAdd (VertexInput v)
{

	//Add
	//
	//
	//
	#if defined(BLENDER_COORD)

		#if defined(SPIRAL_SHADER)
	    float y = v.vertex.y;
	    float x = v.vertex.x;
	    float z = v.vertex.z;
	    float m = sqrt(x * x + y * y);
	    float theta = z + m * _AmpMagnitude;
	    v.vertex.x = x * cos(theta * _Amount) - y * sin(theta * _Amount);
	    v.vertex.y = x * sin(theta * _Amount) + y * cos(theta * _Amount);
	    #endif

	    #if defined(THREEDGLITCH_SHADER)
	    float x = v.vertex.x;
	    float z = v.vertex.z;
	    float y = v.vertex.y;
	    float cutNum = floor(z / _Cutsize);
	    float theta = _Phase +  _PhasePerCut * cutNum;
	    v.vertex.x = x * cos(theta) - y * sin(theta) + sin(_Timer * 2.0 + cutNum * 2.0);
	    v.vertex.y = x * sin(theta) + y * cos(theta);
	    #endif

	    #if defined(SPHERE_SHADER)
	    float3 center = float3(_CenterX, _CenterY, _CenterZ);
	    float3 ver = v.vertex.xyz - center.xyz;
	    float3 target = ver / sqrt(ver.x * ver.x + ver.y * ver.y + ver.z * ver.z) * _Radius + center;
	    v.vertex.xyz = v.vertex.xyz +  (target - v.vertex.xyz) * sqrt(_Amount);
	    #endif

	    #if defined(SLIDE_SHADER)
	    if(v.vertex.z > _SlideCutoff){	
	      	v.vertex.x += _Amount;
	    }
	    #endif

	    #if defined(SLITSCAN_SHADER)
	    v.vertex.x += _Amount * sin(v.vertex.z * 3.14 + _Phase);
	    #endif

	    #if defined(SPLINTER_SHADER)
	   	float2 _offset = float2(_OffsetX,_OffsetY);
		float2 _pos = float2((v.vertex.xz) / 0.0001) + _offset;
	    float3 c = tex2Dlod(_SplintMap, float4(_pos, 0, 0)).rgb;
	    v.vertex.xzy += v.normal * ((c.x + c.y + c.z) / 3.0) * _Amount;
	    #endif
        
	    #if defined(FIREBALL_SHADER)

	    //bef
	    //float3 pos = v.vertex.xyz;
	    //pos.xz += float2(cos(_Time.x) * pos.x * 50.0,sin(_Time.x) * pos.z * 50.0);
	    //v.vertex.xyz += (snoise_grad(pos * 42.324804580243) * pow(snoise(pos * 153.489320570), 2.0)) * 0.3 * v.vertex.y; 

	    //new
	    float3 pos = v.vertex.xyz;
	    pos.xy += float2(cos(_Time.x * 4.0) * pos.x * 10.0,sin(_Time.x * 4.0) * pos.y * 10.0);
	   	float3 noise = (snoise_grad(pos * 42.324804580243) * pow(snoise(pos * 153.489320570), 2.0)) * 0.3 * v.vertex.z;
	    v.vertex.xyz += float3(noise.x * 0.5, noise.y * 0.5, noise.z); 
		float2 wave = float2(0, 0);
	    for(int i = 0; i < 8; i++){
	    	 wave += float2(cos(v.vertex.x * i + _Time.y) * noise.x * 0.01, sin(v.vertex.y * i + _Time.y) * noise.y * 0.01);
	    }
	    v.vertex.xy += wave;
	    if(v.vertex.z > 0){
	    	v.vertex.xy +=  v.vertex.z * 0.2 * float2(cos(v.vertex.z * 50.0 + _Time.y + snoise(pos * 43.489320570)), sin(v.vertex.z * 50.0 + _Time.y + snoise(pos * 123.489320570)));
	    }
			#if defined(FIREBALL_REVERSE)
		    	float3 timeWave = 0.01 * float3(sin(_Time.y) ,-cos(_Time.y * 1.02),-sin(_Time.y * 2.01) * cos(_Time.y * 1.51));
		    #else 
		    	float3 timeWave = 0.01 * float3(sin(_Time.y), cos(_Time.y * 1.02),sin(_Time.y * 2.01) * cos(_Time.y * 1.51));
		    #endif
	    v.vertex.xyz += timeWave;
	  
	    #endif
 	#else

		#if defined(SPIRAL_SHADER)
	    float z = v.vertex.z;
	    float x = v.vertex.x;
	    float y = v.vertex.y;
	    float m = sqrt(x * x + z * z);
	    float theta = y + m * _AmpMagnitude;
	    v.vertex.x = x * cos(theta * _Amount) - z * sin(theta * _Amount);
	    v.vertex.z = x * sin(theta * _Amount) + z * cos(theta * _Amount);
	    #endif

	    #if defined(THREEDGLITCH_SHADER)
	    float x = v.vertex.x;
	    float y = v.vertex.y;
	    float z = v.vertex.z;
	    float cutNum = floor(y / _Cutsize);
	    float theta = _Phase +  _PhasePerCut * cutNum;
	    v.vertex.x = x * cos(theta) - z * sin(theta) + sin(_Timer * 2.0 + cutNum * 2.0);
	    v.vertex.z = x * sin(theta) + z * cos(theta);
	    #endif

	    #if defined(SPHERE_SHADER)
	    float3 center = float3(_CenterX, _CenterY, _CenterZ);
	    float3 ver = v.vertex.xyz - center.xyz;
	    float3 target = ver / sqrt(ver.x * ver.x + ver.y * ver.y + ver.z * ver.z) * _Radius + center;
	    v.vertex.xyz = v.vertex.xyz +  (target - v.vertex.xyz) * sqrt(_Amount);
	    #endif

	    #if defined(SLIDE_SHADER)
	    if(v.vertex.y > _SlideCutoff){	
	      	v.vertex.x += _Amount;
	    }
	    #endif

	    #if defined(SLITSCAN_SHADER)
	    v.vertex.x += _Amount * sin(v.vertex.y * 3.14 + _Phase);
	    #endif

	    #if defined(SPLINTER_SHADER)
	   	float2 _offset = float2(_OffsetX,_OffsetY);
		float2 _pos = float2((v.vertex.xy) / 0.0001) + _offset;
	    float3 c = tex2Dlod(_SplintMap, float4(_pos, 0, 0)).rgb;
	    v.vertex.xyz += v.normal * ((c.x + c.y + c.z) / 3.0) * _Amount;
	    #endif
        
	    #if defined(FIREBALL_SHADER)
	    //bef
	    //float3 pos = v.vertex.xyz;
	    //pos.xy += float2(cos(_Time.x) * pos.x * 50.0,sin(_Time.x) * pos.y * 50.0);
	    //v.vertex.xyz += (snoise_grad(pos * 42.324804580243) * pow(snoise(pos * 153.489320570), 2.0)) * 0.3 * v.vertex.z; 
	    //new
	    float3 pos = v.vertex.xyz;
	    pos.xy += float2(cos(_Time.x * 4.0) * pos.x * 10.0,sin(_Time.x * 4.0) * pos.y * 10.0);
	   	float3 noise = (snoise_grad(pos * 42.324804580243) * pow(snoise(pos * 153.489320570), 2.0)) * 0.3 * v.vertex.z;
	    v.vertex.xyz += float3(noise.x * 0.5, noise.y * 0.5, noise.z); 
		float2 wave = float2(0, 0);
	    for(int i = 0; i < 8; i++){
	    	 wave += float2(cos(v.vertex.x * i + _Time.y) * noise.x * 0.01, sin(v.vertex.y * i + _Time.y) * noise.y * 0.01);
	    }
	    v.vertex.xy += wave;
	    if(v.vertex.z > 0){
	    	v.vertex.xy +=  v.vertex.z * 0.2 * float2(cos(v.vertex.z * 50.0 + _Time.y + snoise(pos * 43.489320570)), sin(v.vertex.z * 50.0 + _Time.y + snoise(pos * 123.489320570)));
	    }
			#if defined(FIREBALL_REVERSE)
		    	float3 timeWave = 0.01 * float3(sin(_Time.y) ,-cos(_Time.y * 1.02),-sin(_Time.y * 2.01) * cos(_Time.y * 1.51));
		    #else 
		    	float3 timeWave = 0.01 * float3(sin(_Time.y), cos(_Time.y * 1.02),sin(_Time.y * 2.01) * cos(_Time.y * 1.51));
		    #endif
	    v.vertex.xyz += timeWave;
	    #endif
 	#endif
    //
    //
    //
   	//
    //EndAdd


	VertexOutputForwardAdd o;
	UNITY_INITIALIZE_OUTPUT(VertexOutputForwardAdd, o);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

	float4 posWorld = mul(unity_ObjectToWorld, v.vertex);
	o.pos = UnityObjectToClipPos(v.vertex);

	//
	//
	//
	//Add
	//
	//
	#if defined(FIREBALL_SHADER)
	o.originPos = v.vertex.xyz;
	#endif
	//
	//
	//
	//End Add
	//
	//

	o.tex = TexCoords(v);
	o.eyeVec = NormalizePerVertexNormal(posWorld.xyz - _WorldSpaceCameraPos);
	float3 normalWorld = UnityObjectToWorldNormal(v.normal);
	#ifdef _TANGENT_TO_WORLD
		float4 tangentWorld = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);

		float3x3 tangentToWorld = CreateTangentToWorldPerVertex(normalWorld, tangentWorld.xyz, tangentWorld.w);
		o.tangentToWorldAndLightDir[0].xyz = tangentToWorld[0];
		o.tangentToWorldAndLightDir[1].xyz = tangentToWorld[1];
		o.tangentToWorldAndLightDir[2].xyz = tangentToWorld[2];
	#else
		o.tangentToWorldAndLightDir[0].xyz = 0;
		o.tangentToWorldAndLightDir[1].xyz = 0;
		o.tangentToWorldAndLightDir[2].xyz = normalWorld;
	#endif
	//We need this for shadow receiving
	TRANSFER_VERTEX_TO_FRAGMENT(o);

	float3 lightDir = _WorldSpaceLightPos0.xyz - posWorld.xyz * _WorldSpaceLightPos0.w;
	#ifndef USING_DIRECTIONAL_LIGHT
		lightDir = NormalizePerVertexNormal(lightDir);
	#endif
	o.tangentToWorldAndLightDir[0].w = lightDir.x;
	o.tangentToWorldAndLightDir[1].w = lightDir.y;
	o.tangentToWorldAndLightDir[2].w = lightDir.z;

	#ifdef _PARALLAXMAP
		TANGENT_SPACE_ROTATION;
		o.viewDirForParallax = mul (rotation, ObjSpaceViewDir(v.vertex));
	#endif
	
	UNITY_TRANSFER_FOG(o,o.pos);
	return o;
}

half4 fragForwardAddInternal (VertexOutputForwardAdd i)
{
	FRAGMENT_SETUP_FWDADD(s)

	UnityLight light = AdditiveLight (s.normalWorld, IN_LIGHTDIR_FWDADD(i), LIGHT_ATTENUATION(i));
	UnityIndirect noIndirect = ZeroIndirect ();

	half4 c = UNITY_BRDF_PBS (s.diffColor, s.specColor, s.oneMinusReflectivity, s.oneMinusRoughness, s.normalWorld, -s.eyeVec, light, noIndirect);
	
	UNITY_APPLY_FOG_COLOR(i.fogCoord, c.rgb, half4(0,0,0,0)); // fog towards black in additive pass

		//Add Frag cord here
	//
	//
	//
	//
	//

	#if defined(FIREBALL_SHADER)

	//bef
	//c.rgb = float3(1.0, 1.0, 1.0) * i.originPos;

	//new
	c.rgb += float3(1.0, 1.0, 1.0) / ((abs(i.originPos.z) * 10.0) * (sqrt(pow(i.originPos.x, 2) + pow(i.originPos.y, 2)) * 300.0)) * 0.075;
	c.rgb += snoise_grad(i.originPos * 100) * 0.15;
	float alpha = 1.0 / ((abs(i.originPos.z) * 250.0) * (sqrt(pow(i.originPos.x, 2) + pow(i.originPos.y, 2)) * 50.0)) * 0.0001;
	//float alpha = 0;
	return OutputForward (c, alpha);
	#endif


	#if defined(TESTPATTERN_SHADER)

	#endif
	//
	//
	//
	//
	//End Add
	//float4 pos							: SV_POSITION;
	//float4 tex							: TEXCOORD0;
	//half3 eyeVec 						: TEXCOORD1;
	//half4 tangentToWorldAndParallax[3]	: TEXCOORD2;	// [3x3:tangentToWorld | 1x3:viewDirForParallax]
	////half4 ambientOrLightmapUV			: TEXCOORD5;	// SH or Lightmap UV
	//SHADOW_COORDS(6)
	//UNITY_FOG_COORDS(7)

	return OutputForward (c, s.alpha);
}

half4 fragForwardAdd (VertexOutputForwardAdd i) : SV_Target		// backward compatibility (this used to be the fragment entry function)
{
	return fragForwardAddInternal(i);
}

// ------------------------------------------------------------------
//  Deferred pass

struct VertexOutputDeferred
{
	float4 pos							: SV_POSITION;
	float4 tex							: TEXCOORD0;
	half3 eyeVec 						: TEXCOORD1;
	half4 tangentToWorldAndParallax[3]	: TEXCOORD2;	// [3x3:tangentToWorld | 1x3:viewDirForParallax]
	half4 ambientOrLightmapUV			: TEXCOORD5;	// SH or Lightmap UVs			

	#if UNITY_SPECCUBE_BOX_PROJECTION || UNITY_LIGHT_PROBE_PROXY_VOLUME
		float3 posWorld						: TEXCOORD6;
	#endif

	#if UNITY_OPTIMIZE_TEXCUBELOD
		#if UNITY_SPECCUBE_BOX_PROJECTION
			half3 reflUVW				: TEXCOORD7;
		#else
			half3 reflUVW				: TEXCOORD6;
		#endif
	#endif

	UNITY_VERTEX_OUTPUT_STEREO
};


VertexOutputDeferred vertDeferred (VertexInput v)
{
	UNITY_SETUP_INSTANCE_ID(v);
	VertexOutputDeferred o;
	UNITY_INITIALIZE_OUTPUT(VertexOutputDeferred, o);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

	float4 posWorld = mul(unity_ObjectToWorld, v.vertex);
	#if UNITY_SPECCUBE_BOX_PROJECTION || UNITY_LIGHT_PROBE_PROXY_VOLUME
		o.posWorld = posWorld;
	#endif
	o.pos = UnityObjectToClipPos(v.vertex);

	o.tex = TexCoords(v);
	o.eyeVec = NormalizePerVertexNormal(posWorld.xyz - _WorldSpaceCameraPos);
	float3 normalWorld = UnityObjectToWorldNormal(v.normal);
	#ifdef _TANGENT_TO_WORLD
		float4 tangentWorld = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);

		float3x3 tangentToWorld = CreateTangentToWorldPerVertex(normalWorld, tangentWorld.xyz, tangentWorld.w);
		o.tangentToWorldAndParallax[0].xyz = tangentToWorld[0];
		o.tangentToWorldAndParallax[1].xyz = tangentToWorld[1];
		o.tangentToWorldAndParallax[2].xyz = tangentToWorld[2];
	#else
		o.tangentToWorldAndParallax[0].xyz = 0;
		o.tangentToWorldAndParallax[1].xyz = 0;
		o.tangentToWorldAndParallax[2].xyz = normalWorld;
	#endif

	o.ambientOrLightmapUV = 0;
	#ifndef LIGHTMAP_OFF
		o.ambientOrLightmapUV.xy = v.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
	#elif UNITY_SHOULD_SAMPLE_SH
		o.ambientOrLightmapUV.rgb = ShadeSHPerVertex (normalWorld, o.ambientOrLightmapUV.rgb);
	#endif
	#ifdef DYNAMICLIGHTMAP_ON
		o.ambientOrLightmapUV.zw = v.uv2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
	#endif
	
	#ifdef _PARALLAXMAP
		TANGENT_SPACE_ROTATION;
		half3 viewDirForParallax = mul (rotation, ObjSpaceViewDir(v.vertex));
		o.tangentToWorldAndParallax[0].w = viewDirForParallax.x;
		o.tangentToWorldAndParallax[1].w = viewDirForParallax.y;
		o.tangentToWorldAndParallax[2].w = viewDirForParallax.z;
	#endif

	#if UNITY_OPTIMIZE_TEXCUBELOD
		o.reflUVW		= reflect(o.eyeVec, normalWorld);
	#endif

	return o;
}

void fragDeferred (
	VertexOutputDeferred i,
	out half4 outDiffuse : SV_Target0,			// RT0: diffuse color (rgb), occlusion (a)
	out half4 outSpecSmoothness : SV_Target1,	// RT1: spec color (rgb), smoothness (a)
	out half4 outNormal : SV_Target2,			// RT2: normal (rgb), --unused, very low precision-- (a) 
	out half4 outEmission : SV_Target3			// RT3: emission (rgb), --unused-- (a)
)
{
	#if (SHADER_TARGET < 30)
		outDiffuse = 1;
		outSpecSmoothness = 1;
		outNormal = 0;
		outEmission = 0;
		return;
	#endif

	FRAGMENT_SETUP(s)
#if UNITY_OPTIMIZE_TEXCUBELOD
	s.reflUVW		= i.reflUVW;
#endif

	// no analytic lights in this pass
	UnityLight dummyLight = DummyLight (s.normalWorld);
	half atten = 1;

	// only GI
	half occlusion = Occlusion(i.tex.xy);
#if UNITY_ENABLE_REFLECTION_BUFFERS
	bool sampleReflectionsInDeferred = false;
#else
	bool sampleReflectionsInDeferred = true;
#endif

	UnityGI gi = FragmentGI (s, occlusion, i.ambientOrLightmapUV, atten, dummyLight, sampleReflectionsInDeferred);

	half3 color = UNITY_BRDF_PBS (s.diffColor, s.specColor, s.oneMinusReflectivity, s.oneMinusRoughness, s.normalWorld, -s.eyeVec, gi.light, gi.indirect).rgb;
	color += UNITY_BRDF_GI (s.diffColor, s.specColor, s.oneMinusReflectivity, s.oneMinusRoughness, s.normalWorld, -s.eyeVec, occlusion, gi);

	#ifdef _EMISSION
		color += Emission (i.tex.xy);
	#endif

	#ifndef UNITY_HDR_ON
		color.rgb = exp2(-color.rgb);
	#endif

	outDiffuse = half4(s.diffColor, occlusion);
	outSpecSmoothness = half4(s.specColor, s.oneMinusRoughness);
	outNormal = half4(s.normalWorld*0.5+0.5,1);
	outEmission = half4(color, 1);
}


//
// Old FragmentGI signature. Kept only for backward compatibility and will be removed soon
//

inline UnityGI FragmentGI(
	float3 posWorld,
	half occlusion, half4 i_ambientOrLightmapUV, half atten, half oneMinusRoughness, half3 normalWorld, half3 eyeVec,
	UnityLight light,
	bool reflections)
{
	// we init only fields actually used
	FragmentCommonData s = (FragmentCommonData)0;
	s.oneMinusRoughness = oneMinusRoughness;
	s.normalWorld = normalWorld;
	s.eyeVec = eyeVec;
	s.posWorld = posWorld;
#if UNITY_OPTIMIZE_TEXCUBELOD
	s.reflUVW = reflect(eyeVec, normalWorld);
#endif
	return FragmentGI(s, occlusion, i_ambientOrLightmapUV, atten, light, reflections);
}
inline UnityGI FragmentGI (
	float3 posWorld,
	half occlusion, half4 i_ambientOrLightmapUV, half atten, half oneMinusRoughness, half3 normalWorld, half3 eyeVec,
	UnityLight light)
{
	return FragmentGI (posWorld, occlusion, i_ambientOrLightmapUV, atten, oneMinusRoughness, normalWorld, eyeVec, light, true);
}

#endif // UNITY_STANDARD_CORE_INCLUDED
