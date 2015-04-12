﻿Shader "OneOneOneTwoThree/CardRedrawLateHighlight" {
		Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_LineWidth("LineWidth", Range (0,0.1)) = 0.05
		_Rate("Rate", Range (0,4)) = 0
		_InnerPow("_InnerPow", Range (0,8)) = 1
	}
	SubShader {
		Tags {"Queue" = "Geometry" "RenderType"="Opaque" }
		
		Pass{
			
			Cull front
			
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
			float _LineWidth;
			float _Rate;
			float _InnerPow;
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
			 
				//o.pos.xy += offset * o.pos.z * 0.005f;
				//o.pos.xy += offset / o.pos.z * 15.0f;
				
				
				//this is the mask
				float fat =  _LineWidth-0.01 + 0.0050f;// *1.1  ;
				float fraction = frac( -_Time.y*_Rate );
				fat *=  pow(fraction, _InnerPow);
				fat +=  0.01;
				o.pos.xy += o.normal.xy * fat * o.pos.z;
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
			
			ZWrite On
			ZTest LEqual
			//Fog Disable
			//Blend SrcAlpha OneMinusSrcAlpha
			Blend OneMinusDstColor OneMinusSrcColor //negative color
			
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			
			float4 _Color;
			float _LineWidth;
			float _Rate;
			
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
			 
				//outline
				float fat =  _LineWidth + 0.0050f ;
				float fraction = smoothstep(0,1,frac( -_Time.y*_Rate ));
				fraction *= pow(fraction,1.5);
				fat *= fraction ;
				
				o.pos.xy += o.normal.xy * fat * o.pos.z;
				o.color = lerp(_Color, float4(0,0,0,0), fraction);
				o.uv = MultiplyUV (UNITY_MATRIX_TEXTURE0, v.texcoord);
				return o; 
			}

			float4 frag (v2f i) : COLOR
			{
		
				return i.color*5;
			}
			
			ENDCG
		}
	}
	FallBack "Diffuse"
}
