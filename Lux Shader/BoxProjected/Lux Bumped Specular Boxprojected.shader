Shader "Lux/Box projected/Bumped Specular Boxprojected" {

Properties {
	_Color ("Diffuse Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
	_SpecTex ("Specular Color (RGB) Roughness (A)", 2D) = "black" {}
	_BumpMap ("Normalmap", 2D) = "bump" {}
	
	_DiffCubeIBL ("Custom Diffuse Cube 1", Cube) = "black" {}
	_SpecCubeIBL ("Custom Specular Cube 1", Cube) = "black" {}

	[HideInInspector]
	_DiffCubeIBL2 ("Custom Diffuse Cube 2", Cube) = "black" {}
	
	[HideInInspector]
	_SpecCubeIBL2 ("Custom Specular Cube 2", Cube) = "black" {}

	[HideInInspector]
	_Influence ("Influence", Range(0.0,1.0)) = 1

	[HideInInspector]
	_CubemapSize ("Cube Size 1", Vector) = (1,1,1,0)
	
	[HideInInspector]
	_CubemapSize2 ("Cube Size 2", Vector) = (1,1,1,0)
	
	
	// _Shininess property is needed by the lightmapper - otherwise it throws errors
	[HideInInspector] _Shininess ("Shininess (only for Lightmapper)", Float) = 1.0//0.5
}

SubShader { 
	Tags { "RenderType"="Opaque" }
	LOD 400
	
	CGPROGRAM
	#pragma surface surf LuxDirect noambient
	#pragma glsl
	#pragma target 3.0



	// #pragma debug

	#pragma multi_compile LUX_LIGHTING_BP LUX_LIGHTING_CT
	#pragma multi_compile LUX_LINEAR LUX_GAMMA
	#pragma multi_compile DIFFCUBE_ON DIFFCUBE_OFF
	#pragma multi_compile LUX_INFLUENCE_OFF LUX_INFLUENCE_OBJECT LUX_INFLUENCE_PIXEL

//	Does not make sense here...
//	#pragma multi_compile SPECCUBE_ON SPECCUBE_OFF

	#pragma multi_compile LUX_AO_OFF LUX_AO_ON

//	#define LUX_LIGHTING_BP
//	#define LUX_GAMMA 
//	#define LUX_LINEAR
//	#define DIFFCUBE_ON

//	We should alway define SPECCUBE_ON
	#define SPECCUBE_ON

//	Activate Box Projection in LuxLightingAmbient
	#define LUX_BOXPROJECTION

	// include should be called after all defines
	#include "../LuxCore/LuxLightingDirect.cginc"

	float4 _Color;
	sampler2D _MainTex; 
	sampler2D _SpecTex;
	sampler2D _BumpMap;
	#ifdef DIFFCUBE_ON
		samplerCUBE _DiffCubeIBL;
		#ifndef LUX_INFLUENCE_OFF
			samplerCUBE _DiffCubeIBL2;
		#endif 
	#endif
	samplerCUBE _SpecCubeIBL;
		#ifndef LUX_INFLUENCE_OFF
			samplerCUBE _SpecCubeIBL2;
		#endif
	#ifdef LUX_AO_ON
		sampler2D _AO; 
	#endif

//	Needed by Box Projection
	//float3 _CubemapPositionWS;
	float3 _CubemapSize;
	float4x4 _CubeMatrix_Trans;
	float4x4 _CubeMatrix_Inv;

	#if defined(LUX_INFLUENCE_OBJECT) || defined(LUX_INFLUENCE_PIXEL)
	float _Influence; 
	#endif	
	
	#ifdef LUX_INFLUENCE_PIXEL
	float3 _CubemapSize2;
	float4x4 _CubeMatrix_Trans2;
	float4x4 _CubeMatrix_Inv2;
	#endif
	
	// Is set by script
	float4 ExposureIBL;

	struct Input {
		float2 uv_MainTex;
		// float2 uv_BumpMap; // Bump and Main Tex have to share the same texcoords to safe texture interpolators
		#ifdef LUX_AO_ON
			float2 uv_AO;
		#endif
		float3 viewDir;
		float3 worldNormal;
		float3 worldRefl;
		// needed by Box Projection
		float3 worldPos;
		INTERNAL_DATA
	};

	void surf (Input IN, inout SurfaceOutputLux o) {
		fixed4 diff_albedo = tex2D(_MainTex, IN.uv_MainTex);
		fixed4 spec_albedo = tex2D(_SpecTex, IN.uv_MainTex);
		// Diffuse Albedo
		o.Albedo = diff_albedo.rgb * _Color.rgb;
		o.Alpha = diff_albedo.a * _Color.a;
		// Bump and Main Tex have to share the same texcoords to safe texture interpolators
		o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
		// Specular Color
		o.SpecularColor = spec_albedo.rgb;
		// Roughness – gamma for BlinnPhong / linear for CookTorrence
		o.Specular = LuxAdjustSpecular(spec_albedo.a);
		
		#include "../LuxCore/LuxLightingAmbient.cginc"
	}
ENDCG
}
FallBack "Specular"
CustomEditor "LuxMaterialInspector"
}
