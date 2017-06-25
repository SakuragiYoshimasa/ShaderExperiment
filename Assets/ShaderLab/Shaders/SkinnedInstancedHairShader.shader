 Shader "Instanced/SkinnedInstancedHairShader" {
    Properties {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _MainColor ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _GradColor ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Scale("Scale", Range(1.0, 10.0)) = 1.0
        
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
    
        CGPROGRAM
        #pragma surface surf Standard vertex:vert
        #pragma instancing_options procedural:setup
        #include "SimplexNoise3D.cginc"
        
        sampler2D _MainTex;

        struct Input {
            float2 uv_MainTex;
            int seg;
        };

        #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
        StructuredBuffer<float4> _PositionBuffer;
        StructuredBuffer<float4> _RotationBuffer;
        StructuredBuffer<float4> _PrevPositionBuffer;
        uint _ArraySize;
        uint _InstanceCount;
        uint _SegmentCount;
        float3 _Gravity;
        float _Scale;
        float _ZScale;
        float _NoisePower;
        float _Frequency;
        float _RadiusAmp;
        #endif

        void vert(inout appdata_full v, out Input data){
            UNITY_INITIALIZE_OUTPUT(Input, data);
        
        #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED

            float phi = v.vertex.x;
            int seg = (int)v.vertex.z;
            data.seg = seg;

            float id = (float)unity_InstanceID;
            float4 base =  _PositionBuffer[unity_InstanceID];
            float3 prev = _PrevPositionBuffer[unity_InstanceID].xyz;

            base.xyz = ((float)(_SegmentCount - seg) / (float)_SegmentCount) * base.xyz +  ((float)seg / (float)_SegmentCount) * prev;

            float4 rotation = _RotationBuffer[unity_InstanceID];
            float radius = base.w;
            float length = rotation.w;

            float2 xy = float2(cos(phi), sin(phi)) * radius * _RadiusAmp * ((float)(_SegmentCount - seg) / (float)_SegmentCount);
            float4 offset = float4(xy, v.vertex.z * length * _ZScale, 1.0);
            float3 n_normal = float3(cos(phi), sin(phi), 0);
            
            float a = rotation.x;
            float b = rotation.y;
            float c = rotation.z;
            
            float4 low1 = float4(cos(a) * cos(b) * cos(c) - sin(a) * sin(c), -cos(a) * cos(b) * sin(c) - sin(a) * cos(c), cos(a) * sin(b), 0);
            float4 low2 = float4(sin(a) * cos(b) * cos(c) + cos(a) * sin(c), -sin(a) * cos(b) * sin(c) + cos(a) * cos(c), sin(a) * sin(b), 0);
            float4 low3 = float4(-sin(b) * cos(c), sin(b) * sin(c), cos(b), 0); 
            float4 low4 = float4(0, 0, 0, 1);
            float4x4 rotateMat;
            rotateMat._11_12_13_14 = low1;
            rotateMat._21_22_23_24 = low2;
            rotateMat._31_32_33_34 = low3;
            rotateMat._41_42_43_44 = low4;

            if(seg != 0){
                offset.x += snoise(float3(
                    (float)seg * 0.02 + sin((_Time.x + id * 0.1) * _Frequency),
                    radius + sin(_Frequency *(_Time.x +(float)seg * 0.03)) * cos(_Frequency * (_Time.y -  id * 0.1)),
                    length + cos(_Frequency * (_Time.y)))) * _NoisePower;
                     
                offset.y += snoise(float3(
                    (float)seg * 0.02 + cos(_Time.y * _Frequency), 
                    radius - sin(_Time.y *  _Frequency) * cos(_Frequency * (_Time.x +(float)seg * 0.03 + id * 0.1)), 
                    length + cos(_Frequency * (_Time.x +(float)seg * 0.03 + id *0.2)))) * _NoisePower;
            }
            
            float3 pos = mul(rotateMat, offset).xyz;
            n_normal = float3(cos(phi), sin(phi), 0);
            if(seg!=0) pos += _Gravity * (abs(pos.x) + abs(pos.y - 1.5)) * 0.03;

            v.vertex.xyz = (base.xyz + pos.xyz) * _Scale;
            v.normal.xyz = mul(rotateMat, n_normal);

        #endif
        }

        void setup(){
            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            #endif
        }

        half _Glossiness;
        half _Metallic;
        fixed4 _MainColor;
        fixed4 _GradColor;

        void surf (Input IN, inout SurfaceOutputStandard o) {
            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            fixed4 c = _MainColor * (float)IN.seg / _SegmentCount + (1.0 -  (float)IN.seg / _SegmentCount) * _GradColor;
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
            #endif
        }
        ENDCG
    }
    FallBack "Diffuse"
}