Shader "UI/CircleTransition_CustomCenter"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (0,0,0,1)
        _Radius ("Circle Radius", Range(0, 1.5)) = 0
        _Smoothness ("Gradient Smoothness", Range(0.01, 1.0)) = 0.2
        
        // [추가됨] 중심점 좌표 (기본값 0.5, 0.5 = 중앙)
        _Center ("Center Point", Vector) = (0.5, 0.5, 0, 0)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            fixed4 _Color;
            float _Radius;
            float _Smoothness;
            float4 _Center; // [추가됨] 중심점 변수 선언

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // [수정됨] 고정값 0.5 대신 외부에서 입력받은 _Center 좌표 사용
                // i.uv (0~1)에서 _Center (0~1)를 빼서 거리를 잼
                float2 uv = i.uv - _Center.xy;

                // 화면 비율에 맞춰 X축 보정 (타원 방지)
                float aspect = _ScreenParams.x / _ScreenParams.y;
                uv.x *= aspect;

                // 중심으로부터의 거리 계산
                float dist = length(uv);

                // 그라데이션 및 알파값 계산
                float alpha = smoothstep(_Radius, _Radius + _Smoothness, dist);

                return fixed4(_Color.rgb, alpha);
            }
            ENDCG
        }
    }
}