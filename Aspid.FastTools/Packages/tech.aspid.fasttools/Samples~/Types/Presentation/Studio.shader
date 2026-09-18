Shader "Aspid/Samples/Types/Studio"
{
    Properties { _Color ("Color", Color) = (1,1,1,1) }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct Input { float4 vertex : POSITION; float3 normal : NORMAL; };
            struct Varyings { float4 position : SV_POSITION; float3 normal : TEXCOORD0; };
            fixed4 _Color;
            Varyings vert(Input input)
            {
                Varyings output;
                output.position = UnityObjectToClipPos(input.vertex);
                output.normal = UnityObjectToWorldNormal(input.normal);
                return output;
            }
            fixed4 frag(Varyings input) : SV_Target
            {
                float light = saturate(dot(normalize(input.normal), normalize(float3(-0.4, 0.8, -0.6))));
                return fixed4(_Color.rgb * (0.48 + 0.52 * light), 1);
            }
            ENDCG
        }
    }
}
