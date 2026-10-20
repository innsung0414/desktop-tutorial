Shader "Deep Blue/Lake Surface"
{
    Properties
    {
        _DeepColor("Deep water", Color) = (0.018,0.15,0.18,1)
        _ShallowColor("Ripple color", Color) = (0.09,0.29,0.28,1)
        _SkyTex("Dawn sky", 2D) = "gray" {}
        _ReflectionStrength("Sky reflection", Range(0,1)) = 0.65
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        Pass
        {
            Name "LakeForward"
            Tags { "LightMode"="UniversalForward" }
            Cull Back ZWrite On
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            TEXTURE2D(_SkyTex); SAMPLER(sampler_SkyTex);
            CBUFFER_START(UnityPerMaterial)
                half4 _DeepColor;
                half4 _ShallowColor;
                half _ReflectionStrength;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; half fog : TEXCOORD1; };
            Varyings Vert(Attributes v)
            {
                Varyings o;
                VertexPositionInputs p=GetVertexPositionInputs(v.positionOS.xyz);
                o.positionCS=p.positionCS; o.positionWS=p.positionWS;
                o.fog=ComputeFogFactor(p.positionCS.z);
                return o;
            }
            half4 Frag(Varyings i) : SV_Target
            {
                float2 p=i.positionWS.xz;
                float t=_Time.y;
                float a=dot(p,float2(1.7,0.6))+t*0.65;
                float b=dot(p,float2(-0.9,2.1))-t*0.47;
                float c=dot(p,float2(5.6,3.8))+t*1.05;
                float d=dot(p,float2(-8.2,4.1))-t*0.8;
                float2 slope=float2(cos(a)*0.046+cos(c)*0.018,cos(b)*0.041+cos(d)*0.015);
                half3 n=normalize(half3(-slope.x,1,-slope.y));
                half3 view=SafeNormalize(GetWorldSpaceViewDir(i.positionWS));
                half fresnel=0.05+0.8*pow(1-saturate(dot(n,view)),4);
                half3 ray=reflect(-view,n);
                float2 uv=float2(atan2(ray.x,ray.z)/(2*PI)+0.5,asin(clamp(ray.y,-1,1))/PI+0.5);
                half3 sky=SAMPLE_TEXTURE2D(_SkyTex,sampler_SkyTex,uv).rgb;
                half3 probe=GlossyEnvironmentReflection(ray,0.16h,1.0h);
                half3 reflection=lerp(sky,probe,0.65);
                half ripple=sin(a)*sin(b)*0.5+0.5;
                half3 water=lerp(_DeepColor.rgb,_ShallowColor.rgb,ripple*0.26);
                Light sun=GetMainLight(TransformWorldToShadowCoord(i.positionWS));
                half3 h=SafeNormalize(view+sun.direction);
                half glint=pow(saturate(dot(n,h)),380)*1.8;
                half3 color=lerp(water,reflection,fresnel*_ReflectionStrength);
                color+=glint*sun.color*sun.shadowAttenuation;
                color*=lerp(0.72,1.0,sun.shadowAttenuation);
                return half4(MixFog(color,i.fog),1);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}
