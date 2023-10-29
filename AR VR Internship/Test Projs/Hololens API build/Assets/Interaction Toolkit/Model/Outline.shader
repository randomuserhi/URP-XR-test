Shader "Unlit/Outline"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineThickness ("Outline Thickness", Range(0,.1)) = 0.03
        _Transparency ("Transparency", Range(0, 1.0)) = 0.25
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderType"="Transparent" }
        LOD 200

        Blend SrcAlpha OneMinusSrcAlpha

        Cull front

        Pass
		{
            CGPROGRAM

            //include useful shader functions
            #include "UnityCG.cginc"

            //define vertex and fragment shader
            #pragma vertex vert
            #pragma fragment frag

            //color of the outline
            fixed4 _OutlineColor;
            //thickness of the outline
            float _OutlineThickness;
            //transparency of outline
            float _Transparency;

            //the object data that's available to the vertex shader
            struct appdata{
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            //the data that's used to generate fragments and can be read by the fragment shader
            struct v2f{
                float4 position : SV_POSITION;
            };

            //the vertex shader
            v2f vert(appdata v){
                v2f o;
                //calculate the position of the expanded object
                float3 normal = normalize(v.normal);
                float3 outlineOffset = normal * _OutlineThickness;
                float3 position = v.vertex + outlineOffset;
                //convert the vertex positions from object space to clip space so they can be rendered
                o.position = UnityObjectToClipPos(position);

                return o;
            }

            //the fragment shader
            fixed4 frag(v2f i) : SV_TARGET{
                fixed4 col = _OutlineColor;
                col.a = _Transparency;
                return col;
            }

            ENDCG
        }
    }
}
