Shader "Snow/HeightSamplingShader"
{
    Properties
    {
        _SnowFieldDepthTex("SnowField Depth Texture",2D) = "white" {}
    }

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

            sampler2D _CameraDepthTexture;  
            
            sampler2D _SnowFieldDepthTex;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = ComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv.xy/i.uv.w;
                //因为深度摄像机相反后，需要处理uv.x的反向
                float2 iuv = float2(1-uv.x,uv.y);
                fixed depth = Linear01Depth(tex2D(_CameraDepthTexture,iuv).r);
                //还原snowfield的深度值
                fixed sfdepth = 1-tex2D(_SnowFieldDepthTex,uv).r;
                //计算snowfield与player的深度差
                fixed difdepth = saturate(sfdepth-depth);
                fixed4 col = fixed4(difdepth,difdepth,difdepth,1);
                return col;
            }
            ENDCG
        }
    }
}
