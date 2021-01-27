Shader "Unlit/commandTest"
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


            struct Point
            {
                float3         vertex;
                float3         normal;
                float4         tangent;
                float2 uv;
            };

            struct Vert
            {
                float3         pos;
                float2         uv;
                float3         normal;
                //   float4         tangent;
            };

            StructuredBuffer<Point> points;
            StructuredBuffer<float3> vertexBuffer;
            StructuredBuffer<Vert> vertBuffer;


            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert ( uint id : SV_VertexID, appdata v )
            {
                v2f o;
               // o.vertex = UnityObjectToClipPos(v.vertex);

              ////  float4 vertex_position = float4( points[id].vertex, 1.0f );
              //  float4 vertex_position = float4( vertexBuffer[id], 1.0f );

              //  o.vertex = mul( UNITY_MATRIX_VP, vertex_position );

              //  o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                o.vertex = UnityObjectToClipPos( float4( vertBuffer[id].pos, 1.0f ) );
                o.uv = TRANSFORM_TEX( vertBuffer[id].uv, _MainTex );

                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                col = float4( i.uv, 0, 1 ); // debug uvs

                return col;
            }
            ENDCG
        }
    }
}
