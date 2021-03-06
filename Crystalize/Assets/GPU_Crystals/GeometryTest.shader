﻿Shader "Unlit/GeometryTest"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _VerticesCount ("Vert Count", float) = 10
        _IndicesCount ("Indices Count", float) = 10
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
         //   #pragma geometry geom

            // make fog work
            #pragma multi_compile_fog
            #pragma target 5.0
         //   #pragma multi_compile_instancing
            #include "UnityCG.cginc"



            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                //float2 uv : TEXCOORD0;
                //UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;

            };

            struct v2g
            {
                float4 objPos : SV_POSITION;
                float2 uv : TEXCOORD0;

             //   float3 normal : NORMAL;
            };

            struct g2f
            {
                float4 vertex : SV_POSITION;
              //  float2 uv : TEXCOORD0;
              //  fixed4 color : COLOR;
            };

            struct Debug
            {
                int i;
                int count;
                float3 vertex;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _VerticesCount;

            StructuredBuffer<float3> VerticesBuffer;
            StructuredBuffer<float2> UvsBuffer;
            RWStructuredBuffer<uint> IndicesBuffer;
            RWStructuredBuffer<Debug> DebugBuffer;
            RWStructuredBuffer<float3> ComputeOutputBuffer;

            v2f vert ( uint id : SV_VertexID, appdata v)
            {
                v2f o;
            //    o.vertex = UnityObjectToClipPos( ComputeOutputBuffer[id] );


                float4 vertex_position = float4( ComputeOutputBuffer[id], 1.0f );

                o.vertex = mul( UNITY_MATRIX_VP, vertex_position );

                return o;

           //     o.vertex = UnityObjectToClipPos( VerticesBuffer[IndicesBuffer[v.id]] );
           ////    DebugBuffer[v.id].vertex = VerticesBuffer[IndicesBuffer[v.id]];
           //     for (uint i = 0; i < ( uint )_VerticesCount; i++)
           //     {
           //         DebugBuffer[i].i = v.id;
           //         DebugBuffer[i].count = IndicesBuffer[i];
           //         DebugBuffer[i].vertex = VerticesBuffer[IndicesBuffer[i]];

           //     }
           //     o.uv = TRANSFORM_TEX(v.uv, _MainTex);
           //     UNITY_TRANSFER_FOG(o,o.vertex);
           //     return o;
            }

            //[maxvertexcount( 256 )] //[instance( 32 )]
            //void geom( triangle v2g IN[3], inout TriangleStream<g2f> tristream, uint id : SV_GSInstanceID)
            //{
            //    g2f o;
            //    o.vertex = UnityObjectToClipPos( float4( VerticesBuffer[IndicesBuffer[id]], 1 ) );
            //    return;
            //    //

            //    //

            //    //for (uint i = 0; i < (uint)_VerticesCount; i++)
            //    //{
            //    //    uint count = ( 256 * InstanceID ) + i;
            //    //    o.vertex = UnityObjectToClipPos( float4( VerticesBuffer[ IndicesBuffer[count]], 0 ) );
            //    //    //o.color = fixed4( .5, .5, .0, 1 );
            //    //  //  o.uv = Buffer[IndicesBuffer[count]];
            //    //    tristream.Append( o );

            //    //    DebugBuffer[count].vertex = VerticesBuffer[IndicesBuffer[count]];
            //    //    DebugBuffer[count].i = i;
            //    //    DebugBuffer[count].count = count;

            //    //}

            //    
            //}



            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
             //   fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return fixed4(.1,0,.4,1);
            }
            ENDCG
        }
    }
}
