Shader "Unlit/Transparent ZWrite"
{
    Properties {
        _MainTex ("Albedo Texture", 2D) = "white" {}
        _TintColor("Tint Color", COLOR) = (1, 1, 1, 1)
        _Transparency("Transparency", Range(0.0, 0.5)) = 0.25
    }
    SubShader {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        LOD 200

        // extra pass that renders to depth buffer only
        Pass {
            ZWrite On
            ColorMask 0
        }
    }
}
