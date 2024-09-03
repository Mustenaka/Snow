Shader "Snow/SnowFieldEffectShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TesselFactor("Tessellation Factor",Range(1,100)) = 5
        [Toggle]_HeightOnOff("Height On Off",int) = 1
        _HeightTex("Height Texture",2D) = "white" {}
        _HeightPower("Height Power",Range(1,100)) = 1
        _HeightBase("Height Base",Range(0,1)) = 0.5
        [Toggle]_HeightInverse("Height Inverse",int) = 1
    }
    SubShader
    {
        Cull Off    //对背面的深度写入无效    
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex tessvert
            #pragma fragment frag
            #pragma hull hs
            #pragma domain ds
            #pragma target 4.6

            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 tangent : TANGENT;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 tangent : TANGENT;
                float3 normal : NORMAL;
            };

            struct tessellation_appdata
            {
                float4 vertex : INTERNALTESSPOS;
                float4 tangent : TANGENT;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            int _TesselFactor;          //细分参数
            int _HeightOnOff;           //高度开启
            sampler2D _HeightTex;       //高度图
            float _HeightPower;         //高度强度
            float _HeightBase;          //高度基准
            int _HeightInverse;         //高度反转

            tessellation_appdata tessvert(appdata v)
            {
                tessellation_appdata o;
                o.vertex = v.vertex;
                o.tangent = v.tangent;
                o.normal = v.normal;
                o.uv = v.uv;
                return o;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                if(_HeightOnOff)
                {
                    //通过高度图的r通道，对vertex进行normal朝向的偏移
                    float r = tex2Dlod(_HeightTex,float4(o.uv,0,0)).r-_HeightBase;
                    v.vertex += _HeightInverse?-float4(v.normal*r*_HeightPower,0):float4(v.normal*r*_HeightPower,0);
                }
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            UnityTessellationFactors hsconst(InputPatch<tessellation_appdata,3> v)
            {
                UnityTessellationFactors o;
                float4 tf = float4(_TesselFactor,_TesselFactor,_TesselFactor,_TesselFactor);
                o.edge[0] = tf.x;
                o.edge[1] = tf.y;
                o.edge[2] = tf.z;
                o.inside = tf.w;
                return o;
            }

            [UNITY_domain("tri")]
            [UNITY_partitioning("fractional_odd")]
            [UNITY_outputtopology("triangle_cw")]
            [UNITY_patchconstantfunc("hsconst")]
            [UNITY_outputcontrolpoints(3)]
            tessellation_appdata hs(InputPatch<tessellation_appdata,3> v,uint id:SV_OutputControlPointID)
            {
                return v[id];
            }

            [UNITY_domain("tri")]
            v2f ds(UnityTessellationFactors tessfactors,const OutputPatch<tessellation_appdata,3> vi,float3 bary:SV_DOMAINLOCATION)
            {
                appdata v;
 
			    v.vertex = vi[0].vertex*bary.x + vi[1].vertex*bary.y + vi[2].vertex*bary.z;
			    v.tangent = vi[0].tangent*bary.x + vi[1].tangent*bary.y + vi[2].tangent*bary.z;
			    v.normal = vi[0].normal*bary.x + vi[1].normal*bary.y + vi[2].normal*bary.z;
			    v.uv = vi[0].uv*bary.x + vi[1].uv*bary.y + vi[2].uv*bary.z;
 
			    v2f o = vert(v);
			    return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
