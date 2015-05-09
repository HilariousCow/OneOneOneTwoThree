Shader "OneOneOneTwoThree/VertexFakeAlpha" {
	Properties {
		
	}
	SubShader {
		Tags {"Queue" = "Transparent" "RenderType"="Transparent" }
		
		Pass{
			
			Cull Off
			
			ZWrite Off
			ZTest LEqual
			
			Blend OneMinusDstColor OneMinusSrcColor
			
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			
			
		
			
			
			struct appdata {
				float4 vertex	: POSITION;
				float4 color : COLOR;
				float3 normal : NORMAL;
				
				float2 texcoord: TEXCOORD0;
			};
 
			struct v2f {
				float4 pos		: SV_POSITION;
				float4 color : COLOR;
				float3 normal	: NORMAL;
				float2 uv		: TEXCOORD0;
			};

			v2f vert (appdata v)
			{
				v2f o;
				
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.normal = (mul((float3x3)UNITY_MATRIX_MVP, v.normal));
				o.color = v.color;
				o.uv = MultiplyUV (UNITY_MATRIX_TEXTURE0, v.texcoord);
				return o; 
			}

			float4 frag (v2f i) : COLOR
			{
				float4 outColor = i.color;
				return outColor;
			}
			
			ENDCG
		}
	} 
	FallBack "Diffuse"
}
