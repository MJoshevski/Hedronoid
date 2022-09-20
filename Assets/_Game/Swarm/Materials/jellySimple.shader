// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: upgraded instancing buffer 'MyProperties' to new syntax.

Shader "Unlit/jellySimple"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1, 1, 1, 1)
		_Cube("Reflection Map", Cube) = "" {}
    }
    SubShader
    {
        //Tags { "Queue" = "Transparent + 50" }
        LOD 100
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off

        Pass
        {



            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
			      #pragma multi_compile_instancing



            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
				        float3 normal: NORMAL;
				        UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
				        //float3 ambient : TEXCOORD1;
				        half3 worldRefl : TEXCOORD1;
				        UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			      samplerCUBE _Cube;
			      float _jellyDimmer;


			      UNITY_INSTANCING_BUFFER_START(MyProperties)
				    UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
			      #define _Color_arr MyProperties
			      UNITY_INSTANCING_BUFFER_END(MyProperties)

            v2f vert (appdata v)
            {
                v2f o;

				        UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				        UNITY_TRANSFER_INSTANCE_ID(v, o);

				        // world space normal
				        float3 worldNormal = UnityObjectToWorldNormal(v.normal);
				
                /*float3 ambient = ShadeSH9(float4(worldNormal, 1.0f));
				        o.ambient = ambient;*/


				        float dilation = sin(v.vertex.z*10.f  + 3 * (_Time.w) + 4.0f * UNITY_ACCESS_INSTANCED_PROP(_Color_arr, _Color).x);
				        v.vertex.xyz += float3(0.4f * dilation * (v.vertex.x), 0.4f * dilation * (v.vertex.y), 0.05f * dilation);

				        //float2 wavy = 0.1f * (sin((worldPos.x)* 10), cos((worldPos.y) * 10));
				        //
				        ////worldPos.xy += wavy;
				
				        o.vertex = UnityObjectToClipPos(v.vertex);

				        // compute world space position of the vertex
				        float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				        // compute world space view direction
				        float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
				
				        // world space reflection vector
				        o.worldRefl = reflect(-worldViewDir, worldNormal);


                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				          UNITY_SETUP_INSTANCE_ID(i);

			            float4 cubeRefl = texCUBE(_Cube, i.worldRefl);
                  // sample the texture
                  fixed4 col = tex2D(_MainTex, i.uv);
				          col.rgb *= (30 * float3(0.6, 0.2, 2));
				          col.rgb = col.rgb * 20 * cubeRefl.rgb * _jellyDimmer;
				          col.a *= (0.4f + col.r *0.01);
				
				

				          //half4 skyData = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, i.worldRefl);
				          // decode cubemap data into actual color
				          //half3 skyColor = DecodeHDR(skyData, unity_SpecCube0_HDR);
				 
                  return  col * UNITY_ACCESS_INSTANCED_PROP(_Color_arr, _Color);
            }
            ENDCG
        }
    }
}
