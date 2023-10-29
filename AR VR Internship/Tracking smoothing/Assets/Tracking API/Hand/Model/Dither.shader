Shader "Unlit/Dither"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        ZWrite On
        ZTest GEqual
        
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
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

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            #define ROUNDING_PREC 0.999
            #define PIXELSIZE 5.0
            inline void PixelClipAlpha_float(float4 posCS, float alpha_in, out float alpha_out) {
               alpha_in = clamp(round(alpha_in), 0.0, 1.0);
               float xfactor = step(fmod(abs(floor(posCS.x)), PIXELSIZE), ROUNDING_PREC);
               float yfactor = step(fmod(abs(floor(posCS.y - PIXELSIZE)), PIXELSIZE), ROUNDING_PREC);
               alpha_out = alpha_in * xfactor * yfactor * alpha_in;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                PixelClipAlpha_float(i.vertex, col.a, col.a);
                return col;
            }
            ENDCG
        }
    }
}
