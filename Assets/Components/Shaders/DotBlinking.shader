Shader "OneOneOneTwoThree/DotBlinking" {
		Properties {
	
	}
	SubShader {
		Tags {"Queue" = "Geometry" "RenderType"="Opaque" }
		
		
		Pass{
			
			Cull off
			
			ZWrite On
			ZTest LEqual
			//Fog Disable
			Blend SrcAlpha OneMinusSrcAlpha
			//Blend OneMinusDstColor OneMinusSrcColor //negative color
			
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
				o.normal   = mul ((float3x3)UNITY_MATRIX_IT_MV, v.normal);
				//float2 offset = TransformViewToProjection(o.normal.xy);
			 
				float offset = v.texcoord.y;
				o.color = 1- saturate( frac(offset-(_Time.y))*2 ) ;
				
				o.uv = MultiplyUV (UNITY_MATRIX_TEXTURE0, v.texcoord);
				return o; 
			}

			float4 frag (v2f i) : COLOR
			{
		
				return float4(1,1,1,i.color.a) ;
			}
			
			ENDCG
		}
	}
	FallBack "Diffuse"
}
