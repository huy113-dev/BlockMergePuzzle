Shader "Custom/SpritePortalDissolve"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _DissolveDir ("Dissolve Dir (XY)", Vector) = (1,0,0,0)
        _PlaneOrigin ("Plane Origin (XY)", Vector) = (0,0,0,0)
        _EdgeWidth ("Edge Width", Float) = 0.42
        _GlowColor ("Portal Glow Color", Color) = (0.35, 0.96, 1.0, 1.0)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float2 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float4 _DissolveDir;
            float4 _PlaneOrigin;
            float _EdgeWidth;
            fixed4 _GlowColor;

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                float4 wp = mul(unity_ObjectToWorld, IN.vertex);
                OUT.worldPos = wp.xy;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
                if (c.a <= 0.005) discard;

                float2 dir = normalize(_DissolveDir.xy + float2(1e-5, 0.0));
                float signedDist = dot(IN.worldPos - _PlaneOrigin.xy, dir);

                // Two-octave blocky square noise for digital voxel disintegration
                float2 cell1 = floor(IN.worldPos * 10.5);
                float2 cell2 = floor(IN.worldPos * 21.0);
                float n = hash21(cell1) * 0.65 + hash21(cell2) * 0.35;

                float threshold = signedDist + (n - 0.5) * _EdgeWidth;
                if (threshold > 0.0)
                {
                    discard;
                }

                // Glowing cyan/white edge highlight right before pixels disintegrate
                float rim = smoothstep(-0.18, 0.0, threshold);
                c.rgb = lerp(c.rgb, _GlowColor.rgb, rim * 0.78);
                return c;
            }
            ENDCG
        }
    }
}
