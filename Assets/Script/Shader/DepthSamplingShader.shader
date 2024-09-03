Shader "Snow/DepthSamplingShader"
{ 
    SubShader
    {
        Tags { "RenderType"="Opaque" }
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
                float4 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _CameraDepthTexture;          //unity提供我们camera采样的深度图

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = ComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //对深度图进行0-1线性转换
                fixed depth = Linear01Depth(tex2D(_CameraDepthTexture,i.uv.xy/i.uv.w).r);
                fixed4 col = fixed4(depth,depth,depth,1);
                return col;
            }
            ENDCG
        }
    }
}
