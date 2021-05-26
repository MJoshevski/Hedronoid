Shader "Custom/Portal"
{
    Properties
    {
        _InactiveColour ("Inactive Colour", Color) = (1, 1, 1, 1)
    }

	// URP variant
	SubShader
	{
		Tags 
		{ 
			"RenderType" = "Opaque"
			"Queue" = "Geometry"
			"RenderPipeline"="UniversalPipeline"
		}
		LOD 100
		Cull Off

		HLSLINCLUDE
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		ENDHLSL

		Pass
		{			
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			struct Attributes
			{
				float4 vertex : POSITION;
			};

			struct Varyings
			{
				float4 vertex : SV_POSITION;
				float4 screenPos : TEXCOORD0;
			};

			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);
			//uniform sampler2D _MainTex;

			int displayMask; // set to 1 to display texture, otherwise will draw test colour

			CBUFFER_START(UnityPerMaterial)
				float4 _InactiveColour;
			CBUFFER_END

			Varyings vert(Attributes v)
			{
				Varyings o;
				o.vertex = TransformObjectToHClip(v.vertex.xyz);
				o.screenPos = ComputeScreenPos(o.vertex);
				return o;
			}

			float4 frag(Varyings i) : SV_Target
			{
				float2 uv = i.screenPos.xy / i.screenPos.w;

				//float4 portalCol = tex2D(_MainTex, uv);
				float4 portalCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

				return portalCol * displayMask + _InactiveColour * (1 - displayMask);	
			}
			ENDHLSL
		}
	}

	// Built-in RP variant
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 screenPos : TEXCOORD0;
            };

            sampler2D _MainTex;

			CBUFFER_START(UnityPerMaterial)
				float4 _InactiveColour;
			CBUFFER_END

            int displayMask; // set to 1 to display texture, otherwise will draw test colour
            

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.screenPos.xy / i.screenPos.w;
                fixed4 portalCol = tex2D(_MainTex, uv);
                return portalCol * displayMask + _InactiveColour * (1-displayMask);
            }
            ENDCG
        }
    }
    Fallback "Standard" // for shadows
}
