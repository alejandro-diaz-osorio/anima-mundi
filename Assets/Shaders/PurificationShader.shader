Shader "Custom/PurificationShader"
{
    Properties
    {
        _CorruptedTex ("Corrupted Texture", 2D) = "white" {}
        _PurifiedTex ("Purified Texture", 2D) = "white" {}
        _TransitionSmoothness ("Transition Smoothness", Range(0, 1)) = 0.5
        _Color ("Tint Color", Color) = (1,1,1,1)
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.5
        
        sampler2D _CorruptedTex;
        sampler2D _PurifiedTex;
        // Unity auto-generates these, so we DON'T declare them manually:
        // float4 _CorruptedTex_ST;
        // float4 _PurifiedTex_ST;
        
        float _TransitionSmoothness;
        float4 _Color;
        half _Glossiness;
        half _Metallic;
        
        // Arrays from script - GPU limit is exactly 3929 elements
        uniform float4 _PurificationPoints[3929];
        uniform int _PurificationCount;
        
        struct Input
        {
            float2 uv_CorruptedTex;
            float3 worldPos;
        };
        
        // Calculate purification amount at a given world position
        float CalculatePurification(float3 worldPos)
        {
            float purificationAmount = 0.0;
            
            // Check each purification point (max 3929 - GPU limit)
            for (int i = 0; i < _PurificationCount && i < 3929; i++)
            {
                float3 pointPos = _PurificationPoints[i].xyz;
                float radius = _PurificationPoints[i].w;
                
                // Calculate distance to purification point
                float distance = length(worldPos - pointPos);
                
                // Calculate influence (1.0 at center, 0.0 at radius edge)
                float influence = 1.0 - saturate(distance / radius);
                
                // Apply smoothness curve
                float smoothness = max(0.01, _TransitionSmoothness);
                influence = smoothstep(0.0, 1.0, influence / (1.0 - smoothness * 0.9));
                
                // Accumulate purification (max operation keeps the highest value)
                purificationAmount = max(purificationAmount, influence);
            }
            
            return saturate(purificationAmount);
        }
        
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Sample both textures
            // Unity automatically applies tiling/offset using uv_CorruptedTex
            fixed4 corruptedColor = tex2D(_CorruptedTex, IN.uv_CorruptedTex);
            fixed4 purifiedColor = tex2D(_PurifiedTex, IN.uv_CorruptedTex);
            
            // Calculate purification amount at this world position
            float purificationAmount = CalculatePurification(IN.worldPos);
            
            // Blend between corrupted and purified textures
            fixed4 finalColor = lerp(corruptedColor, purifiedColor, purificationAmount);
            
            // Apply tint
            finalColor *= _Color;
            
            // Output
            o.Albedo = finalColor.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = finalColor.a;
        }
        ENDCG
    }
    
    FallBack "Standard"
}