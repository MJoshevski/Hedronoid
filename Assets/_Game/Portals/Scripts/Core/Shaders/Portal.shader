Shader "Custom/Portal"
{
    Properties
    {
        _InactiveColour ("Inactive Colour", Color) = (1, 1, 1, 1)
    }

	HLSLINCLUDE
	#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"
	TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
	ENDHLSL

	SubShader
	{
		Tags 
		{ 
			"RenderType" = "Opaque"
			"RenderPipeline"="UniversalRenderPipeline"
		}
		LOD 100
		Cull Off

		Pass
		{
			HLSLINCLUDE
			#pragma vertex vert
			#pragma fragment frag
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			struct Attributes
			{
				float4 vertex : POSITION;
			};

			struct Varyings
			{
				float4 vertex : SV_POSITION;
				float4 screenPos : TEXCOORD0;
			};

			//TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);

			//sampler2D _MainTex;
			int displayMask; // set to 1 to display texture, otherwise will draw test colour

			//TEXTURE2D(_BaseMap);
			//SAMPLER(sampler_BaseMap);

			CBUFFER_START(UnityPerMaterial)
				float4 _InactiveColour;
			CBUFFER_END

			Varyings vert(Attributes v)
			{
				Varyings o;
				o.vertex = GetVertexPositionInputs(v.vertex);
				o.screenPos = ComputeScreenPos(o.vertex);
				return o;
			}

			half4 frag(Varyings i) : SV_Target
			{
				float2 uv = i.screenPos.xy / i.screenPos.w;
				//half4 portalCol = tex2D(_MainTex, uv);
				half4 portalCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
				return portalCol * displayMask + _InactiveColour * (1 - displayMask);
	
			}
			ENDHLSL
		}
	}
    //SubShader
    //{
    //    Tags { "RenderType"="Opaque" }
    //    LOD 100
    //    Cull Off

    //    Pass
    //    {
    //        CGPROGRAM
    //        #pragma vertex vert
    //        #pragma fragment frag
    //        #include "UnityCG.cginc"

    //        struct appdata
    //        {
    //            float4 vertex : POSITION;
    //        };

    //        struct v2f
    //        {
    //            float4 vertex : SV_POSITION;
    //            float4 screenPos : TEXCOORD0;
    //        };

    //        sampler2D _MainTex;
    //        float4 _InactiveColour;
    //        int displayMask; // set to 1 to display texture, otherwise will draw test colour
    //        

    //        v2f vert (appdata v)
    //        {
    //            v2f o;
    //            o.vertex = UnityObjectToClipPos(v.vertex);
    //            o.screenPos = ComputeScreenPos(o.vertex);
    //            return o;
    //        }

    //        fixed4 frag (v2f i) : SV_Target
    //        {
    //            float2 uv = i.screenPos.xy / i.screenPos.w;
    //            fixed4 portalCol = tex2D(_MainTex, uv);
    //            return portalCol * displayMask + _InactiveColour * (1-displayMask);
    //        }
    //        ENDCG
    //    }
    //}
    //Fallback "Standard" // for shadows
}
