Shader "Custom/SpriteFlash"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        
        // 플래시 효과를 위한 변수
        _FlashColor ("Flash Color", Color) = (1,1,1,1) // 기본 흰색
        _FlashAmount ("Flash Amount", Range(0,1)) = 0
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
        Blend One OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
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
            };

            fixed4 _Color;
            fixed4 _FlashColor;
            float _FlashAmount;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap (OUT.vertex);
                #endif
                return OUT;
            }

            sampler2D _MainTex;

            fixed4 frag(v2f IN) : SV_Target
            {
                // 1. 텍스처 색상 가져오기
                fixed4 c = tex2D(_MainTex, IN.texcoord);
                
                // 2. SpriteRenderer의 Tint 색상(_Color) 적용
                c *= IN.color;

                // 3. 플래시 효과 적용 (알파값은 건드리지 않고 RGB만 섞음)
                // c.rgb: 현재 텍스처 색
                // _FlashColor.rgb: 목표 플래시 색(흰색)
                // _FlashAmount: 0이면 원래색, 1이면 흰색
                c.rgb = lerp(c.rgb, _FlashColor.rgb, _FlashAmount);
                
                // 4. 알파 프리멀티플라이 (투명도 처리 필수 과정)
                c.rgb *= c.a;
                
                return c;
            }
        ENDCG
        }
    }
}