Shader "Deep Blue/Forest Ground"
{
    Properties
    {
        _Soil("Shore gravel",2D)="white"{}
        _Moss("Forest moss",2D)="white"{}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            TEXTURE2D(_Soil); SAMPLER(sampler_Soil);
            TEXTURE2D(_Moss); SAMPLER(sampler_Moss);
            struct A { float4 positionOS:POSITION; float3 normalOS:NORMAL; };
            struct V { float4 positionCS:SV_POSITION; float3 p:TEXCOORD0; half3 n:TEXCOORD1; half fog:TEXCOORD2; };
            V Vert(A v){V o;VertexPositionInputs p=GetVertexPositionInputs(v.positionOS.xyz);o.positionCS=p.positionCS;o.p=p.positionWS;o.n=TransformObjectToWorldNormal(v.normalOS);o.fog=ComputeFogFactor(p.positionCS.z);return o;}
            float Hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
            float Noise(float2 p){float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(Hash(i),Hash(i+float2(1,0)),f.x),lerp(Hash(i+float2(0,1)),Hash(i+1),f.x),f.y);}
            half4 Frag(V i):SV_Target
            {
                float noise=Noise(i.p.xz*0.2)*0.7+Noise(i.p.xz*0.7)*0.3;
                float path=(1-smoothstep(1.1,2.4,abs(i.p.x+sin(i.p.z*0.16)*0.6)))*(1-smoothstep(3,6,i.p.z));
                float bank=smoothstep(-0.15,0.25,i.p.y);
                float moss=saturate((noise-0.25)*2.2+smoothstep(0.1,2.0,i.p.y)*0.5)*bank*(1-path);
                half3 soil=SAMPLE_TEXTURE2D(_Soil,sampler_Soil,i.p.xz*0.4).rgb;
                half3 vegetation=SAMPLE_TEXTURE2D(_Moss,sampler_Moss,i.p.xz*0.32).rgb*half3(0.8,0.88,0.8);
                half3 albedo=lerp(soil,vegetation,moss);
                half3 normal=normalize(i.n);
                Light light=GetMainLight(TransformWorldToShadowCoord(i.p));
                half3 lighting=SampleSH(normal)+light.color*saturate(dot(normal,light.direction))*light.shadowAttenuation;
                return half4(MixFog(albedo*lighting,i.fog),1);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}
