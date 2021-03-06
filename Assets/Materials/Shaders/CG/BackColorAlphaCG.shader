Shader "Universe/CG/Back Color Alpha" 
{
    Properties {
       // _MainTex ("Base (RGB)", 2D) = "white" {}
    }
    
    SubShader {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
        Lighting Off
        ZWrite Off
        //ZTest On
        
        Pass {
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            
            //uniform sampler2D _MainTex;
            
            struct vertexInput {
                float4 vertex : POSITION;
               // fixed4 texcoord : TEXCOORD0;
                fixed4 color : COLOR;
            };            
            
            struct fragmentInput {
                float4 position : SV_POSITION;
                //fixed4 texcoord : TEXCOORD0;
                fixed4 color : COLOR;
            };
         
            fragmentInput vert(vertexInput i){
                fragmentInput o;
                o.position = mul (UNITY_MATRIX_MVP, i.vertex);
                //o.texcoord = i.texcoord;
                o.color = i.color;
                return o;
            }
            
            float4 frag(fragmentInput i) : COLOR {
                return i.color;// tex2D(_MainTex, i.texcoord.xy) * i.color;
            }   
            
            ENDCG
        }
    }
    
    Fallback "Universe/FixedPipeline/Vertex Color Alpha"
}
