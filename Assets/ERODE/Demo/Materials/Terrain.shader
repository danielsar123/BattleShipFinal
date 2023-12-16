Shader "Unlit/TerrainUnlit"
{
    Properties
    {

    }
        SubShader
        {
            Tags { "RenderType" = "Opaque" }
            LOD 100

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"
                #include "Lighting.cginc"

                #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
                #include "AutoLight.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    half3 objNormal : TEXCOORD0;
                    SHADOW_COORDS(1)
                    float3 coords : TEXCOORD1;
                    float2 uv : TEXCOORD2;
                    float4 pos : SV_POSITION;
                };

                sampler2D _Grass;
                sampler2D _Rock;
                float4 _MainTex_ST;
                float4 _Albedo;
                float _Tiling;

                v2f vert(float4 pos : POSITION, float3 normal : NORMAL, float2 uv : TEXCOORD0)
                {
                    v2f o;
                    o.pos = UnityObjectToClipPos(pos);
                    o.coords = pos.xyz * _Tiling;
                    o.objNormal = normal;
                    o.uv = uv;
                    TRANSFER_SHADOW(o)
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    float shadow = max(dot(i.objNormal, float3(0, 1, 0)), 0);

                    shadow *= 0.9;
                    shadow += 0.1;

                    float4 col = 0.5;

                    return col * shadow;
                }
                ENDCG
            }
        }
}