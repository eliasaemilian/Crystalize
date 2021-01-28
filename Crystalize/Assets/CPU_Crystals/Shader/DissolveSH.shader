Shader "Custom/DissolveSH"
{
    Properties
    {
        _ColorTop ("Color Top", Color) = (1,1,1,0)
        _ColorBottom ("Color Bottom", Color) = (1,0,1,0)
        _ColorSpawn ("Color at Spawn", Color) = (0,0,0,0)
        _ParallaxTex ("Parallax Texture", 2D) = "white" {}
        _DissolveTex ("Dissolve Texture", 2D) = "white" {}
        _DissolveY ("Current Y of dissolve Effect", Float) = 0
        _DissolveSize ("Size of dissolve Effect", Float) = 2
        _StartingY ("Starting point of Effect", Float) = -10
        _SpawnTime ("Time since Instantiation", Float) = 0

    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
        LOD 100
        ZWrite Off // do not render to depth buffer, ON for solid, OFF for semi & transparent
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag


            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 normal : NORMAL;
                float4 tangent  : TANGENT;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD1;
                float3 viewDir  : TEXCOORD2;
                float3 worldPos : TEXCOORD3; // will be overwritten and just used to store this data
            };

            float4 _ColorTop;
            float4 _ColorBottom;
            float4 _ColorSpawn;
            sampler2D _ParallaxTex;
            sampler2D _DissolveTex;
            float _DissolveY;
            float _DissolveSize;
            float _StartingY;
            float _SpawnTime;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.normal = v.normal;
                o.viewDir = COMPUTE_VIEW_NORMAL;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz; // calc worldPos for this obj
                UNITY_TRANSFER_FOG(o,o.vertex);

                //--------------------------------- Vertex Displacement

                //float4 addPos = o.vertex + (saturate(_SpawnTime / 1));// add something for noise

                //float4 mulStrength = mul(addPos, 2);
                //float3 worldNormal = mul(unity_ObjectToWorld, v.normal).xyz;
                //float4 mulNormals = mul(mulStrength, (worldNormal,0));
                //float4 final = o.vertex + mulNormals;
                //o.vertex = final;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Spawn and Despawn Time Values
                float transition = _DissolveY - i.worldPos.y;
               // float transition = _DissolveY - i.vertex.y;
                float procentual = saturate(_SpawnTime / 1);
                float procentualTop = saturate(_SpawnTime - 0.6 / 1);

                // Vertex Color
                float4 spawnBottom = lerp(_ColorSpawn, _ColorBottom, procentual);
                float4 spawnTop = lerp(_ColorSpawn, _ColorTop, procentualTop);
                float4 blend = lerp(spawnBottom, spawnTop, i.uv.y);

                 // -------------------------- Direct specular Light
                float3 normal = normalize(i.normal);
                float3 camPos = _WorldSpaceCameraPos;
                float3 fragToCam = camPos - i.worldPos;
                float3 viewDir = normalize(fragToCam);
                float lightDir = _WorldSpaceLightPos0.xyz;
                float3 viewReflect = reflect(-viewDir, normal);
                float specularFalloff = max(0, dot(viewReflect, lightDir)); //clamp same as saturate
                specularFalloff *= 0.15;
                //-------------------------------

                //------------------------------- Parallax Effect
                float3 ViewDir = normalize(i.viewDir);
                float3 viewOffset = mul(i.viewDir, 0.8); // Offset Value

                float2 TilingOffset = i.uv * float2(1 , 1) + viewOffset;
             
                float4 _SampleTexture2D_RGBA = tex2D(_ParallaxTex, i.uv);
                float _SampleTexture2D_R = _SampleTexture2D_RGBA.r;
                float combine = mul(_SampleTexture2D_R, 0.1); // Parallax Strength

                // --------------------------- Alpha Spawn Fade In
                float blendY = saturate(i.vertex.y);
                float4 spawnAlpha = saturate(0.2 - blendY);

                blend.a = lerp(spawnAlpha, 0.8 + blendY, procentual - 0.2);
                blend.a = saturate(blend.a);
              //  blend.a = 1.5;

                // Dissolve Clipping
                clip(_StartingY + (transition + (tex2D(_DissolveTex, i.uv)) * _DissolveSize)); // if < 0 pixel will not be rendered to screen

                return blend + specularFalloff + combine;
            }
            ENDCG
        }
    }
}
