Shader "OneOneOneTwoThree/CardRedrawLate" {
		Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_LineWidth("LineWidth", Range (0,20)) = 10
	}
	SubShader {
		Tags {"Queue" = "Geometry" "RenderType"="Opaque" }
		
		Pass{
			
			Cull back
			
			ZWrite On
			ZTest LEqual
			ColorMask 0
			//Offset -10,-10
			//Blend SrcAlpha OneMinusSrcAlpha
			
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			
			float4 _Color;
		
			
			
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
				float4 outColor  = _Color;
				outColor.a = 0;
				return _Color;
			}
			
			ENDCG
		}
		Pass{
			
			Cull front
			
			ZWrite Off
			ZTest LEqual
			//Fog Disable
			//Blend SrcAlpha OneMinusSrcAlpha
			Blend OneMinusDstColor OneMinusSrcColor 
			
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			
			float4 _Color;
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
			 
			
				
				float2 pixelSize = (1/_ScreenParams.xy) * _LineWidth;
				o.pos.xy += (o.normal.xy * pixelSize)* o.pos.z;// * o.pos.z;
				
				
				o.color = v.color;
				o.uv = MultiplyUV (UNITY_MATRIX_TEXTURE0, v.texcoord);
				return o; 
			}

			float4 frag (v2f i) : COLOR
			{
				/*float4 outColor  = _Color;
				outColor.xyz= 1 - outColor.xyz;
				return outColor;*/
				return float4(1,1,1,1);
			}
			
			ENDCG
		}
	}
	FallBack "Diffuse"
}
