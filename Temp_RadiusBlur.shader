// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/Temp_RadiusBlur"
{
    Properties
    {
        [Header(Need Vertex Streams had Center (TEXCOORD1.xyz))][Toggle(USE_WITH_PS)] _UseWithPS ("Use with Particle System", int) = 0
        
        [Space(10)]
        _MainTex ("Texture", 2D) = "white" {}
        [Enum(R, 0, G, 1, B, 2, A, 3)] _Channel ("Channel", int) = 0
        _Sample ("Sample", Range(1, 20)) = 1
        _Power ("Power", Range(0.0, 0.1)) = 0
        //_X ("x", Range(-1, 1)) = 0
        //_Y ("Y", Range(-1, 1)) = 0
        //_Pos ("Pos", vector) = (0,0,0,0)

        [Space(10)][Header(Blend Mode)]
		[Enum(UnityEngine.Rendering.BlendMode)] _Src("Color Blend Src Factor", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _Dst("Color Blend Dst Factor", Float) = 0
        [Toggle] _ZWrite ("ZWrite", Float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Culling ("Culling", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Blend [_Src] [_Dst]
        	ZWrite [_ZWrite]
        	ZTest [_ZTest]
        	Cull [_Culling]

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
			#pragma shader_feature USE_WITH_PS

            #include "UnityCG.cginc"

            struct appdata
            {
                float3 vertex : POSITION;
                float3 normal : NORMAL; 
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float3 center : TEXCOORD1;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 screenPos: TEXCOORD1;
                float2 abc : TEXCOORD2;
                float2 objectCenterUV : TEXCOORD3;
                UNITY_FOG_COORDS(1)
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            int _Channel;
            sampler2D _CameraTexture;
            int _Sample;
            float _Power;
            // float _X;
            // float _Y;
            // float4 _Pos;

            float2 zoom(float2 UV_In, float scale = 1, float2 offset = (0,0))
            {
                float2 offset_c = float2(0.5f + offset.x, 0.5f + offset.y);
                return ((UV_In - offset_c) * scale) + offset_c;
            }

            float2 Unity_PolarCoordinates(float2 UV, float2 Center, float RadialScale, float LengthScale)
            {
                float2 delta = UV - Center;
                float radius = length(delta) * 2 * RadialScale;
                float angle = atan2(delta.x, delta.y) * 1.0/6.28 * LengthScale;
                return float2(radius, angle);
            }

            float2 WorldToScreenPoint(float3 center = (0,0,0))
            {
                float4 clipSpacePos = UnityObjectToClipPos(float4(center,0));
                float4 screenPos = ComputeScreenPos(clipSpacePos);
                screenPos.xy /= screenPos.w;
                return float2(screenPos.xy) - 0.5f;
            }

            float2 WorldToScreenPos(float3 pos){
                pos = normalize(pos - _WorldSpaceCameraPos)*(_ProjectionParams.y + (_ProjectionParams.z - _ProjectionParams.y))+_WorldSpaceCameraPos;
                fixed2 uv =0;
                fixed3 toCam = mul(unity_WorldToCamera, pos);
                fixed camPosZ = toCam.z;
                fixed height = 2 * camPosZ / unity_CameraProjection._m11;
                fixed width = _ScreenParams.x / _ScreenParams.y * height;
                uv.x = (toCam.x + width / 2)/width;
                uv.y = (toCam.y + height / 2)/height;
                return uv - 0.5f;
            }

            v2f vert (appdata v)
            {
                v2f o;
                //float3 worldPos = mul(unity_ObjectToWorld, float4(v.center,1)).xyz;
                //float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                //float3 worldPos = unity_ObjectToWorld._m03_m13_m23;
                //o.abc = WorldToScreenPos(_Pos.xyz);
                //o.abc = WorldToScreenPos(worldPos);
                #ifdef USE_WITH_PS
                    o.abc = WorldToScreenPoint(v.center);
                #else
                    o.abc = WorldToScreenPoint();
                #endif
                //o.abc = unity_ObjectToWorld[3].xy;
                //o.abc.x = (o.abc.x / _ScreenParams.x);
                //o.abc.y = (o.abc.y / _ScreenParams.y);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                o.objectCenterUV = float2(distance((o.screenPos.xy / o.screenPos.w).x, v.uv.x), distance((o.screenPos.xy / o.screenPos.w).y, v.uv.y));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                //o.uv = v.uv;

                o.color = v.color
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 mainTex = tex2D(_MainTex, Unity_PolarCoordinates(i.uv, float2(0.5f,0.5f), 1, 1));
                fixed mainTexChannel = _Channel == 0 ? mainTex.r : _Channel == 1 ? mainTex.g : _Channel == 2 ? mainTex.b : mainTex.a;

                //Get Opaque Texture
                //float2 screenUV = (i.screenPos.xy / i.screenPos.w) * (lerp(1, mainTexChannel, (_Power * i.color.a)));
                float2 screenUV = (i.screenPos.xy / i.screenPos.w);

                // float2 dir = screenUV - i.abc;

                float distances = distance(screenUV, i.uv);
                //float2 distanceXY = screenUV - i.uv;
                float2 distanceXY = float2(distance(screenUV.x, i.uv.x), distance(screenUV.y, i.uv.y));
                fixed4 col = 0;

                for(int j = _Sample; j > 0; j--)
                {
                    // float scale = 1 - (_Power * j);
                    // float2 zoomedUV = i.objectCenterUV + dir * scale;
                    //col = (col + tex2D(_CameraTexture, zoom(screenUV, 1 - (_Power * j), float2(_X, _Y)))) / 2;
                    float stepPower = 1 - (_Power * j);
                    col = (col + tex2D(_CameraTexture, zoom(screenUV, lerp(1, stepPower, saturate(mainTexChannel)), float2(i.abc.x, i.abc.y)))) * 0.8f;
                }

                col = (col + tex2D(_CameraTexture, zoom(screenUV))) * 0.2f;
                //col /= _Sample;
                //col = fixed4(distances, distances, 0, 1);

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;

                /*float2 uv = (i.screenPos.xy / i.screenPos.w);
                float2 direction = uv - 0.5f;
                float dist = length(direction);

                float4 col = float4(0,0,0,0);
                float step = _Power * dist / _Sample;

                for (int j = 0; j < _Sample; ++j)
                {
                    float2 offset = direction * (step * j);
                    col += tex2D(_CameraTexture, uv - offset);
                }

                col /= _Sample; // Lấy trung bình màu từ các mẫu texture
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;*/
            }
            ENDCG
        }
    }
}
