// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Lighting/Ambient Sky Box 2"
{

Properties
{
	_SkyColor ("Sky color", Color) = (0.15,0.3,0.5,1)
	_HorizonColor ("Horizon color", Color) = (0.7,0.85,1,1)
	_GroundColor ("Ground color", Color) = (0.4,0.35,0.3,1)	
	_SkyIntensity ("Sky intensity", Float) = 1.1
	_HorizonBlend ("Horizon blend", Float) = 0.5
	_HorizonOffset ("Horizon offset", Float) = 0
	_HorizonBand ("Horizon color banding", Float) = 0.25
	_SunScatter ("Sun scatter", Float) = 1
	
	_SunIntensity ("Sun intensity", Float) = 0.5
	_ScatterIntensity ("Scatter intensity", Float) = 0.5
	_SunSkyFocus ("Sun sky focus", Float) = 0.2
	_SunSize ("Sun size", Float) = 0.5
}

SubShader
{

Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
Cull Off ZWrite Off

Pass
{
	CGPROGRAM
	#pragma vertex vert
	#pragma fragment frag
	#pragma target 3.0

	#include "UnityCG.cginc"
	#include "Lighting.cginc"
		
	half4 _SkyColor;
	half4 _HorizonColor;
	half4 _GroundColor;
	float _HorizonBlend;
	float _HorizonOffset;
	float _SkyIntensity;
	float _SunScatter;
	float _SunSize;
	float _SunIntensity;
	float _ScatterIntensity;
	float _SunSkyFocus;
	float _HorizonBand;

	struct a2v
	{
		float4 vertex : POSITION;
		float3 normal : NORMAL;
	};

	struct v2f
	{
		float4 pos : SV_POSITION;	
		//float3 vpos : TEXCOORD0;
		float4 posWorld : TEXCOORD0;
		float3 normalDir : TEXCOORD1;
   	};

	float Ramp (float x,float t0,float t1)
	{
		return saturate((x - t0) / (t1 - t0));
	}

	v2f vert (a2v v)
	{
		v2f o;
		o.posWorld = mul(unity_ObjectToWorld, v.vertex);
		o.normalDir = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);
		o.pos = UnityObjectToClipPos(v.vertex);

		return o;
	}

	half4 frag (v2f i) : SV_Target
	{
		//assign skybox colors and masks
		float sunSize = 0.1 * pow(100/_SunSize, 2);		//convert SunSize to a more useful range
		float scatterSq = _SunScatter * _SunScatter;	//convert SunScatter to a more useful range
		float3 y1 = _SkyColor.rgb * (_SkyIntensity * 0.75 + 0.25);	//SkyIntensity effects different parts of the sky differently
		float3 y2 = _HorizonColor.rgb * _SkyIntensity;
		float3 y3 = _GroundColor.rgb * (_SkyIntensity * 0.5 + 0.5);
		float eb = saturate(_HorizonBlend) + 0.01;		//convert HorizonBlend into a more useful range for skybox color assignment
		float dp = _HorizonOffset + (dot(float4(0,1,0,1), normalize(i.posWorld)));	//dp is a ramp for controlling the blending of the different parts of the sky, it's 1 and -1 near the poles and 0 near the horizon
		
		//color the sky
		half4 skylight;
		skylight.rgb = lerp(y2, y1, pow(saturate(dp), eb));
		skylight.rgb = lerp(skylight.rgb, y3, pow(saturate(-dp), eb * 0.35));
		skylight *= _SkyIntensity;

		//make the disk of the sun
		float sunEdge = 10 / max(0.001, scatterSq);												//sunEdge is how sharp the edge of the sun is
		float3 sunRamp = (dot(normalize(i.posWorld), normalize(_WorldSpaceLightPos0.xyz)));		//sunRamp is how far away from the sun a pixel of sky is
		sunRamp = sunRamp * 0.5 + 0.5;
		float3 sun = sunRamp - 1;
		sun = (sun * sunSize + 0.5);
		sun = saturate(sun * sunEdge + 0.5);
		sun = sun * sun;																		//make the edge falloff exponential instead of linear

		//make a mask of the horizon
		float sunMask = dp * 0.5 + 0.501;
		float sharp = 4 / ((saturate(_HorizonBlend) + 0.1) * 0.1) + 1;
		sunMask = saturate((sunMask + 0.5) * sharp - sharp + 0.5);

		//make the variables for calculating light scatter
		float3 sunlight = float3(0,0,0);
		float3 sunScatterLight;
		float3 lightHyperColor = saturate(normalize(_LightColor0.rgb * _LightColor0.rgb)) * length(_LightColor0.rgb) * _SunIntensity; //this is a more saturated light with the same brightness

		//calculate the scattering light
		sunScatterLight = pow(sunRamp, 1 / saturate(scatterSq / sunSize));								//light from the sun (sunRamp) scatters around the sun depending on the scatterSq value
		sunScatterLight = saturate(pow(sunScatterLight, dp * 2)) * sunMask * sunRamp;					//light scatters more near the horizon (dp * tightness of response) and that effect tapers as you move away from the sun (sunRamp) and below the horizon (sunMask)
		sunlight = sunScatterLight * lightHyperColor * 0.5 * _ScatterIntensity;												//colored corona and horizon scattering
		
		sun = 10 * _SunIntensity * saturate(sun * lightHyperColor * _LightColor0.rgb * (_SunIntensity + 1)) / sunMask;	//make sure the disk of the sun is bright (*10) and colorful (hyperColor) and doesn't go below the horizon (sunMask)
		sun *= (1-saturate(pow(1-dp,4)) * float3(0,0.5,1));

		skylight.rgb = lerp(skylight, sunRamp * sunRamp * skylight.rgb * saturate(normalize(_LightColor0.rgb)) + skylight * 0.5, saturate(_SunSkyFocus)); //

		half4 c = half4(0,0,0,1);
		c.rgb = sunlight * sunMask + skylight + max(sun * dp, 0);
		float dp2 = 2 * pow(abs(dp), 0.5);
		
		//return float4(c.rgb * (1-saturate(pow(1-dp,4)) * float3(0.5,0.8,1)), 1);

		c.g = lerp(c.g, c.g * saturate((0.75 * dp2) + 0.4), _HorizonBand * sunMask);
		c.b = lerp(c.b, c.b * saturate((dp2)), _HorizonBand * sunMask);
		c.rgb += lerp(0, saturate(c.rgb * (1 - dp2) * 0.5) * sunMask, _HorizonBand * sunMask);

		c.a = 1;

		//return saturate(pow(1-dp,4)) * float4(1,0.2,0,1);
		return c;
	}

	ENDCG 

} // Pass

} // SubShader

FallBack Off

} // Shader
