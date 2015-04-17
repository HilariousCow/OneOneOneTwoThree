Shader "OneOneOneTwoThree/PreviewTokens" {
	Properties {
		_LineWidth("LineWidth", Range (0,20)) = 10
	}
	SubShader {
		Tags {"Queue" = "Geometry" "RenderType"="Opaque" }
		
		//depth mask first
		Pass{
			
			Cull Back
			
			ZWrite On
			ZTest LEqual
			ColorMask 0
		//	Blend SrcAlpha OneMinusSrcAlpha
			
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
			//	outColor.a = 0;
				return outColor;
			}
			
			ENDCG
		}
		//shell pass
		Pass{
			
			Cull Front
			
			ZWrite Off
			ZTest LEqual
			Blend OneMinusDstColor OneMinusSrcColor 
			
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			
			float _LineWidth;
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
			 
				//o.pos.xy += o.normal.xy * 0.0050f * o.pos.z;// / o.pos.z * 10.0f;
	
				float2 pixelSize = (1/_ScreenParams.xy) * _LineWidth;
				o.pos.xy += (o.normal.xy * pixelSize)* o.pos.z;// * o.pos.z;
				//o.pos.z += o.normal.z*pixelSize;
				
				o.color = v.color;
				o.uv = MultiplyUV (UNITY_MATRIX_TEXTURE0, v.texcoord);
				return o; 
			}

			float4 frag (v2f i) : COLOR
			{
				float4 outColor  = float4(1,1,1,1);
				
				return outColor;
			}
			
			ENDCG
		}
	} 
	FallBack "Diffuse"
}
