﻿Shader "Unlit/commandTest"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };


            struct Vert
            {
                float3         pos;
                float2         uv;
                float3         normal;
                float3         debugCol;
                //   float4         tangent;
            };

            StructuredBuffer<Vert> vertBuffer;


            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 debugCol : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert ( uint id : SV_VertexID, appdata v )
            {
                v2f o;
                
                o.vertex = UnityObjectToClipPos( float4( vertBuffer[id].pos, 1.0f ) );
                o.uv = TRANSFORM_TEX( vertBuffer[id].uv, _MainTex );

                o.debugCol = vertBuffer[id].debugCol;

                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                col = fixed4(i.debugCol, 1); // debug uvs

                return col;
            }
            ENDCG
        }
    }
}
