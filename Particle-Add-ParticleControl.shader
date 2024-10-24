
Shader "YanJia/Particles/AdditiveParticleControl" 
{
	Properties
	{
		[Toggle(USE_PS_COLOR)] _PS_Color_T ("Particle Color", float) = 1
        [Toggle(USE_PS_CUSTOMEDATA)] _PS_CustomData_T ("Particle Custom Data", float) = 1
		[Toggle(USE_PS_BLENDFRAME)] _PS_BlendFrame ("Particle Blend Frame", float) = 0
		_MainColor ("Main Color", Color) = (1,1,1,1)
		_Emission ("Emission", Range(1, 10)) = 1
		_MulAlpha ("Multiply Alpha", float) = 1
		_MainTex ("Main Texture", 2D) = "white" {}
        [Toggle] _UseAlphaRGB ("Use Alpha RGB", int) = 0
		_MaskTex ("Mask Texture", 2D) = "white" {}
		[Enum(R, 0, G, 1, B, 2, A, 3)] _AlphaChannel ("Alpha Channel", float) = 0

		[Space(20)][Header(Soft Particle Setting)]
		[Header(Note. Soft Particle Function only interacts with Opaque RenderType Material)]
		[Space(10)]
		[Toggle(USE_PS_SOFT)] _PS_Soft ("Use Particle Soft", float) = 0
		_Soft_Power ("Soft Power", Range(0.1, 5)) = 1
		_Soft_Distance ("Soft Distance Depth Check", float) = 1
		[Toggle]_Soft_Invert ("Soft Invert", float) = 0

		[Space(20)][Header(Rim Light Setting)]
		[Space(10)]
		[Toggle(USE_PS_RIM)] _PS_Rim ("Use Rim Light", int) = 0
		_Rim_Power ("Rim Power", Range(0.1, 10)) = 1
		[Toggle] _Rim_Invert ("Rim Invert", int) = 0
		[Toggle(USE_FADE_EDGE)] _KeepTwoFace ("Keep 2 Face", int) = 0

		[Space(20)]
		[Header(TOOLTIP FOR STEAMS DATA PARTICLE SYSTEM)]
		[Space(10)]
		[Header(If using BlendFrame Function)]
		[Header(..UV1 use for BlendFrame(uv.xy) AnimBlend(uv.z) AnimFrame(uv.w))]
		[Header(..UV2 use for CustomeData(uv.xyzw))]
		[Header(..Set (Frame over Time) back to 1 Frame)]
		[Header(....For example. There are maximum 16 frames. Set is 15)]
		[Space(10)]
		[Header(If you dont)]
		[Header(..UV1 use for CustomeData(uv.xyzw))]
		[Space(20)]

		[Header(Alpha Clip Setting)]
		[Header(Alpha to Mask using range 0.5 to 1 of Alpha channel)]
		[Space(10)]
		[Toggle(USE_ALPHA_CLIP)] _AlphaClip ("Use Alpha Clip", Float) = 0
		[Toggle(USE_ALPHA_TO_MASK)] _AlphaToMask ("AlphaToMask", Float) = 0
		_Clipping ("Alpha Clip", Range(0, 1)) = 0

		[Space(20)][Header(Trail Setting)]
		[Space(10)]
		[Toggle(USE_TRAIL)] _UseTrail ("Use Trail Effect", Float) = 0
		[Toggle] _TrailFlip ("Trail Flip", Float) = 0
		_TrailPower ("Trail Power", Range(0.1, 10)) = 1
		_TrailMove ("Trail Move", Float) = 0
		
		[Space(10)][Header(Blend Mode)]
		[Enum(UnityEngine.Rendering.BlendMode)] _Src("Color Blend Src Factor", Float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _Dst("Color Blend Dst Factor", Float) = 1
        [Toggle] _ZWrite ("ZWrite", Float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 4
        [Enum(UnityEngine.Rendering.CullMode)] _Culling ("Culling", Float) = 0
	}

	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }

		Pass
		{
        	Blend [_Src] [_Dst]
        	ZWrite [_ZWrite]
        	ZTest [_ZTest]
        	Cull [_Culling]

			CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma multi_compile_fog
			#pragma shader_feature USE_PS_COLOR
			#pragma shader_feature USE_PS_CUSTOMEDATA
			#pragma shader_feature USE_PS_BLENDFRAME
			#pragma shader_feature USE_PS_SOFT
			#pragma shader_feature USE_PS_RIM
			#pragma shader_feature USE_ALPHA_CLIP
			#pragma shader_feature USE_ALPHA_TO_MASK
			#pragma shader_feature USE_TRAIL
			#pragma shader_feature USE_FADE_EDGE

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;

			#ifdef USE_PS_COLOR
				fixed4 color : COLOR;
			#endif

			#ifdef USE_PS_BLENDFRAME
					float4 psgif : TEXCOORD1;
				#ifdef USE_PS_CUSTOMEDATA
					float4 pscus : TEXCOORD2;
				#endif
			#else
				#ifdef USE_PS_CUSTOMEDATA
					float4 pscus : TEXCOORD1;
				#endif
			#endif

			#ifdef USE_PS_RIM
				float3 normal : NORMAL;
			#endif
			};
			
			struct v2f 
			{
				float4 vertex : SV_POSITION;
				UNITY_FOG_COORDS(1)

			#ifdef USE_PS_COLOR
				fixed4 color : COLOR;
			#endif

				float4 uv : TEXCOORD0;

			#ifdef USE_PS_BLENDFRAME
				float blend : TEXCOORD1;
			#endif

			#ifdef USE_PS_SOFT
				float4 softUV : TEXCOORD2;
			#endif

			#ifdef USE_PS_RIM
				float3 normal : TEXCOORD3;
				float4 viewDir : TEXCOORD4;
			#endif

			#ifdef USE_TRAIL
				float2 uvTrail : TEXCOORD5;
			#endif

				float2 uvMask : TEXCOORD6;
			};
	
			sampler2D _MainTex;
			float4 _MainTex_ST;
            int _UseAlphaRGB;
			sampler2D _MaskTex;
			float4 _MaskTex_ST;
			fixed _AlphaChannel;
			float4 _MainColor;
			float _Emission;
			float _MulAlpha;

		#ifdef USE_PS_SOFT
			sampler2D _CameraDepthTexture;
			float _Soft_Power;
			float _Soft_Distance;
			fixed _Soft_Invert;
		#endif

		#ifdef USE_PS_RIM
			float _Rim_Power;
			fixed _Rim_Invert;
		#endif

		#ifdef USE_TRAIL
			fixed _TrailFlip;
			fixed _TrailPower;
			fixed _TrailMove;
		#endif

		#ifdef USE_ALPHA_CLIP
			float _Clipping;
		#endif

			float remapFunc(float In, float InMin, float InMax, float OutMin, float OutMax)
			{
				if (In <= InMin) { return OutMin; }
				else if (In >= InMax) { return OutMax; }
				return OutMin + (In - InMin) * (OutMax - OutMin) / (InMax - InMin);
			}

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);

			#ifdef USE_PS_COLOR
				o.color = v.color;
			#endif

			#ifdef USE_PS_CUSTOMEDATA
			 		//o.uv.xy = TRANSFORM_TEX(float2(v.uv.x + v.pscus.x, v.uv.y + v.pscus.y), _MainTex);
					o.uv.xy = float2(v.uv.x + v.pscus.x, v.uv.y + v.pscus.y);
				#ifdef USE_PS_BLENDFRAME
			 		o.uv.zw = TRANSFORM_TEX(float2(v.psgif.x + v.pscus.x, v.psgif.y + v.pscus.y), _MainTex);
					o.blend = v.psgif.z;
				#endif
			#else
			 		//o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
					o.uv.xy = v.uv;
				#ifdef USE_PS_BLENDFRAME
			 		o.uv.zw = TRANSFORM_TEX(v.psgif.xy,_MainTex);
					o.blend = v.psgif.z;
				#endif
			#endif

				o.uvMask = TRANSFORM_TEX(v.uv, _MaskTex);

			#ifdef USE_PS_SOFT
				o.softUV = ComputeScreenPos(o.vertex);
			#endif

			#ifdef USE_PS_RIM
				o.normal = v.normal;
				o.viewDir = v.vertex;
			#endif

			#ifdef USE_TRAIL
				o.uvTrail = v.uv;
			#endif

				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
	
			fixed4 frag (v2f i) : SV_Target
			{
				float4 color = (1,1,1,1);

			#ifdef USE_PS_COLOR
				color = i.color;
			#else
				color = _MainColor;
			#endif

			#ifdef USE_TRAIL
				half width = pow(_TrailFlip ? 1 - i.uvTrail.x : i.uvTrail.x, _TrailPower);
				i.uv.y = (i.uv.y * 2 - 1) / width * 0.5 + 0.5;
				i.uv.x += frac(_Time.x * _TrailMove);
				clip(i.uv.y);
				clip(1 - i.uv.y);
			#endif
				
				half alphaMask = 
					_AlphaChannel == 0 ? tex2D(_MaskTex, i.uvMask).x : 
					_AlphaChannel == 1 ? tex2D(_MaskTex, i.uvMask).y : 
					_AlphaChannel == 2 ? tex2D(_MaskTex, i.uvMask).z : 
					tex2D(_MaskTex, i.uvMask).w;
				//fixed4 col = tex2D(_MainTex, i.uv.xy) * alphaMask * (color * _Emission);
				fixed4 mainTex = tex2D(_MainTex, TRANSFORM_TEX(i.uv.xy, _MainTex));
                mainTex.w = _UseAlphaRGB ? Luminance((mainTex.x + mainTex.y + mainTex.z) / 3) : mainTex.w;
				// fixed4 col = mainTex * alphaMask * (color * _Emission);
				fixed4 col = fixed4(mainTex.rgb * (color * _Emission), saturate(mainTex.w * alphaMask * color.w));

			#ifdef USE_PS_BLENDFRAME
				fixed4 col2 = tex2D(_MainTex, i.uv.zw);
				col2.w = (_UseAlphaRGB ? Luminance((col2.x + col2.y + col2.z) / 3) : col2.w) * color.w;
				//col2 *= (color * _Emission);
				col2.rgb *= (color * _Emission);
				col = lerp(col, col2, i.blend);
			#endif

			#ifdef USE_PS_RIM
				#ifdef USE_FADE_EDGE
					float rimValue = saturate(abs(dot(UnityObjectToWorldNormal(i.normal), normalize(WorldSpaceViewDir(i.viewDir)))));
				#else
					float rimValue = saturate(dot(UnityObjectToWorldNormal(i.normal), normalize(WorldSpaceViewDir(i.viewDir))));
				#endif
				col.w *= _Rim_Invert ? 1 - pow(rimValue, _Rim_Power) : pow(rimValue, _Rim_Power);
			#endif

			#ifdef USE_FADE_EDGE

			#endif

			#ifdef USE_PS_SOFT
				float2 screenSpaceUV = i.softUV.xy / i.softUV.w;
                float depthTex = saturate(((LinearEyeDepth(tex2D(_CameraDepthTexture, screenSpaceUV)).x) - i.softUV.w) / _Soft_Distance);
                col = fixed4(col.xyz, col.w * (_Soft_Invert ? 1 - pow(depthTex, _Soft_Power) : pow(depthTex, _Soft_Power)));
			#endif

			#ifdef USE_ALPHA_CLIP
				#ifdef USE_ALPHA_TO_MASK
					clip(col.w - (remapFunc(color.w, 0.5, 1, 1.01, 0) * _Emission));
				#else
					clip(col.w - (remapFunc(_Clipping, 0, 1, 0, 1.01) * _Emission));
				#endif
			#endif

				col = fixed4(col.xyz, col.w * _MulAlpha);

				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}