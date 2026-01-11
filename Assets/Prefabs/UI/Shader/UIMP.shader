Shader "Unlit/UIMP"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ColorLeft ("Left Color", Color) = (0.22,0.55,0.80,1) 
        _ColorRight ("Right Color", Color) = (0.42,0.75,1.0,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            fixed4 _ColorLeft;
            fixed4 _ColorRight;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 texCol = tex2D(_MainTex, i.uv);
                fixed4 gradCol = lerp(_ColorRight, _ColorLeft, i.uv.x);
                return texCol * gradCol;
            }
            ENDCG
        }
    }
}
